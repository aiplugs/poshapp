const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const crypto = require('crypto');

/**
 * md4 algorithm is not available anymore in NodeJS 17+ (because of + lib SSL 3).
 * In that case, silently replace md4 by md5 algorithm.
 */
try {
  crypto.createHash('md4');
} catch (e) {
  console.warn('Crypto "md4" is not supported anymore by this Node + + version');
  const origCreateHash = crypto.createHash;
  crypto.createHash = (alg, opts) => {
    return origCreateHash(alg === 'md4' ? 'md5' : alg, opts);
  };
}

module.exports = {
    pluginOptions: {
      electronBuilder: {
        preload: 'src/preload.js',
        externals:['keytar'],
        customFileProtocol: './',
        builderOptions: {
          "appId": "com.aiplugs.poshapp",
          "productName": "POSH App",
          "copyright": "Copyright © 2023",
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
        new MonacoWebpackPlugin({
          languages: ['powershell']
        }),
      ]
    }
  }