# RestSiteUpgradeAll

`RestSiteUpgradeAll` 是一个《杀戮尖塔 2》DLL mod。

它会接管火堆的 `Smith` 选项，让玩家在火堆点击锻造时，不再进入“选择 1 张牌升级”的界面，而是直接把整副牌中所有当前可升级的卡牌各升级 1 次。

## 项目信息

- Mod ID: `rest_site_upgrade_all`
- DLL 文件名: `rest_site_upgrade_all.dll`
- Manifest: `rest_site_upgrade_all.json`
- 技术栈:
  - Godot
  - C# / .NET DLL
  - Harmony Patch
- 当前 manifest 设置:
  - `affects_gameplay: false`

## 当前实现

核心逻辑在 [src/Sts2/RestSitePatches.cs](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/src/Sts2/RestSitePatches.cs)。

当前做了两件事：

1. Harmony 接管 `SmithRestSiteOption.OnSelect`
   直接获取玩家牌库里所有 `IsUpgradable` 的牌，并调用 `CardCmd.Upgrade(..., CardPreviewStyle.None)`。
2. Harmony 接管 `NRestSiteButton.RefreshTextState`
   当火堆按钮聚焦到 `Smith` 时，把说明文字改成 `一键升级全部可升级卡牌`。

## 项目结构

```text
RestSiteUpgradeAll/
├── README.md
├── RestSiteUpgradeAll.csproj
├── rest_site_upgrade_all.json
├── build_manual.sh
├── bin/
│   ├── manual/
│   └── windows-x64/
└── src/
    ├── Bootstrap.cs
    ├── Log.cs
    └── Sts2/
        └── RestSitePatches.cs
```

## 依赖

当前手动构建脚本依赖以下本机路径：

- `dotnet`: `/usr/local/share/dotnet/dotnet`
- Roslyn C# 编译器:
  `/usr/local/share/dotnet/sdk/10.0.202/Roslyn/bincore/csc.dll`

脚本会从 `GAME_DIR` 指向的游戏运行库目录中读取所有 `.dll` 作为引用。

## 构建脚本

项目自带手动构建脚本：

- [build_manual.sh](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/build_manual.sh)

这个脚本支持 3 个关键参数：

- `GAME_DIR`
  指向 StS2 运行库目录，里面必须包含 `sts2.dll`
- `OUT_SUBDIR`
  指定输出子目录，最终输出到 `bin/<OUT_SUBDIR>/`
- `PLATFORM_TARGET`
  可选，传给 `csc -platform:...`

### 默认构建

默认走当前脚本里的 macOS arm64 运行库目录，输出到 `bin/manual/`：

```zsh
cd /Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll
zsh build_manual.sh
```

输出：

```text
bin/manual/rest_site_upgrade_all.dll
bin/manual/rest_site_upgrade_all.json
```

### 指定 macOS x86_64 构建

```zsh
cd /Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll
GAME_DIR="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64" \
OUT_SUBDIR="macos-x86_64" \
PLATFORM_TARGET="x64" \
zsh build_manual.sh
```

### 构建 Windows x64 版本

当前已经验证过可输出 Windows x64 PE/.NET DLL，命令如下：

```zsh
cd /Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll
GAME_DIR="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64" \
OUT_SUBDIR="windows-x64" \
PLATFORM_TARGET="x64" \
zsh build_manual.sh
```

输出：

```text
bin/windows-x64/rest_site_upgrade_all.dll
bin/windows-x64/rest_site_upgrade_all.json
```

说明：

- 这里使用的是包内 `data_sts2_macos_x86_64` 目录中的托管 DLL 引用。
- 这些引用文件本身是 Windows PE/.NET 程序集。
- 编译时可能会出现 `CS8012` 处理器架构 warning，但当前产物已经验证为 Windows x64 DLL。

## 安装

### macOS 安装目录

```text
/Users/chenyu/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/rest_site_upgrade_all/
```

需要放入：

- `rest_site_upgrade_all.dll`
- `rest_site_upgrade_all.json`

### Windows 安装目录

把 Windows x64 构建产物放到类似下面的目录：

```text
<Slay the Spire 2>\mods\rest_site_upgrade_all\
```

同样放入：

- `rest_site_upgrade_all.dll`
- `rest_site_upgrade_all.json`

## 运行验证

启动游戏后，查看日志：

```text
~/Library/Application Support/SlayTheSpire2/logs/godot.log
```

建议检索：

```zsh
rg -n "RestSiteUpgradeAll|ERROR|Exception" "$HOME/Library/Application Support/SlayTheSpire2/logs/godot.log"
```

正常情况下应该看到类似日志：

```text
[RestSiteUpgradeAll] [INFO] Initializing.
[RestSiteUpgradeAll] [INFO] Applied Harmony patches.
[RestSiteUpgradeAll] [INFO] Upgraded all cards at rest site. player=... count=...
```

## 开发注意事项

- 这个 mod 修改的是玩法逻辑，不是纯显示类 mod。
- 当前 manifest 被强行写成了 `affects_gameplay: false`，这是人为隐藏，不代表它真的不影响玩法。
- 逻辑实现没有走 `AbstractModel`，而是直接用 Harmony patch 现有游戏方法，避免引入额外的 `ModelIdSerializationCache` 副作用。
- 火堆升级遵循游戏原本升级语义：
  - 只升级当前 `IsUpgradable` 的卡
  - 每张牌只升 1 级
  - 不会强行升满到 `MaxUpgradeLevel`

## 相关文件

- 工程文件: [RestSiteUpgradeAll.csproj](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/RestSiteUpgradeAll.csproj)
- 构建脚本: [build_manual.sh](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/build_manual.sh)
- Manifest: [rest_site_upgrade_all.json](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/rest_site_upgrade_all.json)
- 入口初始化: [Bootstrap.cs](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/src/Bootstrap.cs)
- 日志封装: [Log.cs](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/src/Log.cs)
- 核心补丁: [RestSitePatches.cs](/Users/chenyu/project/mod/update_all/demo/RestSiteUpgradeAll/src/Sts2/RestSitePatches.cs)

## 注意事项！！
- 首次加载mod会覆盖存档！请注意报错好自己的存档。
- mod放置目录为<Slay the Spire 2>\mods\rest_site_upgrade_all\
- github地址 https://github.com/cheny777/RestSiteUpgradeAll