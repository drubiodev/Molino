# Molino

## Prerequisites

### Copilot CLI (Headless Mode)

The Copilot SDK connects to a headless CLI server over TCP. Start it before running the API:

```bash
# Local development
copilot --headless --port 4321

# Docker
docker run -d --name copilot-cli \
    -p 4321:4321 \
    ghcr.io/github/copilot-cli:latest \
    --headless --port 4321
```

The CLI URL is configurable via `Llm:CopilotCliUrl` in appsettings (default: `localhost:4321`).
