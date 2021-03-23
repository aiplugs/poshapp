param(
    $rid = "win10-x64"
)

if ($IsMacOS) {
    $rid = "osx-x64"
}

$lib = "../../lib/PowerShellEditorServices/src/PowerShellEditorServices/PowerShellEditorServices.csproj"
$csproj = (Get-Content -Path $lib)
if (-not ($csproj | ?{$_ -eq "<NoWarn>NU1605</NoWarn>"})) {
    $csproj.Replace("<AssemblyTitle>PowerShell Editor Services</AssemblyTitle>","<AssemblyTitle>PowerShell Editor Services</AssemblyTitle>`n<NoWarn>NU1605</NoWarn>") | Out-File -Path $lib
}
dotnet publish ../Aiplugs.PoshApp.Deamon -c Release -r $rid
if ($rid -eq "win10-x64") {
    dotnet publish ../Aiplugs.PoshApp.Pses -c Release 
} else {
    dotnet publish ../Aiplugs.PoshApp.Pses -c Release -r $rid
}

if (-not (Test-Path -Path ./bin)) {
    mkdir ./bin
} else {
    Remove-Item -Recurse -Force ./bin/*
}
mkdir ./bin/deamon/
mkdir ./bin/pses/
mkdir ./bin/pses/bin/
mkdir ./bin/pses/bin/Common/
Copy-Item -Recurse -Force -Path ../Aiplugs.PoshApp.Deamon/bin/Release/net5.0/$rid/publish/* -Destination ./bin/deamon/
if ($rid -eq "win10-x64") {
    Copy-Item -Recurse -Force -Path ../Aiplugs.PoshApp.Pses/bin/Release/net5.0/publish/* -Destination ./bin/pses/bin/Common/
} else {
    Copy-Item -Recurse -Force -Path ../Aiplugs.PoshApp.Pses/bin/Release/net5.0/$rid/publish/* -Destination ./bin/pses/bin/Common/
}
if (-not (Test-Path -Path ../PSScriptAnalyzer.zip)) {
    Invoke-RestMethod https://github.com/PowerShell/PSScriptAnalyzer/releases/download/1.19.1/PSScriptAnalyzer.zip -OutFile ../PSScriptAnalyzer.zip
}
mkdir ./bin/pses/bin/Common/Modules
Expand-Archive -Path ../PSScriptAnalyzer.zip -DestinationPath ./bin/pses/bin/Common/Modules/PSScriptAnalyzer

Copy-Item -Recurse  -Path ../../lib/PowerShellEditorServices/module/PowerShellEditorServices/Commands  -Destination ./bin/pses/