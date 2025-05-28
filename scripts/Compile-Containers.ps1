<#

 .SYNOPSIS

Compile containers inside the folder containers with az cli

.DESCRIPTION

Compile containers inside the folder containers with az cli and sends tasks to the registry to compile the code
 
.EXAMPLE

PS > Compile-Containers.ps1 

Compile containers inside the folder containers with az cli in resource group monitoring-rg and registry acr-monitoring

.EXAMPLE

PS > Compile-Containers.ps1 -ResourceGroupName "demo-rg" -RegistryName "acr-demo" -FolderName "containers" -TagName "latest" -SourceFolder "src"

Compiles containers inside the folder containers with az cli in resource group monitoring-rg and registry acr-monitoring
with folder name containers, tag name latest and source folder src
    
. LINK

https://github.com/bovrhovn/meetup-intro-to-kiota
 
#>
param(
    [Parameter(Mandatory = $true)]
    $ResourceGroupName = "rg-web",
    [Parameter(Mandatory = $true)]
    $RegistryName = "webregistry",
    [Parameter(Mandatory = $false)]
    $FolderName = "containers",
    [Parameter(Mandatory = $false)]
    $TagName = "latest",
    [Parameter(Mandatory = $false)]
    $SourceFolder = "KB",
    [Parameter(Mandatory=$false)]
    [switch]$InstallCli,
    [Parameter(Mandatory=$false)]
    [switch]$OpenLog
)
$logPath = "$HOME/Downloads/container-build.log"
Start-Transcript -Path $logPath -Force
if ($InstallCli)
{
    Start-Process Install-AzCli.ps1 -NoNewWindow -Wait
}

Write-Output "Reading registry $RegistryName in Azure"
$registry = Get-AzContainerRegistry -ResourceGroupName $ResourceGroupName -Name $RegistryName
$name = $registry.Name
Write-Output "Registry $name has been read"

Write-Output "Reading the folder $FolderName"
Get-ChildItem -Path $FolderName | ForEach-Object {
    if ($_.Extension.Contains("md")){
        Write-Output "Skipping file $($_.Name) as it is a markdown file."
        return
    }
    $imageRepo = $_.Name.Split('-')[0]
    $imageName = $_.Name.Split('-')[1]
    $dockerFile = $_.FullName
    Write-Output "Building image $imageName with tag $TagName based on $dockerFile"
    $imageNameWithTag = "$($name).azurecr.io/$($imageRepo.ToLower())/$($imageName.ToLower()):$($TagName)"
    Write-Output "Taging image with $imageNameWithTag"
    Write-Information "Call data with AZ cli as we don't have support in Azure PowerShell for this yet."
    # you can install by providing the switch -InstallCli 
    az acr build --registry $registry.Name --image $imageNameWithTag -f $dockerFile $SourceFolder
}
Write-Output "Building images done"
Stop-Transcript
#read it in notepad
if ($OpenLog)
{
    Write-Information "Opening log file $logPath"
    Start-Process "notepad" -ArgumentList $logPath
}
