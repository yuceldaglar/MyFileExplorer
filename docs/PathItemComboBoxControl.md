# PathItemComboBoxControl

A Windows Forms user control that contains a ComboBox and displays a list of `PathItem` values. Each item is shown by its **Name**; the associated **Path** is used for selection by value.

## Overview

`PathItemComboBoxControl` wraps a single ComboBox with `DropDownList` style. You assign a collection of `PathItem` instances to the **Items** property; the control uses **Name** as the display text and **Path** as the value. You can get or set the selection via **SelectedItem** (the `PathItem`) or **SelectedPath** (the path string), and react to changes with the **SelectedItemChanged** event.

## Namespace

`MyFileExplorer`

## Key Features

- **Items property** — Set the list of `PathItem` values to display. The ComboBox shows each item’s **Name**.
- **SelectedItem** — Get or set the currently selected `PathItem`, or null if none is selected.
- **SelectedPath** — Get or set the selection by path (matches `PathItem.Path` case-insensitively). Convenient when you work with paths from another control (e.g. folder tree).
- **SelectedItemChanged** — Event raised when the selected item changes (user or code).
- **SelectedFolderChanged** — Event raised when the selected item changes; event args contain the selected path (`e.FolderPath`). Use this when you need the path directly (e.g. to set a folder tree root).
- **ComboBox property** — Exposes the underlying ComboBox for further customization or events.

## Properties

| Property        | Type                    | Description |
|----------------|-------------------------|-------------|
| **Items**      | `IEnumerable<PathItem>?` | The list of PathItems shown in the ComboBox. Setting this replaces the current list. Null or empty shows no items. |
| **SelectedItem** | `PathItem?`           | The currently selected PathItem, or null if none. Read/write. |
| **SelectedPath**  | `string?`             | The path of the selected item, or null/empty if none. Setting this selects the item whose Path matches (case-insensitive). |

**Items** is in the **Data** category in the Properties window. **SelectedItem** and **SelectedPath** are not browsable (use in code).

## Events

| Event                   | EventArgs         | Description |
|-------------------------|-------------------|-------------|
| **SelectedItemChanged** | —                 | Raised when the selected item changes (user selection or when **SelectedItem** / **SelectedPath** is set in code). |
| **SelectedFolderChanged** | `FolderEventArgs` | Raised when the selected item changes. Use `e.FolderPath` to get the selected path (empty string if none). |

## Usage

### In the designer

1. Add `PathItemComboBoxControl` to the toolbox (if needed: right-click toolbox → Choose Items → Browse to the assembly).
2. Drag the control onto a form.
3. Set **Items** in code (e.g. when loading data or when another control provides a list of paths).

### In code

```csharp
var pathCombo = new PathItemComboBoxControl
{
    Dock = DockStyle.Top,
    Items = new[]
    {
        new PathItem("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
        new PathItem("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
        new PathItem("Downloads", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"))
    }
};
pathCombo.SelectedItemChanged += (s, e) =>
{
    var item = pathCombo.SelectedItem;
    if (item != null)
        Console.WriteLine("Selected: " + item.Path);
};
this.Controls.Add(pathCombo);
```

### Setting and reading selection

```csharp
// Set by item
pathCombo.SelectedItem = new PathItem("Documents", documentsPath);

// Set by path (selects the item whose Path matches)
pathCombo.SelectedPath = @"C:\Users\Me\Documents";

// Read selection
var selected = pathCombo.SelectedItem;  // PathItem or null
var path = pathCombo.SelectedPath;       // path string or null
```

## Behavior details

- **Display** — The ComboBox shows each `PathItem`’s **Name**. **Path** is used only for **SelectedPath** and for identifying the selected item.
- **DataSource** — Setting **Items** assigns a copy of the sequence to the ComboBox’s DataSource (as a list). The control does not keep a reference to the original collection; update **Items** again to refresh the list.
- **DropDownList** — The ComboBox uses `ComboBoxStyle.DropDownList`, so the user can only choose from the list, not type a new value.
- **SelectedPath setter** — When you set **SelectedPath**, the control finds an item whose **Path** matches (case-insensitive). If none matches, the selection is cleared (SelectedIndex = -1).

## Requirements

- .NET 10.0 (or compatible Windows Forms target)
- Windows Forms
- `PathItem` type (same project/namespace)

## Files

- `PathItemComboBoxControl.cs` — Main logic: **Items**, **SelectedItem**, **SelectedPath**, **SelectedItemChanged**, **SelectedFolderChanged**.
- `PathItemComboBoxControl.Designer.cs` — Designer-generated code and ComboBox setup.
- `PathItemComboBoxControl.resx` — Resource file for the control.

## Related types

- **PathItem** — Data model with **Name** and **Path** (see project root or model documentation).
