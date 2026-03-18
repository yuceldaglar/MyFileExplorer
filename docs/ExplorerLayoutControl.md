# ExplorerLayoutControl

A Windows Forms user control that arranges the path combo, folder tree, and folder contents in a split layout: left side has a divider; on the left are **PathItemComboBoxControl** (top) and **FolderTreeControl** (below it); on the right is **FolderContentsControl**.

## Overview

`ExplorerLayoutControl` uses a `SplitContainer` with a vertical divider. The left panel contains the path ComboBox at the top and the folder tree filling the rest. The right panel contains the folder contents view. On creation, the control initializes the PathItem ComboBox from **Program.SavedPathItems** (the list loaded from app settings at startup). When the user selects an item in the combo, the tree root is updated; when a folder is selected in the tree, the folder contents path is set (the control wires **SelectedFolderChanged** → **FolderTree.RootPath** and **FolderSelected** → **FolderContents.CurrentPath**). The control exposes the three child controls so you can customize or handle **FolderDoubleClick** (navigate into subfolder from contents) if needed.

## Namespace

`MyFileExplorer`

## Layout

- **Left of divider**
  - **Top:** `PathItemComboBoxControl` (path dropdown).
  - **Below:** `FolderTreeControl` (folder tree).
- **Right of divider**
  - `FolderContentsControl` (folder contents with toolbar and view styles).

The divider is draggable; minimum width for each side is 100 pixels. Default split position is 250 pixels for the left panel.

## Exposed child controls

| Property               | Type                     | Description |
|------------------------|--------------------------|-------------|
| **PathItemComboBox**   | `PathItemComboBoxControl` | The ComboBox at the top of the left panel. Initialized with **Program.SavedPathItems**. The control listens to **SelectedFolderChanged** and sets the tree root. |
| **FolderTree**         | `FolderTreeControl`       | The folder tree below the combo. **RootPath** is set when the user selects an item in the combo. Selecting a folder in the tree automatically updates the contents view (control wires **FolderSelected** → **FolderContents.CurrentPath**). |
| **FolderContents**     | `FolderContentsControl`  | The contents view on the right. **CurrentPath** is set automatically when a folder is selected in the tree. Use **FolderDoubleClick** to handle navigation into subfolders. |

All three are read-only (use in code, not in the designer Properties window).

## Usage

### In the designer

1. Add `ExplorerLayoutControl` to the toolbox (if needed: right-click toolbox → Choose Items → Browse to the assembly).
2. Drag the control onto a form and set its Dock or size as needed.

### In code

The control loads the path list from **Program.SavedPathItems** (see app settings) and wires the combo to the tree. Just add the control; optionally handle **FolderDoubleClick** to react when the user double-clicks a folder in the contents list.

```csharp
var explorer = new ExplorerLayoutControl
{
    Dock = DockStyle.Fill
};

// Optional: react when user double-clicks a folder in the contents list
explorer.FolderContents.FolderDoubleClick += (s, e) =>
{
    explorer.FolderContents.CurrentPath = e.FolderPath;
};

this.Controls.Add(explorer);
```

## Behavior details

- **SplitContainer** — Orientation is vertical (left/right). You can change the split position at runtime via the control’s `SplitContainer` if you expose it, or leave the default.
- **Child control order (left panel)** — PathItemComboBox is added first with `Dock = Top`, then FolderTree with `Dock = Fill`, so the combo stays at the top and the tree fills the remaining height.
- **Path list** — On creation, the control sets **PathItemComboBox.Items** to **Program.SavedPathItems** (the list loaded from appsettings.json at startup).
- **Combo → tree** — The control listens to **PathItemComboBox.SelectedFolderChanged** and sets **FolderTree.RootPath** to `e.FolderPath` when the selected item changes.
- **Tree → contents** — The control listens to **FolderTree.FolderSelected** and sets **FolderContents.CurrentPath** to the selected folder path, so selecting a folder in the tree automatically shows its contents on the right.
- **Other wiring** — The host may handle **FolderDoubleClick** (navigate into subfolder from contents list) if needed; **Items** and combo → tree are already wired.

## Requirements

- .NET 10.0 (or compatible Windows Forms target)
- Windows Forms
- `PathItemComboBoxControl`, `FolderTreeControl`, and `FolderContentsControl` (same project)
- **Program.SavedPathItems** — The path combo is filled from this list (loaded from appsettings.json at startup). Ensure the app has loaded settings before the control is created.

## Files

- `ExplorerLayoutControl.cs` — Exposes **PathItemComboBox**, **FolderTree**, and **FolderContents**; initializes combo from **Program.SavedPathItems**; listens to **SelectedFolderChanged** (combo → tree root) and **FolderSelected** (tree → **FolderContents.CurrentPath**).
- `ExplorerLayoutControl.Designer.cs` — SplitContainer and child control layout.
- `ExplorerLayoutControl.resx` — Resource file for the control.

## Related documentation

- [PathItemComboBoxControl](PathItemComboBoxControl.md)
- [FolderTreeControl](FolderTreeControl.md)
- [FolderContentsControl](FolderContentsControl.md)
