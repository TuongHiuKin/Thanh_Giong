---
name: tao-dot-quan
description: Thêm hoặc chỉnh đợt quân (WaveData) cho một Phase, gồm loại địch, số lượng, điểm sinh và nhịp. Dùng khi cân bằng độ khó hoặc dựng đợt quân cho Phase 1–4.
---

# Tạo / chỉnh đợt quân

## Nguyên tắc theo kịch bản
- Phase1_XuatTran: 0–1 đợt rất nhẹ (2–3 lính) để tập đánh.
- Phase2_RoiSat: vài đợt tăng dần. Hết đợt cuối của Phase 2 → `GameFlow.GoTo(Phase3_RoiGay)`.
- Phase3_RoiGay: không sinh địch cho đến khi người chơi nhổ được tre (hoặc chỉ vài lính quấy nhiễu, theo GDD).
- Phase4_TreNga: đợt cuối, đông nhất. Hết đợt → `Victory`.
- Tổng thời gian chơi phải nằm trong 5–10 phút.

## Các bước

1. Tạo `Assets/_Game/Data/Waves/<Phase>/WaveData_<Phase>_<SốThứTự>.asset`. Mỗi entry gồm: `EnemyData`, `count`, `spawnPointGroup`, `delayBetweenSpawns`, `startDelay`.
2. Thêm asset vào danh sách wave của `PhaseData_<Phase>.asset` theo đúng thứ tự.
3. Điểm sinh: dùng các nhóm `SpawnPoint` có sẵn trong `ChienTruong.unity` (tên nhóm `SP_North`, `SP_Village`, `SP_BambooEdge`…). Thêm điểm sinh mới là sửa scene chính, nên **hỏi trước**.
4. Giới hạn: tối đa 20 địch sống cùng lúc; nếu `count` lớn hơn, `WaveDirector` sẽ chờ, không tăng giới hạn.
5. Không viết logic riêng cho từng đợt trong code. Nếu đợt cần hành vi đặc biệt, hỏi người dùng có cần event cốt truyện không (skill `them-su-kien-cot-truyen`).
6. Test:
   - EditMode: `WaveDirector` phát `PhaseWavesCleared` khi mọi đợt của Phase hết địch.
   - Chơi thử Phase đó và ghi lại thời gian hoàn thành vào mô tả PR.

## Báo lại cho người dùng
Bảng ngắn: Phase, số đợt, tổng số địch, thời gian chơi thử thực tế.
