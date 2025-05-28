# Agentic AI assistant demos

This repository is focused on AI assistant demos with the use of Agentic AI approaches with the use
of [Azure](https://azure.com) technologies.

## Structure

1. **[scripts](./scripts)**: Contains the scripts used to run the demos and establish development environments.
2. **[src](./src)**: Contains the source code for the demos based on architecture.
3. **[docs](./docs)**: Contains the documentation for the demos, including architecture diagrams and design documents.
4. **[containers](./containers)**: Contains the Dockerfiles and container configurations for the demos.

All the folders contain README files with instructions on how to start and what you need to do.

## Prerequisites

1. [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2)
   installed - we do recommend an editor like [Visual Studio Code](https://code.visualstudio.com) to be able to write
   scripts and work with code.
2. git installed - instructions step by step [here](https://docs.github.com/en/get-started/quickstart/set-up-git)
3. [.NET](https://dot.net) installed to run the application if you want to run it
4. an editor (besides notepad) to see and work with code, yaml, scripts and more (for
   example [Visual Studio Code](https://code.visualstudio.com) or [Visual Studio](https://visualstudio.microsoft.com/)
   or [Jetbrains Rider](https://jetbrains.com/rider))
5. [OPTIONAL] GitHub CLI installed to work with GitHub - [how to install](https://cli.github.com/manual/installation)
6. [OPTIONAL] [Github GUI App](https://desktop.github.com/) for managing changes and work
   on [forked](https://docs.github.com/en/get-started/quickstart/fork-a-repo) repo
7. [OPTIONAL] [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/install)

## Scripts

Scripts are available in [scripts folder](./scripts). The scripts are written
in [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/overview?view=powershell-7.2).

1. [Add-DirToSystemEnv.ps1](./scripts/Add-DirToSystemEnv.ps1) - adds a directory to the system environment variable
   PATH
2. [Compile-Containers.ps1](./scripts/Compile-Containers.ps1) - uses Azure CLI to compile containers with the help of
   Azure Registry Tasks - check also [containers folder](./containers) for dockerfile definition.
3. [Set-EnvVariables.ps1](./scripts/Set-EnvVariables.ps1) - Set environment variables from local.env file

# Links & additional information

- [Azure AI Foundry](https://learn.microsoft.com/en-us/azure/ai-foundry/)
- [Azure AI Studio](https://learn.microsoft.com/en-us/azure/ai-studio/)
- [Spectre Console](https://github.com/spectresystems/spectre.console/)

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks
or logos is subject to and must
follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks?oneroute=true).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft
sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.