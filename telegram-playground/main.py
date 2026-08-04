from telethon import TelegramClient, events
from datetime import datetime, timedelta, timezone
from dotenv import load_dotenv
import redis
import json
import os
import asyncio
import signal

load_dotenv()

r = redis.Redis(host='localhost', port=6379, db=0)

api_id = int(os.getenv("API_ID"))
api_hash = os.getenv("API_HASH")

COMMANDS_QUEUE = "telegram_commands"
RESULTS_QUEUE = "telegram_results"
BATCH_SIZE = 50
BLOCK_TIMEOUT = 5  # seconds to block waiting for a command

client = TelegramClient("session", api_id, api_hash)
listening_tasks: dict[int, asyncio.Task] = {}  # group_id → live listener task


def push_result(result: dict):
    r.rpush(RESULTS_QUEUE, json.dumps(result, ensure_ascii=False))


async def scan_historical(scan_id: str, group_id: int, days_back: int):
    entity = await client.get_entity(group_id)
    group_name = entity.title
    cutoff = datetime.now(timezone.utc) - timedelta(days=days_back)

    batch = []
    total = 0

    async for message in client.iter_messages(group_id):
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
            push_result(scan_id=scan_id, messages=batch, done=False)
            total += len(batch)
            batch = []

    if batch:
        total += len(batch)
        push_result(scan_id=scan_id, messages=batch, done=False)

    push_result(scan_id=scan_id, messages=[], done=True, summary={
        "group_name": group_name,
        "group_id": group_id,
        "total_messages": total,
        "days_back": days_back,
    })

    print(f"  [scan:{scan_id}] Finished. {total} messages from {group_name}.")


def push_result(scan_id: str, messages: list, done: bool, count: int = None):
    payload = {
        "scan_id": scan_id,
        "command": "scan_historical",
        "messages": messages,
        "done": done,
    }
    if count is not None:
        payload["count"] = count
    stop_result(payload)


async def start_live_listener(listen_id: str, group_ids: list[int]):
    @client.on(events.NewMessage(chats=group_ids))
    async def handler(event):
        sender = await event.get_sender()
        username = sender.username if sender else None

        msg = {
            "message_id": event.message.id,
            "date": event.message.date.isoformat(),
            "sender_id": event.message.sender_id,
            "username": username,
            "text": event.message.text,
            "group_id": event.chat_id,
            "group_name": None,  # could resolve entity, but expensive per-message
        }
        push_result(listen_id=list(en_id, msg))

    task = asyncio.current_task()
    for gid in group_ids:
        listening_tasks[gid] = task

    push_result(listen_id=listen_id, started=True, group_ids=group_ids)
    print(f"  [listen:{listen_id}] Live listener started for groups {group_ids}.")

    # Keep alive until stopped by a stop command
    try:
        while True:
            await asyncio.sleep(60)
    except asyncio.CancelledError:
        for gid in group_ids:
            listening_tasks.pop(gid, None)
        client.remove_event_handler(handler)
        push_result(listen_id=listen_id, stopped=True)
        print(f"  [listen:{listen_id}] Live listener stopped.")


def push_listen_event(listen_id: str, **kwargs):
    payload = {"command": "listen", "listen_id": listen_id, **kwargs}
    stop_result(payload)


def stop_listen(listen_id: str, group_ids: list[int]):
    for gid in group_ids:
        task = listening_tasks.pop(gid, None)
        if task and not task.done():
            task.cancel()
    push_listen_event(listen_id=listen_id, stopped=True)


async def process_command(raw: str):
    try:
        cmd = json.loads(raw)
    except json.JSONDecodeError:
        print(f"  Invalid JSON command: {raw}")
        return

    action = cmd.get("action")
    req_id = cmd.get("request_id", "unknown")

    if action == "scan_historical":
        group_ids = cmd.get("group_ids", [])
        days_back = cmd.get("days_back", 7)
        print(f"  [scan:{req_id}] Scanning {len(group_ids)} group(s), {days_back} days back.")
        for gid in group_ids:
            await scan_historical(req_id, gid, days_back)

    elif action == "listen":
        group_ids = cmd.get("group_ids", [])
        print(f"  [listen:{req_id}] Starting live listener for {group_ids}.")
        asyncio.create_task(listen_live_group(req_id, group_ids))

    elif action == "stop_listen":
        group_ids = cmd.get("group_ids", [])
        print(f"  [listen:{req_id}] Stopping listener for {group_ids}.")
        stop_listen(req_id, group_ids)

    elif action == "list_listeners":
        active = {gid: str(t) for gid, t in listening_tasks.items()}
        push_result({
            "command": "list_listeners",
            "request_id": req_id,
            "active_groups": list(listening_tasks.keys()),
        })
        print(f"  [list:{req_id}] Active listeners: {list(listening_tasks.keys())}")

    else:
        push_result({
            "command": "error",
            "request_id": req_id,
            "error": f"Unknown action: {action}",
        })
        print(f"  [error:{req_id}] Unknown action: {action}")


async def main():
    print("Worker started. Waiting for commands on 'telegram_commands'...")
    print("Supported actions: scan_historical, listen, stop_listen, list")
    print("Press Ctrl+C to stop.\n")

    running = True

    def shutdown():
        nonlocal running
        running = False

    loop = asyncio.get_event_loop()
    for sig in (signal.SIGINT, signal.SIGTERM):
        try:
            loop.add_signal_handler(sig, shutdown)
        except NotImplementedError:
            # Windows doesn't support add_signal_handler
            pass

    while running:
        try:
            result = r.blpop(COMMANDS_QUEUE, timeout=PATCH_TIMEOUT)
        except redis.ConnectionError:
            print("Redis connection lost. Retrying in 5s...")
            await asyncio.sleep(5)
            continue

        if result is None:
            # timeout — no command received, loop back and check if we're still running
            continue

        _, raw = result
        await process_command(raw.decode("utf-8"))

    print("Shutting down...")


with client:
    client.loop.run_until_complete(main())