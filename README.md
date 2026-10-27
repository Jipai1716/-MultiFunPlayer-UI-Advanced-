<div align="center">
    <h1>MultiFunPlayer · VR 优化汉化版</h1>
    <p><b>用 VR 控制器（串流桌面）也能舒服地拖动滑动条</b></p>
    <img src="Assets/screenshot.png" style="border-radius: 8px"/>
</div>

<br/>

> 本仓库是 [Yoooi0/MultiFunPlayer](https://github.com/Yoooi0/MultiFunPlayer) 的个人修改版（fork）。
> 原版功能全部保留，主要改动集中在 **界面易用性（尤其是 VR 串流下的滑动条操作）** 与 **中文汉化**。
> 原版文档与更新请见 [上游仓库](https://github.com/Yoooi0/MultiFunPlayer)。

# 这个版本改了什么

## 1. 滑动条（VR 串流下可用）

| 项目 | 原版 | 本版本 |
|---|---|---|
| 普通滑块抓柄 | ~20px 小圆点 | **32×32** 圆角抓柄 + 三条防滑纹，轨道 12px |
| 双端范围滑块抓柄 | 12×14 | **30×34**，轨道 3/4px → **10px**，悬停光晕 48×48 |
| 拖动方式 | 点哪跳哪（绝对定位） | **相对拖动**：按下后按位移增量改变数值，抖动不会跳变 |
| 灵敏度 | — | 拖动约 200px 才走完半程（`DragSensitivity=0.5`） |
| 吸附 | 无 | 靠近 上下限 / 中点 / 0 自动吸附，带迟滞防抖 |
| 轨道高度 | 细 | `Track` 高 44px，整条轨道都是可抓取区域 |

> 为什么这么改：VR 里桌面是以激光笔方式操作鼠标的，指针本身有 3~5px 抖动，绝对定位的滑条几乎无法精确停在某个值上；相对拖动 + 大抓柄 + 吸附可以稳定控制。

## 2. 旋钮控件（`KnobControl`）

用于需要精确微调的参数（目前用在每个轴的 **缩放**）：

* 整个圆盘都可按下，**上下拖动**旋转，灵敏度 0.25（拖过控件高度 = 25% 量程）
* **滚轮**微调（步长 0.01）
* 靠近 **最小值 / 最大值 / 设定吸附点** 自动吸附
* **双击复位**到吸附点（缩放即回到 1.0）
* 自绘：270° 弧 + 指针 + 盘面渐变，跟随主题强调色

## 3. 输出范围编辑器（`AxisRangeEditor`）

每个轴的"上下限"从 3 列小滑条改为一行大控件：

```
☑ L0   下 12.0% ~ 上 88.0%
        ═══════●═══════════════●═══        ← 大号双端滑块（可拖动 / 滚轮）
        下限 [−][+]      上限 [−][+]        ← 每次精确 ±1%
```

* 8 个输出目标（TCP / UDP / Pipe / WebSocket / Serial / File / Buttplug / The Handy）全部统一
* `±1%` 按钮对应输出值域的 `0.01`，方便在没有键盘的环境下微调

## 4. 窗口与排版

* 默认宽度 **600 → 900**，`允许调整窗口大小` 打开后可在 **720 ~ 1600** 之间自由缩放（横向布局默认 1500）
* 基础字号 **13 → 15**
* 卡片间距、开关尺寸同步放大

## 5. 汉化

260+ 条界面文本已被翻译为中文，包括：

* 所有 XAML 标签、提示（ToolTip）、输入框水印、按钮文字
* 设置页 5 个分页：**常规 / 设备 / 主题 / 输入 / 快捷键**
* 枚举显示名（媒体源、脚本库、动作/快捷键动作、更新类型、插值类型等）
* 对话框（错误、确认、文本输入、脚本库管理、快捷键配置）

保留原文的部分：品牌与专有名词（Plex、Emby、Jellyfin、VLC、MPV、MPC-HC、PotPlayer、Whirligig、HereSphere、DeoVR、OFS、XBVR、Stash、Buttplug.io、The Handy、TCP/UDP/WebSocket/Pipe/Serial/File、MultiFunPlayer），以及脚本文件名规则里的英文关键字（`<video name>.funscript` 等必须保持原样）。

# 下载与安装

* 直接下载 Releases 里的 **`MultiFunPlayer-VR-CN-<版本>-win-x64.zip`**
* 解压到任意目录运行 `MultiFunPlayer.exe`（**自包含版本，无需安装 .NET 运行时**）
* 程序是**便携式**的，配置写在程序目录内，不污染系统

如果你要自己编译，见下方「构建」。

# 主要功能（继承自原版）

* 支持 **[DeoVR](https://deovr.com/)、[MPV](https://mpv.io/)、[MPC-HC/BE](https://github.com/clsid2/mpc-hc)、[HereSphere](https://store.steampowered.com/app/1234730/HereSphere/)、[OpenFunscripter](https://github.com/OpenFunscripter/OFS)、[VLC](https://videolan.org/vlc/)、[PotPlayer](https://potplayer.daum.net/)、[Whirligig](http://whirligig.xyz/)、[Plex](https://plex.tv)、[Emby](https://emby.media/)、[Jellyfin](https://jellyfin.org/)** 播放器
* 内置播放器，可**在没有视频文件的情况下播放脚本**
* 输出支持 **[buttplug.io](https://buttplug.io)、TCP、UDP、WebSocket、命名管道、串口、文件、The Handy（实验性）**
* 支持 **[XBVR](https://github.com/xbapps/xbvr)、[Stash](https://github.com/stashapp/stash)** 作为脚本库
* **C# 插件系统**，可自定义扩展
* 同类型输出可多路并发
* 支持 **TCode v0.2 / v0.3** 设备，参数可细调
* 自动探测并连接支持的播放器与输出
* 可配置的**键盘 / 鼠标 / 手柄**快捷键系统
* 支持从 MultiFunPlayer 内跳转、打开、播放/暂停视频
* 使用 pchip / makima 插值做**实时脚本平滑**
* 每轴可配置**速度限制**与**自动归位**
* **智能限制**：依据另一个轴的数值限制本轴范围或速度
* **软启动同步**，避免连接瞬间的突兀动作
* **脚本库**：可加载不放在视频同目录的脚本
* 可把**未脚本化的轴联动**到已脚本化的轴
* 通过随机 / 脚本 / 波形 / 自定义曲线动作提供者**生成额外动作**或**填补脚本空缺**
* 可自定义**主题色**与**深色模式**
* **脚本热力图**，可视化范围与热度
* 支持脚本**书签与章节**
* 真正的**便携应用**，不在程序目录外创建/修改文件

# 使用流程

1. 点右上角 **「+」** 添加视频播放器
2. 需要时用右侧 **「箭头」** 展开设置
3. 点 **连接**（DeoVR / Whirligig / HereSphere 需要先在各自设置里开启远程控制）
4. 点右下角 **「+」** 添加输出
5. 用右侧 **「箭头」** 展开设置（这里就是新的「输出范围」编辑器，可直接拖双端滑块或按 ±1%）
6. 点 **连接**

播放器与输出都连上后，脚本可通过以下方式加载：

* **拖拽**：从资源管理器把脚本文件拖到对应轴的 `文件` 输入框
* **菜单**：轴设置工具栏 → `脚本 → 载入`
* **自动匹配**：脚本按规范命名、与当前播放的视频同名时自动加载

<details>
<summary>常见轴与脚本命名</summary>

| 轴 | 含义 | 有效文件名 |
|-|-|-|
| L0 | 上下 | **`<视频名>.funscript`** |
| L1 | 前后 | **`<视频名>.surge.funscript`** |
| L2 | 左右 | **`<视频名>.sway.funscript`** |
| R0 | 扭转 | **`<视频名>.twist.funscript`** |
| R1 | 翻滚 | **`<视频名>.roll.funscript`** |
| R2 | 俯仰 | **`<视频名>.pitch.funscript`** |

TCode v0.2 附加：`V0` 振动 `.vib.funscript`、`V1` 抽送 `.lube.funscript`、`L3` 吸吮 `.suck.funscript`
TCode v0.3 附加：`V0` 振动 `.vib.funscript`、`A0` 阀门 `.valve.funscript`、`A1` 吸吮 `.suck.funscript`、`A2` 润滑 `.lube.funscript`

</details>

> 以上为推荐命名，其他可识别的脚本名可在 **设备** 设置里查看与配置。
> 查找范围包含所有脚本库目录与当前视频所在目录。

# 构建

需要 [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)（项目目标框架为 `net9.0-windows`）：

```powershell
dotnet publish Source/MultiFunPlayer/MultiFunPlayer.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o dist/MultiFunPlayer
```

框架依赖版（体积小，但需要 .NET 9 桌面运行时）：

```powershell
dotnet publish Source/MultiFunPlayer/MultiFunPlayer.csproj -c Release -r win-x64 --self-contained false -o dist/fdd
```

# 运行环境

* Windows 10 / 11 x64
* 自包含 Release 版无需安装运行时；自行编译的框架依赖版需要 [.NET 9.0 x64 桌面运行时](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Visual C++ 2019 x64 运行库](https://aka.ms/vs/17/release/vc_redist.x64.exe)（串口 / 部分输出可能用到）

# 致谢

* 原项目作者 **[Yoooi0](https://github.com/Yoooi0)**，本项目遵循原仓库 [LICENSE](LICENSE.md)
* 原版支持者：FUNOSR、以及所有 Patreon 支持者

<table>
    <tbody>
        <tr>
            <td style="width: 250px"><a href="https://www.aliexpress.com/store/1103361043/pages/all-items.html"><img src="Assets/logo_funosr.png"/></a></td>
            <td style="min-width: 200px"><strong>感谢 FUNOSR 提供设备用于测试。</strong></td>
        </tr>
        <tr>
            <td><a href="https://www.patreon.com/yoooi"><img src="Assets/logo_patreon.png"/></a></td>
            <td><strong>感谢所有支持原作者的 Patreon 成员。</strong></td>
        </tr>
    </tbody>
</table>
