# Thánh Gióng – Trận Chiến Phù Đổng

File này là luật chung của cả nhóm và của mọi AI làm việc trong repo. Khi luật ở đây khác với yêu cầu trong một prompt, **hỏi lại người dùng trước khi làm**.

Nguồn sự thật về nội dung game: `Docs/GDD.md`. Nếu một tính năng không có trong GDD, không làm.

## 1. Game là gì (tóm tắt GDD)

- **Thể loại:** 3D Third-Person Arena Combat, một map chiến trường gần làng Phù Đổng.
- **Đồ họa:** Stylized / Low Poly 3D.
- **Thời lượng chơi:** 5–10 phút. **Thời gian làm:** 5 tuần.
- **4 giai đoạn (Phase):**
  1. `Phase1_XuatTran` – làm quen di chuyển, camera, đòn đánh.
  2. `Phase2_RoiSat` – đánh các đợt giặc Ân bằng roi sắt.
  3. `Phase3_RoiGay` – roi sắt gãy, người chơi đến bụi tre và nhổ tre.
  4. `Phase4_TreNga` – đánh đợt cuối bằng tre, thắng, cinematic kết.
- **Bắt buộc có:** WASD, camera góc ba, đòn roi sắt, đòn tre, máu/sát thương, enemy đuổi và đánh, Wave System, tương tác nhổ tre, thắng/thua/chơi lại.
- **Chỉ làm khi còn thời gian (stretch):** né (Dodge), mini‑boss, hiệu ứng đặc biệt.
- **KHÔNG làm (ngoài phạm vi):** ngựa sắt, nhiều map/nhiều màn, open world, góc nhìn isometric, hệ thống nhiệm vụ, skill tree, rèn đồ/thu thập tài nguyên, hạt giống/kỹ năng phụ, lưu game, NPC hội thoại. AI không được tự thêm các mục này, kể cả "chỉ là bản nhỏ".

## 2. Công nghệ (đã chốt, không tự đổi)

| Mục | Giá trị |
| --- | --- |
| Engine | Unity **6000.6.0f1** (ghi trong `ProjectSettings/ProjectVersion.txt`, cả nhóm cài đúng bản này) |
| Render | URP 17.6, chỉ dùng `PC_RPAsset` |
| Input | Input System 1.20, **Active Input Handling = Input System Package (New)**. Cấm `UnityEngine.Input.*` |
| Camera | Cinemachine 3.x (`CinemachineCamera` + Orbital Follow) |
| AI di chuyển | AI Navigation 2.0 (NavMesh + `NavMeshAgent`) |
| Cinematic | Timeline |
| UI | uGUI + TextMeshPro |
| Model ngoài | FBX; GLB qua glTFast 6.x nếu buộc phải dùng |
| Nền tảng build | Windows x64 → `Builds/Windows/` (không commit) |

Thêm, bỏ hoặc nâng cấp package phải được cả nhóm đồng ý và ghi vào `Docs/decisions/`. AI không được sửa `Packages/manifest.json` hay `ProjectSettings/` nếu người dùng không yêu cầu rõ.

## 3. Cấu trúc thư mục

```
Assets/
  _Game/                      # mọi thứ nhóm tự làm nằm ở đây
    Scripts/
      Runtime/                # asmdef: ThanhGiong.Runtime
        Core/                 # GameFlow, PhaseId, event channel, helper chung
        Player/               # di chuyển, máu, input của Gióng
        Combat/               # Health, IDamageable, Hitbox/Hurtbox, DamageInfo
        Weapons/              # WeaponBase, IronWhip, Bamboo
        Enemies/              # EnemyController, state machine, EnemySpawner
        Waves/                # WaveDirector
        Story/                # sự kiện roi gãy, BambooGrove, cinematic trigger
        UI/                   # HUD, Pause, Win/Lose, MainMenu
        Audio/  VFX/
      Editor/                 # asmdef: ThanhGiong.Editor (chỉ tool editor)
    Data/                     # ScriptableObject asset: Player/ Weapons/ Enemies/ Waves/ Phases/
    Prefabs/                  # Player/ Enemies/ Weapons/ Environment/ UI/ VFX/
    Scenes/                   # MainMenu.unity, ChienTruong.unity, Sandbox/<TênNgười>.unity
    Art/                      # Models/ Materials/ Textures/ Animations/
    Audio/                    # Music/ SFX/
    Timeline/
  ThirdParty/                 # asset pack tải về, giữ nguyên, không sửa trực tiếp
  Tests/
    EditMode/                 # asmdef: ThanhGiong.Tests.EditMode
    PlayMode/                 # asmdef: ThanhGiong.Tests.PlayMode
Docs/
  GDD.md  Glossary.md  decisions/  ASSET_CREDITS.md
```

