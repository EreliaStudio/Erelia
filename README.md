# Erelia

This is the new Core, Server, and Client project. The original Playground code and backlog are preserved unchanged in `archive/`.

## Windows development

Requirements: CMake 3.25+, Ninja, Clang, vcpkg, the sibling `Sparkle` checkout, and the VS Code C/C++ extension. Set `VCPKG_ROOT` to your vcpkg directory. The VS Code launch profiles prepare an out-of-tree Sparkle 0.1.3 install, configure Erelia, and build the selected target before launch. All generated files stay under `build/`.

To build and test without VS Code:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/dev/prepare-sparkle.ps1 -Configuration Debug
cmake --preset debug
cmake --build --preset debug
ctest --preset debug
```

Replace `Debug`/`debug` with `Release`/`release` for Release. For Core and Server without graphics, prepare Sparkle with `-Variant Core`, then use `headless-debug` or `headless-release`.

The status functions and tests are deliberately small. They verify that the three libraries, executables, package dependencies, and GoogleTest suites build and link. The Client suite currently does not create an OpenGL context.

## Formatting and CI

Erelia uses the same `.clang-format` file as Sparkle. VS Code formats C++ files on save. To check the active source tree locally, run `clang-format --style=file --dry-run --Werror` on the `.cpp` and `.hpp` files under `core/`, `server/`, and `client/`. Each layer keeps its own test suite in a `tests/` subfolder. The archived code is excluded from formatting checks.

CI builds Core and Server without graphics on Linux and Windows, and builds the Client on Windows in both Debug and Release. The Windows Client job installs the same Mesa software OpenGL package used by Sparkle's tests so future OpenGL tests can use it.
