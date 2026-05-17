---
name: git-diff-skill
description: Git diff and patch generation guide, focusing on encoding pitfalls and correct practices when using git diff redirection in the Trae (PowerShell) environment
---

# Git Diff & Patch Generation

参考文档：https://git-scm.com/docs/git-diff

## `git diff A..B` 语法

`git diff A..B` = `git diff A B`，显示把 **B** 变成 **A** 需要做哪些修改。

在 patch 输出中：
- `+` 行 = **A 有、B 没有** 的内容（加到 B 上才能变成 A）
- `-` 行 = **B 有、A 没有** 的内容（从 B 去掉才能变成 A）

日常用语中"查看 A 分支相对于 B 分支的修改/变化"，对应的命令就是 `git diff A..B`。

| 命令 | 含义 | patch 中 `+` 表示 |
|------|------|------------------|
| `git diff main..feature` | main 相对于 feature 的修改（feature 变成 main 需要做哪些改动） | main 新增的内容 |
| `git diff feature..main` | feature 相对于 main 的修改（main 变成 feature 需要做哪些改动） | feature 新增的内容 |
| `git diff v2.0..v1.0` | v2.0 相对于 v1.0 的修改 | v2.0 新增的内容 |

```
git diff A..B -- path/         → 仅限指定目录
git diff A..B --stat -- path/  → 仅统计
```

## 生成 Patch 文件

```
git diff A..B -- path/ > output.patch
```

## ⚠️ PowerShell `>` 重定向编码问题

Trae 的终端环境是 PowerShell。PowerShell 的 `>` 是 `Out-File` 的别名，在接收原生命令（如 `git`）的输出时，会使用 `$OutputEncoding`（默认 **US-ASCII**，代码页 20127）进行编码转换。

`git diff` 输出为 **UTF-8 no BOM**，当输出中包含非 ASCII 字符（如中文注释）时：

1. PowerShell 试图按 ASCII 解码，无法表示的字节被丢弃或替换
2. 导致文件内容损坏、行结构破坏
3. 最终文件可能仅剩数十字节，完全不可用

**对比实测：**
| 方式 | 结果 |
|------|------|
| `git diff ... > file` (PowerShell) | 14 字节，内容几乎为空 |
| `git diff ... > file` (Git Bash) | 56.8 KB，1603 行，正确 |
| `bash -c "git diff ... > file"` | 正确 |

**解决方案：必须通过 Git Bash 执行重定向。** Git 安装路径通常为 `C:\Program Files\Git\bin\bash.exe`。

```powershell
& "C:\Program Files\Git\bin\bash.exe" -c "cd 'e:/path/to/repo' && git diff A..B -- path/ > output.patch"
```

**不需要重定向时（如 `--stat`）可在 PowerShell 直接执行：**
```
git diff A..B --stat -- path/
```

**不推荐的做法：**
- `Set-Content -Encoding UTF8` 可指定编码，但在 Trae 中可能被误判为高风险命令触发手动确认
- 修改 `$OutputEncoding` 为 UTF-8 会全局生效，影响其他脚本
