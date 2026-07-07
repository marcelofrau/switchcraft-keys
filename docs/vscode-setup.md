---
layout: default
title: VS Code Setup
description: Development environment setup
---

# VS Code Setup Guide

Complete guide to set up Visual Studio Code for SwitchcraftKeys development.

## Installation & Extensions

### 1. Install VSCode
Download from [code.visualstudio.com](https://code.visualstudio.com)

### 2. Install Required Extensions
When opening the project in VSCode, you'll be prompted to install recommended extensions. Accept the prompt or install manually:

**Essential**:
- **C# Dev Kit** (`ms-dotnettools.csharp-dev-kit`) — C# language support, debugging
- **.NET Runtime** (`ms-dotnettools.vscode-dotnet-runtime`) — Runtime for debugging

**Optional but Recommended**:
- **Git Graph** (`eamodio.git-graph`) — Visual git branch history
- **GitLens** (`eamodio.gitlens`) — Git blame + history inline
- **PowerShell** (`ms-vscode.PowerShell`) — Better PowerShell support for build scripts

All recommendations are in `.vscode/extensions.json`.

---

## Key Commands

### Build Tasks

Run via **Terminal** → **Run Task** (Ctrl+Shift+B):

| Task | Command | Keyboard |
|------|---------|----------|
| **Build Debug** (default) | `build:debug` | Ctrl+Shift+B |
| **Build Release** | `build:release` | |
| **Test** | `test` | |
| **Clean** | `clean` | |
| **Publish** | `publish` | |
| **Show Version** | `version:show` | |
| **Bump Patch** | `version:bump-patch` | |
| **Bump Minor** | `version:bump-minor` | |
| **Bump Major** | `version:bump-major` | |

---

### Debug & Run

**Debug Configurations** (Debug panel, top-left):

1. **Run App (Debug)**
   - Builds Debug config, launches `SwitchcraftKeys.exe`
   - Attach breakpoints, use Debug Console
   - **Shortcut**: F5

2. **Run App (Release)**
   - Builds Release config, launches optimized `.exe`
   - F5 to launch

3. **Debug (Attach to Process)**
   - Attach to already-running process
   - Useful if app is hanging or started externally

---

## Workflow Examples

### Daily Development

```
1. Open VSCode at project root: code .
2. C# extension loads, IntelliSense activates
3. Edit code (App.axaml, MainViewModel.cs, etc.)
4. Ctrl+Shift+B → build:debug
5. F5 to launch with debugger
6. Place breakpoints, step through code
```

### Before Commit

```
1. Ctrl+Shift+B → build:debug (verify no errors)
2. Ctrl+Shift+B → test (run tests)
3. Ctrl+Shift+B → clean (optional, clean build state)
4. git add / commit
```

### Prepare Release

```
1. Ctrl+Shift+B → version:show (check current version)
2. Ctrl+Shift+B → version:bump-patch (or minor/major)
3. Ctrl+Shift+B → build:release
4. git add / commit -m "chore: bump version to X.Y.Z"
5. git push
```

---

## Debugging Tips

### Breakpoints
- Click line number to set breakpoint
- Conditional breakpoint: right-click line → Conditional Breakpoint
- Logpoint: right-click line → Logpoint (log without stopping)

### Debug Console
- Execute C# expressions at breakpoint: `App.Current.ToString()`
- View variables in left panel (Local, Watch, Call Stack)

### IntelliSense
- Ctrl+Space to trigger autocomplete
- Hover over symbol for definition
- F12 to jump to definition
- Shift+F12 to find all references

### Problems Panel
- **Problems** tab shows build errors/warnings
- Click error to jump to file:line
- Red squiggly = compile error, Yellow = warning

---

## VSCode Settings

### `.vscode/settings.json` Customizations

| Setting | Effect |
|---------|--------|
| `editor.formatOnSave` | Auto-format C# on save |
| `omnisharp.enableEditorConfigSupport` | Respect `.editorconfig` rules |
| `csharp.inlayHints.*` | Show type hints, implicit creations |
| `files.exclude` | Hide bin/, obj/ from Explorer |
| `terminal.integrated.defaultProfile.windows` | Default to PowerShell |

Edit `.vscode/settings.json` to customize further.

---

## Troubleshooting

### C# Extension Not Loading
- Restart VSCode (Ctrl+Shift+P → Developer: Reload Window)
- Check Output panel (C# Log) for errors
- Ensure .NET 8.0 SDK is installed: `dotnet --version`

### Build Fails with "Project file not found"
- Ensure working directory is project root: `D:\workspace\_non_work_\SwitchcraftKeys`
- Check `.csproj` exists at `src/SwitchcraftKeys/SwitchcraftKeys.csproj`

### Debugger Won't Attach
- Run `build:debug` task first (generates `.pdb` debug symbols)
- Try **Debug (Attach to Process)** instead of F5
- Check firewall allows debugger

### IntelliSense Is Slow
- Restart OmniSharp: Ctrl+Shift+P → OmniSharp: Restart OmniSharp
- Large projects may take 10-30s to load initially

### PowerShell Execution Policy Error
If scripts won't run:
```powershell
# In VSCode Terminal:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## Advanced

### Multi-Root Workspaces
Not needed for this project; single-folder workspace is sufficient.

### Custom Launch Configurations
Edit `.vscode/launch.json` to pass command-line args:
```json
{
  "args": ["--debug", "--log-level=trace"]
}
```

### Custom Build Tasks
Edit `.vscode/tasks.json` to add shell tasks (batch, bash, etc.).

---

## References

- [VSCode Debugging](https://code.visualstudio.com/docs/editor/debugging)
- [VSCode Tasks](https://code.visualstudio.com/docs/editor/tasks)
- [C# Dev Kit Docs](https://code.visualstudio.com/docs/csharp/cs-dev-kit)

---

**Last Updated**: 2026-07-01  
**Maintained by**: Marcelo Frau
