---
name: create-enemy
description: Add a new kind of Ân invader (spearman, shield bearer, archer, general) including EnemyData, prefab, pool registration and tests. Use when the user wants to add or tune an enemy type.
---

# Create a new enemy

## Ask first if unclear
- Vietnamese name and code name (per `Docs/Glossary.md`, e.g. `AnSpearman`).
- Role: melee / ranged / tank / mini-boss. A mini-boss is **Stretch**; run `scope-check` first.
- Which phase it appears in.

## Steps

1. **Data**: create `Assets/_Game/Data/Enemies/EnemyData_<CodeName>.asset` (type `EnemyData`). Fill in: `maxHealth`, `moveSpeed`, `attackRange`, `attackDamage`, `telegraphTime`, `attackCooldown`, `recoverTime`, `knockbackResistance`, `scoreValue`. Do not hardcode these numbers anywhere else.
2. **Behaviour**: reuse `EnemyController` and the existing state machine (`Idle, Chase, Telegraph, Attack, Recover, Hurt, Dead`).
   - Only when behaviour is truly different (e.g. shooting arrows) add a new `IEnemyAttack` in `Scripts/Runtime/Enemies/Attacks/`; do not turn the base class into many `if (role == ...)` branches.
   - Every file < 300 lines.
3. **Prefab**: create a Prefab Variant of `Prefabs/Enemies/EnemyBase.prefab` → `Prefabs/Enemies/<CodeName>.prefab`. Assign model, Animator Override Controller, `EnemyData`. It has `NavMeshAgent`, `Health`, Hurtbox, weapon Hitbox.
4. **Animation**: at least Idle, Run, Attack, Hit, Die. Animation Events `HitboxOn/HitboxOff` on the Attack clip; if the events are missing, code still enables the hitbox based on `telegraphTime`.
5. **Pool**: register the prefab with `EnemySpawner` (pool list); never `Instantiate` directly.
6. **Tests**:
   - EditMode: `Health` takes damage according to `EnemyData` and dies at 0 health.
   - PlayMode (if behaviour is new): the enemy chases the Player and deals damage in range.
7. **Records**: add the term to `Docs/Glossary.md`, external assets to `Docs/ASSET_CREDITS.md`.

## Final check
- Drop 10 of them into `Scenes/Sandbox/<PersonName>.unity`: no Console errors, no drop below 60 FPS.
- No `Find*`, `static Instance`, or `Input.*` in the new code.
