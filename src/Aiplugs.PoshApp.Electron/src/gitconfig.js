const fs = require('fs/promises');
const os = require('os');

export async function parseGitConfig(path = `~/.gitconfig`) {
    const config = {};
    const text = await fs.readFile(path.replace(/^~/,os.homedir()), { encoding: 'utf8' });
    const lines = text.split('\n');

    let prefix = '';
    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        const tag = /^\s*\[(.+)\]\s*$/.exec(line);
        if (tag) {
            prefix = tag[1];
            continue;
        }
        const sep = line.indexOf('=');
        if (sep < 0) {
            continue;
        }

        const key = line.substring(0, sep).trim();
        const value = line.substring(sep + 1).trim();

        config[`${prefix}.${key}`] = value;
    }

    return config;
}
