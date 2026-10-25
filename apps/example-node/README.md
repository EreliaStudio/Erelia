# Example RemoteNode application

This is a **compilable starter project** for new Erelia server roles.

## Create a new node

1. Copy this directory under `apps/` with a role-specific name, such as `apps/inventory-node/`.
2. Set `ERELIA_NODE_ROLE` in the copied `CMakeLists.txt` to the role name, such as `InventoryNode`. This automatically names both its CMake target (`EreliaInventoryNode`) and Windows executable (`Erelia - InventoryNode.exe`).
3. Implement the request handler in `src/application.cpp` (inside the node library, not `main.cpp`). It receives the existing Sparkle `RemoteNode::Endpoint` and a routed request; responses can use `endpoint.reply(request, message)`. Do not add any test-only interfaces to Sparkle.
4. Add a self-contained JSON configuration file for each deployed instance, following `config/examples/example-node.json`. Change the node identity and port. A specialization may consume its own `settings` JSON section.
5. Build normally. `apps/CMakeLists.txt` discovers the new app directory automatically. Add C++ tests under this node's own `tests/` directory. CMake automatically creates a node-specific test executable and CTest family. Other Erelia application tests remain independent.

The node static library `EreliaExampleNodeLibrary` links `erelia::server`; only the executable builds `main.cpp`. A copied role automatically creates its own `<Role>Library` and executable. The common `erelia::server::serveRemoteNode` host owns socket start/stop, Sparkle request dispatch, and graceful shutdown when the process is signalled. The handler adds only the role-specific behavior.

Use `--config path.json` to run and `--validate-config path.json` to validate configuration without starting a server. This example does not respond to any protocol message: it is a template, not gameplay infrastructure.

This README is not a replacement for an owner-approved Story and reviewed tests under M0.
