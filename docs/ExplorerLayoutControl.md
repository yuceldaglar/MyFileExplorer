# ExplorerLayoutControl

A Windows Forms user control that arranges the path combo and folder tree with folder contents above, and an interactive terminal panel across the full bottom width.

## Overview

`ExplorerLayoutControl` uses an outer horizontal `SplitContainer` (top/bottom). The top panel contains the existing left/right explorer layout (path combo + folder tree on the left, folder contents on the right). The bottom panel hosts `TerminalControl` and spans full width. On creation, the control initializes the PathItem ComboBox from **Program.SavedPathItems** (the list loaded from app settings at startup). When the user selects an item in the combo, the tree root is updated; when a folder is selected in the tree, the folder contents path is set; when the user double-clicks a folder in the contents list, the control navigates the contents to that folder and selects (and expands to) the corresponding node in the tree (the control wires **SelectedFolderChanged** → **FolderTree.RootPath**, **FolderSelected** → **FolderContents.CurrentPath**, and **FolderDoubleClick** → update contents and tree selection). The control exposes child controls for customization.

## Namespace

`MyFileExplorer`

## Layout

- **Left of divider**
  - **Top:** `PathItemComboBoxControl` (path dropdown).
  - **Below:** `FolderTreeControl` (folder tree).
- **Top area (left/right divider)**
  - **Left:** `PathItemComboBoxControl` (top) and `FolderTreeControl` (below).
  - **Right:** `FolderContentsControl`.
- **Bottom area (full width)**
  - `TerminalControl` (interactive PowerShell/cmd terminal panel).

Both splitters are draggable. The top area uses a left/right divider; the outer layout uses a top/bottom divider so the terminal width follows the whole control.

## Exposed child controls

| Property               | Type                     | Description |
|------------------------|--------------------------|-------------|
| **PathItemComboBox**   | `PathItemComboBoxControl` | The ComboBox at the top of the left panel. Initialized with **Program.SavedPathItems**. The control listens to **SelectedFolderChanged** and sets the tree root. |
| **FolderTree**         | `FolderTreeControl`       | The folder tree below the combo. **RootPath** is set when the user selects an item in the combo. Selecting a folder in the tree automatically updates the contents view (control wires **FolderSelected** → **FolderContents.CurrentPath**). |
| **FolderContents**     | `FolderContentsControl`  | The contents view on the right. **CurrentPath** is set when a folder is selected in the tree. Double-clicking a folder in the list is handled by the control (navigate contents and sync tree selection). |
| **Terminal**           | `TerminalControl`        | Interactive shell panel in the bottom-right area. Hosts a real shell session (`PowerShell` or `cmd`) with live output. |

All three are read-only (use in code, not in the designer Properties window).

## Usage

### In the designer

1. Add `ExplorerLayoutControl` to the toolbox (if needed: right-click toolbox → Choose Items → Browse to the assembly).
2. Drag the control onto a form and set its Dock or size as needed.

### In code

The control loads the path list from **Program.SavedPathItems** (see app settings) and wires combo ↔ tree ↔ contents (including **FolderDoubleClick** so double-clicking a folder in the contents list navigates and updates the tree selection). Just add the control.

```csharp
var explorer = new ExplorerLayoutControl { Dock = DockStyle.Fill };
this.Controls.Add(explorer);
```

## Behavior details

- **SplitContainer** — Orientation is vertical (left/right). You can change the split position at runtime via the control’s `SplitContainer` if you expose it, or leave the default.
- **Outer split** — A horizontal `SplitContainer` places the full explorer area on top and terminal at the bottom.
- **Child control order (left panel)** — PathItemComboBox is added first with `Dock = Top`, then FolderTree with `Dock = Fill`, so the combo stays at the top and the tree fills the remaining height.
- **Path list** — On creation, the control sets **PathItemComboBox.Items** to **Program.SavedPathItems** (the list loaded from appsettings.json at startup).
- **Combo → tree** — The control listens to **PathItemComboBox.SelectedFolderChanged** and sets **FolderTree.RootPath** to `e.FolderPath` when the selected item changes.
- **Tree → contents + terminal** — The control listens to **FolderTree.FolderSelected** and sets **FolderContents.CurrentPath** to the selected folder path, and also updates terminal working directory to the same path.
- **Contents → tree + terminal** — The control listens to **FolderContents.FolderDoubleClick**. When the user double-clicks a folder in the contents list, it sets **FolderContents.CurrentPath** to that folder (navigate), updates terminal working directory, and selects the corresponding node in the tree (expanding ancestors as needed so the selection stays in sync).
- **Terminal lifecycle** — `TerminalControl` is hosted as a normal child control in the layout’s bottom-right panel and manages its own shell lifecycle.

## Requirements

- .NET 10.0 (or compatible Windows Forms target)
- Windows Forms
- `PathItemComboBoxControl`, `FolderTreeControl`, and `FolderContentsControl` (same project)
- **Program.SavedPathItems** — The path combo is filled from this list (loaded from appsettings.json at startup). Ensure the app has loaded settings before the control is created.

## Files

- `ExplorerLayoutControl.cs` — Exposes **PathItemComboBox**, **FolderTree**, **FolderContents**, and **Terminal**; initializes combo from **Program.SavedPathItems**; listens to **SelectedFolderChanged** (combo → tree root), **FolderSelected** (tree → contents path), and **FolderDoubleClick** (contents → navigate and sync tree selection).
- `ExplorerLayoutControl.Designer.cs` — Main left/right split + nested right-side top/bottom split (contents over terminal).
- `ExplorerLayoutControl.resx` — Resource file for the control.

## Related documentation

- [PathItemComboBoxControl](PathItemComboBoxControl.md)
- [FolderTreeControl](FolderTreeControl.md)
- [FolderContentsControl](FolderContentsControl.md)
- [TerminalControl](TerminalControl.md)
