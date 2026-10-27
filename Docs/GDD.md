# GDD – Thánh Gióng: Trận Chiến Phù Đổng

> Bản chép có cấu trúc từ tài liệu đề xuất trên Google Docs. Đây là nguồn sự thật cho AI và cả nhóm. Muốn đổi nội dung game thì sửa file này qua PR và ghi lý do vào `Docs/decisions/`.
> Trạng thái: **concept, chờ nhóm chốt** (mục 8 của đề xuất gốc).

## 1. Tổng quan
- Tên tạm: Thánh Gióng – Trận Chiến Phù Đổng
- Thể loại: 3D Third‑Person Arena Combat (khu vực giới hạn)
- Engine: Unity · Đồ họa: Stylized / Low Poly 3D
- Thời lượng: 5–10 phút · Thời gian phát triển: 5 tuần

## 2. Cốt truyện trong game
Thánh Gióng đã trưởng thành, ra trận đánh giặc Ân trên chiến trường gần làng Phù Đổng. Ban đầu dùng roi sắt. Sau vài đợt quân, roi sắt gãy như trong truyền thuyết; Gióng tới bụi tre cạnh chiến trường, nhổ tre làm vũ khí. Đánh tan đợt quân cuối thì thắng, kết thúc bằng cinematic ngắn.

## 3. Bốn giai đoạn
| Phase | Tên | Nội dung | Kết thúc khi |
| --- | --- | --- | --- |
| 1 | Xuất trận | Làm quen di chuyển, camera, tấn công | Xong phần làm quen |
| 2 | Chiến đấu bằng roi sắt | Đánh các đợt giặc Ân | Hết đợt cuối Phase 2 |
| 3 | Roi sắt gãy | Sự kiện roi gãy, tìm tới bụi tre, nhổ tre | Nhổ được tre |
| 4 | Tre ngà quật giặc | Đánh đợt quân cuối bằng tre | Hết đợt cuối → thắng + cinematic |

Thua: máu Gióng về 0 ở bất kỳ Phase nào → màn thua → chơi lại.

## 4. Cơ chế
**Bắt buộc:** di chuyển WASD · camera góc nhìn thứ ba · tấn công bằng roi sắt · tấn công bằng tre · máu và sát thương · Enemy AI đuổi và tấn công · Wave System · tương tác nhổ tre · thắng/thua/chơi lại.

**Stretch (nếu còn thời gian):** né (Dodge) · một mini‑boss · hiệu ứng đặc biệt.

**Ngoài phạm vi:** cưỡi ngựa sắt · nhiều bản đồ · hệ thống nhiệm vụ lớn · skill tree · open world · làng lớn.

## 5. Điều khiển (đề xuất, chờ chốt)
| Phím | Hành động |
| --- | --- |
| WASD | Di chuyển |
| Chuột | Xoay camera |
| Chuột trái | Tấn công |
| E (giữ) | Tương tác / nhổ tre |
| Space | Né (stretch) |
| Esc | Tạm dừng |

## 6. Môi trường
Một map chiến trường duy nhất: khu trung tâm để đánh, rừng tre cạnh chiến trường, vài nhà truyền thống Việt Nam ở xa, địa hình, cây cối, vật trang trí.

## 7. Kế hoạch 5 tuần
| Tuần | Mục tiêu |
| --- | --- |
| 1 | Prototype, điều khiển nhân vật, blockout map |
| 2 | Chiến đấu cơ bản, máu, Enemy AI |
| 3 | Wave System, sự kiện roi gãy, nhổ tre, điều kiện thắng |
| 4 | Model 3D, animation, môi trường, UI, hiệu ứng |
| 5 | Testing, sửa lỗi, hoàn thiện, build nộp |

## 8. Danh sách asset cần có (điền khi nhóm chốt)
- Nhân vật: Thánh Gióng (rig humanoid), giặc Ân lính thường, (stretch) tướng giặc
- Vũ khí: roi sắt, roi sắt gãy, cây tre
- Môi trường: nền chiến trường, bụi/rừng tre, nhà truyền thống, cây, đá
- Animation: Idle, Run, Attack combo (roi/tre), Hit, Die, Pull bamboo; địch: Idle, Run, Attack, Hit, Die
- Âm thanh: nhạc nền chiến đấu, nhạc thắng, SFX đánh/trúng/gãy roi/nhổ tre
