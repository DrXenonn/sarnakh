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
    host="localhost",
    port=6379,
    db=0,
    socket_timeout=15,
    socket_connect_timeout=5,
    retry_on_timeout=True,
)

api_id = int(os.getenv("API_ID"))
api_hash = os.getenv("API_HASH")

COMMANDS_QUEUE = "telegram_commands"
RESULTS_QUEUE = "telegram_results"

BATCH_SIZE = 50
BLOCK_TIMEOUT = 5

STOP_KEY_PREFIX = "stop:"
STOP_KEY_TTL = 3600
STOP_CHECK_EVERY = 20

client = TelegramClient("session", api_id, api_hash)


def push_result(payload: dict):
    r.rpush(
        RESULTS_QUEUE,
        json.dumps(payload, ensure_ascii=False)
    )


def is_stop_requested(process_id: str) -> bool:
    return r.exists(f"{STOP_KEY_PREFIX}{process_id}") == 1


def clear_stop_flag(process_id: str):
    r.delete(f"{STOP_KEY_PREFIX}{process_id}")


async def scan_historical(
    process_id: str,
    group_id: int,
    days_back: int
) -> bool:
    entity = await client.get_entity(group_id)

    group_name = entity.title

    cutoff = (
        datetime.now(timezone.utc)
        - timedelta(days=days_back)
    )

    batch = []
    total = 0
    stopped = False
    checked = 0

    print(
        f"  [scan:{process_id}] "
        f"Starting group: {group_name} ({group_id})"
    )

    async for message in client.iter_messages(group_id):
        checked += 1

        # Check for cancellation periodically instead of
        # making a Redis request for every message.
        if checked % STOP_CHECK_EVERY == 0:
            if is_stop_requested(process_id):
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

    # Send remaining messages even if the scan was stopped.
    if batch:
        total += len(batch)

        push_result({
            "command": "scan_historical",
            "process_id": process_id,
            "messages": batch,
            "status": "running",
        })

    if stopped:
        print(
            f"  [scan:{process_id}] "
            f"Stopped early: {total} msgs, {group_name}"
        )
    else:
        print(
            f"  [scan:{process_id}] "
            f"Done: {total} msgs, {group_name}"
        )

    # IMPORTANT:
    # This function does NOT report "completed" or "stopped".
    # It only tells the caller whether this particular group
    # was stopped.
    return stopped


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

        print(
            f"  [scan:{process_id}] "
            f"Scanning {len(group_ids)} group(s), "
            f"{days_back}d back"
        )

        stopped = False

        for gid in group_ids:
            stopped = await scan_historical(
                process_id,
                gid,
                days_back
            )

            # If the current group detected a stop request,
            # DO NOT start the next group.
            if stopped:
                break

        # The process-level status is emitted HERE,
        # after all groups have been handled.
        final_status = "stopped" if stopped else "completed"

        push_result({
            "command": "scan_historical",
            "process_id": process_id,
            "messages": [],
            "status": final_status,
        })

        # Now that the process is definitely finished,
        # remove the cancellation flag.
        if stopped:
            clear_stop_flag(process_id)

            print(
                f"  [scan:{process_id}] "
                f"Process stopped"
            )
        else:
            print(
                f"  [scan:{process_id}] "
                f"Process completed"
            )

    elif action == "stop_scan":
        # We don't directly interrupt Telethon here.
        # We set a Redis flag which scan_historical checks
        # every STOP_CHECK_EVERY messages.
        r.set(
            f"{STOP_KEY_PREFIX}{process_id}",
            "1",
            ex=STOP_KEY_TTL
        )

        print(
            f"  [stop:{process_id}] "
            f"Stop requested"
        )

    else:
        push_result({
            "command": "error",
            "process_id": process_id,
            "error": f"Unknown action: {action}",
        })

        print(
            f"  [error:{process_id}] "
            f"Unknown action: {action}"
        )


async def main():
    print(
        "Worker started. "
        "Waiting for commands on 'telegram_commands'..."
    )

    print(
        "Actions: scan_historical, stop_scan\n"
    )

    running = True

    def shutdown():
        nonlocal running
        running = False

    loop = asyncio.get_event_loop()

    for sig in (signal.SIGINT, signal.SIGTERM):
        try:
            loop.add_signal_handler(
                sig,
                shutdown
            )
        except NotImplementedError:
            pass

    while running:
        try:
            result = r.blpop(
                COMMANDS_QUEUE,
                timeout=BLOCK_TIMEOUT
            )

        except (
            redis.ConnectionError,
            redis.exceptions.TimeoutError
        ) as e:
            print(
                f"  [WARN] Redis connection issue "
                f"({type(e).__name__}). "
                f"Retrying in 5s..."
            )

            await asyncio.sleep(5)
            continue

        if result is None:
            continue

        _, raw = result

        try:
            await process_command(
                raw.decode("utf-8")
            )

        except Exception as e:
            print(
                f"  [ERROR] process_command crashed: "
                f"{type(e).__name__}: {e}"
            )

            # Uncomment for full traceback while debugging:
            # import traceback
            # traceback.print_exc()

    print("Shutting down...")


with client:
    client.loop.run_until_complete(main())
