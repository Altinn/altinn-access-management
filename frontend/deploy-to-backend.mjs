import rimraf from 'rimraf';
import ncp from 'ncp';
import { existsSync } from 'fs';
import { spawn } from 'child_process';
import { dirname, resolve } from 'path';
import { fileURLToPath } from 'url';

const BUILD_DIR = './dist/';
const BACKEND_DEPLOY_FOLDER =
  '../backend/src/Altinn.Authorizationadmin/Altinn.Authorizationadmin/wwwroot/AuthorizationAdmin/';

const currentDir = dirname(fileURLToPath(import.meta.url));
const sourceDir = resolve(`${currentDir}/${BUILD_DIR}`);
const targetDir = resolve(`${currentDir}/${BACKEND_DEPLOY_FOLDER}`);

if (!existsSync(targetDir)) {
  console.error(`Target folder "${targetDir}" does not exist`);
  process.exit(1);
}

rimraf(`${BACKEND_DEPLOY_FOLDER}/*`, (err) => {
  if (err) {
    console.error(`Could not clear folder "${targetDir}"`);
    process.exit(1);
  }

  const buildProc = spawn('yarn', ['build', ...process.argv.slice(2)]);

  buildProc.stdout.on('data', (data) => console.log(data.toString()));
  buildProc.stderr.on('data', (data) => console.log(data.toString()));

  buildProc.on('exit', (code) => {
    if (code) {
      console.error(`Build failed`);
      process.exit(1);
    }

    if (!existsSync(sourceDir)) {
      console.error(`Source folder "${sourceDir}" does not exist`);
      process.exit(1);
    }

    ncp(`${sourceDir}/`, `${targetDir}`, (err) => {
      if (err) {
        console.error(`Could not copy from "${sourceDir}" to "${targetDir}"`);
        process.exit(1);
      }
      console.log('Done');
    });
  });
});
