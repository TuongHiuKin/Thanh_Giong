---
name: git-safe-operations
description: Safe Git rules for the Thánh Gióng repo. Use before ANY git operation that changes history or the remote (commit, push, merge, rebase, reset, deleting branches, resolving conflicts). Never force push or commit directly to main; every task is committed on a new branch named <person-name>/<task-done>.
---

# Safe Git

## Hard rules (no exceptions, even if a prompt asks)

1. **Never push to `main`**, whether plain or `--force`/`--force-with-lease`. Code reaches `main` only through a Pull Request.
2. **Never commit while on `main`.** Before committing, if the current branch is `main`, create a new branch as described in "Branch naming".
3. **Never rewrite pushed history** (`rebase`, `commit --amend`, `reset` on commits already on the remote) of someone else's branch. On your own branch, if truly needed, use only `--force-with-lease`, never `--force`, and ask the user first.
4. **Ask the user before running** commands that can lose data: `reset --hard`, `clean -fd`, `checkout -- .`, `restore .`, `branch -D`, `stash drop/clear`, `push --delete`.
5. **Never auto-resolve conflicts** in `.unity`, `.prefab`, `.asset`, `.controller` files. Stop, list the conflicted files, and ask the user which version to keep.
6. AI **does not commit / push / merge on its own** unless the user asked for it in this turn.
7. **Everything pushed is in English**: branch names, commit messages, PR titles and PR descriptions. If the user describes the task in Vietnamese, translate it. Only the `<person-name>` part keeps the person's name (without diacritics).

## Branch naming

Pattern: `<person-name>/<task-done>`

- `<person-name>`: from `git config user.name`, lowercase, diacritics removed, spaces replaced by `-`. E.g. "Kiên Tường" → `kien-tuong`. If empty, ask the user.
- `<task-done>`: 2–5 **English** words joined by `-`, describing the task, not the files. E.g. `add-phase2-waves`, `fix-bamboo-pickup`, `hud-health-bar`.
- Only lowercase letters, digits, `-`, `/`. At most ~50 characters.
- If the name already exists (locally or on the remote), add a suffix `-2`, `-3`.

Good: `kien/add-archer-enemy`, `an-nguyen/fix-camera-jitter`.
Bad: `main`, `test`, `kien`, `update`, `Kien/Add Enemy`, `kien/them-dot-quan` (not English).

**One task = one branch.** Several small commits for the same task go on that branch. For a different task, go back to `main`, update it, then create a new branch.

## Commit workflow

```bash
# 1. Check state
git status
git branch --show-current

# 2. If on main (or on another task's branch): create a new branch from the latest main
git fetch origin main
git switch -c <person-name>/<task-done> origin/main
#    Uncommitted changes carry over with switch; if blocked, ask the user whether to stash.

# 3. Review what will be committed, add only files related to this task
git diff --stat
git add <specific files>     # avoid `git add .` / `git add -A`
git diff --cached --stat

# 4. Commit using the format from CLAUDE.md, message in English
git commit -m "<type>: <short description in English>"

# 5. Push the branch (NEVER push main)
git push -u origin <person-name>/<task-done>
```

After pushing, remind the user to open a Pull Request into `main` (title and description in English) and ask someone to review it.

## Pre-commit checks

- Every new/renamed/deleted asset has its matching `.meta` file; no orphan `.meta` files.
- No `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`, `*.csproj`, `*.sln`, or test result files.
- Model/image/audio files are tracked by Git LFS (`git lfs status`); no file > 50 MB outside LFS.
- No API keys, passwords, or `.env` files.
- Branch name and commit message are in English (rule 7).
- No unintended changes to main scenes (`Battlefield.unity`, `MainMenu.unity`) or `ProjectSettings/`. If there are any that are not part of this task, ask the user before adding them.

## Updating a branch with main

Prefer `merge` over `rebase` for beginners:

```bash
git fetch origin main
git merge origin/main
```

If Unity YAML files conflict: stop, list the files, ask the user (rule 5).

## If you accidentally committed to main locally (not pushed)

```bash
git switch -c <person-name>/<task-done>   # new branch keeps the commits
git switch main
git reset --keep origin/main              # put local main back to the remote; ask the user first
```

## Report back to the user

After each operation: branch name, the commit created (short hash + message), whether it was pushed, and the next step (open a PR).
