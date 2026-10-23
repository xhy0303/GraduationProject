# 恢复与验证工具

这些工具位于 Assets 外，不会进入正式游戏。工程修复报告见 `Docs/Review/2026-10-08-资源恢复与运行修复.md`。

- `build_candidate.py` 从原 Windows 导出和仓库源码建立候选工程，检测到候选 Assets 已存在时会拒绝覆盖。保留 `Builds/RecoveredOriginal` 和 `Builds/BeforeResourceRepair`。
- `OneMoreTimeRepairTools.cs` 用于隔离候选工程的资源规范化、场景审计和 macOS 构建。
- `OneMoreTimeRuntimeChecks.cs` / `OneMoreTimeGameplayChecks.cs` 是 Editor 批处理运行验证，需要复制到候选项目的 `Assets/Editor/Recovery` 后由 Unity `-executeMethod` 启动。运行测试不要添加 `-quit` 或 `-nographics`；结束时工具自行退出。
- 测试会新建游戏，必须先把候选项目 companyName 设为 `CodexIsolatedValidation`、productName 设为 `OneMoreTimeRecoveryChecks`。测试入口会校验公司名，拒绝使用正式存档；构建后再次运行测试前务必重新核对磁盘上的 ProjectSettings。不要在主工程运行这些测试。
- `fix_portal_power.py` 在项目根目录运行，为 Portal ShaderGraph 的 Power 底数接入 Absolute；重复执行不重复插入节点。

最终原始运行日志在 `Logs/resource-runtime-verified.log`、`Logs/resource-gameplay-checks-2.log`；零错误/零警告的构建日志在 `Logs/resource-mac-build-final.log`。精简报告与截图已复制到 `Docs/Review/RepairValidation`。

## 户外视觉补丁

将 Polybrush `Samples~/SamplesUniversalRP/Shader/Lit Vertex Color URP.ShaderGraph` 及其 meta 复制到候选项目 `Assets/Recovered/Shaders/PolybrushLitVertexColor.shadergraph`。在候选项目 Editor 目录放置 `OutdoorVisualRepair.cs`，执行 `OutdoorVisualRepair.Apply` 恢复草地、调色贴图和户外环境光。`OutdoorVisualRepair.Capture` 在独立测试存档中启动新游戏并输出菜单/城堡近景；需要同目录的 GameplayChecks 截图辅助方法。
