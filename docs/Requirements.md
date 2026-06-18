# Overview

This tool is a AI agent cli runner with name `ai-runner`. Ensures that the AI CLI is wrapped with configured terminal multiplexer. It executes all configured agents in it's own directory.

# Cross-cutting requirement

**ACR-1.1**: The `ai-runner` is created in C# (.Net 10) as a ASP.NET application.
**ACR-1.2**: Application shall follow the Clean Architecture principles.

# Configuration

**ACR-2.1**: Following configuration controls application:
`{
    "root-folder": "c:/_data/_agents",
    "owner-name": "Peter",
    "commands": [
        {
            "name": "start",
            "command": "tmux new -d -s {{agent-name}}-agent \"cd {{root-folder}}/instances/{{agent-name}}-agent && copilot\""
        },
        {
            "name": "stop",
            "command": "tmux kill-session -t {{agent-name}}-agent"
        },
        {
            "name": "has-session",
            "command": "tmux has-session -t {{agent-name}}-agent"
        },
        { "name": "list-sessions", "command": "tmux list-sessions" },
        {
            "name": "send-keys",
            "command": "tmux send-keys -t {{agent-name}}-agent:0.0 \"{{command}}\" Enter"
        }
    ],
    "agents":[
        {
            "name":"boss",
            "report-to": null,
            "role": "boss"
        }
    ]
}`

The `root-folder` is the main folder where the agents are configured. Main root folder contains a `instances` folder where the agent folders are collected with the following template `{agent-name}-agent`, based on above example: `boss-agent`. Root folder also contains a `templates` directory where the agent template files are present organized by role.

The `commands` is a list of named command templates. Each command has a `name` and a `command` string that may contain template placeholders (e.g. `{{agent-name}}`, `{{root-folder}}`). Template values are resolved before execution.

The `owner-name` is the name of the human owner of the agents. Master control and target for important decisions. In agent configuration referenced as {{OWNER_NAME}}

The agent configuration: `name` is the name of the bot, referenced in the agent configuration as {{BOT_NAME}}.

# Requirements

**ACR-3.1**: Read configuration and initialize the structures. If root folder does not exists creates it. Also verifies the specified subfolders: `instances` and `templates`, if not exists it will create it.

**ACR-3.2**: During startup, it check or initialize the agents. Under the `instances` folder shall be an agent folder, which is generated agent name + "-agent" postfix.

**ACR-3.3**: After startup initialization need to start the agents: All agents need to be started in it's own directory {{agent-name}}-agent. The application looks up the command named `start` from the `commands` configuration list and resolves its template placeholders (`{{agent-name}}`, `{{root-folder}}`) before execution. The resolved command string is split into executable and arguments (first whitespace-delimited token is the executable, the remainder is passed as arguments) and launched as a new process in the agent's working directory.

**ACR-4.1**: Each agent has a persistent memory store (`memory.json`) in its agent folder. The memory is initialized during startup if it does not exist.

**ACR-4.2**: Memory entries have a unique ID, content (free text), optional tags, and a creation timestamp.

**ACR-4.3**: Memories can be saved per-agent via `SaveAgentMemoryUseCase`. Memories can be searched per-agent by content or tags (case-insensitive) via `SearchAgentMemoryUseCase`.
