# FolderContentsControl

A Windows Forms user control that displays the contents of a folder (files and subfolders) with a toolbar and configurable view style, similar to Windows File Explorer.

## Overview

`FolderContentsControl` shows the contents of a single directory. You set the folder path via the `CurrentPath` property; the control lists subfolders first, then files, with system folder and file icons. A toolbar at the top provides a **View** menu to switch between Large Icons, Small Icons, List, Details, and Tiles. Double-clicking a folder raises an event so the host can navigate into it (e.g. when used with `FolderTreeControl`).

## Namespace

`MyFileExplorer`

## Key Features

- **CurrentPath property** — Set the folder to display; the list refreshes automatically when this value changes.
- **View styles** — Large Icons, Small Icons, List, Details, and Tiles (like Windows Explorer). The current view is reflected in the View toolbar menu with a check mark.
- **Details columns** — In Details view, columns show Name, Size, Type, and Date Modified. File sizes are formatted (B, KB, MB, GB).
- **System icons** — Folder and file items use icons from the Windows shell (via `ShellIconHelper`).
- **Folder navigation** — Double-clicking a folder raises `FolderDoubleClick` with the folder path so the host can update `CurrentPath` or sync with a tree.
- **Error handling** — Access-denied or not-found directories show a single placeholder row instead of crashing.

## Properties

| Property        | Type   | Description |
|----------------|--------|-------------|
| **CurrentPath** | string | The folder path whose contents are displayed. Setting this property refreshes the list. Empty or null is allowed (list will be empty). |
| **ContentsView** | View  | The view style: `LargeIcon`, `SmallIcon`, `List`, `Details`, or `Tile`. Default is `LargeIcon`. |

Both properties are visible in the Visual Studio Properties window under **Behavior** and **Appearance** respectively.

## Events

| Event                | EventArgs           | Description |
|----------------------|---------------------|-------------|
| **FolderDoubleClick** | `FolderEventArgs`   | Raised when the user double-clicks a folder. Use `e.FolderPath` to navigate (e.g. set `CurrentPath = e.FolderPath`). |

## Usage

### In the designer

1. Add `FolderContentsControl` to the toolbox (if needed: right-click toolbox → Choose Items → Browse to the assembly).
2. Drag the control onto a form.
3. Set **CurrentPath** in the Properties window or in code when the user selects a folder (e.g. from a tree).

### In code

```csharp
var folderContents = new FolderContentsControl
{
    Dock = DockStyle.Fill,
    CurrentPath = @"C:\Projects"
};
folderContents.FolderDoubleClick += (s, e) =>
{
    folderContents.CurrentPath = e.FolderPath;
};
this.Controls.Add(folderContents);
```

### Changing the folder at runtime

```csharp
folderContentsControl.CurrentPath = @"D:\Documents";
```

The list updates immediately. If the new path is the same as the current one (by case-insensitive comparison), the list is not reloaded.

## Item data and ListView access

Each list item’s **Tag** holds internal data (path, type, size, etc.). To handle selection or customize the list, use the **ListView** property:

```csharp
folderContentsControl.ListView.SelectedIndexChanged += (s, e) =>
{
    if (folderContentsControl.ListView.SelectedItems.Count > 0)
    {
        var item = folderContentsControl.ListView.SelectedItems[0];
        // item.Text is the display name; item.Tag has full path and metadata
    }
};
```

## Behavior details

- **Path validation** — If `CurrentPath` is set to a path that does not exist or is not a directory, the list is cleared and nothing is shown.
- **Sort order** — Subfolders and files are each sorted case-insensitively by name. Folders are listed before files.
- **Details view** — Columns (Name, Size, Type, Date Modified) are created when switching to Details view. Sizes are shown as B, KB, MB, or GB; folders show no size.
- **Access denied** — If the process cannot read the directory, a single gray “(Access denied)” row is shown.
- **Deleted folder** — If the folder no longer exists when contents are loaded, a single “(Not found)” row is shown.

## Requirements

- .NET 10.0 (or compatible Windows Forms target)
- Windows Forms
- Read access to the directories you want to display
- Windows (for shell icons via `ShellIconHelper`)

## Files

- `FolderContentsControl.cs` — Main logic, `CurrentPath` / `ContentsView` properties, content loading, view menu, and `FolderDoubleClick` handling.
- `FolderContentsControl.Designer.cs` — Designer-generated code: toolbar with View drop-down, ListView.
- `FolderContentsControl.resx` — Resource file for the control.
- `ShellIconHelper.cs` — Helper that uses the Windows shell (SHGetFileInfo) to provide folder and file icons for the ListView.

## Example integration with FolderTreeControl

Use a tree for navigation and the contents control to show the selected folder:

```csharp
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        var folderTree = new FolderTreeControl
        {
            Dock = DockStyle.Left,
            Width = 250,
            RootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        var folderContents = new FolderContentsControl
        {
            Dock = DockStyle.Fill
        };

        folderContents.FolderDoubleClick += (s, e) =>
        {
            folderContents.CurrentPath = e.FolderPath;
        };

        folderTree.FolderSelected += (s, e) =>
        {
            folderContents.CurrentPath = e.FolderPath;
        };

        Controls.Add(folderContents);
        Controls.Add(folderTree);
    }
}
```

Selecting a folder in the tree shows its contents; double-clicking a folder in the list navigates into it.
