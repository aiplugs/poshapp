param(
    $rid = "win10-x64",
    $conf = "Release"
)

function Modify-Lib() {
    $lib = "../../lib/PowerShellEditorServices/src/PowerShellEditorServices/PowerShellEditorServices.csproj"
    $csproj = (Get-Content -Path $lib)
    if (-not ($csproj | ?{$_ -eq "<NoWarn>NU1605</NoWarn>"})) {
        $csproj.Replace("<AssemblyTitle>PowerShell Editor Services</AssemblyTitle>","<AssemblyTitle>PowerShell Editor Services</AssemblyTitle>`n<NoWarn>NU1605</NoWarn>") | Out-File -Path $lib
    }
}

function Build-Deamon() {
    dotnet publish ../Aiplugs.PoshApp.Deamon -c $conf 
    #-r $rid
}

function Clear-Bin() {
    if (-not (Test-Path -Path ./bin)) {
        mkdir ./bin
    } else {
        Remove-Item -Recurse -Force ./bin/*
    }

    mkdir ./bin/deamon/
    mkdir ./bin/deamon/bin/
    mkdir ./bin/deamon/bin/Common/
}

function Copy-Artifacts() {
    Copy-Item -Recurse -Force -Path ../Aiplugs.PoshApp.Deamon/bin/$conf/net6.0/publish/* -Destination ./bin/deamon/bin/Common/
}

function Copy-Modules() {
    if (-not (Test-Path -Path ../PSScriptAnalyzer.zip)) {
        Invoke-RestMethod https://github.com/PowerShell/PSScriptAnalyzer/releases/download/1.21.0/PSScriptAnalyzer.1.21.0.nupkg -OutFile ../PSScriptAnalyzer.zip
    }
    if (-not (Test-Path -Path ../PackageManagement.zip)) {
        Invoke-RestMethod https://psg-prod-eastus.azureedge.net/packages/packagemanagement.1.4.7.nupkg -OutFile ../PackageManagement.zip
    }
    if (-not (Test-Path -Path ../PowerShellGet.zip)) {
        Invoke-RestMethod https://psg-prod-eastus.azureedge.net/packages/powershellget.2.2.5.nupkg -OutFile ../PowerShellGet.zip
    }
    Expand-Archive -Path ../PSScriptAnalyzer.zip -DestinationPath ./bin/deamon/bin/Common/Modules/PSScriptAnalyzer
    Expand-Archive -Path ../PackageManagement.zip -DestinationPath ./bin/deamon/bin/Common/Modules/PackageManagement
    Expand-Archive -Path ../PowerShellGet.zip -DestinationPath ./bin/deamon/bin/Common/Modules/PowerShellGet
    Copy-Item -Recurse  -Path ../../lib/PowerShellEditorServices/module/PowerShellEditorServices  -Destination ./bin/deamon/bin/Common/Modules/PowerShellEditorServices
}


Modify-Lib
Build-Deamon
Clear-Bin
Copy-Artifacts
Copy-Modules








