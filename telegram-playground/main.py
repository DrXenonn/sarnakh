from telethon import TelegramClient
from datetime import datetime, timedelta, timezone
from dotenv import load_dotenv
import redis
import json
import os
import asyncio
import signal

load_dotenv()

r = redis.Redis(
    host='localhost',
    port=6379,
    db=0,
    socket_timeout=15,          # read timeout, comfortably above BLOCK_TIMEOUT
    socket_connect_timeout=5,
    retry_on_timeout=True,      # auto-retry once on socket-level timeout
)

api_id = int(os.getenv("API_ID"))
api_hash = os.getenv("API_HASH")

COMMANDS_QUEUE = "telegram_commands"
RESULTS_QUEUE = "telegram_results"
BATCH_SIZE = 50
BLOCK_TIMEOUT = 5
STOP_KEY_PREFIX = "stop:"          # stop:{process_id} -> set to request cancellation
STOP_KEY_TTL = 3600                # auto-expire stop flags after 1h so they don't pile up
STOP_CHECK_EVERY = 20              # check the stop flag every N messages (cheap, avoids a Redis round-trip per message)

client = TelegramClient("session", api_id, api_hash)


def push_result(payload: dict):
    r.rpush(RESULTS_QUEUE, json.dumps(payload, ensure_ascii=False))


def is_stop_requested(process_id: str) -> bool:
    return r.exists(f"{STOP_KEY_PREFIX}{process_id}") == 1


def clear_stop_flag(process_id: str):
    r.delete(f"{STOP_KEY_PREFIX}{process_id}")


async def scan_historical(process_id: str, group_id: int, days_back: int):
    entity = await client.get_entity(group_id)
    group_name = entity.title
    cutoff = datetime.now(timezone.utc) - timedelta(days=days_back)

    batch = []
    total = 0
    stopped = False
    checked = 0

    async for message in client.iter_messages(group_id):
        checked += 1
        if checked % STOP_CHECK_EVERY == 0 and is_stop_requested(process_id):
            stopped = True
            break

        if message.date is None:
            continue
        if message.date < cutoff:
            break
        if not message.text:
            continue

        sender = await message.get_sender()
        username = sender.username if sender else None

        batch.append({
            "message_id": message.id,
            "date": message.date.isoformat(),
            "sender_id": message.sender_id,
            "username": username,
            "text": message.text,
            "group_id": group_id,
            "group_name": group_name,
        })

        if len(batch) >= BATCH_SIZE:
            push_result({
                "command": "scan_historical",
                "process_id": process_id,
                "messages": batch,
                "status": "running",
            })
            total += len(batch)
            batch = []

    if batch:
        total += len(batch)
        push_result({
            "command": "scan_historical",
            "process_id": process_id,
            "messages": batch,
            "status": "running",
        })

    # Terminator: signals the scan for this group is finished, either completed
    # naturally or stopped early. status is one of: "completed" | "stopped".
    # No summary block - the consumer already knows group_id/group_name/days_back
    # from the original command and every message it already received.
    push_result({
        "command": "scan_historical",
        "process_id": process_id,
        "messages": [],
        "status": "stopped" if stopped else "completed",
    })

    if stopped:
        clear_stop_flag(process_id)
        print(f"  [scan:{process_id}] Stopped early: {total} msgs, {group_name}")
    else:
        print(f"  [scan:{process_id}] Done: {total} msgs, {group_name}")


async def process_command(raw: str):
    try:
        cmd = json.loads(raw)
    except json.JSONDecodeError:
        print(f"  Invalid JSON: {raw}")
        return

    action = cmd.get("action")
    process_id = cmd.get("process_id", "?")

    if action == "scan_historical":
        group_ids = cmd.get("group_ids", [])
        days_back = cmd.get("days_back", 7)
        print(f"  [scan:{process_id}] Scanning {len(group_ids)} group(s), {days_back}d back")
        for gid in group_ids:
            await scan_historical(process_id, gid, days_back)

    elif action == "stop_scan":
        # Sets a flag scan_historical checks periodically. Auto-expires so a
        # stop request for a process that already finished doesn't linger forever.
        r.set(f"{STOP_KEY_PREFIX}{process_id}", "1", ex=STOP_KEY_TTL)
        print(f"  [stop:{process_id}] Stop requested")

    else:
        push_result({
            "command": "error",
            "process_id": process_id,
            "error": f"Unknown action: {action}",
        })
        print(f"  [error:{process_id}] Unknown action: {action}")


async def main():
    print("Worker started. Waiting for commands on 'telegram_commands'...")
    print("Actions: scan_historical, stop_scan\n")

    running = True

    def shutdown():
        nonlocal running
        running = False

    loop = asyncio.get_event_loop()
    for sig in (signal.SIGINT, signal.SIGTERM):
        try:
            loop.add_signal_handler(sig, shutdown)
        except NotImplementedError:
            pass

    while running:
        try:
            result = r.blpop(COMMANDS_QUEUE, timeout=BLOCK_TIMEOUT)
        except (redis.ConnectionError, redis.exceptions.TimeoutError) as e:
            print(f"  [WARN] Redis connection issue ({type(e).__name__}). Retrying in 5s...")
            await asyncio.sleep(5)
            continue

        if result is None:
            continue

        _, raw = result

        try:
            await process_command(raw.decode("utf-8"))
        except Exception as e:
            print(f"  [ERROR] process_command crashed: {type(e).__name__}: {e}")
            # Uncomment for full traceback during debugging:
            # import traceback; traceback.print_exc()

    print("Shutting down...")


with client:
    client.loop.run_until_complete(main())
