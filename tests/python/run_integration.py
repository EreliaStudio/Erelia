"""Real Windows-input integration: launches all four M1 applications and compares approved goldens."""
import argparse
import ctypes
import ctypes.wintypes
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import time
import traceback
import xml.etree.ElementTree as ET

import pyautogui
from PIL import ImageChops, ImageGrab, Image

pyautogui.FAILSAFE = True
TITLE = "Erelia M1"
USER32 = ctypes.windll.user32
SW_RESTORE = 9


class Session:
    def __init__(self, build, result_root, tag, response_enabled, timeout_ms=900):
        self.build = build
        self.dir = result_root / tag
        self.dir.mkdir(parents=True, exist_ok=True)
        self.processes = []
        self.log = {}
        self.response_enabled = response_enabled
        self.timeout_ms = timeout_ms
        self.ports = [free_port() for _ in range(4)]

    def __enter__(self):
        client_port, router_port, a_port, b_port = self.ports
        configs = {
            "ServerA": {"port": a_port, "respond": self.response_enabled, "logFile": str(self.dir / "ServerA.log")},
            "ServerB": {"port": b_port, "respond": self.response_enabled, "logFile": str(self.dir / "ServerB.log")},
            "Router": {"port": router_port,
                       "serverA": {"host": "127.0.0.1", "port": a_port},
                       "serverB": {"host": "127.0.0.1", "port": b_port},
                       "logFile": str(self.dir / "Router.log")},
            "Client": {"router": {"host": "127.0.0.1", "port": router_port},
                       "timeoutMs": self.timeout_ms, "commandPort": client_port,
                       "logFile": str(self.dir / "Client.log")}
        }
        for name, data in configs.items():
            path = self.dir / (name + ".json")
            path.write_text(json.dumps(data, indent=2), encoding="utf-8")
            self.log[name] = self.dir / (name + ".log")
        for name in ("ServerA", "ServerB"):
            self.launch(name)
            self.expect(name, "SERVER_" + name[-1] + "_READY")
        self.launch("Router")
        self.expect("Router", "ROUTER_READY")
        self.launch("Client")
        self.expect("Client", "CLIENT_READY")
        self.expect("Client", "COMMAND_SOCKET_READY")
        self.hwnd = self.window()
        return self

    def executable(self, name):
        folders = {"Client": "client", "Router": "router", "ServerA": "server-a", "ServerB": "server-b"}
        names = {"Client": "Erelia-Client.exe", "Router": "Erelia-Router.exe",
                 "ServerA": "Erelia-Server-A.exe", "ServerB": "Erelia-Server-B.exe"}
        path = self.build / "apps" / folders[name] / names[name]
        if not path.is_file():
            raise RuntimeError(f"Missing executable: {path}")
        return path

    def launch(self, name):
        path = self.executable(name)
        output = (self.dir / (name + ".stdout.log")).open("w", encoding="utf-8")
        process = subprocess.Popen([str(path), "--config", str(self.dir / (name + ".json"))],
                                   cwd=str(path.parent), stdout=output, stderr=subprocess.STDOUT)
        self.processes.append((name, process, output))

    def expect(self, name, event, timeout=15):
        deadline = time.monotonic() + timeout
        while time.monotonic() < deadline:
            if self.log[name].exists() and event in self.log[name].read_text(encoding="utf-8", errors="replace"):
                return
            for kind, process, _ in self.processes:
                if kind == name and process.poll() is not None:
                    raise AssertionError(f"{name} exited {process.returncode} before {event}")
            time.sleep(.05)
        raise AssertionError(f"Timed out waiting for {name}: {event}")

    def window(self):
        deadline = time.monotonic() + 12
        while time.monotonic() < deadline:
            handle = USER32.FindWindowW(None, TITLE)
            if handle and USER32.IsWindowVisible(handle):
                USER32.ShowWindow(handle, SW_RESTORE)
                USER32.SetForegroundWindow(handle)
                return handle
            time.sleep(.05)
        raise AssertionError("Client graphical window was not visible")

    def _origin(self):
        point = ctypes.wintypes.POINT(0, 0)
        if not USER32.ClientToScreen(self.hwnd, ctypes.byref(point)):
            raise RuntimeError("ClientToScreen failed")
        return point.x, point.y

    def click(self, x, y):
        USER32.SetForegroundWindow(self.hwnd)
        left, top = self._origin()
        pyautogui.moveTo(left + x, top + y, duration=0.05)
        pyautogui.mouseDown()
        pyautogui.mouseUp()

    def key_down(self, name):
        pyautogui.keyDown(name)

    def key_up(self, name):
        pyautogui.keyUp(name)

    def command(self, text):
        with socket.create_connection(("127.0.0.1", self.ports[0]), timeout=4) as conn:
            conn.sendall((text + "\n").encode("utf-8"))
            answer = conn.recv(256).decode("utf-8").strip()
            if answer != "OK":
                raise AssertionError(f"Command failed: {answer}")

    def screenshot(self, target, status, references, errors):
        # Compare one panel only; the other panel may have an independent status.
        left, top = self._origin()
        image = ImageGrab.grab(bbox=(left, top, left + 640, top + 300))
        panel = image.crop((0, 0, 320, 300) if target == "A" else (320, 0, 640, 300)).convert("RGB")
        actual_dir = self.dir / "actual"
        diff_dir = self.dir / "difference"
        actual_dir.mkdir(exist_ok=True)
        diff_dir.mkdir(exist_ok=True)
        filename = f"{target}_{status}.png"
        actual = actual_dir / filename
        expected = references / filename
        panel.save(actual)
        if not expected.is_file():
            errors.append(f"Missing approved golden: {filename} (actual image saved: {actual})")
            return
        with Image.open(expected) as reference:
            if reference.size != panel.size:
                errors.append(f"Golden size mismatch: {filename}")
                return
            differences = ImageChops.difference(panel, reference.convert("RGB"))
            tolerance = differences.point(lambda value: 255 if value > 4 else 0)
            if tolerance.getbbox():
                differences.save(diff_dir / filename)
                errors.append(f"Golden image mismatch: {filename} (actual and difference retained)")
            else:
                actual.unlink()

    def __exit__(self, exc_type, exc, tb):
        for _, process, _ in reversed(self.processes):
            if process.poll() is None:
                process.terminate()
        for _, process, output in reversed(self.processes):
            try:
                process.wait(timeout=6)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait(timeout=5)
            output.close()


