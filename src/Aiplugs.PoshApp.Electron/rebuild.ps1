npm i -D electron-rebuild
Remove-Item -Recurse -Force -Path "node_modules/nodegit/build"
.\node_modules\.bin\electron-rebuild.cmd -f -w nodegit
npm uninstall electron-rebuild