[![NuGet](https://img.shields.io/nuget/v/Kompozap.svg)](https://www.nuget.org/packages/Kompozap)
# Kompozap

> A clean, interactive terminal UI to help simplify Docker Compose deployments.

Kompozap replaces manual `docker compose` commands and manual YAML version updates with a straightforward terminal workflow. Select the services you want to update, and Kompozap handles the building, pushing, and version bumping automatically.

## Features

* **Interactive TUI:** A easy-to-use terminal interface powered by [Spectre.Console](https://github.com/spectreconsole/spectre.console).
* **Selective Shipping:** Choose exactly which compose services to build and push using terminal prompts.
* **Zero-Touch Versioning:** Automatically generates new image tags and patches your `docker-compose.yml` with the updated references after a successful push.
* **Flexible Configuration**: Easily adjust paths, Docker commands, and automatic tagging rules.

## Installation

Install Kompozap globally as a .NET tool:

```bash
dotnet tool install --global Kompozap
```

## Usage
Run the tool directly from your terminal:
```bash
kompozap
```

## Configuration

Kompozap supports standard .NET configuration sources. 
The easiest way to configure it is by placing an `appsettings.json` file in the same directory as the Kompozap executable:

```json
{
  "WorkingDirectory": "../deployment",
  "Docker": {
    "ComposePath": "docker-compose.yml",
    "BuildArguments": "compose build",
    "PushArguments": "compose push"
  }
}
```
