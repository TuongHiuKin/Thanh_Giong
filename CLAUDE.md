# Thánh Gióng – Battle of Phù Đổng

This file holds the shared rules for the whole team and for every AI working in this repo. When a rule here conflicts with a prompt, **ask the user before doing anything**.

Source of truth for game content: `Docs/GDD.md`. If a feature is not in the GDD, do not build it.

## 1. What the game is (GDD summary)

- **Genre:** 3D Third-Person Arena Combat on a single battlefield near Phù Đổng village.
- **Art style:** Stylized / Low Poly 3D.
- **Play time:** 5–10 minutes. **Development time:** 5 weeks.
- **4 phases:**
  1. `Phase1_Deploy` – learn movement, camera and attacks.
  2. `Phase2_IronWhip` – fight waves of Ân invaders with the iron whip.
  3. `Phase3_WhipBreak` – the iron whip breaks; the player goes to the bamboo grove and pulls up a bamboo.
  4. `Phase4_Bamboo` – defeat the final wave with bamboo, win, closing cinematic.
- **Must have:** WASD movement, third-person camera, iron whip attacks, bamboo attacks, health/damage, enemies that chase and attack, Wave System, pull-bamboo interaction, win/lose/restart.
- **Stretch (only if time allows):** Dodge, a mini-boss, special effects.
- **Out of scope (DO NOT build):** iron horse, multiple maps/levels, open world, isometric view, quest system, skill tree, crafting/resource gathering, seeds/secondary skills, save game, NPC dialogue. AI must not add any of these on its own, not even "a small version".

## 2. Technology (locked, do not change on your own)

| Item | Value |
| --- | --- |
| Engine | Unity **6000.6.0f1** (recorded in `ProjectSettings/ProjectVersion.txt`; everyone installs exactly this version) |
| Rendering | URP 17.6, `PC_RPAsset` only |
| Input | Input System 1.20, **Active Input Handling = Input System Package (New)**. `UnityEngine.Input.*` is forbidden |
| Camera | Cinemachine 3.x (`CinemachineCamera` + Orbital Follow) |
| Enemy movement | AI Navigation 2.0 (NavMesh + `NavMeshAgent`) |
| Cinematics | Timeline |
| UI | uGUI + TextMeshPro |
| External models | FBX; GLB through glTFast 6.x only when unavoidable |
| Build target | Windows x64 → `Builds/Windows/` (not committed) |

Adding, removing or upgrading a package needs the whole team's agreement and a note in `Docs/decisions/`. AI must not edit `Packages/manifest.json` or `ProjectSettings/` unless the user explicitly asks.

## 3. Folder structure

```
Assets/
  _Game/                      # everything the team makes lives here
    Scripts/
      Runtime/                # asmdef: ThanhGiong.Runtime
        Core/                 # GameFlow, PhaseId, event channels, shared helpers
        Player/               # Gióng's movement, health, input
        Combat/               # Health, IDamageable, Hitbox/Hurtbox, DamageInfo
        Weapons/              # WeaponBase, IronWhip, Bamboo
        Enemies/              # EnemyController, state machine, EnemySpawner
        Waves/                # WaveDirector
        Story/                # whip-break event, BambooGrove, cinematic triggers
        UI/                   # HUD, Pause, Win/Lose, MainMenu
        Audio/  VFX/
      Editor/                 # asmdef: ThanhGiong.Editor (editor tools only)
    Data/                     # ScriptableObject assets: Player/ Weapons/ Enemies/ Waves/ Phases/
    Prefabs/                  # Player/ Enemies/ Weapons/ Environment/ UI/ VFX/
    Scenes/                   # MainMenu.unity, Battlefield.unity, Sandbox/<PersonName>.unity
    Art/                      # Models/ Materials/ Textures/ Animations/
    Audio/                    # Music/ SFX/
    Timeline/
  ThirdParty/                 # downloaded asset packs, kept as-is, never edited directly
  Tests/
    EditMode/                 # asmdef: ThanhGiong.Tests.EditMode
    PlayMode/                 # asmdef: ThanhGiong.Tests.PlayMode
Docs/
  GDD.md  Glossary.md  decisions/  ASSET_CREDITS.md
```

