# Overview

This tool is a AI agent cli runner with name `ai-runner`. Ensures that the AI CLI is wrapped with configured terminal multiplexer. It executes all configured agents in it's own directory.

# Cross-cutting requirement

**ACR-1.1**: The `ai-runner` is created in C# (.Net 10) as a ASP.NET application.
**ACR-1.2**: Application shall follow the Clean Architecture principles.

# Configuration

**ACR-2.1**: Following configuration controls application:
`{
    "root-folder": "c:/_data/_agents",
    "terminal-multiplexer": "tmux",
    "arguments": "copilot",
    "owner-name": "Peter",
    "agents":[
        {
            "name":"boss",
            "report-to": null,
            "role": "boss"
        }
    ]
}`

The `root-folder` is the main folder where the agents are configured. Main root folder contains a `instances` folder where the agent folders are collected with the following template `{agent-name}-agent`, based on above example: `boss-agent`. Root folder also contains a `templates` directory where the agent template files are present organized by role.

The `terminal-multiplexer` is the name of the wrapping process around the AI cli.
The `cli` is the name of the AI CLI starting command.

The `owner-name` is the name of the human owner of the agents. Master control and target for important decisions. In agent configuration referenced as {{OWNER_NAME}}

The agent configuration: `name` is the name of the bot, referenced in the agent configuration as {{BOT_NAME}}.

# Requirements

**ACR-3.1**: Read configuration and initialize the structures. If root folder does not exists creates it. Also verifies the specified subfolders: `instances` and `templates`, if not exists it will create it.

**ACR-3.2**: During startup, it check or initialize the agents. Under the `instances` folder shall be an agent folder, which is generated agent name + "-agent" postfix.

**ACR-3.3**: After startup initialization need to start the agents: All agents need to be started in it's own directory {{agent-name}}-agent. The application start a new process which contains the `teminal-multiplexer` and the `arguments`. The argument is a template text, and before use it is used the template values need to be replaced. Template values are {{agent-name}}, {{root-forlder}}. The template values are initialized from config and made available in an in-memory dictionary to use everywhere though a service.
