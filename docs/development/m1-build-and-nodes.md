# M1 scaffold: build, run, and extend

This guide documents the **current M1 C++ scaffold**, not the complete M1 engineering milestone. Full graphical CI, Python MockPlayer scenarios, golden images, automated M0 traceability, and milestone acceptance remain separate engineering work.

## Platform and dependencies

- Windows x64, CMake 3.25+, Ninja, C++23 capable clang-cl using the MSVC ABI.
- Sparkle v0.1.3 from the **pinned immutable revision** in [the CI workflow](../../.github/workflows/m1-build-and-tests.yml), installed outside Erelia.
- GLEW, stb, GoogleTest and Sparkle's remaining dependencies resolved through vcpkg.
- Sparkle's package is loaded via `find_package(sparkle CONFIG REQUIRED COMPONENTS Core Graphics)`. Erelia does not compile Sparkle sources into its own project or change Sparkle.

Use the workflow as the executable reference for configuring vcpkg, building and installing Sparkle, and configuring the consumer. To configure an existing local Sparkle install:

```powershell
cmake -S . -B build -G Ninja `
  -DCMAKE_BUILD_TYPE=Release `
  -DCMAKE_CXX_COMPILER=clang-cl `
  "-DCMAKE_TOOLCHAIN_FILE=$env:VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake" `
  -DVCPKG_TARGET_TRIPLET=x64-windows-static `
  -DCMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded `
  "-DCMAKE_PREFIX_PATH=C:/path/to/installed/sparkle" `
  -DBUILD_TESTING=ON

cmake --build build --parallel
ctest --test-dir build --output-on-failure
```

### Independent test families

```powershell
ctest --test-dir build -N
ctest --test-dir build -L "^SparkleAddonCore$" --output-on-failure
ctest --test-dir build -L "^SparkleAddonGraphics$" --output-on-failure  # only when that suite exists
ctest --test-dir build -L "^Core$" --output-on-failure
ctest --test-dir build -L "^Router$" --output-on-failure
ctest --test-dir build -L "^Client$" --output-on-failure
ctest --test-dir build -L "^Server$" --output-on-failure
ctest --test-dir build -L "^ClientServerIntegration$" --output-on-failure
```

New C++ test sources placed under an existing `tests/<family>/` folder are discovered through `GLOB_RECURSE CONFIGURE_DEPENDS` and `gtest_discover_tests`; you do not add each test to a global list. Approved production tests must carry M0 metadata once the traceability bootstrap is implemented.

## Client configuration (no arguments)

After building, `Erelia - Client.exe` has a `client.json` file **in the same directory**. The build seeds the example only when the destination config is absent.

Launch `Erelia - Client.exe` by double-clicking it or a shortcut, with no parameters. It finds `client.json` relative to the executable itself, not the process's current working directory. Missing or invalid configurations produce a startup error. The current Client opens a normal Sparkle graphical window but does not connect to a server.

## Server configurations (one file per instance)

```powershell
& ".\\Erelia - Router.exe" --config C:\Erelia\config\router.json
& ".\\Erelia - Server.exe" --config C:\Erelia\config\world-node.json
& ".\\Erelia - Server.exe" --config C:\Erelia\config\chat-node.json
```

These commands launch separate listeners, and console interrupt/termination signals request a clean stop. The Router does not yet install routes or connect to its listed peers. A per-instance file declares its own identity and listening port; the Router's file includes the peer addresses it needs. There is no global configuration registry. The generic `settings` object in a node's JSON is reserved for role-specific configuration and is not interpreted by the common host.

Use `--validate-config path.json` to parse a server's file and exit without binding a port.

## Creating a new RemoteNode role

The buildable starter under [apps/example-node](../../apps/example-node/) demonstrates how to create an independent node process with very little boilerplate. Copy the entire directory to `apps/my-node`, change its executable target name in the directory's own `CMakeLists.txt`, implement the request callback in `main.cpp`, and supply the role's own JSON config.

The common `erelia::server::serveRemoteNode` host uses Sparkle's `spk::RemoteNode::Endpoint`: it starts and stops the socket and dispatches incoming requests to the node-provided callback. Replies can be sent with `endpoint.reply(request, response)`. The example does not reply yet because no protocol or gameplay messages are part of this milestone.

The root CMake project automatically discovers newly added `apps/*/CMakeLists.txt`, and ordinary tests in `tests/Server/` are discovered automatically. Node-specific messages, policies and configuration must be approved with their own Story rather than silently invented by the template.

## Executable names and preview artifacts

The CMake targets retain their stable API names (`EreliaClient`, `EreliaServerRouter`, `EreliaRemoteNode`, `EreliaExampleNode`), but their Windows executable filenames are `Erelia - Client.exe`, `Erelia - Router.exe`, `Erelia - Server.exe`, and `Erelia - ExampleNode.exe`. Quote these filenames in PowerShell. New specialized nodes can follow the `Erelia - <Role>.exe` convention without modifying the root CMake file.

The CI workflow publishes a downloadable **M1 scaffold preview** with all currently built app executables and per-instance example JSON configurations. It is not a playable game, installer, or evidence of passing graphical/MockPlayer tests. Each app lives in its own directory. Run the Client from its directory (it reads the adjacent `client.json`); pass `--config <path>` to Router and server executables.

## Build and CI expectations

The hosted Windows pipeline builds the complete C++ scaffold, including the example node, and runs every CTest family, publishing per-family JUnit. It does **not** prove that a non-interactive hosted runner can run the graphical Client or inject user input; those belong to the later dedicated Windows interactive runner.

Do not merge the draft PR or claim M1 accepted solely because these builds pass. Owner reviews and the M0/M1 evidence gates still apply.
