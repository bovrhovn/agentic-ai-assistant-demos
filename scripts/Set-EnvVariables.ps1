<#

.SYNOPSIS

This script will set the environment variables from local.env file. 

.DESCRIPTION
 
This script will set the environment variables from local.env file. 
  
.EXAMPLE

PS > Set-EnvVariables -EnvFileToReadFrom "local.env"
set environment variables from local.env file

. LINK

https://github.com/bovrhovn/meetup-intro-to-kiota
 
#>
param(   
    [Parameter(Mandatory = $true)]
    $EnvFileToReadFrom = "local.env"    
)

Write-Output "Setting environment variables from $EnvFileToReadFrom file."

Get-Content $EnvFileToReadFrom | ForEach-Object {
    $name, $value = $_.split('=')
    Set-Content env:\$name $value
    Write-Information "Writing $name to environment variable with $value."
}

Write-Output "All environment variables set from $EnvFileToReadFrom file."
