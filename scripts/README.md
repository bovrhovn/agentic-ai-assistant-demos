# Script Descriptions and Usage

## `Install-AZCLI.ps1`

Installs the Azure CLI on your local Windows machine. Prompts for confirmation before downloading and installing. After
installation, it logs you in, checks your subscription, and sets recommended configuration.

**How to invoke:**

```powershell
.\Install-AZCLI.ps1
```

---

## `Install-Bicep.ps1`

Downloads and installs (or upgrades) the Bicep CLI to the latest version. Adds Bicep to your user PATH for easy access.

**How to invoke:**

```powershell
.\Install-Bicep.ps1
```

---

## `Compile-Containers.ps1`

Compiles Docker containers using Azure CLI and Azure Container Registry Tasks. It requires the Azure CLI to be installed.

**How to invoke:**

```powershell
.\Compile-Containers.ps1 -ResourceGroupName "demo-rg" -RegistryName "acr-demo" -FolderName "containers" -TagName "latest" -SourceFolder "src"
```

---

## `local.env`

Environment variable file for local configuration. Not a script; used for storing key-value pairs.

**How to use:**
Reference in your scripts or applications to load environment variables. Use Add-DirToSystemEnv.ps1 to add the directory
containing this file to your system PATH.

```powershell
.\Set-EnvVariables.ps1 -EnvFileToReadFrom "local.env"
```
