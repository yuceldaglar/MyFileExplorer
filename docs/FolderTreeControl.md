# FolderTreeControl

A Windows Forms user control that displays folders and subfolders as a tree view, starting from a configurable root path.

## Overview

`FolderTreeControl` provides a tree view of the file system. You set a root directory via the `RootPath` property; the control shows that folder as the root and loads subfolders when the user expands nodes. Subfolders are loaded on demand (lazy loading) for better performance with large directory structures.

## Namespace

`MyFileExplorer`

## Key Features

- **RootPath property** — Set the root directory; the tree refreshes automatically when this value changes.
- **FolderSelected event** — Fired when a folder is selected in the tree (user clicks a node), with the folder path in the event args.
- **Lazy loading** — Child folders are loaded when a node is expanded, not when the tree is first built.
- **Sorted folders** — Subfolders are displayed in case-insensitive alphabetical order.
- **Error handling** — Access-denied or not-found directories show placeholder text instead of crashing.

## Properties

| Property   | Type   | Description |
|-----------|--------|-------------|
| **RootPath** | string | The root directory path. The tree displays all folders starting from this path. Setting this property clears the tree and reloads from the new path. Empty or null is allowed (tree will be empty). |

`RootPath` is visible in the Visual Studio Properties window under the **Behavior** category.

## Events

| Event             | EventArgs         | Description |
|-------------------|-------------------|-------------|
| **FolderSelected** | `FolderEventArgs` | Raised when the user selects a folder node in the tree. Use `e.FolderPath` to get the full path of the selected folder. |

## Usage

### In the designer

1. Add `FolderTreeControl` to the toolbox (if needed: right-click toolbox → Choose Items → Browse to the assembly).
2. Drag the control onto a form.
3. Set **RootPath** in the Properties window, e.g. `C:\` or `C:\Users\Public`.

### In code

```csharp
var folderTree = new FolderTreeControl
{
    Dock = DockStyle.Fill,
    RootPath = @"C:\Projects"
};
this.Controls.Add(folderTree);
```

### Changing the root at runtime

```csharp
folderTreeControl.RootPath = @"D:\Backup";
```

The tree updates immediately. If the new path is the same as the current one (by case-insensitive comparison), the tree is not reloaded.

## Node data

Each tree node’s **Tag** stores the full path of the folder (`string`). To react to folder selection, use the **FolderSelected** event:

```csharp
folderTreeControl.FolderSelected += (s, e) =>
{
    Console.WriteLine("Selected: " + e.FolderPath);
    folderContentsControl.CurrentPath = e.FolderPath; // e.g. show contents
};
```

You can also use the **TreeView** property to handle selection or customize the tree:

```csharp
folderTreeControl.TreeView.AfterSelect += (s, e) =>
{
    if (e.Node?.Tag is string path)
        Console.WriteLine("Selected: " + path);
};
```

## Behavior details

- **Root path validation** — If `RootPath` is set to a path that does not exist or is not a directory, the tree is cleared and nothing is shown.
- **Placeholder nodes** — Nodes that have not yet been expanded contain a single placeholder child. On first expand, that child is replaced by the real subfolders.
- **Access denied** — If the process cannot read a directory, the node shows a single child with the text “(Access denied)”.
- **Deleted folders** — If a folder was removed after the node was created, expanding it shows “(Not found)”.

## Requirements

- .NET 10.0 (or compatible Windows Forms target)
- Windows Forms
- Read access to the directories you want to display

## Files

- `FolderTreeControl.cs` — Main logic, `RootPath` property, `FolderSelected` event, and tree population.
- `FolderTreeControl.Designer.cs` — Designer-generated code and `TreeView` setup.
- `FolderTreeControl.resx` — Resource file for the control.

## Example integration

```csharp
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        var folderTree = new FolderTreeControl
        {
            Dock = DockStyle.Fill,
            RootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };
        folderTree.FolderSelected += (s, e) =>
        {
            folderContentsControl.CurrentPath = e.FolderPath;
        };
        Controls.Add(folderTree);
    }
}
```

This shows the current user’s profile folder as the root of the tree and updates the folder contents view when a folder is selected.
