# Terminal output pane should never show a caret

## Problem Statement

How might we stream shell output into MyFileExplorer’s terminal output pane without ever showing an insertion caret there, while typing always clearly belongs to the command line?

## Recommended Direction

Prioritize **Cluster A**: keep the existing `RichTextBox` log for compatibility, but treat it as strictly display-only in Win32 terms—suppress caret drawing and reduce reliance on “selection = scroll anchor” patterns that invite a visible caret at EOF. Validate with a quick focus audit (who has focus while output streams during typing). If RTF is not load-bearing, fall back to **Cluster B** (simpler read-only surface) as a second step.

## Key Assumptions to Validate

- [ ] While the user types in `commandTextBox`, `outputTextBox` does not receive keyboard focus (spy with a debug assert or logging on `Enter`/`Leave`).
- [ ] The visible caret is from `outputTextBox` selection/scroll updates, not from a different control in the tab template.
- [ ] Hiding or avoiding the caret does not break screen reader announcement of new output (smoke test with Narrator if you care about a11y for this pane).

## MVP Scope

- In: output pane shows no blinking caret during normal use; command line caret unchanged; streaming output and scroll behavior still readable.
- Out: full terminal emulation, coloring redesign, or tab system refactor unless focus audit proves necessary.

## Not Doing (and Why)

- Full VT/PTY terminal host — disproportionate for a caret/UX polish issue.
- Redesigning the entire terminal into a single-surface conhost clone — only if product strategy explicitly targets that.
- Replacing `RichTextBox` with a custom renderer on day one — keep as escape hatch if WinForms caret suppression is too brittle.

## Open Questions

- Do you need RTF/colored output in the log today, or is monochrome `Consolas` enough?
- Should clicking the output pane ever select/copy text without showing a caret (selection-only UX)?
