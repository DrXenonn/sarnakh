#!/usr/bin/env python3
"""
List all Telegram chats/channels with their IDs.
Reads credentials from .env file.
"""
import asyncio
import os
from telethon import TelegramClient
from dotenv import load_dotenv

load_dotenv()

api_id = int(os.getenv("API_ID"))
api_hash = os.getenv("API_HASH")

async def main():
    async with TelegramClient("session", api_id, api_hash) as client:
        print(f"{'ID':<20} {'Type':<10} {'Title'}")
        print("-" * 80)

        async for dialog in client.iter_dialogs():
            dtype = "Channel" if dialog.is_channel else ("Group" if dialog.is_group else "User")
            print(f"{dialog.id:<20} {dtype:<10} {dialog.name}")

if __name__ == "__main__":
    asyncio.run(main())