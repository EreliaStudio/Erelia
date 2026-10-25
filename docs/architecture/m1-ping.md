# M1 — Ping routing and external GUI tests

The Windows Client uses spk::Application, spk::PushButton and a Sparkle GridLayout. Two buttons dispatch /ping A and /ping B via one spk::CommandParser. Its optional loopback command socket queues a single text line for the same command executor and returns OK/ERROR; arbitrary commands are redacted in logs. The loopback socket is not authenticated and is disabled by default. Future credentials must never be logged, including error strings.

The Router uses spk::NodeRouter and two spk::RemoteNode links. Server A and Server B use separate spk::RemoteNode::Endpoint applications. All applications read executable-relative JSON by default and accept --config. The Client loads a fixed translations/en.i18l JSON catalog through spk::Translator.

The Python runner on a dedicated *interactive Windows user session* controls process lifecycles and sends real OS input to the Client. It verifies network events through per-process spk::logger file outputs and compares screenshots of the two stable panels for Idle / Received / Failed. Waiting is validated by unit tests and state logs only. Initial golden PNGs are deliberately not committed; missing references fail and actual images are downloadable from CI. The owner manually approves reference images. Every CI test job retains logs and JUnit evidence even on failure. No software should auto-approve goldens.

CI interactive self-hosted runner labels required: self-hosted, Windows, X64, erelia-interactive. Register the runner as an interactive user process in an unlocked Windows session, not as a background service. The CI job is mandatory and will remain queued until a matching runner is connected. Sparkle is pinned to 5117c64a7052457ca2de1864bf23e0e29487bdcf, unchanged.
