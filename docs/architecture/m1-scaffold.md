# M1 engineering scaffold — implementation staging

**Status:** preliminary scaffold; not an approved production architecture, not an M1 acceptance record.

## Scope

The branch contains source-folder boundaries, CMake targets, five independent GTest/CTest suites, three executable placeholders, sample per-instance XML files and a hosted Windows build/test pipeline. There is no operational multiplayer routing or XML loader. Sparkle is an external installed dependency and remains unchanged.

## Module and executable boundaries

- `src/core`: non-graphical shared Erelia logic; links `sparkle::core`.
- `src/sparkle_addon`: Erelia-owned extensions; links Core. Sparkle itself must not change.
- `src/client`: graphical client composition; links `sparkle::sparkle`.
- `src/server`: server-side reusable logic. Separate router and remote-node entry points currently share placeholder code.
- `apps/client`, `apps/server-router`, `apps/remote-node`: three independently built executables.
- `tests/{Core,Client,Server,SparkleAddon,ClientServerIntegration}`: independently labeled CTest suites.

Add source files under their owning module; CMake discovers `*.cpp` automatically. An additional executable owns an app folder and `CMakeLists.txt`, which `apps/CMakeLists.txt` discovers. Adding an ordinary test needs only an additional test source file.

## Network configuration — candidate design, not yet active

Every process instance gets its **own XML** describing its identity, role and locally published endpoint. To start a second remote-node instance, provide a distinct configuration such as `world-node.xml` or `chat-node.xml`. No shared monolithic file should duplicate every service definition.

**Unresolved consequential question:** how does each process discover other instances? Candidate A: read all the separate XML files in a shared deployment directory; candidate B: each process file includes explicit outgoing peer connection information; candidate C: use a central directory/discovery service. Do not implement discovery, routing configuration, or a production XML parser until the owner decides. The example files are data-shape sketches, not an approved schema.

The implementation step must build on Sparkle's existing `spk::NodeRouter`, `spk::RemoteNode`, and `spk::RemoteNode::Endpoint`. No networking is required for this scaffold, and the placeholders intentionally start no sockets.

## Dependency strategy

The pipeline builds and installs a pinned, unmodified external Sparkle revision, then consumes it through `find_package(sparkle CONFIG REQUIRED COMPONENTS Core Graphics)`. This avoids an Erelia-side copy or patch. Publication via `erelia-sparkle` vcpkg can replace the package acquisition later without changing targets.

## M0 limitations before production Story delivery

Erelia's M0 status still says pending acceptance; SDD and ERD domain prefixes and IDs are not approved yet. This scaffold's smoke tests therefore do not falsely claim approved requirement IDs. They are temporary bootstrap verification artifacts, not proof of M0 traceability compliance. The first authorized M1 infrastructure Story must obtain scope/design/test approval, record its bootstrap exception, establish traceability tooling and backfill formally approved IDs before strict acceptance. Normal feature development remains blocked until those gates operate.

## Exclusions

No gameplay, authentication, networking protocol, real remote-node connectivity, XML parsing, Python player tests, screenshots or graphical CI in this scaffold.
