# M1 engineering scaffold — implementation staging

**Status:** preliminary branch scaffold; neither a final technical approval nor an M1 acceptance record.

## Scope

This branch establishes the C++23 source-module boundaries, three application entry points plus a buildable example node, CMake, five GTest/CTest suites, separate example JSON configurations and a hosted Windows build/test workflow. The Server Router and RemoteNode launch Sparkle listeners and dispatch loops, but do not define routes or gameplay-specific message handling. The graphical Client constructs an ordinary Sparkle window without connecting. No gameplay is implemented.

## Module and executable boundaries

- `src/core`: non-graphical shared Erelia logic, configuration loading, `sparkle::core`.
- `src/sparkle_addon`: Erelia-owned extensions. Do not modify Sparkle.
- `src/client`: graphical Client composition, `sparkle::sparkle`.
- `src/server`: server-side shared logic; Router and RemoteNode have independent entry points.
- `apps/client`, `apps/server-router`, `apps/remote-node`: three main executable targets. `apps/example-node` is a copyable and buildable role-specific RemoteNode template.
- `tests/{Core,Client,Server,SparkleAddon,ClientServerIntegration}`: independently labeled CTest suites.

Source files and tests are found through scoped CMake `GLOB_RECURSE ... CONFIGURE_DEPENDS`. A new executable may be introduced with a new directory under `apps` and a local `CMakeLists.txt`.

## JSON configuration contracts (accepted direction)

Use Sparkle's existing `spk::JSON::Value` and `spk::JSON::Reader`. Each process has a self-contained JSON file describing only what that process needs; references to peers are stored in the process that uses them. There is no shared configuration directory or discovery daemon.

### Graphical Client

`EreliaClient.exe` is launched by double-clicking it or its shortcut. **It accepts no CLI configuration parameters.** It resolves its own executable location through the Windows API and reads `client.json` from the same directory, **not the working directory**. A missing or invalid file causes the launch to fail explicitly.

The build copies `config/examples/client.json` next to the built executable if it does not already exist. This seed operation does not overwrite a locally edited `client.json`. Distribution must place a suitable `client.json` alongside the binary.

The current Client config contains its identity, the Server Router address and basic window properties. Router connectivity is only a future dependency; the Client scaffold does not connect.

### Server Router

`EreliaServerRouter --config <router.json>` launches an instance.
`EreliaServerRouter --validate-config <router.json>` validates its file and exits without starting sockets.

The Router config defines its identity, listening port and the named RemoteNode endpoints it needs to reach. No other node config is read.

### RemoteNode

`EreliaRemoteNode --config <node.json>` launches an instance.
`EreliaRemoteNode --validate-config <node.json>` validates its file and exits without starting sockets.

`world-node.json` and `chat-node.json` demonstrate launching separate instances from one executable. Each config contains that instance's identity and listening port. A RemoteNode may additionally define a `settings` object for role-specific data, without changing the common JSON loader.

To create a distinct new node type, copy `apps/example-node` into a new directory under `apps/`, rename the executable target in its local CMakeLists, and replace its placeholder request handler. `erelia::server::serveRemoteNode()` owns the `spk::RemoteNode::Endpoint`, socket lifecycle, periodic dispatch and shutdown; the node supplies only its request handling. See [example node instructions](../../apps/example-node/README.md). The root CMake project and CI source/test discovery do not need edits. A new node requires its own owner-approved Story and appropriately traced tests.

The generic server launcher and example node begin listening once `--config` is provided; `--validate-config` merely validates and exits. They terminate on console interrupt/termination signals. The Router currently has no registered routes. Do not interpret listener availability as a completed routing protocol.

### Current files

- `config/examples/client.json`
- `config/examples/router.json`
- `config/examples/world-node.json`
- `config/examples/chat-node.json`
- `config/examples/example-node.json`

The JSON fields are a minimal M1 scaffold, not a final routing/deployment schema. The implementing Stories should add only fields actually needed by their acceptance criteria. Configuration read/validation is implemented; **network connectivity is not**.

## Testing and pipeline

CTest has five distinct test families. Temporary bootstrap tests verify target linking, JSON parsing, independent RemoteNode settings, and config location rules. The Client-specific CTest check verifies its adjacent `client.json` without opening a window in noninteractive CI. Server config checks use `--validate-config`. Actual Client GUI launch and graphical smoke validation remain future acceptance work on the dedicated graphical runner.

The hosted Windows pipeline builds an immutable revision of Sparkle externally, installs its CMake package, builds Erelia against installed public targets and executes the five test families independently. This branch's execution status must be verified before acceptance.

## M0 limitations

M0 STATUS currently indicates pending review/acceptance, with no approved SDD/ERD prefixes or requirement IDs. The scaffold's smoke tests do not claim formal traceability. Before delivery as an approved Story, use the explicit M1 bootstrap exception, obtain the necessary approvals, establish metadata generation/validation and reconcile the tests to the approved M0 requirements. Do not treat this preliminary branch as an approved implementation.

## Exclusions

No gameplay, login, Router-to-RemoteNode links/routing table, automatic peer discovery, Python mock players, screenshot testing or dedicated graphical CI yet. Sparkle must remain unchanged.
