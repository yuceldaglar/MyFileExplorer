# TerminalControl

A Windows Forms user control that hosts an interactive shell session (`PowerShell` or `cmd`) with command input and real-time output streaming.

## Overview

`TerminalControl` starts a background shell process and keeps it alive as a session while the control is active. Users type commands in the input box (Enter to execute), and output is streamed line-by-line to the terminal area. The shell can be switched between PowerShell and Command Prompt; switching shells while running restarts the process.

## Namespace

`MyFileExplorer`

## Key Features

- **Interactive shell session** - Starts a real shell process and keeps session state between commands.
- **Command input** - Press Enter in the command box to execute.
- **Input focus forwarding** - Clicking the black log runs `FocusCommandInput()` (`MouseDown` on the output box, plus `GotFocus` on the output box deferred to the same helper so the log never keeps keyboard focus if something else activates it). There is **no** code that forwards `KeyDown`/`WM_CHAR` from the log to the command line: typed characters only appear in whichever control actually has focus; the log is read-only.
- **Live output** - Standard output and error are both streamed into the output area.
- **Output as log** - The output pane is a read-only **multiline `TerminalOutputTextBox`** (`TextBox` subclass): **non-selectable** so the log never keeps keyboard focus or the system insertion caret, **`HideSelection`** so the collapsed EOF selection stays invisible while you type below, **`WordWrap = false`**, scroll bars, and **`WM_VSCROLL` / `SB_BOTTOM`** after each append. After every append the selection is collapsed to a single character (not a zero-width insertion point at EOF) to avoid a blinking caret in the black area.
- **Shell selection** - Supports `PowerShell` and `CommandPrompt` via `ShellType`.
- **Process lifecycle controls** - Start/Stop button and automatic stop on control teardown.
- **Clear output** - Clear button and `ClearOutput()` method.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| **ShellType** | `TerminalShellType` | Shell process used by the control (`PowerShell` or `CommandPrompt`). Changing it while running restarts the shell. |
| **AutoStartShell** | `bool` | If `true`, starts the shell when the control handle is created. Default: `true`. |
| **IsShellRunning** | `bool` | Indicates whether a shell process is currently running. |

Non-browsable accessors:

- **OutputTextBox** (`TextBox`, multiline read-only) - Output log area.
- **CommandTextBox** (`TextBox`) - Input area.

## Events

| Event | EventArgs | Description |
|-------|-----------|-------------|
| **OutputReceived** | `TerminalOutputEventArgs` | Raised when a new output line is appended. |
| **CommandSent** | `TerminalCommandEventArgs` | Raised after a command is submitted to the shell process. |
| **ShellExited** | `EventArgs` | Raised when the shell process exits. |

## Usage

### In the designer

1. Add `TerminalControl` to the toolbox (if needed: right-click toolbox -> Choose Items -> Browse to the assembly).
2. Drag the control onto a form or parent user control.
3. Set **ShellType** and **AutoStartShell** in Properties as needed.

### In code

```csharp
var terminal = new TerminalControl
{
    Dock = DockStyle.Fill,
    ShellType = TerminalShellType.PowerShell,
    AutoStartShell = true
};

terminal.OutputReceived += (s, e) =>
{
    Debug.WriteLine("TERMINAL: " + e.Line);
};

this.Controls.Add(terminal);
```

### Executing commands programmatically

```csharp
terminal.StartShell();
terminal.SendCommand("pwd");
terminal.SendCommand("dir");
```

## Behavior details

- **Session model** - Commands run in one long-lived process, so shell state (for example current working directory) persists.
- **Input behavior** - Pressing Enter sends command text and clears the input box. Clicking output returns focus to the input box.
- **Output behavior** - Stdout and stderr lines are appended as plain text. The viewport follows new lines using `WM_VSCROLL` / `SB_BOTTOM` instead of `ScrollToCaret()`. The log is a read-only multiline `TextBox` so the command line keeps the normal insertion caret without RichEdit drawing a second one in the black pane.
- **Shell switching** - Changing `ShellType` while running stops the current shell and starts the selected one.
- **Shutdown behavior** - The control attempts graceful `exit`; if needed it force-terminates the shell process tree.
- **Design-time safety** - Process startup is skipped in design mode.

## Requirements

- .NET 10.0 (or compatible Windows Forms target)
- Windows Forms
- Windows shell executables available on PATH (`powershell.exe`, `cmd.exe`)

## Files

- `TerminalControl.cs` - Main logic, shell process lifecycle, command submission, output handling, events.
- `TerminalControl.Designer.cs` - Designer-generated UI layout (toolbar row, output box, input box).
- `TerminalOutputTextBox.cs` - Non-selectable multiline read-only `TextBox` used for the log stream.
- `TerminalControl.resx` - Resource file for the control (when generated by designer tooling).

## Related documentation

- [ExplorerLayoutControl](ExplorerLayoutControl.md)
