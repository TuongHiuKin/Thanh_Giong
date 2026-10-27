---
name: tao-man-hinh-ui
description: Tạo màn hình UI (HUD máu và đợt quân, prompt tương tác, pause, thắng, thua, menu chính) bằng uGUI + TextMeshPro theo đúng kiến trúc event. Dùng khi thêm hoặc sửa bất kỳ màn hình hay widget UI nào.
---

# Tạo màn hình UI

## Danh sách màn hình trong phạm vi
`MainMenu` (Chơi, Thoát), `HUD` (máu, Phase/đợt hiện tại, số địch còn lại, prompt tương tác, chỉ dẫn tới bụi tre), `Pause` (Tiếp tục, Chơi lại, Về menu), `Victory`, `Defeat` (Chơi lại, Về menu). Màn hình khác là ngoài phạm vi.

## Các bước

1. Prefab `Assets/_Game/Prefabs/UI/<TenManHinh>.prefab`, script `Scripts/Runtime/UI/<TenManHinh>View.cs`.
2. **UI chỉ nghe, không điều khiển gameplay**: View subscribe event (`Health.Changed`, `WaveDirector.WaveStarted`, `GameFlow.PhaseChanged`, `InteractPromptChannel`) trong `OnEnable`, huỷ trong `OnDisable`. Nút bấm gọi các method công khai của `GameFlow` (Pause, Resume, Restart, QuitToMenu), không gọi thẳng vào Player hay Enemy.
3. Canvas: Screen Space – Overlay, Canvas Scaler "Scale With Screen Size" 1920×1080, match 0.5. Tách HUD (cập nhật thường xuyên) và menu ra 2 Canvas khác nhau.
4. Chữ dùng TextMeshPro với font hỗ trợ tiếng Việt có dấu (font asset trong `Assets/_Game/Art/Fonts/`). Kiểm tra chữ "Thánh Gióng", "Phù Đổng", "nhổ tre" hiển thị đúng.
5. Điều hướng được bằng bàn phím/tay cầm: dùng `InputSystemUIInputModule`, đặt `firstSelected` cho mỗi menu.
6. Không cập nhật text trong `Update` mỗi frame; chỉ cập nhật khi event báo thay đổi.
7. Style: tông màu dân gian (đỏ son, vàng đồng, nâu tre). Asset UI ngoài ghi vào `Docs/ASSET_CREDITS.md`.
8. Test PlayMode: Defeat hiện khi máu Player về 0; nút Chơi lại đưa về Phase1.
