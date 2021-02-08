dotnet publish ../Aiplugs.PoshApp.Deamon -c Release -r win10-x64 
dotnet publish ../Aiplugs.PoshApp.Pses -c Release

if (-not (Test-Path -Path ./bin)) {
    mkdir ./bin
} else {
    Remove-Item -Recurse ./bin/*
}
mkdir ./bin/deamon/
mkdir ./bin/pses/
mkdir ./bin/pses/bin/
mkdir ./bin/pses/bin/Common/
Copy-Item -Recurse -Force -Path ../Aiplugs.PoshApp.Deamon/bin/Release/net5.0/win10-x64/publish/* -Destination ./bin/deamon/
Copy-Item -Recurse -Force -Path ../Aiplugs.PoshApp.Pses/bin/Release/net5.0/publish/* -Destination ./bin/pses/bin/Common/
if (-not (Test-Path -Path ../PSScriptAnalyzer.zip)) {
    Invoke-RestMethod https://github.com/PowerShell/PSScriptAnalyzer/releases/download/1.19.1/PSScriptAnalyzer.zip -OutFile ../PSScriptAnalyzer.zip
}
mkdir ./bin/pses/bin/Common/Modules
Expand-Archive -Path ../PSScriptAnalyzer.zip -DestinationPath ./bin/pses/bin/Common/Modules/PSScriptAnalyzer

Copy-Item -Recurse  -Path ../../lib/PowerShellEditorServices/module/PowerShellEditorServices/Commands  -Destination ./bin/pses/