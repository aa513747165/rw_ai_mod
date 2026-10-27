#!/usr/bin/env python3
"""新链静态验证入口：聚合 tools/validate/ 包下各链模块的 SUITES。

约定：
- 每条新链一个模块 tools/validate/<链名>.py，内部从 validate_mod 复用 helper：
      from validate_mod import fail, require_files, parse_xml, ...
- 模块末尾注册：SUITES = {"<链名>": validate_<链名>, ...}
- 套件名全包内唯一，冲突会直接报错退出。

用法：
    python tools/validate_new.py all        # 跑全部新链
    python tools/validate_new.py <链名>     # 跑单条新链
旧链仍走 python tools/validate_mod.py <套件>。
"""
from __future__ import annotations

import argparse
import importlib
import pkgutil
import sys
from pathlib import Path

# 保证 tools/ 在 sys.path 中，链模块才能 import validate_mod 的 helper
TOOLS_DIR = Path(__file__).resolve().parent
if str(TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(TOOLS_DIR))

import validate as _validate_pkg


def collect_suites() -> dict:
    """扫描 validate 包下所有模块，合并各自注册的 SUITES。"""
    suites = {}
    for info in pkgutil.iter_modules(_validate_pkg.__path__):
        module = importlib.import_module(f"validate.{info.name}")
        for name, fn in getattr(module, "SUITES", {}).items():
            if name in suites:
                raise SystemExit(f"[FAIL] 套件名冲突: {name}（validate.{info.name}）")
            suites[name] = fn
    return suites


def main() -> int:
    suites = collect_suites()
    parser = argparse.ArgumentParser(description="验证 myModeTest Mod 新链的静态文件。")
    parser.add_argument("suite", choices=sorted(suites) + ["all"])
    args = parser.parse_args()
    try:
        if args.suite == "all":
            for name, fn in suites.items():
                fn()
        else:
            suites[args.suite]()
    except (AssertionError, OSError) as exc:
        print(f"[FAIL] {args.suite}: {exc}")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
