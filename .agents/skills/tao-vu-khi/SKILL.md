---
name: tao-vu-khi
description: Thêm hoặc chỉnh vũ khí của Thánh Gióng (roi sắt, tre) gồm WeaponData, prefab, combo, hitbox và hiệu ứng. Dùng khi làm hệ thống đánh hoặc cân bằng sát thương vũ khí.
---

# Tạo / chỉnh vũ khí

Game chỉ có 2 vũ khí theo kịch bản: **roi sắt** (`IronWhip`, Phase 1–2) và **tre** (`Bamboo`, Phase 4). Thêm vũ khí thứ ba là ngoài phạm vi, chạy `kiem-tra-pham-vi` trước.

## Các bước

1. **Dữ liệu** `Assets/_Game/Data/Weapons/WeaponData_<Ten>.asset`: `damage`, `range`, `attackRate`, `comboSteps` (danh sách: sát thương nhân, thời gian, knockback, clip), `hitboxShape`, `hitStopTime`, `cameraShake`, `sfx`, `vfx`.
   - Gợi ý khác biệt để người chơi cảm nhận đổi vũ khí: tre có tầm xa hơn, quét rộng hơn, knockback mạnh hơn roi sắt.
2. **Prefab** `Prefabs/Weapons/<Ten>.prefab`: model + collider trigger làm Hitbox (`WeaponHitbox`), điểm gắn vào bàn tay phải của Player.
3. **Code**: chỉ mở rộng `WeaponBase` khi hành vi thật sự khác (vd tre quét vòng tròn). Đổi vũ khí luôn qua `PlayerWeaponHolder.Equip(WeaponData)`.
4. **Input**: dùng action `Attack` trong `InputSystem_Actions.inputactions`. Không thêm phím mới mà không cập nhật bảng điều khiển trong `Docs/GDD.md`.
5. **Mỗi đòn trúng chỉ tính 1 lần cho mỗi mục tiêu** trong một nhịp đánh (giữ HashSet mục tiêu đã trúng, xoá khi hitbox tắt).
6. **Feedback**: hit stop ngắn, rung camera qua Cinemachine Impulse, VFX lấy từ pool.
7. **Test** EditMode: sát thương từng bước combo đúng theo `WeaponData`; một nhát không trúng cùng địch 2 lần.

## Lưu ý cho roi sắt
Sự kiện roi gãy **không** nằm trong vũ khí. `IronWhip` chỉ phát event `Broken` khi `GameFlow` yêu cầu; phần kịch bản xem skill `them-su-kien-cot-truyen`.
