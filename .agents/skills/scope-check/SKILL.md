---
name: scope-check
description: Compare a new feature or request against Docs/GDD.md and the 5-week schedule before building it. Use when there is a new idea, when a request sounds like it expands the game, or before starting any task larger than one work session.
---

# Scope check

Purpose: avoid repeating the old repo's mistake, where the game drifted from a "5–10 minute arena" into "6 isometric levels + open world + iron horse".

## Steps

1. Read `Docs/GDD.md` (mechanics, phases, weekly plan) and section 1 of `CLAUDE.md`.
2. Put the request into exactly one bucket:
   - **Required**: on the required mechanics list or part of the 4 phases.
   - **Stretch**: Dodge, mini-boss, special effects.
   - **Out of scope**: on the "DO NOT build" list, or not in the GDD at all.
3. Work out the current week from the GDD (Week 1 prototype … Week 5 testing/build). If it is Stretch and the Required items for the current week are not done, mark it **Postpone**.
4. Rough estimate: number of new scripts, prefabs, and art/animation assets needed.
5. Answer the user with this template:

```
Request: <one sentence>
Bucket: Required | Stretch | Out of scope
Phase/week: <...>
Effort: ~<n> scripts, <n> prefabs, <n> assets
Recommendation: Do now | Postpone until <...> | Don't build (reason)
```

6. If "Out of scope": do not write code. Only proceed if the user explicitly confirms, and then remind them to update `Docs/GDD.md` and record the decision in `Docs/decisions/`.
