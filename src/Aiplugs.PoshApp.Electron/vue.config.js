const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
module.exports = {
    pluginOptions: {
      electronBuilder: {
        preload: 'src/preload.js',
        externals:['keytar'],
        customFileProtocol: './',
        builderOptions: {
          "appId": "com.aiplugs.poshapp",
          "productName": "POSH App",
          "copyright": "Copyright © 2022",
          "compression": "maximum",
          "generateUpdatesFilesForAllChannels": true,
          "directories": {
            "output": "build"
          },
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