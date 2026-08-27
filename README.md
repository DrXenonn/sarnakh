# Sarnakh (سرنخ)

Sarnakh monitors public Telegram communities and turns relevant conversations into actionable leads.

It is built for cases where potential customers, requests, or opportunities are already being discussed in Telegram groups, but manually watching those conversations at scale is impractical.

## How It Works

Sarnakh consists of two main components:

- **Telegram Worker** — a lightweight Python service that reads raw messages from configured Telegram groups and forwards them in batches.
- **Sarnakh API** — the main .NET backend, which runs incoming messages through a filtering pipeline and persists the resulting leads.

The worker doesn't do any filtering or scoring itself — it's purely responsible for collecting raw messages and handing them off. All the intelligence lives in the API's pipeline, which matches messages against configured keywords, scores them for relevance, and saves the ones worth acting on as leads.

```text
            +-------------------+
            |  Telegram Groups  |
            +-------------------+
               |  raw messages
                      v
  +---------------------------------------+
  |        Telegram Worker (Python)       |
  |  reads messages, groups into batches  |
  +---------------------------------------+
              |  message batches
                      v
                 +---------+
                 |  Redis  |
                 +---------+
                      |
                      v
    +------------------------------------+
    |         Sarnakh API (.NET)         |
    |  Pipeline: keyword match -> score  |
    |             save leads             |
    +------------------------------------+
                      |
                      v
       +------------------------------+
       |  PostgreSQL  (leads stored)  |
       +------------------------------+
```

- The **worker** does no filtering — it just reads raw messages and batches them.
- **Redis** carries those batches from the worker to the API.
- The **API's pipeline** does all the thinking: keyword matching, relevance scoring, and deciding what becomes a lead.
- Qualifying leads are persisted to **PostgreSQL**, and the **Telegram Bot** (part of the API) is used to manage searches and review results.

## Features

- Monitor multiple public Telegram groups
- Configure searches with weighted keywords
- Process incoming Telegram messages through a pipeline
- Score messages based on keyword matches
- Track searches and their current status
- Collect and review discovered leads
- Telegram bot interface for managing searches
- JWT-based authentication
- PostgreSQL persistence
- Redis-based communication between the Telegram worker and API

## Tech Stack

### Backend

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- PostgreSQL
- Redis
- Serilog
- FluentValidation
- AutoMapper

### Telegram Worker

- Python
- Telethon
- Redis
- python-dotenv

### Testing

- xUnit
- Microsoft.NET.Test.Sdk

## Project Structure

```text
Sarnakh/
├── Sarnakh.Api/
│   ├── Bot/
│   ├── Data/
│   ├── Endpoints/
│   ├── Jobs/
│   ├── Models/
│   ├── Pipeline/
│   ├── Services/
│   └── ...
│
├── Sarnakh.Api.Tests/
│   ├── Dtos/
│   ├── Mappings/
│   └── PipelineSteps/
│
└── telegram-worker/
    ├── main.py
    ├── list_chats.py
    └── requirements.txt
```

## Development

Sarnakh currently requires a configured development environment rather than being a plug-and-play application.

The API is built with .NET 10. The Telegram worker requires Python and a Telegram API configuration for Telethon.

PostgreSQL and Redis can be started through the provided Docker Compose configuration:

```bash
cd Sarnakh.Api
docker compose up -d
```

The API can then be run through the project's development script:

```bash
./dev.sh
```

The Telegram worker uses a Python virtual environment and its required packages can be installed with:

```bash
cd telegram-worker
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

Configuration is intentionally environment-specific and is not included in the repository.

## Testing

Run the test suite from the repository root:

```bash
dotnet test
```

The current test suite covers components including DTOs, Telegram mappings, and pipeline keyword matching.

## Architecture

Sarnakh separates Telegram collection from application logic.

The Telegram worker is responsible for communicating with Telegram and collecting messages. Redis acts as the communication layer between the worker and the API.

The API owns the application state and processing logic. Incoming Telegram data is passed through the pipeline, where messages can be evaluated against the configuration of an active search.

This separation allows Telegram-specific functionality to remain isolated from the core application while keeping the processing logic in the main backend.

## Status

Sarnakh is an actively developed project. The current focus is on building and refining the core monitoring, processing, and lead-management workflow.
