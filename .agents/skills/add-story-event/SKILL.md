---
name: add-story-event
description: Build or change events from the Thánh Gióng script – the iron whip breaking, guiding the player to the bamboo grove, pulling bamboo, phase transitions and the closing cinematic. Use when working on the phase flow, environment interaction or Timeline.
---

# Story events and phase transitions

## Standard flow (do not reorder unless the GDD changes)

```
Intro → Phase1_Deploy → Phase2_IronWhip
  → (Phase 2 waves cleared) WhipBreakEvent: slow-mo, whip breaks, shard VFX, Player loses weapon
  → Phase3_WhipBreak: guidance (marker/HUD) to the BambooGrove
  → PullBamboo: hold Interact ~1.5s, pull animation, Equip(WeaponData_Bamboo)
  → Phase4_Bamboo → (final wave cleared) Victory → closing Timeline cinematic → victory screen
Any phase: Player health = 0 → Defeat → defeat screen → Restart
```

## Steps

1. Every phase change calls `GameFlow.GoTo(PhaseId)`. Other systems only listen to `PhaseChanged`.
2. Each event is its own component in `Scripts/Runtime/Story/` (e.g. `WhipBreakSequence`, `BambooGrove`, `EndingCinematic`), each file < 300 lines.
3. **Pull-bamboo interaction**: `BambooGrove` implements `IInteractable` and only allows interaction while the current phase is `Phase3_WhipBreak`. Show the prompt "Hold E to pull bamboo" via an event for the UI. The hold time lives in `PhaseData_Phase3`.
4. **While an event plays**: lock gameplay input by switching action maps (to a `Cutscene` map); do not set `Time.timeScale = 0` (Timeline and UI still need to run). Slow-mo uses `timeScale` 0.2–0.4 and always returns to 1 when finished or cancelled.
5. **Cinematic**: use Timeline + Cinemachine in `Assets/_Game/Timeline/`. Include a skip button. The end of the Timeline calls back into `GameFlow`; never let the Timeline change scenes itself.
6. **Restart**: `GameFlow.Restart()` reloads `Battlefield.unity`; do not use static state to remember progress.
7. **Tests**:
   - EditMode: the phase transition table is valid; `Phase4` cannot be entered before the bamboo is pulled.
   - PlayMode: run Phase2 → WhipBreak → PullBamboo → Phase4 quickly via a cheat/test hook (Editor only).

## Stay true to the legend
On-screen text/dialogue follows `Docs/GDD.md`. Do not add characters, the iron horse, Gióng ascending to heaven, or other details not in the GDD; if you want to, propose it to the user first.
