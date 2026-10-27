---
name: kiem-tra-pham-vi
description: Đối chiếu một tính năng hoặc yêu cầu mới với Docs/GDD.md và lịch 5 tuần trước khi làm. Dùng khi có ý tưởng mới, khi yêu cầu nghe như mở rộng game, hoặc trước khi bắt đầu một việc lớn hơn một buổi.
---

# Kiểm tra phạm vi

Mục đích: tránh lặp lại lỗi repo cũ, nơi game trôi từ "arena 5–10 phút" thành "6 màn isometric + open world + ngựa sắt".

## Các bước

1. Đọc `Docs/GDD.md` (mục cơ chế, giai đoạn, kế hoạch tuần) và mục 1 của `CLAUDE.md`.
2. Xếp yêu cầu vào đúng một nhóm:
   - **Bắt buộc**: có trong danh sách cơ chế bắt buộc hoặc 4 Phase.
   - **Stretch**: Dodge, mini‑boss, hiệu ứng đặc biệt.
   - **Ngoài phạm vi**: có trong danh sách "KHÔNG làm", hoặc không xuất hiện trong GDD.
3. Xác định tuần hiện tại theo GDD (Tuần 1 prototype … Tuần 5 test/build). Nếu là Stretch mà các mục Bắt buộc của tuần hiện tại chưa xong, đánh dấu **Hoãn**.
4. Ước lượng thô: số script mới, số prefab, số asset art/animation cần thêm.
5. Trả lời người dùng theo mẫu:

```
Yêu cầu: <một câu>
Phân loại: Bắt buộc | Stretch | Ngoài phạm vi
Thuộc Phase/tuần: <...>
Khối lượng: ~<n> script, <n> prefab, <n> asset
Khuyến nghị: Làm ngay | Hoãn đến khi <...> | Không làm (lý do)
```

6. Nếu "Ngoài phạm vi": không viết code. Chỉ làm khi người dùng xác nhận rõ, và khi đó nhắc họ cập nhật `Docs/GDD.md` + ghi quyết định vào `Docs/decisions/`.
