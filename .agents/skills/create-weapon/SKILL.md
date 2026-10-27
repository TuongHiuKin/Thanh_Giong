---
name: create-weapon
description: Add or tune Thánh Gióng's weapons (iron whip, bamboo) including WeaponData, prefab, combo, hitbox and effects. Use when working on the combat system or balancing weapon damage.
---

# Create / tune a weapon

The script has only 2 weapons: the **iron whip** (`IronWhip`, Phases 1–2) and the **bamboo** (`Bamboo`, Phase 4). A third weapon is out of scope; run `scope-check` first.

## Steps

1. **Data** `Assets/_Game/Data/Weapons/WeaponData_<Name>.asset`: `damage`, `range`, `attackRate`, `comboSteps` (list: damage multiplier, timing, knockback, clip), `hitboxShape`, `hitStopTime`, `cameraShake`, `sfx`, `vfx`.
   - Suggested differences so the player feels the switch: bamboo has longer reach, wider sweeps and stronger knockback than the iron whip.
2. **Prefab** `Prefabs/Weapons/<Name>.prefab`: model + trigger collider as the Hitbox (`WeaponHitbox`), attached to the Player's right-hand socket.
3. **Code**: only extend `WeaponBase` when behaviour is truly different (e.g. bamboo sweeps in a circle). Always switch weapons through `PlayerWeaponHolder.Equip(WeaponData)`.
4. **Input**: use the `Attack` action in `InputSystem_Actions.inputactions`. Do not add new keys without updating the controls table in `Docs/GDD.md`.
5. **Each swing hits each target at most once** (keep a HashSet of targets already hit, cleared when the hitbox turns off).
6. **Feedback**: short hit stop, camera shake via Cinemachine Impulse, VFX taken from a pool.
7. **Test** in EditMode: each combo step deals damage according to `WeaponData`; one swing never hits the same enemy twice.

## Note on the iron whip
The whip-break event does **not** live in the weapon. `IronWhip` only raises a `Broken` event when `GameFlow` asks; for the story part see the `add-story-event` skill.
