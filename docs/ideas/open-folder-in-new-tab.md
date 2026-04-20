# Open folder in another tab (tree + list)

## Problem Statement

How might we open a chosen folder in a **new explorer tab** and **switch to it**, from the **folder tree** and **folder list** context menus, without navigating the **current** tab away?

## Recommended Direction

Add a context-menu item **"Open in another tab"** in both **FolderTreeControl** and **FolderContentsControl**. Choosing it calls into **Form1** (or a small coordinator) to **create a new tab** (`CreateNewTab(selectTab: true)`) and **navigate that tab's** `ExplorerLayoutControl` to the folder path—reusing existing **restore/navigation** logic so combo, tree selection, and list stay consistent. In the **list**, show the command only for **folder** rows; **hide or disable** when selection is not exactly one folder (or define a clear rule for multi-select). After creation, **focus stays on the new tab**.

## Key Assumptions to Validate

- [ ] **Restore/navigation** for an arbitrary path on a **brand-new** tab matches double-click / Open behavior (tree expanded to path, correct root in combo).
- [ ] **Multi-select:** behavior is defined (recommend: enabled only for **one** folder).
- [ ] **Permissions:** inaccessible paths fail gracefully (same as existing Open).

## MVP Scope

- Tree: menu item + handler when a **valid folder path** is selected.
- List: menu item when **single selected item is a folder**; hidden/disabled otherwise.
- New tab opens and **becomes selected**; title updates via existing **CurrentPathChanged** plumbing.

## Not Doing (and Why)

- **Open in background tab** — reduces ambiguity with "which tab is active"; focus switch was the chosen behavior.
- **Empty list background → current folder in new tab** — scoped to folder rows only.
- **Files → open parent in new tab** — folders-only command keeps semantics crisp.
- **Middle-click / keyboard chord** — not in initial scope; can follow later for power users.

## Open Questions

- Should **"Open in another tab"** sit next to **Open** in the menu order (e.g. directly under **Open**)?
