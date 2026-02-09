const fs = require("fs");

const ENV_PATH = ".env";
const EXAMPLE_PATH = ".env.example";

try {
  if (!fs.existsSync(ENV_PATH) && fs.existsSync(EXAMPLE_PATH)) {
    fs.copyFileSync(EXAMPLE_PATH, ENV_PATH);
  }
} catch (err) {
  // Failing here should surface clearly in Dev Containers logs.
  console.error("Failed to initialize .env from .env.example:", err);
  process.exitCode = 1;
}