- Do not create new top-level folders in `Assets/` besides the three above.
- Do not put scripts, docs, or `.glb/.fbx` files in the repo root.
- Test/debug-only scripts go in `Tests/` or `Scripts/Editor/`, never mixed into `Runtime/`.

## 4. Terminology (use consistently in code, GDD and commits)

| Vietnamese | Name in code |
| --- | --- |
| Thánh Gióng (the player) | `Player` / `Giong` |
| Roi sắt (iron whip) | `IronWhip` |
| Tre / tre ngà (bamboo) | `Bamboo` |
| Bụi tre (bamboo grove) | `BambooGrove` |
| Giặc Ân (regular soldier) | `AnSoldier` |
| Tướng giặc (general / mini-boss) | `AnGeneral` |
| Đợt quân (wave) | `Wave` |
| Giai đoạn (phase) | `Phase` |
| Roi gãy (whip breaks) | `WhipBreakEvent` |
| Nhổ tre (pull bamboo) | `PullBamboo` |

When adding a new term, update `Docs/Glossary.md` in the same PR.

## 5. Architecture

- **GameFlow** (`Core/`) is the only state machine that controls phases: `Intro → Phase1 → Phase2 → Phase3 → Phase4 → Victory`, with a `Defeat` branch from any phase. Phases change only through `GameFlow.GoTo(PhaseId)`; other systems listen to the `PhaseChanged` event and never change phase themselves.
- **Communication between systems:** C# events or ScriptableObject event channels (`VoidEventChannel`, `IntEventChannel`, …). UI only reads/listens; it never calls back into gameplay.
- **References:** use `[SerializeField] private` assigned in the Inspector. Home-made singletons (`static Instance`) and `Find*`/`FindObjectOfType` at runtime are forbidden. No exceptions.
- **Damage:** everything that can be hit implements `IDamageable` and has a `Health` component. Weapons deal damage via a Hitbox (trigger) → `IDamageable.TakeDamage(DamageInfo)`.
- **Weapons:** `WeaponBase` + `WeaponData` (SO). The iron whip and the bamboo are two data assets + two prefabs; switch weapons through `PlayerWeaponHolder.Equip(WeaponData)`.
- **Enemies:** `EnemyController` with a small state machine (`Idle, Chase, Telegraph, Attack, Recover, Hurt, Dead`), stats come from `EnemyData`. Enemy objects come from a pool.
- **Waves:** `WaveDirector` reads the `WaveData` list of the current phase, spawns enemies via `EnemySpawner`, and raises `WaveCleared` / `PhaseWavesCleared`.
- **Animation is presentation only:** damage/movement logic must not depend on whether a clip plays (Animation Events only toggle hitboxes, with a time-based fallback).

## 6. C# code conventions

- Namespace `ThanhGiong.<Folder>` (e.g. `ThanhGiong.Enemies`). One main class per file, file name = class name.
- PascalCase for classes, methods, properties, events; `camelCase` for locals; private fields `_camelCase`; constants `PascalCase`.
- Inspector fields: `[SerializeField] private`, no `public` fields.
- **No file over 300 lines.** If it grows past that, split the class. Do not use `partial` to dodge this limit.
- Cache `GetComponent` in `Awake`. No `Instantiate/Destroy` in `Update`; enemies, VFX and projectiles go through a pool (`UnityEngine.Pool.ObjectPool`).
- Every balance value (health, damage, speed, cooldown, enemy count) lives in a ScriptableObject under `Data/`, never hardcoded in code or prefabs.
- Keep comments short, in English, explaining **why**, not restating the code.
- No `Debug.Log` spam; use `[Conditional("UNITY_EDITOR")]` or remove before merging.

