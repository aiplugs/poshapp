@echo off

pushd

dotnet publish -r win10-x64 --output %~dp0obj\Host\bin

cd %~dp0obj\Host\node_modules\.bin

./electron.cmd "..\..\main.js"

popd