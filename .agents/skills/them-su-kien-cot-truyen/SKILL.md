---
name: them-su-kien-cot-truyen
description: Dựng hoặc sửa sự kiện theo kịch bản Thánh Gióng – roi sắt gãy, chỉ đường tới bụi tre, nhổ tre, chuyển Phase và cinematic kết. Dùng khi làm luồng Phase, tương tác môi trường hoặc Timeline.
---

# Sự kiện cốt truyện và chuyển Phase

## Luồng chuẩn (không đổi thứ tự nếu GDD chưa đổi)

```
Intro → Phase1_XuatTran → Phase2_RoiSat
  → (hết đợt Phase 2) WhipBreakEvent: slow‑mo, roi gãy, VFX mảnh vỡ, Player mất vũ khí
  → Phase3_RoiGay: chỉ dẫn (marker/HUD) tới BambooGrove
  → PullBamboo: giữ phím Interact ~1.5s, animation nhổ tre, Equip(WeaponData_Bamboo)
  → Phase4_TreNga → (hết đợt cuối) Victory → Timeline cinematic kết → màn hình thắng
Bất kỳ Phase nào: máu Player = 0 → Defeat → màn hình thua → Chơi lại
```

## Các bước

1. Mọi chuyển Phase gọi `GameFlow.GoTo(PhaseId)`. Hệ thống khác chỉ nghe `PhaseChanged`.
2. Mỗi sự kiện là một component riêng trong `Scripts/Runtime/Story/` (vd `WhipBreakSequence`, `BambooGrove`, `EndingCinematic`), mỗi file < 300 dòng.
3. **Tương tác nhổ tre**: `BambooGrove` implement `IInteractable`, chỉ cho tương tác khi Phase hiện tại là `Phase3_RoiGay`. Hiện prompt "Giữ E để nhổ tre" qua event cho UI. Thời gian giữ để trong `PhaseData_Phase3`.
4. **Trong lúc sự kiện chạy**: khoá input gameplay qua action map (chuyển sang map `Cutscene`), không `Time.timeScale = 0` (Timeline và UI vẫn cần chạy). Slow‑mo dùng `timeScale` 0.2–0.4 và luôn trả về 1 khi xong hoặc khi bị huỷ.
5. **Cinematic**: dùng Timeline + Cinemachine trong `Assets/_Game/Timeline/`. Có nút bỏ qua. Kết thúc Timeline gọi tiếp `GameFlow`, không để Timeline tự đổi scene.
6. **Chơi lại**: `GameFlow.Restart()` tải lại `ChienTruong.unity`; không dùng static state để nhớ tiến trình.
7. **Test**:
   - EditMode: bảng chuyển Phase hợp lệ, không thể vào `Phase4` khi chưa nhổ tre.
   - PlayMode: chạy nhanh luồng Phase2 → WhipBreak → PullBamboo → Phase4 bằng cheat/test hook (chỉ trong Editor).

## Bám sát truyền thuyết
Nội dung thoại/chữ hiển thị lấy theo `Docs/GDD.md`. Không tự thêm nhân vật, ngựa sắt, cảnh hóa thánh bay về trời hay chi tiết khác nếu GDD không có; nếu muốn, đề xuất cho người dùng trước.