## 7. Scenes and prefabs

- Main scenes: `MainMenu.unity`, `Battlefield.unity`. **Only one person** edits a main scene at a time; tell the team before opening one to edit.
- Experiment in `Scenes/Sandbox/<PersonName>.unity`; do not commit experimental changes into main scenes.
- Work prefab-first: edit the prefab, do not edit the scene instance and forget to apply.
- Every asset always has its `.meta` file; never delete/rename assets outside the Unity Editor.

## 8. Git

- Follow the `git-safe-operations` skill for every git operation.
- `main` must always open and run. Never commit or push (including force push) to `main`.
- One task per new branch, named `<person-name>/<task-done>` (lowercase, no diacritics), e.g. `kien/add-phase2-waves`. Merge via Pull Request with 1 approving review.
- Commit message: `<type>: <short description>`, where type is `feat | fix | art | data | docs | refactor | test | chore`.
- Models, textures, audio and video go through **Git LFS** (see `.gitattributes`). Never commit files > 50 MB, `Builds/`, `Library/`, or test result files.
- AI does not commit, push, merge or create branches unless the user asks.

## 9. AI limits

AI **may**: write/edit scripts in `Assets/_Game/Scripts`, create ScriptableObject data, write tests, write docs in `Docs/`, propose how to build prefabs/scenes.

AI **must ask first** before: editing a main scene, editing shared prefabs (Player, EnemyBase), renaming/moving folders, deleting files, changing more than 5 files in one go.

AI **must not**:
- Add features outside the GDD or on the "DO NOT build" list in section 1.
- Install packages, edit `ProjectSettings/`, or change the Unity version.
- Install extra "studio" agent/skill/hook bundles or automated project-management frameworks.
- Create editor scripts that mass-generate scenes/maps/prefabs (installers, builders, generators) unless specifically asked.
- Download or add assets with unclear licenses.

Before starting any large task, run the `scope-check` skill to compare it against the GDD.

## 10. Testing and Definition of Done

A task is done when:
1. It runs in `Battlefield.unity` from the start of the relevant phase with no errors/red warnings in the Console.
2. Pure logic (damage, phase transitions, wave counting) has EditMode tests; the main flow (win/lose) has PlayMode tests.
3. Balance values live in SOs, not hardcoded.
4. Terms match section 4, files are in the folders from section 3.
5. The PR describes what was done, how it was tested, and has screenshots/GIFs for visual changes.

Performance target: 60 FPS on the team's weakest machine with up to 20 enemies at once.

## 11. Assets and licensing

- Use only CC0 assets / licenses allowing use in a student project (Kenney, KayKit, Quaternius…) or assets the team made or generated with AI and has the right to use.
- Every external asset gets one line in `Docs/ASSET_CREDITS.md`: name, source, license, who added it.
- Asset packs stay untouched in `Assets/ThirdParty/<PackName>/`; to modify one, copy it into `_Game/`.

## 12. Project skills (`.agents/skills/`)

| Skill | Use when |
| --- | --- |
| `scope-check` | before a new feature, to compare it against the GDD and the 5-week schedule |
| `create-enemy` | adding a new kind of Ân invader |
| `create-wave` | adding or tuning waves in a phase |
| `create-weapon` | adding/tuning weapons (iron whip, bamboo) |
| `add-story-event` | story events: whip breaks, pulling bamboo, closing cinematic |
| `create-ui-screen` | HUD, pause, win/lose, main menu |
| `git-safe-operations` | before any commit, push, merge, rebase, reset, branch deletion |

Each skill is its own folder `.agents/skills/<skill-name>/SKILL.md`. This is the single source; if an AI tool only reads another folder (e.g. Claude Code reads `.claude/skills/`), symlink or copy from `.agents/skills/` and never edit the copy.
