---
name: tao-ke-dich
description: Thêm một loại giặc Ân mới (lính giáo, lính khiên, cung thủ, tướng) gồm EnemyData, prefab, gắn vào pool và test. Dùng khi người dùng muốn thêm hoặc chỉnh một loại kẻ địch.
---

# Tạo kẻ địch mới

## Hỏi trước nếu chưa rõ
- Tên tiếng Việt và tên code (theo `Docs/Glossary.md`, vd `AnSpearman`).
- Vai trò: cận chiến / tầm xa / tank / mini‑boss. Mini‑boss là **Stretch**, chạy `kiem-tra-pham-vi` trước.
- Xuất hiện ở Phase nào.

## Các bước

1. **Dữ liệu**: tạo `Assets/_Game/Data/Enemies/EnemyData_<TenCode>.asset` (kiểu `EnemyData`). Điền: `maxHealth`, `moveSpeed`, `attackRange`, `attackDamage`, `telegraphTime`, `attackCooldown`, `recoverTime`, `knockbackResistance`, `scoreValue`. Không hardcode các số này ở đâu khác.
2. **Hành vi**: dùng lại `EnemyController` và state machine có sẵn (`Idle, Chase, Telegraph, Attack, Recover, Hurt, Dead`).
   - Chỉ khi hành vi khác hẳn (vd bắn tên) mới thêm một `IEnemyAttack` mới trong `Scripts/Runtime/Enemies/Attacks/`, không sửa class gốc thành nhiều nhánh `if (role == ...)`.
   - Mỗi file < 300 dòng.
3. **Prefab**: tạo Prefab Variant từ `Prefabs/Enemies/EnemyBase.prefab` → `Prefabs/Enemies/<TenCode>.prefab`. Gán model, Animator Override Controller, `EnemyData`. Có `NavMeshAgent`, `Health`, Hurtbox, Hitbox vũ khí.
4. **Animation**: cần tối thiểu Idle, Run, Attack, Hit, Die. Animation Event `HitboxOn/HitboxOff` trên clip Attack; nếu thiếu event, code vẫn bật hitbox theo `telegraphTime`.
5. **Pool**: đăng ký prefab vào `EnemySpawner` (danh sách pool), không `Instantiate` trực tiếp.
6. **Test**:
   - EditMode: `Health` nhận sát thương đúng theo `EnemyData`, chết khi máu về 0.
   - PlayMode (nếu có hành vi mới): địch đuổi theo Player và gây sát thương trong tầm.
7. **Ghi chép**: thêm thuật ngữ vào `Docs/Glossary.md`, asset ngoài vào `Docs/ASSET_CREDITS.md`.

## Kiểm tra cuối
- Thả 10 con vào `Scenes/Sandbox/<TênNgười>.unity`: không lỗi Console, không tụt dưới 60 FPS.
- Không có `Find*`, `static Instance`, `Input.*` trong code mới.
