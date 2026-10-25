# Erelia — M1 Ping network
Erelia is a C++23 MMO project built with Sparkle. M1 implements one Client, one Router and two independent Server executables. The Client uses a graphical Sparkle window with two buttons; both buttons execute `/ping A` or `/ping B` through `spk::CommandParser`. The Router routes by message type, servers reply with the original timestamp and request ID, and the Client shows only Idle / Waiting / Received / Failed.

## Applications
- `Erelia-Client` (Windows GUI) — Core + Graphics.
- `Erelia-Router` (Windows/Linux) — Core.
- `Erelia-Server-A` and `Erelia-Server-B` (Windows/Linux) — Core.

Default JSON files live beside the executables. Override with `--config /path/to/config.json`. Example configs are in `config/`. The Client loads `translations/en.i18l` relative to its executable. Logging is configured through optional `logFile` in the JSON.

The optional Client `commandPort` accepts one UTF-8 command per TCP connection on **127.0.0.1 only** and returns `OK` or `ERROR`. It is disabled by default (`0`); it is **not authenticated** and must never be exposed to untrusted local processes. The command executor logs known Ping commands but redacts arbitrary command input to avoid future password leakage. No authentication commands are implemented.

## Tests
CTest labels: Client, Router, ServerA and ServerB. Python graphical integration uses a dedicated interactive Windows desktop and captures actual screenshots. Golden references must be approved manually; a missing reference is a failing test whose actual screenshot remains in CI artifacts. Each failed test retains logs.
