# Ruka AI Server

Ruka AI is the optional cloud-backed default provider for Ruka Desktop Assistant.

## Required environment variables

- `RUKA_OPENAI_API_KEY`: server-side API key. Never put this in the desktop app or repository.
- `RUKA_AI_MODEL`: model name (default: `gpt-4o-mini`)
- `RUKA_RATE_LIMIT`: requests per minute per installation/IP bucket (default: 30)

## Endpoints

- `GET /health`
- `POST /v1/chat`

The desktop client sends an installation identifier in `X-Ruka-Installation`. The server forwards only the bounded conversation payload to the configured AI provider.

## Deployment

Deploy this ASP.NET Core 8 application to a server you control. Configure the environment variables as secrets. The desktop app should point to the public HTTPS base URL, for example:

`https://your-ruka-server.example/v1/chat`

Do not commit `RUKA_OPENAI_API_KEY`.

## Important

This is the first cloud-provider implementation. Production deployments should add durable authentication, abuse prevention, usage accounting, privacy controls, and monitoring before opening the endpoint to the public internet.