def free_port():
    with socket.socket() as conn:
        conn.bind(("127.0.0.1", 0))
        return conn.getsockname()[1]


def run(build, results, references):
    errors = []
    with Session(build, results, "responses", True) as game:
        game.screenshot("A", "Idle", references, errors)
        game.screenshot("B", "Idle", references, errors)
        game.click(170, 225)
        game.expect("ServerA", "PING_RECEIVED target=A")
        game.expect("Client", "PING_RESULT target=A status=Received")
        assert "PING_RECEIVED" not in game.log["ServerB"].read_text(encoding="utf-8")
        game.expect("Client", "CLIENT_STATUS target=A status=client.received")
        time.sleep(.25)
        game.screenshot("A", "Received", references, errors)

        game.click(470, 225)
        game.expect("ServerB", "PING_RECEIVED target=B")
        game.expect("Client", "PING_RESULT target=B status=Received")
        game.expect("Client", "CLIENT_STATUS target=B status=client.received")
        time.sleep(.25)
        game.screenshot("B", "Received", references, errors)

        # Both buttons issue real OS input; both requests are allowed simultaneously.
        a_before = game.log["ServerA"].read_text(encoding="utf-8").count("PING_RECEIVED")
        b_before = game.log["ServerB"].read_text(encoding="utf-8").count("PING_RECEIVED")
        game.click(170, 225)
        game.click(470, 225)
        game.expect("ServerA", "PING_RECEIVED target=A", timeout=8)
        game.expect("ServerB", "PING_RECEIVED target=B", timeout=8)
        deadline = time.monotonic() + 8
        while time.monotonic() < deadline:
            a_after = game.log["ServerA"].read_text(encoding="utf-8").count("PING_RECEIVED")
            b_after = game.log["ServerB"].read_text(encoding="utf-8").count("PING_RECEIVED")
            if a_after > a_before and b_after > b_before:
                break
            time.sleep(.05)
        else:
            raise AssertionError("Concurrent pings were not independently forwarded")
        # The optional localhost API uses exactly the same command executor.
        game.expect("Client", "PING_RESULT target=A status=Received")
        game.command("/ping A")
        game.expect("Client", "COMMAND_RESULT source=socket status=Accepted")
        assert "COMMAND_ATTEMPT source=button text=/ping A" in game.log["Client"].read_text(encoding="utf-8")
        assert "COMMAND_ATTEMPT source=socket text=/ping A" in game.log["Client"].read_text(encoding="utf-8")

    with Session(build, results, "timeouts", False, timeout_ms=500) as game:
        game.click(170, 225)
        game.click(470, 225)
        game.expect("Client", "PING_RESULT target=A status=Failed")
        game.expect("Client", "PING_RESULT target=B status=Failed")
        game.expect("Client", "CLIENT_STATUS target=A status=client.failed")
        game.expect("Client", "CLIENT_STATUS target=B status=client.failed")
        time.sleep(.25)
        game.screenshot("A", "Failed", references, errors)
        game.screenshot("B", "Failed", references, errors)
    return errors


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--build", type=Path, required=True)
    parser.add_argument("--results", type=Path, required=True)
    parser.add_argument("--references", type=Path, required=True)
    args = parser.parse_args()
    args.results.mkdir(parents=True, exist_ok=True)
    failures = []
    try:
        failures.extend(run(args.build.resolve(), args.results.resolve(), args.references.resolve()))
    except Exception:
        failures.append(traceback.format_exc())
    (args.results / "result.txt").write_text("\n".join(failures) or "PASS\n", encoding="utf-8")
    suite = ET.Element("testsuite", name="Erelia.PythonIntegration", tests="1", failures=str(int(bool(failures))))
    case = ET.SubElement(suite, "testcase", name="PingFromRealUserInput")
    if failures:
        ET.SubElement(case, "failure", message="Integration verification failed").text = "\n".join(failures)
    ET.ElementTree(suite).write(args.results / "results.xml", encoding="utf-8", xml_declaration=True)
    for failure in failures:
        print(failure, file=sys.stderr)
    sys.exit(1 if failures else 0)


if __name__ == "__main__":
    main()
