# myModeTest

`myModeTest` 是面向 RimWorld 1.7 的独立资源生产 Mod。首版参考 Hardcore SK 的玻璃资源链，实现“石块/岩块 → 碎石 → 砂 → 玻璃配合料 → 玻璃”。

## 目录

- `About/`：Mod 元数据。
- `1.7/Defs/`：资源、配方、建筑和研究定义。
- `1.7/Textures/`：已获授权复制的 HSK 静态贴图。
- `1.7/Assemblies/`：后续 C# 编译输出目录。
- `Source/MyModeTest/`：C# 工程源码。

## 编译

RimWorld 1.7 使用 `net10.0`，编译前需要安装 .NET 10 SDK。工程默认通过 `RimWorldPath` 使用 `D:\RimWorld v1.7.4979`，并引用该目录下的 `Assembly-CSharp.dll` 与 `UnityStubs.dll`。

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj
```

如需使用其他游戏目录，可以执行：

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj -p:RimWorldPath="D:\Games\RimWorld"
```

也可以通过环境变量 `RIMWORLD_PATH` 提供游戏目录。

## 静态验证

```bash
python tools/validate_mod.py all
```

Windows GBK 控制台或中文输出乱码时，可使用 `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`。

## 参考来源

贴图和流程参考 Hardcore SK `1.7-super-unstable` 分支，固定提交：`0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。本 Mod 不依赖 HSK 程序集或 HSK Def。
