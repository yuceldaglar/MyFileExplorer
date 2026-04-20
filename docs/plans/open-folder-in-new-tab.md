# Implementation Plan: Open folder in another tab

## Overview

Add **Open in another tab** to the folder **tree** and **folder list** context menus. Choosing it creates a **new** `TabPage` with a fresh `ExplorerLayoutControl`, navigates that instance to the **target folder path** (without changing the current tab), and **selects** the new tab. Implementation follows existing patterns: `FolderEventArgs`, `CreateNewTab`, `CurrentPathChanged` for titles, and navigation logic already used by `RestoreState` / `RestorePath` in `ExplorerLayoutControl`.

## Architecture Decisions

- **Navigation API:** Add a **public** `NavigateToFolder(string path)` (or equivalent) on `ExplorerLayoutControl` that delegates to existing **private** `RestorePath` logic so combo, tree selection, and list stay consistent. Avoid duplicating path logic in `Form1`.
- **Event bubbling:** `Form1` does not reference `FolderTreeControl` / `FolderContentsControl` directly. **`ExplorerLayoutControl`** exposes a single event, e.g. `OpenFolderInNewTabRequested`, subscribed in `CreateNewTab` alongside `CurrentPathChanged`. Child controls raise their own events (or call into layout) that the layout forwards.
- **List menu visibility:** Show **Open in another tab** only when **exactly one** item is selected **and** it is a **folder** (`FolderItemTag.IsFolder`). Hide or disable otherwise (spec: folders-only; recommend **visible + enabled** only when applicable, mirroring how **Open** is tied to `onItem`).
- **Menu order:** Place the new item **immediately after** **Open** in both menus (resolves open question in the idea doc).
- **Multi-select:** Command applies only to **single-folder** selection; no “open first of many” in MVP.

## Dependency Graph

```
NavigateToFolder (ExplorerLayoutControl)
    │
    ├── Task: Tree context menu + event (FolderTreeControl)
    │         └── ExplorerLayoutControl subscribes → raises OpenFolderInNewTabRequested
    │
    ├── Task: List context menu + event (FolderContentsControl)
    │         └── ExplorerLayoutControl subscribes → same event
    │
    └── Task: Form1 handler (CreateNewTab + NavigateToFolder on NEW session)
```

**Order:** Foundation (`NavigateToFolder` + event contract on `ExplorerLayoutControl`) → child controls → `Form1` wiring last so everything compiles incrementally.

## Task List

### Phase 1: Foundation

#### Task 1: Public navigation entry on `ExplorerLayoutControl`

**Description:** Expose a single public method to navigate the **entire** explorer (combo, tree, contents, terminal sync per existing rules) to an existing directory path, reusing `RestorePath` (or equivalent internal flow). This is what the **new** tab will call after creation.

**Acceptance criteria:**

- [ ] Calling `NavigateToFolder` with a path that exists under a known saved root behaves like session restore / double-click navigation (tree selection, list path, `CurrentPathChanged` fires).
- [ ] Calling with a path that requires changing tree root (e.g. different drive) still matches behavior already implemented by `RestorePath`.
- [ ] Invalid or non-directory paths are ignored or handled without crashing (align with existing `Open` / tree guards).

**Verification:**

- [ ] Build succeeds: `dotnet build` (from repo root).
- [ ] Manual: temporarily call `NavigateToFolder` from an existing code path or debug harness if needed; otherwise defer full manual check until Task 4.

**Dependencies:** None

**Files likely touched:**

- `ExplorerLayoutControl.cs`

**Estimated scope:** Small (1 file)

---

#### Task 2: `OpenFolderInNewTabRequested` on `ExplorerLayoutControl`

**Description:** Add a public `event EventHandler<FolderEventArgs>? OpenFolderInNewTabRequested`. Wire **FolderTreeControl** first: subscribe in `ExplorerLayoutControl` constructor (or existing init) and forward the child event. Leave **FolderContentsControl** wiring for Task 3 so Task 2 stays compilable—**or** add stub subscription with no-op until Task 3. Prefer: add tree event + forward in Task 2; extend forwarding in Task 3.

**Acceptance criteria:**

- [ ] Event is raised with correct `FolderPath` when the tree will signal (after Task 2b menu exists).
- [ ] Uses existing `FolderEventArgs` type.

**Verification:**

- [ ] Build succeeds.

**Dependencies:** Task 1 optional for compile, but end-to-end needs Task 1 for `Form1` to call navigation.

**Files likely touched:**

- `ExplorerLayoutControl.cs`
- `FolderTreeControl.cs` (event + raise from click handler)

**Estimated scope:** Small–Medium (2 files)

*Note: Split if preferred: 2a event on tree only, 2b Explorer forward.*

---

### Phase 2: Context menus and child events

#### Task 3: Folder tree — menu item and click behavior

**Description:** In `FolderTreeControl.Designer.cs`, add **Open in another tab** immediately after **Open**. In `FolderTreeControl.cs`, add a new event (e.g. `OpenFolderInNewTabRequested`), enable the item in `TreeContextMenu_Opening` when `hasPath` matches **Open** (same guard), handle click by raising the event with `GetSelectedFolderPath()`. Ensure `UnloadedMarker` is excluded like other commands.

