const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
module.exports = {
    pluginOptions: {
      electronBuilder: {
        preload: 'src/preload.js',
        externals:['nodegit','keytar'],
        builderOptions: {
          "appId": "com.aiplugs.poshapp",
          "productName": "POSH App",
          "copyright": "Copyright © 2019",
          "compression": "maximum",
          "generateUpdatesFilesForAllChannels": true,
          "directories": {
            "output": "build"
          },
          "afterSign": "notarize.js",
          "win": {
            "publisherName": [
              "Yoshiyuki Taniguchi"
            ],
            "verifyUpdateCodeSignature" : false,
            "icon": "./src/assets/icon/poshapp-icon-256x256.ico"
          },
          "nsis": {
            "oneClick": false,
            "perMachine": false,
            "allowToChangeInstallationDirectory": false,
            "license": "./eula.txt"
          },
          "mac": {
            "icon": "./src/assets/icon/poshapp-icon-512x512.icns",
            "hardenedRuntime": true,
            "gatekeeperAssess": false,
            "entitlements": "entitlements.mac.plist",
            "entitlementsInherit": "entitlements.mac.plist",
            "binaries": [
              "build/mac/POSH App.app/Contents/Resources/bin/Aiplugs.PoshApp.Deamon"
            ]
          },
          "linux": {
            "icon": "./src/assets/icon/"
          },
          "publish": [
            {
              "provider": "github",
              "owner": "aiplugs",
              "repo": "poshapp"
            }
          ],
          "extraResources": [
            {
              "from": "bin",
              "to": "bin",
              "filter": [
                "**/*"
              ]
            }
          ]
        }
      }
    },
    configureWebpack: {
      plugins: [
        new MonacoWebpackPlugin(),
      ]
    }
  }