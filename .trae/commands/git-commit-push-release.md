---
name: "git-commit-push-release"
description: "提交工作区改动并发布新版本。流程：版本号校验 → 代码审查 → 提交推送 → 创建PR合并到1.x → 改动审查 → 生成发布日志"
---

# Git Commit, Push & Release

## GitHub 操作方式

本流程使用 **Github仓库操作 Agent** (`github-repo-operator`) 与 GitHub 交互。Agent 内置了 GitHub MCP 工具，支持以下操作：
- 获取仓库信息、分支、标签
- 创建/合并 Pull Request
- 列出提交记录
- 操作 Issue 和 Release

调用方式：使用 `Task` 工具，设置 `subagent_type="github-repo-operator"`，在 `query` 中描述需要执行的操作。

## 仓库信息

| 项 | 值 |
|----|-----|
| 仓库 | [JasonMa0012/LWGUI](https://github.com/JasonMa0012/LWGUI) |
| 发布分支 | `1.x` |
| 开发分支 | `dev` |
| 合并方向 | `dev` → `1.x` |

## 流程

### Step 1: 版本号校验

确认 `package.json` 中的 `version` 高于最新已发布的 git tag（按日期排序，仅查询发布分支上的 tag）：

```bash
git tag --sort=-creatordate --merged 1.x | Select-Object -First 1
```

如果版本号未递增，提示用户修改 `package.json` 中的版本号后再继续。

### Step 2: 审查已暂存的改动

以只读模式审查已暂存（Staged）的改动，忽略 Unstaged 文件：

```bash
git diff --cached
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

仅提交已 Staged 的文件，不额外 `git add`：

```bash
git commit -m "<message>"
git push origin dev
```

### Step 4: 创建 PR 并合并到 1.x

通过 **Github仓库操作 Agent** (`github-repo-operator`) 执行以下操作：

1. 创建 PR：
   ```
   使用 Task 工具，subagent_type="github-repo-operator"
   请求: 创建 PR，owner="JasonMa0012", repo="LWGUI", title="Release <version>", head="dev", base="1.x", body="Release version <version>"
   ```

2. 合并 PR：
   ```
   使用 Task 工具，subagent_type="github-repo-operator"
   请求: 合并 PR，owner="JasonMa0012", repo="LWGUI", pullNumber=<PR_NUMBER>, merge_method="merge"
   ```

### Step 5: 审查上一个已发布版本之后的所有改动

通过 **Github仓库操作 Agent** (`github-repo-operator`) 执行：

1. 获取上一个 tag 对应的提交日期：
   ```
   使用 Task 工具，subagent_type="github-repo-operator"
   请求: 获取 tag 信息，owner="JasonMa0012", repo="LWGUI", tag="<PREVIOUS_TAG>"
   ```

2. 获取该日期之后 `1.x` 上的所有提交：
   ```
   使用 Task 工具，subagent_type="github-repo-operator"
   请求: 列出提交，owner="JasonMa0012", repo="LWGUI", sha="1.x", since="<commit.date>"
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

> 注意：当前 GitHub MCP 工具不支持创建 Release，需用户手动在 GitHub 发布页面创建。
