---
name: "git-commit-push-release"
description: "提交工作区改动并发布新版本。流程：版本号校验 → 代码审查 → 提交推送 → 创建PR合并到1.x → 改动审查 → 生成发布日志"
---

# Git Commit, Push & Release

## 仓库信息

| 项 | 值 |
|----|-----|
| 仓库 | [JasonMa0012/LWGUI](https://github.com/JasonMa0012/LWGUI) |
| 发布分支 | `1.x` |
| 开发分支 | `dev` |
| 合并方向 | `dev` → `1.x` |

## 流程

### Step 1: 版本号校验

确认 `package.json` 中的 `version` 高于最新已发布的 git tag：

```bash
git tag --sort=-v:refname | head -1
```

如果版本号未递增，提示用户修改 `package.json` 中的版本号后再继续。

### Step 2: 审查工作区改动

以只读模式审查所有暂存/未暂存的改动：

```bash
git diff HEAD
```

评估代码质量，重点关注：
- 空引用风险
- Undo/Redo 记录完整性
- EditorGUI 状态正确性
- 命名一致性
- 未使用的变量/导入

若无问题则继续。有问题则向用户说明并等待处理。

### Step 3: 生成改动描述并提交

根据改动内容生成简洁的 commit message，格式：`<type>: <description>`

类型：`Add` / `Fix` / `Optimize` / `Change` / `Remove`

然后执行：

```bash
git add -A
git commit -m "<message>"
git push origin dev
```

### Step 4: 创建 PR 并合并到 1.x

通过 GitHub MCP 工具：

```
mcp_github_create_pull_request(
    owner="JasonMa0012", repo="LWGUI",
    title="Release <version>",
    head="dev", base="1.x",
    body="Release version <version>"
)
```

然后合并 PR：

```
mcp_github_merge_pull_request(
    owner="JasonMa0012", repo="LWGUI",
    pullNumber=<PR_NUMBER>,
    merge_method="merge"
)
```

### Step 5: 审查上一个已发布版本之后的所有改动

获取上一个 tag 到当前 `1.x` 的提交列表和差异：

```
mcp_github_list_commits(owner="JasonMa0012", repo="LWGUI", sha="1.x", since="<PREVIOUS_TAG_DATE>")
mcp_github_get_commit(owner="JasonMa0012", repo="LWGUI", sha="<PREVIOUS_TAG>...1.x")
```

仔细审查改动，查找：
- **潜在 BUG**：空引用、Undo 记录缺失、GUI 状态不一致、属性类型不匹配
- **性能问题**：重复材质属性查询、每帧分配、不必要的转换
- **代码质量**：命名不规范、死代码、复杂方法、缺失 `#if UNITY_EDITOR` 守卫

向用户汇报所有发现的严重问题。

### Step 6: 生成发布日志

面向 LWGUI 用户，使用英文描述功能层面的变化。格式：

```markdown
Version: x.x.x

## What's Changed
- Add [功能描述]
- Fix [修复描述]
- Optimize [优化描述]

**Full Changelog**: https://github.com/JasonMa0012/LWGUI/compare/<PREV_TAG>...<NEW_TAG>
```

规则：
- 每行以 `- ` + 动词开头：`Add` / `Fix` / `Optimize` / `Change` / `Remove`
- 只描述用户可感知的功能变化，不涉及代码细节
- 按 Add > Optimize > Change > Fix > Remove 排序
- 末尾附 Full Changelog 对比链接

> 注意：MCP 不支持创建 Release，需用户手动在 GitHub 发布页面创建。
