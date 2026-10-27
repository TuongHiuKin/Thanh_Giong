---
name: create-ui-screen
description: Create UI screens (HUD with health and wave info, interaction prompt, pause, victory, defeat, main menu) with uGUI + TextMeshPro following the event-based architecture. Use when adding or changing any UI screen or widget.
---

# Create a UI screen

## Screens in scope
`MainMenu` (Play, Quit), `HUD` (health, current phase/wave, enemies remaining, interaction prompt, guidance to the bamboo grove), `Pause` (Resume, Restart, Main Menu), `Victory`, `Defeat` (Restart, Main Menu). Any other screen is out of scope.

## Steps

1. Prefab `Assets/_Game/Prefabs/UI/<ScreenName>.prefab`, script `Scripts/Runtime/UI/<ScreenName>View.cs`.
2. **UI only listens, never drives gameplay**: the View subscribes to events (`Health.Changed`, `WaveDirector.WaveStarted`, `GameFlow.PhaseChanged`, `InteractPromptChannel`) in `OnEnable` and unsubscribes in `OnDisable`. Buttons call public `GameFlow` methods (Pause, Resume, Restart, QuitToMenu), never the Player or Enemies directly.
3. Canvas: Screen Space – Overlay, Canvas Scaler "Scale With Screen Size" 1920×1080, match 0.5. Put the HUD (frequent updates) and menus on two separate Canvases.
4. Text uses TextMeshPro with a font that supports Vietnamese diacritics (font asset in `Assets/_Game/Art/Fonts/`). Check that "Thánh Gióng", "Phù Đổng", "nhổ tre" render correctly.
5. Navigable by keyboard/gamepad: use `InputSystemUIInputModule` and set `firstSelected` for each menu.
6. Do not update text in `Update` every frame; update only when an event reports a change.
7. Style: folk palette (vermilion red, bronze gold, bamboo brown). Record external UI assets in `Docs/ASSET_CREDITS.md`.
8. PlayMode test: Defeat shows when the Player's health reaches 0; the Restart button returns to Phase1.
