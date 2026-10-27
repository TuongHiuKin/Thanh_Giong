# GDD – Thánh Gióng: Battle of Phù Đổng

> Structured copy of the proposal on Google Docs. This is the source of truth for the AI and the whole team. To change game content, edit this file through a PR and record the reason in `Docs/decisions/`.
> Status: **concept, waiting for the team to finalize** (section 8 of the original proposal).

## 1. Overview
- Working title: Thánh Gióng – Battle of Phù Đổng
- Genre: 3D Third-Person Arena Combat (bounded area)
- Engine: Unity · Art: Stylized / Low Poly 3D
- Play time: 5–10 minutes · Development time: 5 weeks

## 2. In-game story
Thánh Gióng has grown up and rides out to fight the Ân invaders on a battlefield near Phù Đổng village. He starts with an iron whip. After several waves, the whip breaks as in the legend; Gióng goes to the bamboo grove beside the battlefield and pulls up a bamboo to use as his weapon. Defeating the final wave wins the game, ending with a short cinematic.

## 3. Four phases
| Phase | Name | Content | Ends when |
| --- | --- | --- | --- |
| 1 | Deploy (Xuất trận) | Learn movement, camera, attacks | Tutorial part is done |
| 2 | Iron whip combat (Chiến đấu bằng roi sắt) | Fight waves of Ân invaders | Last wave of Phase 2 is cleared |
| 3 | The whip breaks (Roi sắt gãy) | Whip-break event, go to the bamboo grove, pull bamboo | Bamboo is pulled |
| 4 | Bamboo strikes the invaders (Tre ngà quật giặc) | Defeat the final wave with bamboo | Final wave cleared → victory + cinematic |

Defeat: Gióng's health reaches 0 in any phase → defeat screen → restart.

## 4. Mechanics
**Required:** WASD movement · third-person camera · iron whip attacks · bamboo attacks · health and damage · enemy AI that chases and attacks · Wave System · pull-bamboo interaction · win/lose/restart.

**Stretch (if time allows):** Dodge · one mini-boss · special effects.

**Out of scope:** riding the iron horse · multiple maps · large quest system · skill tree · open world · large village.

## 5. Controls (proposed, to be finalized)
| Key | Action |
| --- | --- |
| WASD | Move |
| Mouse | Rotate camera |
| Left mouse | Attack |
| E (hold) | Interact / pull bamboo |
| Space | Dodge (stretch) |
| Esc | Pause |

## 6. Environment
A single battlefield map: a central fighting area, a bamboo forest beside the battlefield, a few traditional Vietnamese houses in the distance, terrain, trees, and decorative props.

## 7. 5-week plan
| Week | Goal |
| --- | --- |
| 1 | Prototype, character controls, map blockout |
| 2 | Basic combat, health, enemy AI |
| 3 | Wave System, whip-break event, pulling bamboo, win condition |
| 4 | 3D models, animation, environment, UI, effects |
| 5 | Testing, bug fixing, polish, final build |

## 8. Required assets (fill in once the team decides)
- Characters: Thánh Gióng (humanoid rig), regular Ân soldier, (stretch) Ân general
- Weapons: iron whip, broken iron whip, bamboo pole
- Environment: battlefield ground, bamboo grove/forest, traditional houses, trees, rocks
- Animation: Idle, Run, Attack combo (whip/bamboo), Hit, Die, Pull bamboo; enemies: Idle, Run, Attack, Hit, Die
- Audio: battle music, victory music, SFX for swing/hit/whip break/pulling bamboo
