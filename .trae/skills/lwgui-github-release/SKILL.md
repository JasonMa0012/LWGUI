---
name: lwgui-github-release
description: Use when the user asks to publish a new LWGUI release, merge dev to release branch, or generate release notes and code review reports for LWGUI
---

# LWGUI GitHub Release Workflow

## Project Info

| Item | Detail |
|------|--------|
| Repository | [JasonMa0012/LWGUI](https://github.com/JasonMa0012/LWGUI) |
| Package | `com.jasonma.lwgui` |
| Description | A Lightweight, Flexible, Powerful Shader GUI System for Unity |
| Unity | 2021.3+ |
| License | MIT |

## Branches

| Branch | Purpose |
|--------|---------|
| `1.x` | Release branch — all releases are tagged from here |
| `dev` | Development branch — all feature/bugfix work happens here |

Release workflow: `dev` → (merge) → `1.x` → (tag + release)

## When User Says "发布 x.x.x 版本"

This triggers a multi-step workflow. Execute each step in order.

### Step 1: Check Prerequisites

1. Get the latest release tag to verify version progression:
   ```
   mcp_github_list_releases(owner="JasonMa0012", repo="LWGUI", perPage=3)
   ```

2. Get the two branches' latest commit SHAs:
   ```
   mcp_github_get_commit(owner="JasonMa0012", repo="LWGUI", sha="1.x")
   mcp_github_get_commit(owner="JasonMa0012", repo="LWGUI", sha="dev")
   ```

3. Confirm `dev` has commits ahead of `1.x`. If not, inform the user there's nothing to release.

### Step 2: Create PR from dev to 1.x

Use the MCP tool to create a pull request:

```
mcp_github_create_pull_request(
    owner="JasonMa0012",
    repo="LWGUI",
    title="Release x.x.x",
    head="dev",
    base="1.x",
    body="Release version x.x.x\n\nChanges since last release: ..."
)
```

### Step 3: Merge the PR

```
mcp_github_merge_pull_request(
    owner="JasonMa0012",
    repo="LWGUI",
    pullNumber=<PR_NUMBER>,
    merge_method="merge"
)
```

### Step 4: Analyze Changes Since Last Release

1. Get the diff between the previous release tag and the current `1.x` HEAD:
   ```
   mcp_github_get_commit(owner="JasonMa0012", repo="LWGUI", sha="PREVIOUS_TAG...1.x")
   ```
   Or list commits:
   ```
   mcp_github_list_commits(owner="JasonMa0012", repo="LWGUI", sha="1.x", since="PREVIOUS_RELEASE_DATE")
   ```

2. Get the files changed in the merge PR:
   ```
   mcp_github_pull_request_read(method="get_files", owner="JasonMa0012", repo="LWGUI", pullNumber=<PR_NUMBER>)
   ```
   And the diff:
   ```
   mcp_github_pull_request_read(method="get_diff", owner="JasonMa0012", repo="LWGUI", pullNumber=<PR_NUMBER>)
   ```

3. For key changed files, read the actual content for code review:
   ```
   mcp_github_get_file_contents(owner="JasonMa0012", repo="LWGUI", path="PATH_TO_FILE", ref="1.x")
   ```

### Step 5: Code Review

For each changed file, perform thorough code review. Pay special attention to:

#### Potential Bugs
- Null reference risks (Unity objects, MaterialProperty, Shader properties)
- Undo/Redo record completeness (`Undo.RecordObject`)
- GUI state correctness (EditorGUI.BeginChangeCheck / EndChangeCheck pairs)
- SerializedProperty usage (property type matching, array bounds)
- Keyword state consistency (Material.EnableKeyword/DisableKeyword pairs)

#### Optimization Opportunities
- Repeated `Material.GetFloat/GetVector/etc.` calls — cache results
- Repeated `GUIContent` allocations — use static readonly instances
- Repeated `GUIStyle` construction per frame — cache
- `MaterialPropertyBlock` where direct material property access suffices
- Excessive `Repaint()` / `GUI.changed = true` calls
- Boxing in generic comparisons (int/float/bool)
- Unnecessary `.ToArray()` / `.ToList()` conversions
- `foreach` on non-enumerator-optimized collections in hot paths

#### Code Quality
- Naming consistency (public fields lowercase, private with `_` prefix)
- Unused variables, imports, or dead code
- Overly complex methods that could be split
- Missing `#if UNITY_EDITOR` guards for editor-only code
- Hard-coded paths or magic numbers

#### Unity Best Practices
- `AssetDatabase` operations: check existence before access
- `EditorUtility.SetDirty` vs `Undo.RecordObject` — use appropriately
- Avoid `Resources.Load` in editor code (use `AssetDatabase.LoadAssetAtPath`)
- `SerializedObject.ApplyModifiedProperties` always called after modifications

### Step 6: Generate Release Description

Use this exact format based on historical releases:

```markdown
## What's Changed
- Add [Feature Description]
- Fix [Bug Description]
- Optimize [Description]
- Change [Description]

**Full Changelog**: https://github.com/JasonMa0012/LWGUI/compare/PREVIOUS_TAG...NEW_TAG
```

Format rules:
- Each line starts with `- ` then a verb: `Add`, `Fix`, `Optimize`, `Change`, or `Remove`
- One line per logical change, ordered by: Add > Optimize > Change > Fix > Remove
- If a change has a related PR, append: ` by @JasonMa0012 in https://github.com/JasonMa0012/LWGUI/pull/N`
- If a change has a notable commit, link inline: `[Description](commit_url)`
- End with `**Full Changelog**: compare URL`

### Step 7: Present Output to User

Output two sections:

**1. Release Description** — the formatted description for the user to paste into the GitHub Release UI (MCP does not support creating releases).

**2. Code Review Report** — structured findings:
```
### Potential Bugs
- [File/Line] Description (severity: high/medium/low)

### Optimization Opportunities
- [File/Line] Description

### Code Quality Issues
- [File/Line] Description
```

## MCP Tool Mapping

| Action | MCP Tool |
|--------|----------|
| List releases | `mcp_github_list_releases` |
| Get branch commit | `mcp_github_get_commit` |
| List commits | `mcp_github_list_commits` |
| Create PR | `mcp_github_create_pull_request` |
| Merge PR | `mcp_github_merge_pull_request` |
| Get PR diff/files | `mcp_github_pull_request_read` |
| Read file content | `mcp_github_get_file_contents` |
| **Create Release** | ❌ NOT supported by MCP — user must do manually |

## Common Mistakes

- Forgetting to verify `dev` has new commits before creating PR
- Not reading the actual changed file content — only relying on commit messages leads to missed issues
- Outputting release description with wrong Prev Tag (use the one from step 1)
- Skipping code review for "simple" changes — even one-liners can have null reference bugs
- Mixing up `1.x` and `dev` in MCP calls (merge direction: `head=dev`, `base=1.x`)