- Không tạo thư mục cấp cao mới trong `Assets/` ngoài 3 thư mục trên.
- Không để script, tài liệu, file `.glb/.fbx` ở thư mục gốc repo.
- Script chỉ dùng để kiểm thử/debug đặt trong `Tests/` hoặc `Scripts/Editor/`, không lẫn vào `Runtime/`.

## 4. Thuật ngữ (dùng thống nhất trong code, GDD, commit)

| Tiếng Việt | Tên trong code |
| --- | --- |
| Thánh Gióng (người chơi) | `Player` / `Giong` |
| Roi sắt | `IronWhip` |
| Tre / tre ngà | `Bamboo` |
| Bụi tre | `BambooGrove` |
| Giặc Ân (lính thường) | `AnSoldier` |
| Tướng giặc / mini‑boss | `AnGeneral` |
| Đợt quân | `Wave` |
| Giai đoạn | `Phase` |
| Sự kiện roi gãy | `WhipBreakEvent` |
| Nhổ tre | `PullBamboo` |

Thêm thuật ngữ mới thì cập nhật `Docs/Glossary.md` trong cùng PR.

## 5. Kiến trúc

- **GameFlow** (`Core/`) là state machine duy nhất điều khiển Phase: `Intro → Phase1 → Phase2 → Phase3 → Phase4 → Victory`, nhánh `Defeat` từ mọi Phase. Chuyển Phase chỉ qua `GameFlow.GoTo(PhaseId)`; hệ thống khác lắng nghe event `PhaseChanged`, không tự đổi Phase.
- **Giao tiếp giữa hệ thống:** C# event hoặc ScriptableObject event channel (`VoidEventChannel`, `IntEventChannel`…). UI chỉ đọc/nghe, không gọi ngược vào gameplay.
- **Tham chiếu:** dùng `[SerializeField] private` gán trong Inspector. Cấm singleton tự chế (`static Instance`) và `Find*`/`FindObjectOfType` ở runtime. Ngoại lệ duy nhất: không có.
- **Sát thương:** mọi thứ nhận đòn implement `IDamageable` và có component `Health`. Vũ khí gây sát thương qua Hitbox (trigger) → `IDamageable.TakeDamage(DamageInfo)`.
- **Vũ khí:** `WeaponBase` + `WeaponData` (SO). Roi sắt và tre là 2 asset dữ liệu + 2 prefab, đổi vũ khí qua `PlayerWeaponHolder.Equip(WeaponData)`.
- **Kẻ địch:** `EnemyController` với state machine nhỏ (`Idle, Chase, Telegraph, Attack, Recover, Hurt, Dead`), chỉ số lấy từ `EnemyData`. Đối tượng địch lấy từ pool.
- **Wave:** `WaveDirector` đọc danh sách `WaveData` của Phase hiện tại, sinh địch qua `EnemySpawner`, báo `WaveCleared` / `PhaseWavesCleared`.
- **Animation chỉ để hiển thị**: logic sát thương/di chuyển không phụ thuộc vào việc clip có chạy hay không (Animation Event chỉ dùng để bật/tắt hitbox, có fallback theo thời gian).

## 6. Quy ước code C#

- Namespace `ThanhGiong.<Thư mục>` (vd `ThanhGiong.Enemies`). Một class chính mỗi file, tên file = tên class.
- PascalCase cho class, method, property, event; `camelCase` cho biến cục bộ; field private dạng `_camelCase`; hằng số `PascalCase`.
- Field hiện ra Inspector: `[SerializeField] private`, không `public` field.
- **Không file nào quá 300 dòng.** Vượt thì tách class. Không dùng `partial` để né giới hạn này.
- Cache `GetComponent` trong `Awake`. Không `Instantiate/Destroy` trong `Update`; địch, VFX, projectile đi qua pool (`UnityEngine.Pool.ObjectPool`).
- Mọi giá trị cân bằng (máu, sát thương, tốc độ, cooldown, số địch) nằm trong ScriptableObject ở `Data/`, không hardcode trong code hay prefab.
- Comment ngắn, tiếng Việt hoặc tiếng Anh đều được, nhưng giải thích **vì sao**, không kể lại code.
- Không để lại `Debug.Log` spam; dùng `[Conditional("UNITY_EDITOR")]` hoặc xoá trước khi merge.

## 7. Scene và prefab

- Scene chính: `MainMenu.unity`, `ChienTruong.unity`. Mỗi lúc **chỉ một người** sửa scene chính; báo trong nhóm trước khi mở để sửa.
- Thử nghiệm trong `Scenes/Sandbox/<TênNgười>.unity`, không commit thay đổi thử nghiệm vào scene chính.
- Làm việc theo prefab: sửa prefab, không sửa instance trong scene rồi quên apply.
- Mọi asset luôn đi kèm file `.meta`; không xoá/đổi tên asset bên ngoài Unity Editor.

