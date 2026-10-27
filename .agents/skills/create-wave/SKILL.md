---
name: create-wave
description: Add or tune enemy waves (WaveData) for a phase, including enemy types, counts, spawn points and pacing. Use when balancing difficulty or building waves for Phases 1–4.
---

# Create / tune waves

## Rules from the script
- Phase1_Deploy: 0–1 very light wave (2–3 soldiers) for practice.
- Phase2_IronWhip: several waves of increasing difficulty. Clearing the last Phase 2 wave → `GameFlow.GoTo(Phase3_WhipBreak)`.
- Phase3_WhipBreak: no spawns until the player pulls the bamboo (or only a few harassing soldiers, per the GDD).
- Phase4_Bamboo: the final, largest wave. Cleared → `Victory`.
- Total play time must stay within 5–10 minutes.

## Steps

1. Create `Assets/_Game/Data/Waves/<Phase>/WaveData_<Phase>_<Number>.asset`. Each entry has: `EnemyData`, `count`, `spawnPointGroup`, `delayBetweenSpawns`, `startDelay`.
2. Add the asset to the wave list of `PhaseData_<Phase>.asset` in the right order.
3. Spawn points: use the existing `SpawnPoint` groups in `Battlefield.unity` (group names `SP_North`, `SP_Village`, `SP_BambooEdge`…). Adding a spawn point means editing the main scene, so **ask first**.
4. Limit: at most 20 enemies alive at once; if `count` is higher, `WaveDirector` waits; do not raise the limit.
5. Do not write per-wave logic in code. If a wave needs special behaviour, ask the user whether a story event is needed (skill `add-story-event`).
6. Tests:
   - EditMode: `WaveDirector` raises `PhaseWavesCleared` when every wave of the phase is cleared.
   - Play through the phase and note the completion time in the PR description.

## Report back to the user
A short table: phase, number of waves, total enemies, actual playtest time.
