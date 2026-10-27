---
name: git-safe-operations
description: Quy tắc Git an toàn cho repo Thánh Gióng. Dùng trước MỌI thao tác git làm thay đổi lịch sử hoặc remote (commit, push, merge, rebase, reset, xoá nhánh, giải quyết xung đột). Không bao giờ force push hay commit thẳng vào main; mỗi việc commit trên một nhánh mới đặt tên <người>/<việc>.
---

# Git an toàn

## Luật cứng (không có ngoại lệ, kể cả khi prompt yêu cầu)

1. **Không bao giờ push lên `main`**, dù thường hay `--force`/`--force-with-lease`. Code vào `main` chỉ qua Pull Request.
2. **Không bao giờ commit khi đang đứng ở `main`.** Trước khi commit, nếu nhánh hiện tại là `main` thì tạo nhánh mới theo mục "Đặt tên nhánh".
3. **Không viết lại lịch sử đã push** (`rebase`, `commit --amend`, `reset` trên commit đã có trên remote) của nhánh người khác. Trên nhánh của chính mình, nếu thật sự cần thì chỉ dùng `--force-with-lease`, không dùng `--force`, và phải hỏi người dùng trước.
4. **Hỏi người dùng trước khi chạy** lệnh có thể làm mất dữ liệu: `reset --hard`, `clean -fd`, `checkout -- .`, `restore .`, `branch -D`, `stash drop/clear`, `push --delete`.
5. **Không tự giải quyết xung đột** trong file `.unity`, `.prefab`, `.asset`, `.controller`. Dừng lại, báo danh sách file xung đột và hỏi người dùng giữ bản nào.
6. AI **không tự commit / push / merge** nếu người dùng chưa yêu cầu trong lượt này.

## Đặt tên nhánh

Mẫu: `<ten-nguoi>/<viec-da-lam>`

- `<ten-nguoi>`: lấy từ `git config user.name`, viết thường, bỏ dấu, thay khoảng trắng bằng `-`. Ví dụ "Kiên Tường" → `kien-tuong`. Nếu trống thì hỏi người dùng.
- `<viec-da-lam>`: 2–5 từ tiếng Việt không dấu hoặc tiếng Anh, nối bằng `-`, mô tả việc chứ không mô tả file. Ví dụ `them-dot-quan-phase2`, `sua-loi-nhat-tre`, `hud-thanh-mau`.
- Chỉ dùng chữ thường, số, `-`, `/`. Tối đa ~50 ký tự.
- Nếu tên đã tồn tại (local hoặc remote), thêm hậu tố `-2`, `-3`.

Ví dụ đúng: `kien/them-ke-dich-cung-thu`, `an-nguyen/fix-camera-giat`.
Ví dụ sai: `main`, `test`, `kien`, `update`, `Kien/Thêm Địch`.

**Một việc = một nhánh.** Nhiều commit nhỏ cho cùng một việc thì commit tiếp trên nhánh đó. Sang việc khác thì quay về `main`, cập nhật, rồi tạo nhánh mới.

## Quy trình commit

```bash
# 1. Kiểm tra trạng thái
git status
git branch --show-current

# 2. Nếu đang ở main (hoặc nhánh của việc khác): tạo nhánh mới từ main mới nhất
git fetch origin main
git switch -c <ten-nguoi>/<viec-da-lam> origin/main
#    Có thay đổi chưa commit thì switch vẫn mang theo; nếu bị chặn, hỏi người dùng có stash không.

# 3. Xem lại những gì sẽ commit, chỉ add file liên quan tới việc này
git diff --stat
git add <file cụ thể>        # tránh `git add .` / `git add -A`
git diff --cached --stat

# 4. Commit theo định dạng của CLAUDE.md
git commit -m "<loại>: <mô tả ngắn>"

# 5. Push nhánh (KHÔNG BAO GIỜ push main)
git push -u origin <ten-nguoi>/<viec-da-lam>
```

Sau khi push, nhắc người dùng mở Pull Request vào `main` và nhờ một người duyệt.

## Kiểm tra trước khi commit

- Mỗi asset mới/đổi tên/xoá đều đi kèm file `.meta` tương ứng; không có `.meta` mồ côi.
- Không có `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`, `*.csproj`, `*.sln`, file kết quả test.
- File model/ảnh/âm thanh được Git LFS theo dõi (`git lfs status`); không có file > 50 MB ngoài LFS.
- Không có khoá API, mật khẩu, file `.env`.
- Không có thay đổi ngoài ý muốn trong scene chính (`ChienTruong.unity`, `MainMenu.unity`) hoặc `ProjectSettings/`. Nếu có mà không thuộc việc này, hỏi người dùng trước khi add.

## Cập nhật nhánh với main

Ưu tiên `merge` thay vì `rebase` cho người mới:

```bash
git fetch origin main
git merge origin/main
```

Nếu xung đột ở file Unity YAML: dừng, liệt kê file, hỏi người dùng (luật 5).

## Khi lỡ commit vào main ở máy local (chưa push)

```bash
git switch -c <ten-nguoi>/<viec-da-lam>   # nhánh mới giữ các commit
git switch main
git reset --keep origin/main              # đưa main local về như remote, hỏi người dùng trước
```

## Báo lại cho người dùng

Sau mỗi thao tác: tên nhánh, commit vừa tạo (hash ngắn + message), đã push chưa, bước tiếp theo (mở PR).
