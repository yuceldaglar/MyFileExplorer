# Implementation Plan: Terminal output pane — no caret

## Overview

The embedded terminal (`TerminalControl`) uses a read-only black `RichTextBox` for streamed shell output and a separate `TextBox` for commands. Output updates set `SelectionStart` to the end and call `ScrollToCaret()`, which can surface a visible insertion caret in the output pane even when the user’s intent is “log only.” This plan implements **Cluster A** from `docs/ideas/terminal-output-no-caret.md`: keep `RichTextBox`, make the output surface behave as display-only from a caret/focus perspective, and only escalate to **Cluster B** (different control) if WinForms behavior remains unacceptable.

**Relevant code today:** `AppendOutputLine` and state restore both use `SelectionStart` + `ScrollToCaret()` on `outputTextBox`; output is plain `AppendText` only (no per-line RTF coloring in code).

## Architecture Decisions

- **Prefer localized WinForms fixes** over replacing the output control, unless validation shows an unfixable platform quirk.
- **Preserve copy/select if possible** — the idea doc leaves “click output to select without caret” open; default is to keep mouse selection/copy working unless product chooses otherwise.
- **No new test project required for MVP** — solution has a single `MyFileExplorer.csproj`; verification is `dotnet build` plus manual UI checks unless you add UI test infrastructure later.

## Task List

### Phase 1: Validate assumptions

## Task 1: Focus and caret source checklist

**Description:** Run through the scenarios in the idea doc before changing code: confirm whether `commandTextBox` keeps focus while output streams, and whether the distracting caret is tied to `outputTextBox` (vs. another hosted control in the tab chrome). Record the outcome in a short note (commit message, PR description, or comment on the idea/plan doc).

**Acceptance criteria:**

- [ ] While typing in the command line with continuous shell output (e.g. a long-running or noisy command), you have noted whether `outputTextBox` ever receives focus.
- [ ] You have confirmed the caret appears in the black output `RichTextBox` (not only in the command line).

**Verification:**

- [ ] Manual: run the app, open a terminal tab, run something that streams output; type in the command box; observe focus and caret location.
- [ ] Build succeeds: `dotnet build` (no code change expected for this task alone).

**Dependencies:** None

**Files likely touched:**

- None required; optionally `docs/ideas/terminal-output-no-caret.md` (check off assumption lines) or this plan file (add a “Validation notes” subsection).

**Estimated scope:** XS

---

### Phase 2: Implement Cluster A

## Task 2: Make output `RichTextBox` non-interactive for caret purposes

**Description:** Apply the smallest set of WinForms changes so the output pane does not show a blinking insertion caret during normal use, while `FocusCommandInput()` behavior and streaming append remain intact. Order of attempt (stop when acceptance is met): set `HideSelection = true` and `TabStop = false` on `outputTextBox` if not already; if insufficient, introduce a thin subclass of `RichTextBox` (e.g. `TerminalOutputRichTextBox`) that uses Win32 `HideCaret` / `WndProc` handling on focus or post-append, **or** sets `ControlStyles.Selectable` to `false` if copy/select remains acceptable. Wire the designer to use the subclass only if a subclass is required.

**Acceptance criteria:**

- [ ] With shell running and output streaming, the black output area does not show a blinking caret while the user types in the command line.
- [ ] Command line caret and keyboard input behave unchanged (Enter still runs commands, history keys if any still work).
- [ ] Clicking the output area still ends with focus on the command input (existing `OutputTextBox_MouseDown` → `FocusCommandInput()`).

**Verification:**

- [ ] `dotnet build` succeeds.
- [ ] Manual: stream output + type; click output then type; restart shell; clear output.

**Dependencies:** Task 1 (recommended so you do not chase the wrong control)

**Files likely touched:**

- `TerminalControl.Designer.cs` (properties / control type)
- `TerminalControl.cs` (constructor hooks, or helper calls after append)
- New file optional: `TerminalOutputRichTextBox.cs` (only if subclass is needed)

**Estimated scope:** S–M

---

## Task 3: Reduce “selection as scroll driver” (only if Task 2 insufficient)

**Description:** If a caret or insertion flash remains at EOF after Task 2, replace `ScrollToCaret()`-driven scrolling with an alternative that scrolls the viewport without relying on the selection caret as the anchor (e.g. `WinForms`/`Win32` vertical scroll APIs, or scroll-after-append patterns that do not leave a visible insertion point). Keep `MaxPersistedOutputChars` / truncation behavior unchanged.

**Acceptance criteria:**

- [ ] Same as Task 2 acceptance for “no caret in output,” with streaming scroll still pinned to latest output.
- [ ] `ApplyRestoredState` / restore path still shows the tail of persisted output without regression.

**Verification:**

- [ ] Manual: large output buffer, clear, restore session if applicable.
- [ ] `dotnet build` succeeds.

**Dependencies:** Task 2

**Files likely touched:**

- `TerminalControl.cs` (`AppendOutputLine`, state restore block around `SelectionStart` / `ScrollToCaret`)

**Estimated scope:** S

---

### Phase 3: Polish and record

## Task 4: Document behavior and any trade-offs

**Description:** Update `docs/TerminalControl.md` to state that the output pane is display-first (no caret), and document any deliberate limitation (e.g. reduced selectability if `Selectable` was disabled).

**Acceptance criteria:**

- [ ] Doc reflects actual behavior after Tasks 2–3.
- [ ] Open questions from the idea doc (RTF need, selection-without-caret) are answered or explicitly deferred.

**Verification:**

- [ ] Doc reads consistently with `TerminalControl` public API (`OutputTextBox` still exposed).

**Dependencies:** Task 2 (and Task 3 if executed)

**Files likely touched:**

- `docs/TerminalControl.md`

**Estimated scope:** XS

---

## Checkpoint: After Tasks 1–2

- [ ] `dotnet build` clean
- [ ] Manual: no caret in output during typing + streaming
- [ ] If Task 2 failed acceptance, proceed to Task 3 before doc polish

## Checkpoint: Complete

- [ ] All acceptance criteria for executed tasks satisfied
- [ ] Task 4 done or explicitly skipped with reason
- [ ] Ready for review / merge

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Hiding caret breaks text selection/copy | Med | Prefer `HideCaret`/focus tricks before `Selectable=false`; validate copy in Task 2 verification. |
| Focus stolen by another parent when output updates | High | Task 1 catches this; fix host tab layout/focus logic instead of only patching RTB. |
| `ScrollToCaret` replacement scroll miscalculation on high DPI | Med | Manual check on your target DPI; add follow-up task if needed. |

## Open Questions

- **RTF / color:** Codebase does not set `SelectionColor` today; monochrome is enough unless you plan ANSI coloring later.
- **Selection without caret:** If product requires drag-select in output with zero caret ever, specify that in Task 2 so implementation does not trade away selection.

## Parallelization

- Task 1 can run immediately.
- Tasks 2–3 are sequential.
- Task 4 can run in parallel with final manual QA once behavior is frozen, but should not ship before Task 2 is done.