**Acceptance criteria:**

- [ ] Menu item appears in the correct order under **Open**.
- [ ] Enabled only when a valid folder path is selected (same as **Open**).
- [ ] Click raises event with full path; does **not** invoke `FolderSelected` (current tab unchanged).

**Verification:**

- [ ] Manual: right-click tree node → **Open in another tab** → (after Task 4) new tab opens at that path; original tab path unchanged.

**Dependencies:** Task 2 (Explorer forwards event).

**Files likely touched:**

- `FolderTreeControl.cs`
- `FolderTreeControl.Designer.cs`

**Estimated scope:** Small

---

#### Task 4: Folder list — menu item and opening logic

**Description:** In `FolderContentsControl.Designer.cs`, add **Open in another tab** after **Open**. Add event on `FolderContentsControl` (or reuse a shared name pattern). In `ContextMenuStrip_Opening`, set **visible** and **enabled** so it appears only for **single** selection of a **folder** (`FolderItemTag` with `IsFolder`). Handler reads path from selection and raises event; does not run normal **Open** / `FolderDoubleClick` on current tab.

**Acceptance criteria:**

- [ ] Hidden or disabled when selection is empty, multiple items, or a file.
- [ ] Visible and enabled for exactly one folder row.
- [ ] Click does not navigate the **current** explorer (only raises event).

**Verification:**

- [ ] Manual: single folder → command works; file or multi-select → command absent or disabled.

**Dependencies:** Task 2/3 (Explorer must subscribe to contents event).

**Files likely touched:**

- `FolderContentsControl.cs`
- `FolderContentsControl.Designer.cs`
- `ExplorerLayoutControl.cs` (subscribe + forward)

**Estimated scope:** Small–Medium (3 files if Explorer updated here)

---

#### Task 5: `Form1` — create tab and navigate

**Description:** In `CreateNewTab`, after wiring `CurrentPathChanged`, subscribe to `explorer.OpenFolderInNewTabRequested`. Handler: `CreateNewTab(selectTab: true)` to obtain a new session, then call `NavigateToFolder` on **that** session’s explorer with `e.FolderPath`. Unsubscribe in `DisposeSession` to avoid leaks.

**Acceptance criteria:**

- [ ] Tree and list commands both result in a **new** tab showing the target folder; **previous** tab’s path unchanged.
- [ ] New tab is **selected**; window/title text updates via existing `CurrentPathChanged` / `UpdateTabTitle`.
- [ ] No duplicate subscriptions when opening multiple tabs over time.

**Verification:**

- [ ] Manual full flow: tree → new tab; list → new tab; `Ctrl+T` still creates empty tab; session save/restore unaffected (smoke).

**Dependencies:** Tasks 1–4.

**Files likely touched:**

- `Form1.cs`

**Estimated scope:** Small

---

## Checkpoint: After Tasks 1–5

- [ ] `dotnet build` succeeds with no new warnings (treat nullable/analysis as per project norms).
- [ ] Manual: tree **Open in another tab** and list **Open in another tab** both work; current tab unchanged.
- [ ] Optional: quick regression on **Open**, double-click folder, **Ctrl+T** / **Ctrl+W**.

---

## Phase 3: Polish (optional follow-up)

#### Task 6: Documentation touch-up

**Description:** Update `docs/ExplorerLayoutControl.md` (and optionally `FolderTreeControl.md` / `FolderContentsControl.md`) with the new command and event.

**Acceptance criteria:**

- [ ] Public API (`NavigateToFolder`, event name) documented briefly.

**Dependencies:** Task 5 complete.

**Estimated scope:** XS

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| `RestorePath` side effects (terminal sync, `_isRestoringState`) differ for “new tab” vs restore | Med | Reuse one code path; test new tab from tree and from list on different roots. |
| Recursive re-entrancy if handler triggers another `OpenFolderInNewTabRequested` | Low | Handler only creates tab + navigates; no user gesture loop. |
| `DisposeSession` misses unsubscribe | Med | Mirror `CurrentPathChanged` unsubscribe pattern exactly. |

## Open Questions

- **Resolved in plan:** Menu order — **immediately after Open** in both menus.
- **Product:** If **one folder + one file** selected — MVP keeps command **off** (not single folder-only).

## Parallelization Opportunities

- After **Task 1** is merged, **Task 3** (tree) and **Task 4** (list + `ExplorerLayoutControl` subscription for contents) can be implemented in parallel by two contributors if **Task 2** defines the event contract first.
- **Task 5** must be last.

---

## Verification (plan complete)

- [x] Every task has acceptance criteria  
- [x] Every task has verification steps  
- [x] Dependencies ordered  
- [x] No single task exceeds ~5 files  
- [x] Checkpoints defined  

**Human review:** Approve task order and event naming (`OpenFolderInNewTabRequested` vs alternative) before implementation.