## 8. Git

- Mọi thao tác git làm theo skill `git-safe-operations`.
- Nhánh `main` luôn mở được và chạy được. Không commit, không push (kể cả force) vào `main`.
- Mỗi việc một nhánh mới, tên `<ten-nguoi>/<viec-da-lam>` (viết thường, không dấu), ví dụ `kien/them-dot-quan-phase2`. Merge qua Pull Request, cần 1 người duyệt.
- Commit message: `<loại>: <mô tả ngắn>`, loại là `feat | fix | art | data | docs | refactor | test | chore`.
- Model, texture, audio, video đi qua **Git LFS** (xem `.gitattributes`). Không commit file > 50 MB, không commit `Builds/`, `Library/`, file kết quả test.
- AI không tự commit, push, merge hay tạo nhánh nếu người dùng không yêu cầu.

## 9. Giới hạn khi dùng AI

AI **được** làm: viết/sửa script trong `Assets/_Game/Scripts`, tạo ScriptableObject data, viết test, viết tài liệu trong `Docs/`, đề xuất cách dựng prefab/scene.

AI **phải hỏi trước** khi: sửa scene chính, sửa prefab dùng chung (Player, EnemyBase), đổi tên/di chuyển thư mục, xoá file, thay đổi hơn 5 file trong một lượt.

AI **không được**:
- Thêm tính năng ngoài GDD hoặc trong danh sách "KHÔNG làm" ở mục 1.
- Cài package, sửa `ProjectSettings/`, đổi phiên bản Unity.
- Cài thêm bộ agent/skill/hook "studio" hay framework quản lý dự án tự động.
- Tạo script editor tự sinh scene/map/prefab hàng loạt (installer, builder, generator) nếu không được yêu cầu cụ thể.
- Tải hoặc đưa vào repo asset không rõ giấy phép.

Trước khi bắt đầu một việc lớn, chạy skill `kiem-tra-pham-vi` để đối chiếu với GDD.

## 10. Kiểm thử và Definition of Done

Một việc được coi là xong khi:
1. Chạy được trong `ChienTruong.unity` từ đầu Phase liên quan, không có lỗi/cảnh báo đỏ trong Console.
2. Logic thuần (sát thương, chuyển Phase, đếm wave) có EditMode test; luồng chính (thắng/thua) có PlayMode test.
3. Giá trị cân bằng nằm trong SO, không hardcode.
4. Thuật ngữ đúng mục 4, file đúng thư mục mục 3.
5. PR mô tả: làm gì, thử thế nào, ảnh/gif nếu có thay đổi hình ảnh.

Mục tiêu hiệu năng: 60 FPS trên máy yếu nhất của nhóm với tối đa 20 địch cùng lúc.

## 11. Asset và giấy phép

- Chỉ dùng asset CC0 / giấy phép cho phép dùng trong dự án học tập (Kenney, KayKit, Quaternius…) hoặc asset nhóm tự làm/tạo bằng AI mà nhóm có quyền dùng.
- Mỗi asset ngoài ghi một dòng vào `Docs/ASSET_CREDITS.md`: tên, nguồn, giấy phép, người thêm.
- Asset pack giữ nguyên trong `Assets/ThirdParty/<TênPack>/`; muốn chỉnh thì copy sang `_Game/`.

## 12. Skills của dự án (`.agents/skills/`)

| Skill | Dùng khi |
| --- | --- |
| `kiem-tra-pham-vi` | trước một tính năng mới, để đối chiếu với GDD và lịch 5 tuần |
| `tao-ke-dich` | thêm một loại giặc Ân mới |
| `tao-dot-quan` | thêm hoặc chỉnh đợt quân trong một Phase |
| `tao-vu-khi` | thêm/chỉnh vũ khí (roi sắt, tre) |
| `them-su-kien-cot-truyen` | sự kiện kịch bản: roi gãy, nhổ tre, cinematic kết |
| `tao-man-hinh-ui` | HUD, pause, thắng/thua, menu chính |
| `git-safe-operations` | trước mọi lệnh commit, push, merge, rebase, reset, xoá nhánh |

Mỗi skill là một thư mục riêng `.agents/skills/<ten-skill>/SKILL.md`. Đây là bản gốc duy nhất; nếu công cụ AI nào chỉ đọc thư mục khác (vd Claude Code đọc `.claude/skills/`) thì dùng symlink hoặc copy từ `.agents/skills/`, không sửa bản copy.
