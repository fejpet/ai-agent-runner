# 🤖 AGENT.md – Autonomous GitHub Issue Implementation Agent (C#)

## 🎯 Purpose

This agent autonomously processes prioritized GitHub issues and executes the full development lifecycle:

1. Selects the next prioritized issue
2. Analyzes and creates an implementation plan
3. Checks for existing implementations
4. Creates a feature branch
5. Implements missing functionality
6. Builds and validates compilation
7. Writes and executes tests
8. Commits changes with proper messages
9. Creates a GitHub Pull Request

---

## ⚙️ Environment Assumptions

- Language: **C# (.NET 6+)**
- Build tool: `dotnet CLI`
- Test framework: `xUnit` / `NUnit` / `MSTest`
- GitHub access via:
  - `gh` CLI or
  - REST API
- Repository already cloned locally
- Agent has permissions to:
  - Create branches
  - Push code
  - Create pull requests

---

## 🔁 Execution Workflow

### 1. 🔍 Select Next Issue

- Retrieve issues sorted by priority:
  - Labels: `priority:*`, `bug`, `enhancement`
  - Or GitHub Project board order
- Skip:
  - Closed issues
  - Assigned issues (unless assigned to agent)
- Select highest priority open issue

---

### 2. 📋 Analyze Issue

#### Input:

- Issue title
- Description
- Comments
- Linked PRs/issues

#### Tasks:

- Summarize issue into:
  - **Short title** (for branch + commit)
  - **Goal**
  - **Acceptance criteria**
- Identify:
  - Affected modules/files
  - Dependencies
  - Risks / unknowns

---

### 3. 🔎 Check Existing Implementation

- Search codebase for:
  - Keywords from issue
  - Related classes / methods
- Determine:
  - ✅ Already implemented → validate correctness
  - ⚠️ Partially implemented → identify missing parts
  - ❌ Not implemented → proceed

---

### 4. 🧠 Create Implementation Plan

Produce structured plan:

```text
Plan:
1. Modify / create [class/file]
2. Add logic for [feature]
3. Update interfaces / DTOs (if needed)
4. Add validation / error handling
5. Add unit tests
6. Create or update documentation under docs folder.
```
