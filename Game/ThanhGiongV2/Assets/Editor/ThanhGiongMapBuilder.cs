using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace ThanhGiong.Editor
{
    public class ThanhGiongMapBuilder : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/";
        private const string BAMBOO_PATH = "Assets/Waldemarst/FreeJapaneseGarden/Prefabs/Trees/Bamboo/";
        private const string ARCH_ROOT_PATH = "Assets/Art/Models/Architecture/";
        private const string ARCH_MAT_FOLDER = "Assets/Art/Models/Architecture/Materials";
        private const string CHAPTER1_SCENE_PATH = "Assets/Scenes/ThanhGiong_Chapter1.unity";
        private const string TERRAIN_FOLDER = "Assets/Scenes/TerrainLayers";
        private const string MASTER_TERRAIN_ASSET = "Assets/Scenes/TerrainLayers/ThanhGiong_MasterTerrain.asset";
        private const string TERRAIN_MAT_ASSET = "Assets/Scenes/TerrainLayers/M_ThanhGiong_Terrain_URP.mat";

        // 9 Bộ Asset Kiến Trúc Meshy AI Đông Sơn & Quân Sự Cổ Đại (FBX + PBR Textures)
        private const string ARCH_CAU_GO      = ARCH_ROOT_PATH + "CauGo_SongPhuDong/Meshy_AI_rustic_wooden_river_bridge_20261029_image-to-3d-texture";
        private const string ARCH_CHOI_LO_REN = ARCH_ROOT_PATH + "ChoiLoRen_PhuDong/Meshy_AI_ancient_blacksmith_forge_20261029_image-to-3d-texture";
        private const string ARCH_CONG_DON    = ARCH_ROOT_PATH + "CongDon_GiacAn/Meshy_AI_palisade_fortress_gate_20261029_image-to-3d-texture";
        private const string ARCH_CONG_LANG   = ARCH_ROOT_PATH + "CongLang_PhuDong/Meshy_AI_dong_son_village_gate_20261029_image-to-3d-texture";
        private const string ARCH_DAI_DA_SOC  = ARCH_ROOT_PATH + "DaiDaThieng_NuiSoc/Meshy_AI_sacred_stone_altar_monument_20261029_image-to-3d-texture";
        private const string ARCH_DINH_LANG   = ARCH_ROOT_PATH + "DinhLang_DongSon/Meshy_AI_dong_son_longhouse_20261029_image-to-3d-texture";
        private const string ARCH_LEU_TRAI    = ARCH_ROOT_PATH + "LeuTrai_GiacAn/Meshy_AI_bronze_age_war_tent_20261029_image-to-3d-texture";
        private const string ARCH_LONG_GO     = ARCH_ROOT_PATH + "LongGo_GiamDan/Meshy_AI_wooden_prisoner_cage_20261029_image-to-3d-texture";
        private const string ARCH_THAP_CANH   = ARCH_ROOT_PATH + "ThapCanh_GiacAn/Meshy_AI_wooden_archer_watchtower_20261029_image-to-3d-texture";

        // Kích thước Quy Hoạch Tổng Thể Thung Lũng Phù Đổng 7 Chương (300m x 300m)
        private const float MAP_SIZE = 300f;
        private const float TERRAIN_MAX_HEIGHT = 40f;
        private const float GROUND_Y = 10.0f;
        private const float WATER_Y = 8.6f;
        private const float BRIDGE_Z = 145f;
        private const float SOC_SUMMIT_Y = 21.5f;

        // Nhóm phân khu chuẩn quy hoạch 7 Chương + Hệ thống Cổng Khóa Chuyển Màn trong Hierarchy
        private const string GRP_TERRAIN   = "[0_DIA_HINH_THUNG_LUNG_PHU_DONG]";
        private const string GRP_RIVER     = "[1_SONG_PHU_DONG_CAU_GO_VA_BIEN_CHAN]";
        private const string GRP_CH1_MINE  = "[CHUONG_1_MO_QUANG_SAT_CHAN_NUI]";
        private const string GRP_VILLAGE   = "[CHUONG_2_LANG_PHU_DONG_VA_LO_REN]";
        private const string GRP_CH3_ROAD  = "[CHUONG_3_THON_XOM_VEN_RUNG_GIAI_CUU]";
        private const string GRP_CH4_FORT  = "[CHUONG_4_TIEN_DON_GIAC_AN]";
        private const string GRP_ARENA     = "[CHUONG_5_DAI_CHIEN_TRUONG_BAI_SAY]";
        private const string GRP_BAMBOO    = "[CHUONG_6_RUNG_TRE_NGA_VA_HO_NUOC]";
        private const string GRP_CH7_SOC   = "[CHUONG_7_DINH_NUI_SOC_BAY_VE_TROI]";
        private const string GRP_GATES     = "[HE_THONG_6_CONG_KHOA_CHUYEN_CHUONG]";
        private const string GRP_MOUNTAINS = "[DAY_NUI_BAO_BOC_360_DO]";

        // Tọa độ Hồ Nước Rừng Tre (Đông Bắc - Chương 6, tách biệt hoàn toàn với Đỉnh Núi Sóc)
        private static readonly Vector2 POND_CENTER = new Vector2(162f, 232f);
        private const float POND_RADIUS_X = 11.0f;
        private const float POND_RADIUS_Z = 9.5f;

        // Tọa độ Mỏ Quặng Sắt (Tây Bắc - Chương 1) & Đỉnh Núi Sóc (Cao điểm Đông Bắc - Chương 7)
        private static readonly Vector2 QUARRY_CENTER = new Vector2(72f, 238f);
        private static readonly Vector2 SOC_RAMP_START = new Vector2(226f, 184f);
        private static readonly Vector2 SOC_SUMMIT_CENTER = new Vector2(245f, 246f);

        private struct ExclusionCircle
        {
            public Vector2 center;
            public float radius;
        }

        private static readonly List<ExclusionCircle> s_Exclusions = new List<ExclusionCircle>();
        private static readonly List<Rect> s_ExclusionRects = new List<Rect>();

        // =====================================================================
        // MENU 1: QUY HOẠCH TOÀN BỘ MAP 7 CHƯƠNG LIỀN MẠCH + 6 CỔNG KHÓA TỪNG MÀN
        // =====================================================================
        [MenuItem("Thánh Gióng/1. Quy Hoạch Toàn Bộ Map 7 Chương & Cổng Khóa Từng Màn (1 Map Liền Mạch)", false, 1)]
        public static void BuildGddBattlefieldMap()
        {
            if (!Application.isBatchMode)
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Quy Hoạch Bản Đồ Thánh Gióng 7 Chương Liền Mạch (300m x 300m)",
                    "Xây dựng toàn bộ 7 Chương trên cùng 1 Map (KHÔNG ĐÈ LÊN NHAU) kèm 6 Cổng Khóa Chuyển Màn (xong màn trước mới mở cổng qua màn sau):\n\n" +
                    "• Chương 1 [Mỏ Quặng Sắt Tây Bắc]: Lối mòn lên vách núi, 3 mỏ quặng sắt & lồng giam dân phu.\n" +
                    "• Chương 2 [Làng Phù Đổng & Lò Rèn]: 5 Nhà sàn sân vườn, Cây Đa, Giếng Cổ, Bến Nước, Xưởng Rèn & Cổng Làng.\n" +
                    "• Chương 3 [Xóm Ven Rừng Bị Cướp Phá]: Qua Cầu Gỗ xuôi Nam cứu dân làng, đấu Tướng Tiên Phong.\n" +
                    "• Chương 4 [Tiền Đồn Giặc Ân]: Rào chông, 2 Tháp Canh cung thủ, Cổng Trại & đấu Tướng Phòng Tuyến.\n" +
                    "• Chương 5 [Đại Chiến Bãi Sậy]: Sàn đấu lớn phẳng (có vách đá/tre chặn đi tắt từ Cầu Gỗ).\n" +
                    "• Chương 6 [Rừng Tre Ngà & Hồ Nước]: Bụi Tre Ngà vàng óng, rừng tre xanh & đầm nước.\n" +
                    "• Chương 7 [Đỉnh Núi Sóc]: Dốc cỏ xanh cao 21.5m + Phiến đá cởi Giáp Sắt bay về trời!\n\n" +
                    "Bạn có muốn quy hoạch và dựng ngay toàn bộ 7 Chương không?",
                    "Quy Hoạch 7 Chương Ngay", "Hủy");

                if (!confirm) return;
            }

            Random.InitState(20261029);

            // 1. Đồng bộ 9 bộ kiến trúc mới từ thư mục Assets gốc, dọn vật liệu trùng & cập nhật Shader URP
            SyncExternalArchitectureAssets();
            CleanRedundantModelMaterials();
            FixBambooAssets();
            ForceReimportMeshyModels();

            // 2. Mở Scene đích và dọn sạch 100% vật thể/núi cũ gây đè (chỉ giữ Camera, Đèn Mặt Trời, Global Volume)
            Material existingTerrainMat = PrepareCleanChapterScene();

            // 3. Điêu khắc Địa Hình 300m x 300m cho đủ 7 Chương (Lòng sông, Hồ tre, Thềm Mỏ Sắt Chương 1, Dốc cỏ Đỉnh Núi Sóc Chương 7)
            Terrain masterTerrain = BuildMasterTerrain(existingTerrainMat);

            // 4. Đăng ký các vùng cấm đè
            ResetExclusionZones();

            // 5. Xây dựng tuần tự Hệ thống Sông/Biên chặn + 7 Chương độc lập + 6 Cổng Khóa Chuyển Chương + Núi 360°
            BuildZone1_RiverBridgeAndBarriers(masterTerrain);
            BuildChapter1_IronOreQuarry(masterTerrain);
            BuildZone2_PhuDongVillage(masterTerrain);
            BuildChapter3_RaidedHamletRoad(masterTerrain);
            BuildChapter4_AnInvaderFortress(masterTerrain);
            BuildChapter5_BaiSayBattleArena(masterTerrain);
            BuildZone3_SacredBambooAndPond(masterTerrain);
            BuildChapter7_SocMountainSummit(masterTerrain);
            BuildAllChapterProgressionGates(masterTerrain);
            BuildZone5_PerimeterMountains(masterTerrain);

            // 6. Cài đặt Ánh sáng, Camera Game & đưa ngay Camera Scene View tới góc nhìn toàn cảnh 7 Chương
            SetupCameraAndLighting();
            FocusSceneViewOverview();

            // 7. Lưu Scene & Assets
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Quy Hoạch Bản Đồ 7 Chương & 9 Kiến Trúc Mới Thành Công!",
                    "Đã tích hợp trọn bộ 9 Kiến Trúc Meshy AI mới vào 7 Chương liền mạch (300m x 300m, không đè lên nhau):\n\n" +
                    "✓ Chương 1: Mỏ Quặng Sắt Tây Bắc (Lều trinh sát Giặc Ân + Lồng Gỗ nhốt dân phu + 3 mỏ sắt)\n" +
                    "✓ Chương 2: Làng Phù Đổng (Đình Làng Đông Sơn lớn, Chòi Lò Rèn, Cổng Làng Cổ, 5 Nhà sàn sân vườn, Cây Đa, Giếng Cổ)\n" +
                    "✓ Sông Phù Đổng: Cầu Gỗ Cổ Truyền mới khớp chuẩn cao độ hai bờ\n" +
                    "✓ Chương 3: Xóm ven rừng bị cướp phá (2 Lồng Gỗ cứu dân + Lều Tiên Phong + Cổng Đồn khóa màn)\n" +
                    "✓ Chương 4: Tiền Đồn Giặc Ân (2 Tháp Canh Gỗ cao 8.4m, 3 Lều Doanh Trại, Cổng Đồn cọc nhọn trước & sau)\n" +
                    "✓ Chương 5: Đại Chiến Bãi Sậy (Sàn đấu phẳng rộng + Tháp Canh & Lều Chỉ Huy tuyến Đông + Cổng khóa Bắc)\n" +
                    "✓ Chương 6 & 7: Rừng Tre Ngà, Hồ Nước & Đài Đá Thiêng trên Đỉnh Núi Sóc cao 21.5m!",
                    "Khám Phá 7 Chương Ngay!");
            }

            Debug.Log("[ThanhGiong] Đã tích hợp 9 Kiến Trúc Meshy AI mới & quy hoạch hoàn chỉnh Map 7 Chương thành công!");
        }

        // =====================================================================
        // MENU 2: LÀM MỚI RIÊNG KHU LÀNG PHÙ ĐỔNG CHI TIẾT, CÂY CỐI & NHÂN VẬT
        // =====================================================================
        [MenuItem("Thánh Gióng/2. Làm Mới Khu Làng Phù Đổng Chi Tiết & Cây Cối (Nhanh - Giữ Nguyên Map)", false, 2)]
        public static void AddMeshyAIModelsToCurrentScene()
        {
            GameObject masterTerrainGroup = GameObject.Find(GRP_TERRAIN);
            Terrain activeTerrain = Terrain.activeTerrain;

            // Nếu người dùng chưa chạy Quy hoạch 7 Chương mới (thiếu khu Mỏ Quặng Chương 1 hoặc dùng địa hình cũ), gợi ý chạy Menu 1 luôn
            if (masterTerrainGroup == null || activeTerrain == null || activeTerrain.terrainData.size.x < 250f || GameObject.Find(GRP_CH1_MINE) == null)
            {
                if (!Application.isBatchMode)
                {
                    bool runFull = EditorUtility.DisplayDialog(
                        "Map Hiện Tại Chưa Lên Quy Hoạch Đủ 7 Chương",
                        "Scene hiện tại chưa có đủ 7 Chương và Hệ thống 6 Cổng Khóa Chuyển Màn mới.\n\n" +
                        "Bạn có muốn chạy Quy Hoạch Toàn Bộ Map 7 Chương (300m x 300m liền mạch, không đè lên nhau) ngay bây giờ không?",
                        "Quy Hoạch 7 Chương Ngay", "Hủy");
                    if (runFull)
                    {
                        BuildGddBattlefieldMap();
                    }
                }
                return;
            }

            Random.InitState(20261029);
            SyncExternalArchitectureAssets();
            CleanRedundantModelMaterials();
            FixBambooAssets();
            ForceReimportMeshyModels();
            ResetExclusionZones();

            // Cập nhật lại hệ thống đường mòn ngõ xóm, lối ra Giếng Làng, Cây Đa & Bến Nước trên Terrain
            if (activeTerrain.terrainData != null)
            {
                PaintMasterTerrainAlphamap(activeTerrain.terrainData);
                EditorUtility.SetDirty(activeTerrain.terrainData);
                activeTerrain.Flush();
            }

            // Nếu Cầu Gỗ và kiến trúc các chương khác đang là bản cũ chưa bọc Upright Wrapper, tự động dựng thẳng lại đồng bộ toàn map (~1 giây)
            GameObject existingBridge = GameObject.Find("CauGo_SongPhuDong");
            bool needsFullArchRefresh = (existingBridge != null && existingBridge.transform.childCount == 0);
            if (needsFullArchRefresh)
            {
                DestroyIfExists(GRP_RIVER);
                DestroyIfExists(GRP_CH1_MINE);
                DestroyIfExists(GRP_CH3_ROAD);
                DestroyIfExists(GRP_CH4_FORT);
                DestroyIfExists(GRP_ARENA);
                DestroyIfExists(GRP_CH7_SOC);
                DestroyIfExists(GRP_GATES);

                BuildZone1_RiverBridgeAndBarriers(activeTerrain);
                BuildChapter1_IronOreQuarry(activeTerrain);
            }

            DestroyIfExists("[2_KHU_LANG_PHU_DONG]");
            DestroyIfExists(GRP_VILLAGE);
            BuildZone2_PhuDongVillage(activeTerrain);

            if (needsFullArchRefresh)
            {
                BuildChapter3_RaidedHamletRoad(activeTerrain);
                BuildChapter4_AnInvaderFortress(activeTerrain);
                BuildChapter5_BaiSayBattleArena(activeTerrain);
                BuildChapter7_SocMountainSummit(activeTerrain);
                BuildAllChapterProgressionGates(activeTerrain);
            }

            FocusVillageZone();

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Đã Nâng Cấp Chi Tiết & Kiến Trúc Làng Phù Đổng!",
                    "Đã tích hợp các kiến trúc Đông Sơn mới và cây cối làng Việt cổ vào [CHUONG_2_LANG_PHU_DONG_VA_LO_REN] (không đè lên nhau):\n\n" +
                    "✓ Đình Làng Đông Sơn lớn uy nghi hướng mặt ra Sân Đình trung tâm\n" +
                    "✓ Chòi Lò Rèn Vũ Khí có lò lửa & đe sắt ngay đầu lối lên Mỏ Quặng Sắt\n" +
                    "✓ Cổng Làng Cổ Đông Sơn vòm thoáng rộng 5.2m đón thẳng ra Cầu Gỗ\n" +
                    "✓ 5 Khuôn viên Nhà Sàn sân vườn riêng, Cây Đa Cổ Thụ, Giếng Đá Cổ, Bến Nước & Lũy Tre 2 lớp!",
                    "Xem Ngôi Làng Ngay!");
            }

            Debug.Log("[ThanhGiong] Đã làm mới chi tiết Khu Làng Phù Đổng (Cây đa, Giếng nước, Cổng làng, Vườn nhà sàn, Bến nước & Lũy tre)!");
        }

        // =====================================================================
        // MENU 3: SỬA LỖI MÀU TRE URP & LÀM MỚI RỪNG TRE NƯỚC
        // =====================================================================
        [MenuItem("Thánh Gióng/3. Sửa Lỗi Màu Tre URP & Làm Mới Khu Rừng Tre Nước", false, 3)]
        public static void FixBambooAssets()
        {
            Shader speedTreeShader = Shader.Find("Universal Render Pipeline/Nature/SpeedTree8");
            Shader billboardShader = Shader.Find("Universal Render Pipeline/Nature/SpeedTree7 Billboard");
            if (speedTreeShader == null) speedTreeShader = Shader.Find("Universal Render Pipeline/Lit");
            if (billboardShader == null) billboardShader = speedTreeShader;

            Color greenBarkColor = new Color(0.38f, 0.72f, 0.25f, 1f);
            Color greenLeavesColor = new Color(0.35f, 0.78f, 0.22f, 1f);
            Color goldenBarkColor = new Color(0.95f, 0.82f, 0.30f, 1f);
            Color goldenLeavesColor = new Color(0.90f, 0.76f, 0.28f, 1f);

            UpdateBambooMaterial("Assets/Waldemarst/FreeJapaneseGarden/Materials/Trees/Bamboo/M_FJG_Tree_Bamboo_Bark_Green.mat",
                speedTreeShader, greenBarkColor, false);
            UpdateBambooMaterial("Assets/Waldemarst/FreeJapaneseGarden/Materials/Trees/Bamboo/M_FJG_Tree_Bamboo_Sprout_Green.mat",
                speedTreeShader, greenLeavesColor, true);
            UpdateBambooMaterial("Assets/Waldemarst/FreeJapaneseGarden/Materials/Trees/Bamboo/M_FJG_Tree_Bamboo_Bark_Dry.mat",
                speedTreeShader, goldenBarkColor, false);
            UpdateBambooMaterial("Assets/Waldemarst/FreeJapaneseGarden/Materials/Trees/Bamboo/M_FJG_Tree_Bamboo_Sprout_Dry.mat",
                speedTreeShader, goldenLeavesColor, true);

            string[] billboardGuids = AssetDatabase.FindAssets("billboard_material t:Material", new[] { "Assets/Waldemarst/FreeJapaneseGarden/Prefabs/Trees/Bamboo" });
            foreach (string guid in billboardGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    mat.shader = billboardShader;
                    bool isDry = path.Contains("Dry");
                    Color tint = isDry ? goldenLeavesColor : greenLeavesColor;
                    mat.color = tint;
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
                    if (mat.HasProperty("_HueVariation")) mat.SetColor("_HueVariation", new Color(1f, 0.5f, 0f, 0.05f));
                    if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.33f);
                    EditorUtility.SetDirty(mat);
                }
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Waldemarst/FreeJapaneseGarden/Prefabs/Trees/Bamboo" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject root = scope.prefabContentsRoot;
                    LODGroup lodGroup = root.GetComponent<LODGroup>();
                    if (lodGroup != null)
                    {
                        LOD[] lods = lodGroup.GetLODs();
                        if (lods.Length >= 4)
                        {
                            lods[0].screenRelativeTransitionHeight = 0.55f;
                            lods[1].screenRelativeTransitionHeight = 0.25f;
                            lods[2].screenRelativeTransitionHeight = 0.04f;
                            lods[3].screenRelativeTransitionHeight = 0.005f;
                            lodGroup.SetLODs(lods);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[ThanhGiong] Đã chuẩn hóa Shader URP & LOD cho toàn bộ Tre!");
        }

        // =====================================================================
        // MENU 4 & GÓC NHÌN NHANH TỚI TỪNG CHƯƠNG (1 -> 7) TRONG SCENE VIEW
        // =====================================================================
        [MenuItem("Thánh Gióng/4. Đưa Camera Scene Tới Toàn Cảnh Thung Lũng 7 Chương", false, 4)]
        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/0. Toàn Cảnh Bản Đồ 7 Chương", false, 20)]
        public static void FocusSceneViewOverview()
        {
            FocusSceneViewAt(new Vector3(145f, GROUND_Y, 150f), Quaternion.Euler(40f, 8f, 0f), 135f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 1. Mỏ Quặng Sắt Chân Núi (Tây Bắc)", false, 21)]
        public static void FocusChapter1QuarryZone()
        {
            FocusSceneViewAt(new Vector3(72f, GROUND_Y + 2f, 236f), Quaternion.Euler(28f, -15f, 0f), 38f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 2. Làng Phù Đổng & Xưởng Rèn (Bờ Tây)", false, 22)]
        public static void FocusVillageZone()
        {
            FocusSceneViewAt(new Vector3(76f, GROUND_Y + 2f, 145f), Quaternion.Euler(28f, -62f, 0f), 54f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 2b. Cận Cảnh Cổng Làng - Cây Đa - Giếng Cổ", false, 23)]
        public static void FocusVillageLandmarksZone()
        {
            FocusSceneViewAt(new Vector3(92f, GROUND_Y + 1.5f, 145f), Quaternion.Euler(24f, -72f, 0f), 32f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 3. Đường Xuất Trận & Xóm Ven Rừng (Đông Nam)", false, 24)]
        public static void FocusChapter3HamletZone()
        {
            FocusSceneViewAt(new Vector3(172f, GROUND_Y + 1.5f, 78f), Quaternion.Euler(28f, 35f, 0f), 42f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 4. Tiền Đồn Giặc Ân Kiên Cố (Cực Đông Nam)", false, 25)]
        public static void FocusChapter4FortressZone()
        {
            FocusSceneViewAt(new Vector3(228f, GROUND_Y + 2f, 78f), Quaternion.Euler(30f, 55f, 0f), 40f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 5. Đại Chiến Trường Bãi Sậy (Trung Tâm Đông)", false, 26)]
        public static void FocusArenaZone()
        {
            FocusSceneViewAt(new Vector3(205f, GROUND_Y + 1f, 145f), Quaternion.Euler(32f, 25f, 0f), 54f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 6. Thánh Địa Tre Ngà & Hồ Nước (Đông Bắc)", false, 27)]
        public static void FocusBambooZone()
        {
            FocusSceneViewAt(new Vector3(182f, GROUND_Y + 2f, 212f), Quaternion.Euler(30f, -22f, 0f), 46f);
        }

        [MenuItem("Thánh Gióng/Góc Nhìn Nhanh 7 Chương/Chương 7. Đỉnh Núi Sóc Bay Về Trời (Cao Điểm Đông Bắc)", false, 28)]
        public static void FocusChapter7SocMountainZone()
        {
            FocusSceneViewAt(new Vector3(242f, SOC_SUMMIT_Y - 1f, 236f), Quaternion.Euler(26f, 28f, 0f), 42f);
        }

        private static void FocusSceneViewAt(Vector3 pivot, Quaternion rotation, float size)
        {
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv == null && SceneView.sceneViews.Count > 0)
            {
                sv = SceneView.sceneViews[0] as SceneView;
            }
            if (sv != null)
            {
                sv.LookAt(pivot, rotation, size);
                sv.Repaint();
            }
        }

        // =====================================================================
        // BƯỚC CHUẨN BỊ SCENE SẠCH SẼ (XOÁ 100% NÚI & VẬT THỂ CŨ GÂY ĐÈ)
        // =====================================================================
        private static string ToAbsoluteProjectPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath);
        }

        private static Material PrepareCleanChapterScene()
        {
            EnsureAssetFolder("Assets", "Scenes");

            Scene active = SceneManager.GetActiveScene();
            if (active.path != CHAPTER1_SCENE_PATH)
            {
                if (File.Exists(ToAbsoluteProjectPath(CHAPTER1_SCENE_PATH)))
                {
                    EditorSceneManager.OpenScene(CHAPTER1_SCENE_PATH, OpenSceneMode.Single);
                }
                else
                {
                    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(newScene, CHAPTER1_SCENE_PATH);
                }
            }

            // Lưu lại Material URP chuẩn của Terrain cũ (nếu có) trước khi dọn Scene
            Material capturedTerrainMat = null;
            Terrain oldTerrain = Object.FindAnyObjectByType<Terrain>();
            if (oldTerrain != null && oldTerrain.materialTemplate != null)
            {
                Shader sh = oldTerrain.materialTemplate.shader;
                if (sh != null && sh.name != "Hidden/InternalErrorShader")
                {
                    capturedTerrainMat = oldTerrain.materialTemplate;
                }
            }

            // Chỉ giữ lại Main Camera, Directional Light chính và Global Volume (Post-Processing); xoá toàn bộ núi/cây/nhà cũ
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            Camera keptCam = null;
            Light keptSun = null;

            foreach (GameObject go in roots)
            {
                if (go == null) continue;

                Camera cam = go.GetComponent<Camera>();
                if (cam != null && keptCam == null)
                {
                    keptCam = cam;
                    go.name = "Main Camera";
                    go.tag = "MainCamera";
                    continue;
                }

                Light light = go.GetComponent<Light>();
                if (light != null && light.type == LightType.Directional && keptSun == null)
                {
                    keptSun = light;
                    go.name = "Directional Light";
                    continue;
                }

                // Giữ lại Global Volume / Post Process Volume của URP để ánh sáng đẹp
                if (go.GetComponent("Volume") != null && go.GetComponent<Renderer>() == null)
                {
                    continue;
                }

                Object.DestroyImmediate(go);
            }

            if (keptCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                keptCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            if (keptSun == null)
            {
                GameObject sunGo = new GameObject("Directional Light");
                keptSun = sunGo.AddComponent<Light>();
                keptSun.type = LightType.Directional;
            }

            return capturedTerrainMat;
        }

        // =====================================================================
        // ĐỊA HÌNH THUNG LŨNG MASTER 300m x 300m (SÔNG, HỒ TRE, ĐƯỜNG LÀNG, ĐẤU TRƯỜNG)
        // =====================================================================
        private static float GetRiverCenterX(float worldZ)
        {
            // Tại BRIDGE_Z (145m), Sin(0) = 0 -> sông thẳng góc hoàn hảo với Cầu Gỗ tại X = 126m
            return 126f + 6.5f * Mathf.Sin((worldZ - BRIDGE_Z) * 0.032f);
        }

        private static float GetPondNormalizedDist(float worldX, float worldZ)
        {
            float dx = (worldX - POND_CENTER.x) / POND_RADIUS_X;
            float dz = (worldZ - POND_CENTER.y) / POND_RADIUS_Z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static float GetIronQuarryHeightBoost(float worldX, float worldZ)
        {
            if (worldX < 40f || worldX > 108f || worldZ < 200f || worldZ > 268f) return 0f;
            Vector2 p = new Vector2(worldX, worldZ);

            // Thềm sàn Mỏ Quặng Sắt phẳng cao +1.6m (Y = 11.6m) nối dốc thoai thoải từ cổng Bắc làng (68, 200)
            float basin = SoftEllipseMask(p, QUARRY_CENTER, 19.5f, 14.5f, 7.5f) * 1.6f;
            Vector2 trailA = new Vector2(68f, 200f);
            Vector2 trailB = new Vector2(71f, 228f);
            Vector2 ab = trailB - trailA;
            float t = Mathf.Clamp01(Vector2.Dot(p - trailA, ab) / Mathf.Max(0.01f, ab.sqrMagnitude));
            float dLat = Vector2.Distance(p, trailA + ab * t);
            float trailCross = dLat <= 4.5f ? 1f : (dLat < 11f ? 1f - Mathf.SmoothStep(0f, 1f, (dLat - 4.5f) / 6.5f) : 0f);
            float ramp = Mathf.SmoothStep(0f, 1f, t) * 1.6f * trailCross;

            // Vách núi đá vôi dựng cao phía sau lưng mỏ quặng (Z = 249..265)
            float backCliff = 0f;
            if (worldZ > 249f && worldX > 46f && worldX < 98f)
            {
                float xMask = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Mathf.Abs(worldX - 72f) - 18f) / 8f));
                float zRise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((worldZ - 249f) / 14f));
                backCliff = zRise * 7.2f * xMask;
            }

            return Mathf.Max(basin, ramp) + backCliff;
        }

        private static float GetSocMountainHeightBoost(float worldX, float worldZ)
        {
            if (worldX < 214f || worldZ < 176f) return 0f;
            Vector2 p = new Vector2(worldX, worldZ);
            float peakBoost = SOC_SUMMIT_Y - GROUND_Y; // +11.5m (Đỉnh Núi Sóc ở cao độ Y = 21.5m)

            // 1. Đỉnh Núi Sóc phẳng hình tròn đường kính 21m (r = 10.5m) để đặt Đài Đá Thiêng Cởi Giáp & Ngựa Sắt bay về trời
            float dSummit = Vector2.Distance(p, SOC_SUMMIT_CENTER);
            float summitBoost = 0f;
            if (dSummit <= 10.5f)
            {
                summitBoost = peakBoost;
            }
            else if (dSummit < 26.5f)
            {
                summitBoost = peakBoost * (1f - Mathf.SmoothStep(0f, 1f, (dSummit - 10.5f) / 16.0f));
            }

            // 2. Con dốc cỏ xanh thoai thoải rộng 10m (góc dốc êm ~11 độ, nhân vật đi bộ/cưỡi ngựa mượt mà) từ (226, 184) lên (245, 246)
            Vector2 ab = SOC_SUMMIT_CENTER - SOC_RAMP_START;
            float t = Mathf.Clamp01(Vector2.Dot(p - SOC_RAMP_START, ab) / Mathf.Max(0.01f, ab.sqrMagnitude));
            float dLat = Vector2.Distance(p, SOC_RAMP_START + ab * t);
            float spineBoost = peakBoost * Mathf.SmoothStep(0f, 1f, t);

            float cross = 0f;
            if (dLat <= 5.0f)
            {
                cross = 1f;
            }
            else if (dLat < 15.5f)
            {
                cross = 1f - Mathf.SmoothStep(0f, 1f, (dLat - 5.0f) / 10.5f);
            }

            return Mathf.Max(summitBoost, spineBoost * cross);
        }

        private static Terrain BuildMasterTerrain(Material referenceTerrainMat)
        {
            EnsureAssetFolder("Assets", "Scenes");
            EnsureAssetFolder("Assets/Scenes", "TerrainLayers");

            TerrainData td = AssetDatabase.LoadAssetAtPath<TerrainData>(MASTER_TERRAIN_ASSET);
            bool isNewTd = false;
            if (td == null)
            {
                td = new TerrainData();
                isNewTd = true;
            }

            int hRes = 513;
            int aRes = 512;

            // QUAN TRỌNG: Phải đặt heightmapResolution TRƯỚC khi đặt td.size trong Unity
            td.heightmapResolution = hRes;
            td.size = new Vector3(MAP_SIZE, TERRAIN_MAX_HEIGHT, MAP_SIZE);
            td.alphamapResolution = aRes;
            td.baseMapResolution = 512;
            td.SetDetailResolution(256, 16);

            // Tạo 4 TerrainLayer (1 RGBA Splatmap Pass chuẩn URP): Cỏ Xanh - Đất Nện Đường/Đấu Trường - Cát Ven Sông - Đá Chân Núi
            TerrainLayer tlGrass = GetOrCreateStylizedLayer("TL_Grass", "Tex_Grass",
                new Color(0.29f, 0.54f, 0.23f), new Color(0.34f, 0.59f, 0.26f), 14f);
            TerrainLayer tlDirt = GetOrCreateStylizedLayer("TL_DirtRoad", "Tex_DirtRoad",
                new Color(0.62f, 0.49f, 0.34f), new Color(0.57f, 0.44f, 0.30f), 10f);
            TerrainLayer tlSand = GetOrCreateStylizedLayer("TL_RiverSand", "Tex_RiverSand",
                new Color(0.76f, 0.69f, 0.50f), new Color(0.70f, 0.63f, 0.45f), 10f);
            TerrainLayer tlRock = GetOrCreateStylizedLayer("TL_MountainRock", "Tex_MountainRock",
                new Color(0.45f, 0.46f, 0.44f), new Color(0.39f, 0.40f, 0.38f), 16f);

            td.terrainLayers = new TerrainLayer[] { tlGrass, tlDirt, tlSand, tlRock };

            // 1. Điêu khắc Heightmap 7 Chương [zIdx, xIdx]
            float[,] heights = new float[hRes, hRes];
            for (int zIdx = 0; zIdx < hRes; zIdx++)
            {
                float worldZ = ((float)zIdx / (hRes - 1)) * MAP_SIZE;
                float riverCx = GetRiverCenterX(worldZ);

                for (int xIdx = 0; xIdx < hRes; xIdx++)
                {
                    float worldX = ((float)xIdx / (hRes - 1)) * MAP_SIZE;
                    float h = GROUND_Y;

                    // A. Đào lòng Sông Phù Đổng chảy dọc Bắc - Nam (đáy Y = 5.6m, mặt nước Y = 8.6m, bờ đất phẳng Y = 10.0m)
                    float distRiver = Mathf.Abs(worldX - riverCx);
                    if (distRiver < 14.5f)
                    {
                        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distRiver - 7.5f) / 7.0f));
                        h = Mathf.Lerp(5.6f, GROUND_Y, t);
                    }

                    // B. Đào lòng Hồ Nước Rừng Tre (Chương 6) tại (162, 232)
                    float pondR = GetPondNormalizedDist(worldX, worldZ);
                    if (pondR < 1.0f)
                    {
                        float tp = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((pondR - 0.52f) / 0.48f));
                        float pondH = Mathf.Lerp(6.2f, GROUND_Y, tp);
                        h = Mathf.Min(h, pondH);
                    }

                    // C. Điêu khắc Thềm Mỏ Quặng Sắt & Vách Núi Vôi (Chương 1 - Tây Bắc)
                    if (distRiver > 15.5f)
                    {
                        h += GetIronQuarryHeightBoost(worldX, worldZ);
                    }

                    // D. Điêu khắc Con Dốc Cỏ Xanh & Đỉnh Núi Sóc cao 21.5m (Chương 7 - Cao điểm Đông Bắc)
                    h += GetSocMountainHeightBoost(worldX, worldZ);

                    // E. Nâng nhẹ sườn đồi ngoại vi sát mép bản đồ (< 25m từ rìa ngoài cùng, trừ cửa sông Bắc/Nam)
                    float edgeDist = Mathf.Min(
                        Mathf.Min(worldX, MAP_SIZE - worldX),
                        Mathf.Min(worldZ, MAP_SIZE - worldZ));

                    if (edgeDist < 25f && distRiver > 16f)
                    {
                        float fe = Mathf.SmoothStep(0f, 1f, (25f - edgeDist) / 25f);
                        float ridgeWave = 0.85f + 0.30f * Mathf.Sin(worldX * 0.08f) * Mathf.Cos(worldZ * 0.08f);
                        float riverFade = Mathf.Clamp01((distRiver - 16f) / 10f);
                        h += fe * fe * 10.5f * ridgeWave * riverFade;
                    }

                    heights[zIdx, xIdx] = Mathf.Clamp01(h / TERRAIN_MAX_HEIGHT);
                }
            }
            td.SetHeights(0, 0, heights);

            // 2. Sơn phân vùng mặt đất Alphamap cho toàn bộ 7 Chương
            PaintMasterTerrainAlphamap(td);

            if (isNewTd)
            {
                AssetDatabase.CreateAsset(td, MASTER_TERRAIN_ASSET);
            }
            else
            {
                EditorUtility.SetDirty(td);
            }

            GameObject terrainGroup = CreateGroup(GRP_TERRAIN, null);
            GameObject terrainGo = Terrain.CreateTerrainGameObject(td);
            terrainGo.name = "Terrain_Master_300x300";
            terrainGo.transform.SetParent(terrainGroup.transform, false);
            terrainGo.transform.position = Vector3.zero;

            Terrain terrain = terrainGo.GetComponent<Terrain>();
            Material urpTerrainMat = GetOrCreateUrpTerrainMaterial(referenceTerrainMat);
            if (urpTerrainMat != null)
            {
                terrain.materialTemplate = urpTerrainMat;
            }
            terrain.drawInstanced = true;
            terrain.Flush();
            Physics.SyncTransforms();

            return terrain;
        }

        private static void PaintMasterTerrainAlphamap(TerrainData td)
        {
            if (td == null || td.alphamapLayers < 4) return;
            int aRes = td.alphamapResolution;
            float[,,] splat = new float[aRes, aRes, 4];

            for (int zIdx = 0; zIdx < aRes; zIdx++)
            {
                float worldZ = ((zIdx + 0.5f) / aRes) * MAP_SIZE;
                float riverCx = GetRiverCenterX(worldZ);

                for (int xIdx = 0; xIdx < aRes; xIdx++)
                {
                    float worldX = ((xIdx + 0.5f) / aRes) * MAP_SIZE;
                    Vector2 p = new Vector2(worldX, worldZ);

                    float wGrass = 1f;
                    float wDirt = 0f;
                    float wSand = 0f;
                    float wRock = 0f;

                    // 1. Vùng cát ven sông Phù Đổng & bờ Hồ Rừng Tre (Chương 6)
                    float distRiver = Mathf.Abs(worldX - riverCx);
                    if (distRiver < 17.5f)
                    {
                        wSand = Mathf.Max(wSand, 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distRiver - 13.0f) / 4.5f)));
                    }

                    float pondR = GetPondNormalizedDist(worldX, worldZ);
                    if (pondR < 1.18f)
                    {
                        float ps = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((pondR - 0.88f) / 0.30f));
                        wSand = Mathf.Max(wSand, ps);
                    }

                    // 2. Đường mòn Chương 1 (Từ Chòi Lò Rèn lên Mỏ Quặng Sắt Tây Bắc)
                    float roadToQuarry1 = SoftSegmentMask(p, new Vector2(74.5f, 163.5f), new Vector2(68f, 200f), 2.8f, 1.8f);
                    float roadToQuarry2 = SoftSegmentMask(p, new Vector2(68f, 200f), new Vector2(72f, 232f), 3.4f, 2.0f);

                    // 3. Đường làng, Đình Làng & Sân Đình Chương 2 (Làng Phù Đổng, Đình Làng Đông Sơn, Chòi Lò Rèn, 5 Ngõ Nhà Sàn, Cây Đa, Giếng Nước -> Bến Nước)
                    float villageYard = SoftEllipseMask(p, new Vector2(82f, 145f), 16.5f, 17.5f, 5.0f);
                    float dinhLangForecourt = SoftEllipseMask(p, new Vector2(76f, 132f), 9.5f, 7.5f, 3.5f);
                    float roadVillageToBridge = SoftSegmentMask(p, new Vector2(55f, 145f), new Vector2(113f, 145f), 4.2f, 2.2f);
                    float roadHouseNW = SoftSegmentMask(p, new Vector2(58f, 171f), new Vector2(77f, 152f), 2.5f, 1.8f);
                    float roadHouseSW = SoftSegmentMask(p, new Vector2(58f, 119f), new Vector2(75f, 136f), 2.5f, 1.8f);
                    float roadHouseNE = SoftSegmentMask(p, new Vector2(83f, 180f), new Vector2(83f, 156f), 2.5f, 1.8f);
                    float roadHouseSE = SoftSegmentMask(p, new Vector2(83f, 109f), new Vector2(85f, 134f), 2.5f, 1.8f);
                    float roadForge = SoftSegmentMask(p, new Vector2(74.5f, 162.5f), new Vector2(80f, 153f), 3.2f, 1.8f);
                    float roadBanyan = SoftSegmentMask(p, new Vector2(87f, 152f), new Vector2(94.5f, 160.5f), 2.2f, 1.6f);
                    float roadWell = SoftSegmentMask(p, new Vector2(87f, 137f), new Vector2(93.5f, 125f), 2.2f, 1.6f);
                    float roadWaterLanding = SoftSegmentMask(p, new Vector2(93.5f, 125f), new Vector2(107.5f, 123f), 2.0f, 1.5f);

                    // 4. Tuyến đường Chương 3 (Đầu Cầu Đông xuôi Nam xuống Xóm Ven Rừng bị giặc đốt phá)
                    float roadCh3A = SoftSegmentMask(p, new Vector2(139f, 145f), new Vector2(150f, 124f), 4.2f, 2.2f);
                    float roadCh3B = SoftSegmentMask(p, new Vector2(150f, 124f), new Vector2(158f, 94f),  4.0f, 2.2f);
                    float roadCh3C = SoftSegmentMask(p, new Vector2(158f, 94f),  new Vector2(174f, 76f),  4.5f, 2.5f);
                    float ch3HamletYard = SoftEllipseMask(p, new Vector2(175f, 76f), 17f, 15f, 5f) * 0.85f;
                    float roadCh3ToCh4 = SoftSegmentMask(p, new Vector2(182f, 76f), new Vector2(212f, 76f), 4.5f, 2.2f);

                    // 5. Tuyến đường Chương 4 -> Chương 5 (Từ Tiền Đồn Giặc Ân mở cổng hậu tiến lên Đại Chiến Trường Bãi Sậy)
                    float roadCh4ToCh5 = SoftSegmentMask(p, new Vector2(224f, 96f), new Vector2(216f, 126f), 5.2f, 2.5f);
                    float battleArenaFloor = SoftEllipseMask(p, new Vector2(206f, 146f), 32f, 27f, 7f) * 0.90f;

                    // 6. Tuyến đường Chương 5 -> Chương 6 (Lối chạy lên Thánh Địa Tre Ngà) & Chương 6 -> Chương 7 (Lối mòn cỏ lên Đỉnh Núi Sóc)
                    float roadCh5ToBamboo = SoftSegmentMask(p, new Vector2(188f, 168f), new Vector2(188f, 204f), 4.6f, 2.6f);
                    float bambooSanctuaryGlade = SoftEllipseMask(p, new Vector2(188f, 204f), 9.5f, 9.5f, 4f) * 0.85f;
                    float roadToSocGate = SoftSegmentMask(p, new Vector2(206f, 174f), SOC_RAMP_START, 3.6f, 2.2f);
                    float socTrailRibbon = SoftSegmentMask(p, SOC_RAMP_START, SOC_SUMMIT_CENTER, 2.2f, 1.8f) * 0.52f; // Giữ sắc cỏ xanh chủ đạo trên dốc Núi Sóc

                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadToQuarry1, roadToQuarry2));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(villageYard, dinhLangForecourt));
                    wDirt = Mathf.Max(wDirt, roadVillageToBridge);
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadHouseNW, roadHouseSW));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadHouseNE, roadHouseSE));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadForge, roadBanyan));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadWell, roadWaterLanding));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadCh3A, Mathf.Max(roadCh3B, roadCh3C)));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(ch3HamletYard, roadCh3ToCh4));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadCh4ToCh5, battleArenaFloor));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadCh5ToBamboo, bambooSanctuaryGlade));
                    wDirt = Mathf.Max(wDirt, Mathf.Max(roadToSocGate, socTrailRibbon));

                    // Không sơn đường đất đè xuống lòng sông sâu
                    if (distRiver < 12.5f) wDirt = 0f;

                    // 7. Vùng đá xám: Nền Mỏ Quặng Sắt (Chương 1), Nền Tiền Đồn Giặc Ân (Chương 4), Vách Đá Chắn & Chân Núi Biên
                    float quarryRockFloor = SoftEllipseMask(p, QUARRY_CENTER, 21f, 16f, 6f) * 0.88f;
                    float outpostRockFloor = SoftEllipseMask(p, new Vector2(230f, 78f), 22f, 22f, 6f) * 0.80f;
                    float dividerRidgeRock = SoftSegmentMask(p, new Vector2(163f, 116f), new Vector2(163f, 178f), 3.0f, 2.5f) * 0.72f;
                    wRock = Mathf.Max(wRock, Mathf.Max(quarryRockFloor, Mathf.Max(outpostRockFloor, dividerRidgeRock)));

                    // Vách sườn dốc đứng hai bên hông Núi Sóc sơn đá núi tự nhiên, còn mặt đường dốc và đỉnh giữ cỏ xanh mướt
                    if (worldX > 214f && worldZ > 178f)
                    {
                        float dSpine = PointToSegmentDistance(p, SOC_RAMP_START, SOC_SUMMIT_CENTER);
                        float dPeak = Vector2.Distance(p, SOC_SUMMIT_CENTER);
                        if (dSpine > 6.5f && dSpine < 16f && dPeak > 9.5f)
                        {
                            wRock = Mathf.Max(wRock, 0.58f);
                        }
                    }

                    float edgeDist = Mathf.Min(
                        Mathf.Min(worldX, MAP_SIZE - worldX),
                        Mathf.Min(worldZ, MAP_SIZE - worldZ));
                    if (edgeDist < 26f && distRiver > 16f)
                    {
                        float er = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((26f - edgeDist) / 18f));
                        wRock = Mathf.Max(wRock, er);
                    }

                    // Hòa trộn trọng số chuẩn hóa = 1.0
                    float nonGrass = Mathf.Clamp01(Mathf.Max(wSand, Mathf.Max(wDirt, wRock)));
                    wGrass = 1f - nonGrass;

                    if (wSand > 0.01f)
                    {
                        wDirt *= (1f - wSand);
                        wRock *= (1f - wSand);
                    }

                    float sum = wGrass + wDirt + wSand + wRock;
                    if (sum < 0.0001f) { wGrass = 1f; sum = 1f; }

                    splat[zIdx, xIdx, 0] = wGrass / sum;
                    splat[zIdx, xIdx, 1] = wDirt / sum;
                    splat[zIdx, xIdx, 2] = wSand / sum;
                    splat[zIdx, xIdx, 3] = wRock / sum;
                }
            }
            td.SetAlphamaps(0, 0, splat);
        }

        private static Material GetOrCreateUrpTerrainMaterial(Material referenceMat)
        {
            if (referenceMat != null && referenceMat.shader != null && referenceMat.shader.name != "Hidden/InternalErrorShader")
            {
                return referenceMat;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(TERRAIN_MAT_ASSET);
            Shader sh = Shader.Find("Universal Render Pipeline/Terrain/Lit");
            if (sh == null) sh = Shader.Find("Nature/Terrain/Standard");
            if (sh == null) return null;

            if (mat == null)
            {
                mat = new Material(sh);
                mat.name = "M_ThanhGiong_Terrain_URP";
                AssetDatabase.CreateAsset(mat, TERRAIN_MAT_ASSET);
            }
            else
            {
                mat.shader = sh;
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static TerrainLayer GetOrCreateStylizedLayer(string layerName, string texName, Color cA, Color cB, float tileSize)
        {
            string texPath = $"{TERRAIN_FOLDER}/{texName}.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
            {
                int sz = 64;
                Texture2D gen = new Texture2D(sz, sz, TextureFormat.RGBA32, true);
                for (int y = 0; y < sz; y++)
                {
                    for (int x = 0; x < sz; x++)
                    {
                        // Họa tiết hạt mịn Low-Poly tự nhiên, không bị bóng nhựa
                        int bx = x / 4;
                        int by = y / 4;
                        float hash = Mathf.Repeat(Mathf.Sin(bx * 12.9898f + by * 78.233f) * 43758.5453f, 1f);
                        Color col = Color.Lerp(cA, cB, hash);
                        col.a = 0.04f; // Độ nhẵn thấp (matte)
                        gen.SetPixel(x, y, col);
                    }
                }
                gen.Apply();
                File.WriteAllBytes(ToAbsoluteProjectPath(texPath), gen.EncodeToPNG());
                Object.DestroyImmediate(gen);

                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport);
                TextureImporter ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
                if (ti != null)
                {
                    ti.wrapMode = TextureWrapMode.Repeat;
                    ti.filterMode = FilterMode.Bilinear;
                    ti.SaveAndReimport();
                }
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            }

            string layerPath = $"{TERRAIN_FOLDER}/{layerName}.terrainlayer";
            TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
            bool isNew = false;
            if (layer == null)
            {
                layer = new TerrainLayer();
                isNew = true;
            }

            layer.diffuseTexture = tex;
            layer.tileSize = new Vector2(tileSize, tileSize);
            layer.metallic = 0f;
            layer.smoothness = 0.05f;
            layer.specular = Color.black;

            if (isNew)
            {
                AssetDatabase.CreateAsset(layer, layerPath);
            }
            else
            {
                EditorUtility.SetDirty(layer);
            }

            return layer;
        }

        // =====================================================================
        // HỆ THỐNG SÔNG PHÙ ĐỔNG, CẦU GỖ & BIÊN CHẶN KHÔNG CHO LỘI SÔNG ĐI TẮT
        // =====================================================================
        private static void BuildZone1_RiverBridgeAndBarriers(Terrain terrain)
        {
            GameObject zoneRoot = CreateGroup(GRP_RIVER, null);
            GameObject waterGroup = CreateGroup("DONG_SONG_PHU_DONG", zoneRoot);
            GameObject decorGroup = CreateGroup("CANH_QUAN_DOI_BO_SONG", zoneRoot);
            GameObject riverBarrierGroup = CreateGroup("COLLIDER_CHAN_LOI_SONG_DI_TAT", zoneRoot);

            GameObject pWater = Load(PREFAB_PATH + "SM_Water.prefab");
            GameObject pBridge = Load(PREFAB_PATH + "SM_Bridge.prefab");
            GameObject pPebble = Load(PREFAB_PATH + "SM_Pebble_01.prefab");
            GameObject pRock2 = Load(PREFAB_PATH + "SM_Rock_02.prefab");
            GameObject pBush = Load(PREFAB_PATH + "SM_Bush_01.prefab");
            GameObject pSignPost = Load(PREFAB_PATH + "SM_SignPost_01.prefab");
            GameObject pFirefly = FindOptionalPrefab("Firefly");
            List<GameObject> treePrefabs = FindForestTreePrefabs();

            // 1. Dải mặt nước Sông Phù Đổng (Y = 8.6m, nằm gọn trong lòng sông sâu 5.6m)
            if (pWater != null)
            {
                int idx = 0;
                for (float z = 12f; z <= MAP_SIZE - 12f; z += 14f)
                {
                    float cx = GetRiverCenterX(z);
                    float nextCx = GetRiverCenterX(z + 7f);
                    float prevCx = GetRiverCenterX(z - 7f);
                    float tangentAngle = Mathf.Atan2(nextCx - prevCx, 14f) * Mathf.Rad2Deg;

                    GameObject w = Spawn(pWater, waterGroup, new Vector3(cx, WATER_Y, z), new Vector3(0f, tangentAngle, 0f), new Vector3(2.62f, 1f, 1.65f));
                    if (w != null) w.name = $"MatNuoc_Song_{++idx:00}";
                }
            }

            // 2. Cầu Gỗ Cổ Truyền Meshy AI nối Khu Làng Phù Đổng (Chương 2) sang Tuyến Xuất Trận Bờ Đông (Chương 3) tại Z = 145m
            GameObject pMeshyBridge = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_CAU_GO + ".fbx");
            if (pMeshyBridge != null)
            {
                GameObject bridge = SpawnCustomMeshyBridge(pMeshyBridge, zoneRoot, GetRiverCenterX(BRIDGE_Z), BRIDGE_Z, 34.5f);
                if (bridge != null) bridge.name = "CauGo_SongPhuDong";
            }
            else if (pBridge != null)
            {
                GameObject bridge = SpawnAndFitBridge(pBridge, zoneRoot, GetRiverCenterX(BRIDGE_Z), BRIDGE_Z, 33.5f);
                if (bridge != null) bridge.name = "CauGo_VuotSong_PhuDong";
            }

            // 3. Hệ thống Collider trong suốt dọc hai đoạn sông Bắc & Nam Cầu Gỗ (bắt buộc người chơi phải mở Cổng Làng & đi qua Cầu Gỗ!)
            for (float z = 20f; z <= 280f; z += 20f)
            {
                if (Mathf.Abs(z - BRIDGE_Z) < 14f) continue; // Chừa trống hoàn toàn lối đi trên mặt Cầu Gỗ
                float cx = GetRiverCenterX(z);
                CreateInvisibleBlocker($"ChanLoiSong_Z{z:000}", riverBarrierGroup, new Vector3(cx, GROUND_Y + 2f, z), new Vector3(16f, 8f, 22f));
            }

            // Biển chỉ đường ở hai đầu cầu
            if (pSignPost != null)
            {
                GameObject signWest = SpawnGrounded(pSignPost, decorGroup, terrain, new Vector3(107f, GROUND_Y, BRIDGE_Z - 5.5f), new Vector3(0f, 80f, 0f), Vector3.one * 1.25f);
                if (signWest != null) signWest.name = "BienChiDuong_DauCauTay";

                GameObject signEast = SpawnGrounded(pSignPost, decorGroup, terrain, new Vector3(146f, GROUND_Y, BRIDGE_Z - 5.5f), new Vector3(0f, 165f, 0f), Vector3.one * 1.25f);
                if (signEast != null) signEast.name = "BienChiDuong_DauCauDong_ChiXuongChuong3";
            }

            // 4. Đá cuội, bụi cỏ ven sông & cây bóng mát thưa dọc hai bờ
            for (float z = 42f; z <= 252f; z += 16f)
            {
                if (Mathf.Abs(z - BRIDGE_Z) < 22f) continue;

                float cx = GetRiverCenterX(z);

                if (pPebble != null)
                {
                    SpawnGrounded(pPebble, decorGroup, terrain, new Vector3(cx - 13.8f, GROUND_Y, z + 2f), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(1.1f, 1.5f));
                    SpawnGrounded(pPebble, decorGroup, terrain, new Vector3(cx + 13.8f, GROUND_Y, z - 2f), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(1.1f, 1.5f));
                }

                if (pRock2 != null && ((int)z % 32 == 10))
                {
                    SpawnGrounded(pRock2, decorGroup, terrain, new Vector3(cx - 15.2f, GROUND_Y, z - 3f), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.15f);
                }

                if (treePrefabs.Count > 0 && Mathf.Abs(z - BRIDGE_Z) > 34f && ((int)z % 32 == 26))
                {
                    Vector2 candidateWest = new Vector2(cx - 18.5f, z);
                    if (IsClearForDecoration(candidateWest, 5f))
                    {
                        GameObject tp = treePrefabs[Random.Range(0, treePrefabs.Count)];
                        SpawnGrounded(tp, decorGroup, terrain, new Vector3(candidateWest.x, GROUND_Y, candidateWest.y),
                            new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(0.95f, 1.2f));
                    }

                    Vector2 candidateEast = new Vector2(cx + 18.5f, z + 5f);
                    if (IsClearForDecoration(candidateEast, 5f))
                    {
                        GameObject tp = treePrefabs[Random.Range(0, treePrefabs.Count)];
                        SpawnGrounded(tp, decorGroup, terrain, new Vector3(candidateEast.x, GROUND_Y, candidateEast.y),
                            new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(0.95f, 1.2f));
                    }
                }

                if (pBush != null && ((int)z % 16 == 10))
                {
                    Vector2 bushPos = new Vector2(cx - 16.0f, z + 4f);
                    if (IsClearForDecoration(bushPos, 2f))
                    {
                        SpawnGrounded(pBush, decorGroup, terrain, new Vector3(bushPos.x, GROUND_Y, bushPos.y),
                            new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.1f);
                    }
                }
            }

            if (pFirefly != null)
            {
                Spawn(pFirefly, decorGroup, new Vector3(GetRiverCenterX(BRIDGE_Z - 16f), WATER_Y + 1.5f, BRIDGE_Z - 16f), Vector3.zero, Vector3.one);
                Spawn(pFirefly, decorGroup, new Vector3(GetRiverCenterX(BRIDGE_Z + 16f), WATER_Y + 1.5f, BRIDGE_Z + 16f), Vector3.zero, Vector3.one);
            }
        }

        private static GameObject CreateInvisibleBlocker(string name, GameObject parent, Vector3 worldPos, Vector3 boxSize)
        {
            GameObject go = CreateGroup(name, parent);
            go.transform.position = worldPos;
            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.size = boxSize;
            return go;
        }

        private static GameObject SpawnAndFitBridge(GameObject bridgePrefab, GameObject parent, float centerX, float centerZ, float targetSpanX)
        {
            GameObject bridge = Spawn(bridgePrefab, parent, new Vector3(centerX, GROUND_Y, centerZ), Vector3.zero, Vector3.one);
            if (bridge == null) return null;

            if (!TryGetCombinedMeshBounds(bridge, out Bounds b0)) return bridge;
            if (b0.size.x < 0.01f || b0.size.z < 0.01f) return bridge;

            // Xoay trục dài của cầu trùng với trục X (Đông - Tây) để bắc ngang qua dòng sông Bắc - Nam
            bool rotated90 = false;
            if (b0.size.z > b0.size.x)
            {
                bridge.transform.eulerAngles = new Vector3(0f, 90f, 0f);
                rotated90 = true;
            }

            if (!TryGetCombinedMeshBounds(bridge, out Bounds b1)) return bridge;

            float scaleLength = targetSpanX / Mathf.Max(0.1f, b1.size.x);
            float desiredWalkwayWidth = Mathf.Clamp(b1.size.z * scaleLength, 6.8f, 9.2f);
            float scaleWidth = desiredWalkwayWidth / Mathf.Max(0.1f, b1.size.z);
            float scaleHeight = Mathf.Min(scaleLength, scaleWidth) * 0.80f;

            if (rotated90)
            {
                bridge.transform.localScale = new Vector3(scaleWidth, scaleHeight, scaleLength);
            }
            else
            {
                bridge.transform.localScale = new Vector3(scaleLength, scaleHeight, scaleWidth);
            }

            if (TryGetCombinedMeshBounds(bridge, out Bounds b2))
            {
                float shiftX = centerX - b2.center.x;
                float shiftZ = centerZ - b2.center.z;
                bridge.transform.position += new Vector3(shiftX, 0f, shiftZ);

                // Đo chính xác cao độ mặt ván dốc ở hai đầu cầu để khớp phẳng với mặt đường 10.0m, còn trụ cầu tự động cắm xuống lòng sông
                float entryRampY = b2.min.y + 0.35f;
                MeshFilter mf = bridge.GetComponentInChildren<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Vector3[] verts = mf.sharedMesh.vertices;
                    if (verts != null && verts.Length > 0 && TryGetCombinedMeshBounds(bridge, out Bounds bCentered))
                    {
                        float tipBand = bCentered.size.x * 0.08f;
                        float centerWalkwayHalfZ = bCentered.size.z * 0.25f;
                        float minRampTipY = float.MaxValue;
                        int count = 0;

                        Transform tr = mf.transform;
                        for (int i = 0; i < verts.Length; i++)
                        {
                            Vector3 wp = tr.TransformPoint(verts[i]);
                            bool nearCenterWidth = Mathf.Abs(wp.z - centerZ) <= centerWalkwayHalfZ;
                            bool nearEitherTip = (wp.x <= bCentered.min.x + tipBand) || (wp.x >= bCentered.max.x - tipBand);
                            if (nearCenterWidth && nearEitherTip)
                            {
                                if (wp.y < minRampTipY) minRampTipY = wp.y;
                                count++;
                            }
                        }

                        if (count > 0 && minRampTipY < float.MaxValue)
                        {
                            entryRampY = minRampTipY;
                        }
                    }
                }

                float shiftY = (GROUND_Y - 0.05f) - entryRampY;
                bridge.transform.position += new Vector3(0f, shiftY, 0f);
            }

            return bridge;
        }

        private static GameObject SpawnCustomMeshyBridge(GameObject bridgePrefab, GameObject parent, float centerX, float centerZ, float targetSpanX)
        {
            GameObject bridge = CreateUprightArchitectureWrapper(
                bridgePrefab, "CauGo_SongPhuDong", parent, new Vector3(centerX, GROUND_Y, centerZ));
            if (bridge == null) return null;

            SetupArchitecturePbrMaterial(bridge, "M_CauGo_SongPhuDong", ARCH_CAU_GO, 0.04f, 0.22f);

            if (!TryGetCombinedMeshBounds(bridge, out Bounds b0) || b0.size.x < 0.01f || b0.size.z < 0.01f)
                return bridge;

            bool rotated90 = false;
            if (b0.size.z > b0.size.x)
            {
                bridge.transform.eulerAngles = new Vector3(0f, 90f, 0f);
                rotated90 = true;
            }

            if (!TryGetCombinedMeshBounds(bridge, out Bounds b1)) return bridge;

            float desiredWalkwayWidth = 8.4f;
            float desiredTotalHeight = 6.5f;

            float scaleSpan = targetSpanX / Mathf.Max(0.05f, b1.size.x);
            float scaleWidth = desiredWalkwayWidth / Mathf.Max(0.05f, b1.size.z);
            float scaleHeight = desiredTotalHeight / Mathf.Max(0.05f, b1.size.y);

            if (rotated90)
                bridge.transform.localScale = new Vector3(scaleWidth, scaleHeight, scaleSpan);
            else
                bridge.transform.localScale = new Vector3(scaleSpan, scaleHeight, scaleWidth);

            float crownDeltaY = 1.02f;
            if (TryGetCombinedMeshBounds(bridge, out Bounds b2))
            {
                bridge.transform.position += new Vector3(centerX - b2.center.x, 0f, centerZ - b2.center.z);

                if (TryGetCombinedMeshBounds(bridge, out Bounds bCentered))
                {
                    // Tỷ lệ chuẩn mặt ván đầu cầu = 58.8% tổng chiều cao (cọc trụ bên dưới cắm sâu xuống lòng sông), đỉnh vòm giữa cầu = 74.5%
                    float entryDeckTopY = bCentered.min.y + bCentered.size.y * 0.588f;
                    float midDeckTopY = bCentered.min.y + bCentered.size.y * 0.745f;

                    MeshFilter mf = bridge.GetComponentInChildren<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        Vector3[] verts = mf.sharedMesh.vertices;
                        if (verts != null && verts.Length > 0)
                        {
                            int stride = Mathf.Max(1, verts.Length / 4500);
                            float tipBand = bCentered.size.x * 0.06f;
                            float midBand = bCentered.size.x * 0.06f;
                            float centerHalfZ = bCentered.size.z * 0.18f;
                            float measuredTipTop = float.MinValue;
                            float measuredMidTop = float.MinValue;
                            int tipHits = 0, midHits = 0;

                            Transform tr = mf.transform;
                            for (int i = 0; i < verts.Length; i += stride)
                            {
                                Vector3 wp = tr.TransformPoint(verts[i]);
                                if (Mathf.Abs(wp.z - centerZ) > centerHalfZ) continue;

                                if (wp.x <= bCentered.min.x + tipBand || wp.x >= bCentered.max.x - tipBand)
                                {
                                    if (wp.y > measuredTipTop) measuredTipTop = wp.y;
                                    tipHits++;
                                }
                                else if (Mathf.Abs(wp.x - centerX) <= midBand)
                                {
                                    if (wp.y > measuredMidTop) measuredMidTop = wp.y;
                                    midHits++;
                                }
                            }

                            if (tipHits > 3 && measuredTipTop > bCentered.min.y)
                                entryDeckTopY = measuredTipTop;
                            if (midHits > 3 && measuredMidTop > entryDeckTopY)
                                midDeckTopY = measuredMidTop;
                        }
                    }

                    crownDeltaY = Mathf.Clamp(midDeckTopY - entryDeckTopY, 0.45f, 1.45f);
                    float shiftY = (GROUND_Y + 0.04f) - entryDeckTopY;
                    bridge.transform.position += new Vector3(0f, shiftY, 0f);
                }
            }

            // Hệ thống mặt sàn Collider vòm êm ái + 2 thành lan can gỗ để nhân vật & Ngựa Sắt phi qua cầu mượt 100%
            GameObject deckColRoot = CreateGroup("ColliderSanCau_Va_LanCan", parent);
            deckColRoot.transform.position = new Vector3(centerX, GROUND_Y, centerZ);

            float rampSpan = 12.2f;
            float rampPitchDeg = Mathf.Atan2(crownDeltaY, rampSpan) * Mathf.Rad2Deg;
            float rampHyp = Mathf.Sqrt(rampSpan * rampSpan + crownDeltaY * crownDeltaY) + 0.6f;

            // Dốc lên đầu cầu Tây
            GameObject westRamp = CreateGroup("SanDoc_DauCauTay", deckColRoot);
            westRamp.transform.position = new Vector3(centerX - 11.0f, GROUND_Y + crownDeltaY * 0.5f - 0.18f, centerZ);
            westRamp.transform.eulerAngles = new Vector3(0f, 0f, rampPitchDeg);
            westRamp.AddComponent<BoxCollider>().size = new Vector3(rampHyp, 0.40f, 6.4f);

            // Đỉnh vòm giữa cầu
            GameObject midDeck = CreateGroup("SanGiua_VomCau", deckColRoot);
            midDeck.transform.position = new Vector3(centerX, GROUND_Y + crownDeltaY - 0.18f, centerZ);
            midDeck.AddComponent<BoxCollider>().size = new Vector3(10.6f, 0.40f, 6.4f);

            // Dốc xuống đầu cầu Đông
            GameObject eastRamp = CreateGroup("SanDoc_DauCauDong", deckColRoot);
            eastRamp.transform.position = new Vector3(centerX + 11.0f, GROUND_Y + crownDeltaY * 0.5f - 0.18f, centerZ);
            eastRamp.transform.eulerAngles = new Vector3(0f, 0f, -rampPitchDeg);
            eastRamp.AddComponent<BoxCollider>().size = new Vector3(rampHyp, 0.40f, 6.4f);

            // Lan can hai bên thành cầu
            CreateInvisibleBlocker("LanCanCau_Bac", deckColRoot, new Vector3(centerX, GROUND_Y + 1.5f, centerZ + 3.55f), new Vector3(targetSpanX, 3.0f, 0.65f));
            CreateInvisibleBlocker("LanCanCau_Nam", deckColRoot, new Vector3(centerX, GROUND_Y + 1.5f, centerZ - 3.55f), new Vector3(targetSpanX, 3.0f, 0.65f));

            return bridge;
        }

        // =====================================================================
        // CHƯƠNG 1: SỰ THỨC TỈNH & NGUYÊN LIỆU MỎ QUẶNG SẮT (TÂY BẮC, X: 46..98, Z: 204..256)
        // Lối mòn đá sỏi ven suối dẫn lên vách núi vôi, 3 mỏ quặng sắt thô & lồng giam dân phu
        // =====================================================================
        private static void BuildChapter1_IronOreQuarry(Terrain terrain)
        {
            GameObject ch1Root = CreateGroup(GRP_CH1_MINE, null);
            GameObject trailGroup = CreateGroup("1_LOI_MON_SOI_DA_LEN_MO", ch1Root);
            GameObject cliffGroup = CreateGroup("2_VACH_DA_VOI_VA_3_MO_QUANG_SAT", ch1Root);
            GameObject campGroup = CreateGroup("3_TRAI_TRINH_SAT_VA_LONG_DAN_PHU", ch1Root);

            GameObject pRock2 = Load(PREFAB_PATH + "SM_Rock_02.prefab");
            GameObject pRock3 = Load(PREFAB_PATH + "SM_Rock_03.prefab");
            GameObject pRock5 = Load(PREFAB_PATH + "SM_Rock_05.prefab");
            GameObject pRock7 = Load(PREFAB_PATH + "SM_Rock_07.prefab");
            GameObject pPebble = Load(PREFAB_PATH + "SM_Pebble_01.prefab");
            GameObject pSignPost = Load(PREFAB_PATH + "SM_SignPost_01.prefab");
            GameObject pCampfire = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pFence2 = Load(PREFAB_PATH + "SM_Fence_2.prefab");
            GameObject pSpikes = Load(PREFAB_PATH + "SM_Spikes_01.prefab");
            GameObject pStump = Load(PREFAB_PATH + "SM_Stump_01.prefab");
            GameObject pFallenTree = Load(PREFAB_PATH + "SM_FallenTree.prefab");
            GameObject pTree2 = Load(PREFAB_PATH + "SM_Tree_02.prefab");
            GameObject pFir = Load(PREFAB_PATH + "Fir.prefab");
            GameObject pBush = Load(PREFAB_PATH + "SM_Bush_01.prefab");

            // 1. Lối mòn đá sỏi từ cổng Bắc Làng Phù Đổng (68, 200) dẫn lên Mỏ Quặng (72, 232)
            SpawnGrounded(pSignPost, trailGroup, terrain, new Vector3(63.5f, GROUND_Y, 203f), new Vector3(0f, 15f, 0f), Vector3.one * 1.25f);
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Vector2 center = Vector2.Lerp(new Vector2(68f, 204f), new Vector2(71f, 226f), t);
                SpawnGrounded(pPebble, trailGroup, terrain, new Vector3(center.x - 4.2f, GROUND_Y, center.y),
                    new Vector3(0f, i * 45f, 0f), Vector3.one * 1.3f);
                SpawnGrounded(pPebble, trailGroup, terrain, new Vector3(center.x + 4.2f, GROUND_Y, center.y + 1.0f),
                    new Vector3(0f, i * 70f, 0f), Vector3.one * 1.3f);
            }
            SpawnGrounded(pTree2, trailGroup, terrain, new Vector3(58f, GROUND_Y, 214f), new Vector3(0f, 40f, 0f), Vector3.one * 1.15f, -0.08f);
            SpawnGrounded(pFir, trailGroup, terrain, new Vector3(83f, GROUND_Y, 215f), new Vector3(0f, 120f, 0f), Vector3.one * 1.15f, -0.08f);

            // 2. Vách đá mỏ sắt vòng cung phía Bắc - Tây - Đông thềm mỏ (chừa lòng sân giữa rộng 30m x 22m để đánh lính trinh sát)
            Vector2[] quarryPerimeterRocks =
            {
                new Vector2(52f, 236f), new Vector2(54f, 245f), new Vector2(62f, 251f),
                new Vector2(72f, 253f), new Vector2(82f, 251f), new Vector2(90f, 245f), new Vector2(92f, 235f)
            };
            for (int i = 0; i < quarryPerimeterRocks.Length; i++)
            {
                Vector2 rp = quarryPerimeterRocks[i];
                GameObject rPrefab = (i % 2 == 0) ? pRock7 : pRock2;
                SpawnGrounded(rPrefab, cliffGroup, terrain, new Vector3(rp.x, GROUND_Y, rp.y),
                    new Vector3(0f, i * 52f, 0f), Vector3.one * Random.Range(1.45f, 1.95f), -0.25f);
            }

            // 3 MỎ QUẶNG SẮT THU THẬP (Chương 1 yêu cầu thu thập đủ 3 gùi quặng sắt mang về cho Bác Thợ Rèn)
            Material ironOreMat = GetOrCreateIronOreMaterial();
            Vector2[] orePositions =
            {
                new Vector2(58.5f, 243.5f),
                new Vector2(72.0f, 247.5f),
                new Vector2(85.0f, 242.5f)
            };

            for (int i = 0; i < orePositions.Length; i++)
            {
                Vector2 op = orePositions[i];
                GameObject oreNode = CreateGroup($"MoQuangSat_ThuThap_{i + 1:00}", cliffGroup);
                oreNode.transform.position = new Vector3(op.x, SampleTerrainY(terrain, op.x, op.y), op.y);

                GameObject mainOre = SpawnGrounded(pRock5, oreNode, terrain, new Vector3(op.x, GROUND_Y, op.y),
                    new Vector3(0f, i * 110f, 0f), Vector3.one * 1.25f, -0.08f);
                GameObject sideOre = SpawnGrounded(pRock3, oreNode, terrain, new Vector3(op.x + 1.1f, GROUND_Y, op.y - 0.6f),
                    new Vector3(12f, i * 65f + 45f, 0f), Vector3.one * 0.85f, -0.06f);

                ApplySharedMaterialToHierarchy(mainOre, ironOreMat);
                ApplySharedMaterialToHierarchy(sideOre, ironOreMat);

                BoxCollider triggerCol = oreNode.AddComponent<BoxCollider>();
                triggerCol.center = new Vector3(0f, 1.0f, 0f);
                triggerCol.size = new Vector3(3.2f, 2.2f, 3.2f);
                triggerCol.isTrigger = true;
            }

            // 3. Chốt lính trinh sát Giặc Ân (Lều Doanh Trại Meshy AI) & Lồng Gỗ nhốt dân phu mỏ đá
            GameObject pWarTent = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_LEU_TRAI + ".fbx");
            if (pWarTent != null)
            {
                SpawnFittedArchitecture(
                    pWarTent, "LeuTrinhSat_GiacAn_MoQuang", campGroup, terrain,
                    new Vector2(57.8f, 234.0f), 132f, 5.8f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);
            }
            SpawnWoodenPrisonerCage("LongGo_GiamDanPhuMoDa", campGroup, terrain, pFence2, new Vector2(83.5f, 231.5f), -35f);
            SpawnGrounded(pCampfire, campGroup, terrain, new Vector3(62.8f, GROUND_Y, 229.5f), Vector3.zero, Vector3.one * 1.25f, -0.03f);
            SpawnGrounded(pSpikes, campGroup, terrain, new Vector3(60.0f, GROUND_Y, 225.0f), new Vector3(0f, -160f, 0f), Vector3.one * 1.15f, -0.06f);
            SpawnGrounded(pStump, campGroup, terrain, new Vector3(65.5f, GROUND_Y, 228.8f), Vector3.zero, Vector3.one * 1.15f, -0.04f);
            SpawnGrounded(pFallenTree, campGroup, terrain, new Vector3(79.5f, GROUND_Y, 225.5f), new Vector3(0f, -35f, 0f), Vector3.one * 0.95f, -0.05f);
            if (pBush != null)
            {
                SpawnGrounded(pBush, campGroup, terrain, new Vector3(53f, GROUND_Y, 226f), Vector3.zero, Vector3.one * 1.2f);
                SpawnGrounded(pBush, campGroup, terrain, new Vector3(91f, GROUND_Y, 226f), Vector3.zero, Vector3.one * 1.2f);
            }
        }

        private static Material GetOrCreateIronOreMaterial()
        {
            string matPath = $"{TERRAIN_FOLDER}/M_QuangSat_DongSon.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            if (sh == null) return null;

            bool isNew = false;
            if (mat == null)
            {
                mat = new Material(sh);
                mat.name = "M_QuangSat_DongSon";
                isNew = true;
            }
            else
            {
                mat.shader = sh;
            }

            // Sắc thép quặng sắt đen ánh kim pha vệt sáng đỏ rực nhẹ để người chơi nhận diện ngay mục tiêu thu thập
            Color ironBase = new Color(0.18f, 0.21f, 0.26f, 1f);
            mat.color = ironBase;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", ironBase);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.85f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.58f);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.28f, 0.11f, 0.02f, 1f));
            }

            if (isNew) AssetDatabase.CreateAsset(mat, matPath);
            else EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void ApplySharedMaterialToHierarchy(GameObject go, Material mat)
        {
            if (go == null || mat == null) return;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r is MeshRenderer) r.sharedMaterial = mat;
            }
        }

        private static GameObject SpawnWoodenPrisonerCage(string name, GameObject parent, Terrain terrain, GameObject fencePrefab, Vector2 center, float rotY = 0f)
        {
            GameObject pCageFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_LONG_GO + ".fbx");
            if (pCageFbx != null)
            {
                return SpawnFittedArchitecture(
                    pCageFbx, name, parent, terrain,
                    center, rotY, 3.05f, true,
                    "M_LongGo_GiamDan", ARCH_LONG_GO, 0.04f, 0.22f, false, true, -0.04f);
            }

            GameObject cage = CreateGroup(name, parent);
            cage.transform.position = new Vector3(center.x, SampleTerrainY(terrain, center.x, center.y), center.y);

            float half = 1.35f;
            Vector3 wallSpec = new Vector3(2.8f, 1.85f, 0.28f);
            SpawnFittedWallOrSlab(fencePrefab, cage, terrain, new Vector3(center.x, GROUND_Y, center.y + half), 0f, wallSpec);
            SpawnFittedWallOrSlab(fencePrefab, cage, terrain, new Vector3(center.x, GROUND_Y, center.y - half), 0f, wallSpec);
            SpawnFittedWallOrSlab(fencePrefab, cage, terrain, new Vector3(center.x + half, GROUND_Y, center.y), 90f, wallSpec);
            SpawnFittedWallOrSlab(fencePrefab, cage, terrain, new Vector3(center.x - half, GROUND_Y, center.y), 90f, wallSpec);

            BoxCollider col = cage.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.0f, 0f);
            col.size = new Vector3(3.0f, 2.0f, 3.0f);
            return cage;
        }

        // =====================================================================
        // CHƯƠNG 2: LÀNG PHÙ ĐỔNG CHI TIẾT & XƯỞNG RÈN (BỜ TÂY SÔNG, X: 34..110, Z: 88..202)
        // Bao gồm: 5 Nhà sàn + Vườn cây riêng, Cổng Làng, Cây Đa Cổ Thụ & Miếu Thổ Thần,
        // Giếng Đá Cổ, Bến Nước Ven Sông, Sân Đình & Xưởng Rèn, Vườn Cây Hoa Cỏ, Lũy Tre 2 Lớp
        // =====================================================================
        private struct VillageHouseSpec
        {
            public string name;
            public Vector2 pos;
            public float rotY;
            public float scale;
            public bool litHearth;
        }

        private static void BuildZone2_PhuDongVillage(Terrain terrain)
        {
            GameObject villageRoot = CreateGroup(GRP_VILLAGE, null);
            GameObject housesGroup = CreateGroup("1_CUM_NHA_SAN_VA_KHUON_VIEN_VUON", villageRoot);
            GameObject gateBanyanGroup = CreateGroup("2_CONG_LANG_VA_CAY_DA_CO_THU", villageRoot);
            GameObject wellDockGroup = CreateGroup("3_GIENG_LANG_CO_VA_BEN_NUOC", villageRoot);
            GameObject yardForgeGroup = CreateGroup("4_SAN_DINH_VA_XUONG_REN_VU_KHI", villageRoot);
            GameObject charsGroup = CreateGroup("5_NHAN_VAT_LICH_SU", villageRoot);
            GameObject orchardFloraGroup = CreateGroup("6_VUON_CAY_XANH_VA_HOA_CO_LANG", villageRoot);
            GameObject hedgeGroup = CreateGroup("7_LUY_TRE_XANH_BAO_BOC_LANG", villageRoot);

            // Model nhân vật & nhà sàn Meshy AI
            GameObject housePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri.obj");
            GameObject warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Meshy_AI_vietnamese_warrior_compressed_20261027_texture_remesh_tri.obj");
            GameObject horseWarriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Meshy_AI_warrior_riding_horse_compressed_20261027_texture_remesh_tri.obj");
            GameObject envoyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Meshy_AI_Imperial_Envoy_texture_remesh_tri.obj");
            GameObject emperorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Meshy_AI_Sun_Emperor_texture_remesh_tri.obj");

            // Cây thân gỗ tán rộng (Low Poly Forest URP)
            GameObject pTree1 = Load(PREFAB_PATH + "SM_Tree_01.prefab");
            GameObject pTree2 = Load(PREFAB_PATH + "SM_Tree_02.prefab");
            GameObject pTreeGreen = Load(PREFAB_PATH + "SM_Tree_Green.prefab");
            GameObject pTreeColor1 = Load(PREFAB_PATH + "SM_Tree_Color.prefab");
            GameObject pTreeColor2 = Load(PREFAB_PATH + "SM_Tree_Color_2.prefab");
            GameObject[] villageShadeTrees = { pTree1, pTreeGreen, pTree2, pTreeColor1, pTreeColor2 };

            // Bụi cây, dương xỉ, hoa dại, thảm cỏ, nấm & rễ cây cổ thụ
            GameObject pBush0 = Load(PREFAB_PATH + "SM_Bush.prefab");
            GameObject pBush1 = Load(PREFAB_PATH + "SM_Bush_01.prefab");
            GameObject pFoliageBush = Load(PREFAB_PATH + "SM_FoliageBush_01.prefab");
            GameObject pFoliage1 = Load(PREFAB_PATH + "SM_Foliage_01.prefab");
            GameObject pFoliage2 = Load(PREFAB_PATH + "SM_Foliage_02.prefab");
            GameObject pFlower1 = Load(PREFAB_PATH + "SM_Flower_01.prefab");
            GameObject pFlower2 = Load(PREFAB_PATH + "SM_Flower_02.prefab");
            GameObject pGrass1 = Load(PREFAB_PATH + "SM_Grass_01.prefab");
            GameObject pGrass2 = Load(PREFAB_PATH + "SM_Grass_02.prefab");
            GameObject pGrass3 = Load(PREFAB_PATH + "SM_Grass_03.prefab");
            GameObject pGrass4 = Load(PREFAB_PATH + "SM_Grass_04.prefab");
            GameObject pMushroom1 = Load(PREFAB_PATH + "SM_Mushroom_01.prefab");
            GameObject pMushroom2 = Load(PREFAB_PATH + "SM_Mushroom_02.prefab");
            GameObject pRoot1 = Load(PREFAB_PATH + "SM_Root_01.prefab");

            // Kiến trúc làng: Tường đá, hàng rào gỗ, biển làng, bếp lửa, gốc cây, đá tảng, mặt nước, phiến đá bước dạo
            GameObject pStoneWall1 = Load(PREFAB_PATH + "SM_StoneWall_01.prefab");
            GameObject pStoneWall2 = Load(PREFAB_PATH + "SM_StoneWall_02.prefab");
            GameObject pStoneWall3 = Load(PREFAB_PATH + "SM_StoneWall_03.prefab");
            GameObject pPlatform3 = Load(PREFAB_PATH + "SM_Platform_03.prefab");
            GameObject pRuneStone3 = Load(PREFAB_PATH + "SM_RuneStone_03.prefab");
            GameObject pSign1 = Load(PREFAB_PATH + "SM_Sign_01.prefab");
            GameObject pFence1 = Load(PREFAB_PATH + "SM_Fence_1.prefab");
            GameObject pFence2 = Load(PREFAB_PATH + "SM_Fence_2.prefab");
            GameObject pCampfireLit = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pCampfireUnlit = Load(PREFAB_PATH + "SM_Campfire.prefab");
            GameObject pStump = Load(PREFAB_PATH + "SM_Stump_01.prefab");
            GameObject pFallenTree = Load(PREFAB_PATH + "SM_FallenTree.prefab");
            GameObject pRock3 = Load(PREFAB_PATH + "SM_Rock_03.prefab");
            GameObject pRock5 = Load(PREFAB_PATH + "SM_Rock_05.prefab");
            GameObject pRock7 = Load(PREFAB_PATH + "SM_Rock_07.prefab");
            GameObject pPebble = Load(PREFAB_PATH + "SM_Pebble_01.prefab");
            GameObject pWater = Load(PREFAB_PATH + "SM_Water.prefab");
            GameObject pFox = Load(PREFAB_PATH + "SM_Fox_01.prefab");
            GameObject pFirefly = FindOptionalPrefab("Firefly");

            // Tre xanh làng Việt & Đá bước dạo sân vườn
            GameObject pBambooBig1 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_01.prefab");
            GameObject pBambooBig2 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_02.prefab");
            GameObject pBambooMid1 = Load(BAMBOO_PATH + "Tree_Bamboo_Mid_Green_01.prefab");
            GameObject pBambooMid2 = Load(BAMBOO_PATH + "Tree_Bamboo_Mid_Green_02.prefab");
            GameObject pBambooSmall1 = Load(BAMBOO_PATH + "Tree_Bamboo_Small_Green_01.prefab");
            GameObject pBambooSmall2 = Load(BAMBOO_PATH + "Tree_Bamboo_Small_Green_02.prefab");
            GameObject pStepStone1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Waldemarst/FreeJapaneseGarden/Prefabs/Misc/Rocks/JG_TobiIshi_A_01.prefab");
            GameObject pStepStone2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Waldemarst/FreeJapaneseGarden/Prefabs/Misc/Rocks/JG_TobiIshi_A_02.prefab");
            if (pStepStone1 == null) pStepStone1 = pPebble;
            if (pStepStone2 == null) pStepStone2 = pPebble;

            // Tọa độ chuẩn các công trình trọng điểm trong Làng Phù Đổng (đảm bảo cách xa nhau 20m - 36m, tuyệt đối không đè lên nhau)
            VillageHouseSpec[] houseSpecs =
            {
                new VillageHouseSpec { name = "NhaSan_01_MeGiong_TrungTam", pos = new Vector2(50f, 145f), rotY = 90f,  scale = 4.45f, litHearth = true  },
                new VillageHouseSpec { name = "NhaSan_02_DanLang_TayBac",   pos = new Vector2(54f, 175f), rotY = 134f, scale = 4.20f, litHearth = false },
                new VillageHouseSpec { name = "NhaSan_03_DanLang_TayNam",   pos = new Vector2(54f, 115f), rotY = 46f,  scale = 4.20f, litHearth = false },
                new VillageHouseSpec { name = "NhaSan_04_DanLang_DongBac",  pos = new Vector2(83f, 186f), rotY = 176f, scale = 4.15f, litHearth = true  },
                new VillageHouseSpec { name = "NhaSan_05_DanLang_DongNam",  pos = new Vector2(83f, 103f), rotY = 4f,   scale = 4.15f, litHearth = false }
            };

            Vector2 courtyardCenter = new Vector2(82f, 145f);
            Vector2 dinhLangCenter2D = new Vector2(76.0f, 127.0f);
            Vector2 forgeCenter2D = new Vector2(74.5f, 163.5f);
            Vector2 banyanCenter2D = new Vector2(96.0f, 163.0f);
            Vector2 wellCenter2D = new Vector2(93.5f, 125.0f);
            Vector2 gateCenter2D = new Vector2(103.5f, 145.0f);
            float dockZ = 123f;

            AddExclusionCircle(dinhLangCenter2D, 11.0f);
            AddExclusionCircle(forgeCenter2D, 7.5f);
            AddExclusionCircle(gateCenter2D, 8.5f);

            // Danh sách các trục đường đất trong làng để không một gốc cây hay bụi cỏ nào mọc chắn giữa đường đi
            Vector4[] villageRoadSegments =
            {
                new Vector4(55f, 145f, 113f, 145f),   // Trục chính Nhà Mẹ Gióng -> Sân Đình -> Cổng Làng -> Cầu Gỗ
                new Vector4(58f, 171f, 77f,  152f),   // Ngõ Nhà Tây Bắc
                new Vector4(58f, 119f, 75f,  136f),   // Ngõ Nhà Tây Nam
                new Vector4(83f, 180f, 83f,  156f),   // Ngõ Nhà Đông Bắc
                new Vector4(83f, 109f, 85f,  134f),   // Ngõ Nhà Đông Nam
                new Vector4(76f, 131f, 80f,  143f),   // Sân bậc thềm Đình Làng Đông Sơn nối ra Sân Đình
                new Vector4(74.5f, 162.5f, 80f, 153f),// Lối vào Chòi Lò Rèn
                new Vector4(74.5f, 163.5f, 68f, 200f),// Lối mòn từ Chòi Lò Rèn lên Mỏ Quặng Sắt (Chương 1)
                new Vector4(87f, 152f, 94.5f, 160.5f),// Lối lên Cây Đa Cổ Thụ
                new Vector4(87f, 137f, 93.5f, 125f),  // Lối xuống Giếng Làng
                new Vector4(93.5f, 125f, 108f, 123f)  // Lối từ Giếng Làng ra Bến Nước Ven Sông
            };

            List<Vector2> placedTreeTrunks = new List<Vector2>();
            placedTreeTrunks.Add(banyanCenter2D);

            // -----------------------------------------------------------------
            // 1. QUẦN THỂ 5 NHÀ SÀN ĐÔNG SƠN & KHUÔN VIÊN SÂN VƯỜN TỪNG NHÀ
            // -----------------------------------------------------------------
            string houseAlbedo = "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri_albedo.jpg";
            string houseNormal = "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri_normal.jpg";
            string houseRough = "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri_metallic_roughness.jpg";

            for (int hIdx = 0; hIdx < houseSpecs.Length; hIdx++)
            {
                VillageHouseSpec spec = houseSpecs[hIdx];
                GameObject compoundRoot = CreateGroup($"KhuonVien_{spec.name}", housesGroup);

                if (housePrefab != null)
                {
                    GameObject house = Spawn(housePrefab, compoundRoot, new Vector3(spec.pos.x, GROUND_Y, spec.pos.y),
                        new Vector3(0f, spec.rotY, 0f), Vector3.one * spec.scale);
                    if (house != null)
                    {
                        house.name = spec.name;
                        SetupModelMaterial(house, "M_NhaSan_ChimLac", houseAlbedo, houseNormal, houseRough);
                        SnapToTerrain(house, terrain, -0.04f);
                        AddFittedBoxCollider(house);
                    }
                }
                AddExclusionCircle(spec.pos, 10.5f);

                // Hệ trục hướng mặt tiền (fwd) và ngang hông (right) của từng ngôi nhà sàn
                float rad = spec.rotY * Mathf.Deg2Rad;
                Vector2 fwd = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
                Vector2 right = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad));

                // A. Lối đá bước dạo trước cầu thang nhà sàn nối ra ngõ xóm (cách tâm nhà 5.9m -> 8.7m, không chạm cột nhà)
                for (int s = 0; s < 3; s++)
                {
                    float distFwd = 5.9f + s * 1.35f;
                    float sway = (s % 2 == 0 ? -0.22f : 0.22f);
                    Vector2 sp = spec.pos + fwd * distFwd + right * sway;
                    GameObject stPrefab = (s % 2 == 0) ? pStepStone1 : pStepStone2;
                    GameObject st = SpawnGrounded(stPrefab, compoundRoot, terrain, new Vector3(sp.x, GROUND_Y, sp.y),
                        new Vector3(0f, spec.rotY + (s * 25f), 0f), Vector3.one * 1.15f, -0.02f);
                    EnsureMaterialsUrpLit(st);
                }

                // B. Hàng rào gỗ thấp hai bên cổng sân trước (để mở cổng chính rộng 5.6m thông thoáng ở giữa)
                Vector2 fenceLeftPos = spec.pos + fwd * 6.8f - right * 4.5f;
                Vector2 fenceRightPos = spec.pos + fwd * 6.8f + right * 4.5f;
                SpawnGrounded(pFence1, compoundRoot, terrain, new Vector3(fenceLeftPos.x, GROUND_Y, fenceLeftPos.y),
                    new Vector3(0f, spec.rotY, 0f), Vector3.one * 1.12f);
                SpawnGrounded(pFence2, compoundRoot, terrain, new Vector3(fenceRightPos.x, GROUND_Y, fenceRightPos.y),
                    new Vector3(0f, spec.rotY, 0f), Vector3.one * 1.12f);

                // C. Luống hoa dại & bụi dương xỉ sát chân hàng rào trước nhà
                Vector2 flowerLeft = spec.pos + fwd * 6.2f - right * 6.1f;
                Vector2 flowerRight = spec.pos + fwd * 6.2f + right * 6.1f;
                SpawnGrounded((hIdx % 2 == 0) ? pFlower1 : pFlower2, compoundRoot, terrain,
                    new Vector3(flowerLeft.x, GROUND_Y, flowerLeft.y), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.25f);
                SpawnGrounded(pFoliageBush, compoundRoot, terrain,
                    new Vector3(flowerRight.x, GROUND_Y, flowerRight.y), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.10f);
                SpawnGrounded((hIdx % 2 == 0) ? pFlower2 : pFlower1, compoundRoot, terrain,
                    new Vector3(flowerRight.x + 0.9f, GROUND_Y, flowerRight.y - 0.6f), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.20f);

                // D. Bếp củi gia đình, gốc củi & gỗ đun bên góc sân nhà (cách tâm nhà 8.0m thoáng đãng)
                Vector2 hearthPos = spec.pos + fwd * 3.8f - right * 6.8f;
                GameObject hearthPrefab = spec.litHearth ? pCampfireLit : pCampfireUnlit;
                SpawnGrounded(hearthPrefab, compoundRoot, terrain, new Vector3(hearthPos.x, GROUND_Y, hearthPos.y),
                    new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 0.95f, -0.02f);

                Vector2 stumpPos = hearthPos - fwd * 1.45f - right * 0.6f;
                SpawnGrounded(pStump, compoundRoot, terrain, new Vector3(stumpPos.x, GROUND_Y, stumpPos.y),
                    new Vector3(0f, spec.rotY + 35f, 0f), Vector3.one * 0.95f, -0.03f);

                Vector2 woodpilePos = spec.pos - fwd * 1.6f - right * 7.1f;
                SpawnGrounded(pFallenTree, compoundRoot, terrain, new Vector3(woodpilePos.x, GROUND_Y, woodpilePos.y),
                    new Vector3(0f, spec.rotY + 85f, 0f), Vector3.one * 0.68f, -0.04f);

                // Chú cáo/cún nhỏ nằm sưởi ấm bên sân vườn Nhà Mẹ Gióng
                if (hIdx == 0 && pFox != null)
                {
                    Vector2 petPos = hearthPos + fwd * 1.5f + right * 0.8f;
                    GameObject pet = SpawnGrounded(pFox, compoundRoot, terrain, new Vector3(petPos.x, GROUND_Y, petPos.y),
                        new Vector3(0f, 115f, 0f), Vector3.one * 1.15f, -0.01f);
                    if (pet != null) pet.name = "ThuNho_SanNhaMeGiong";
                }

                // E. 2 Cây ăn quả / cây bóng mát tỏa tán bên hông & sau vườn mỗi nhà sàn (cách tâm nhà ~8.9m, không lấn vào mái nhà!)
                Vector2[] gardenTreeCandidates =
                {
                    spec.pos - fwd * 3.0f - right * 8.4f,
                    spec.pos + fwd * 1.2f + right * 8.6f
                };

                for (int t = 0; t < gardenTreeCandidates.Length; t++)
                {
                    Vector2 tp = gardenTreeCandidates[t];
                    if (IsSafeVillagePropPos(tp, houseSpecs, villageRoadSegments, courtyardCenter, forgeCenter2D, banyanCenter2D, wellCenter2D, 7.8f, 3.6f, 5.8f)
                        && IsFarFromOtherTrees(tp, placedTreeTrunks, 5.2f))
                    {
                        GameObject treeAsset = villageShadeTrees[(hIdx * 2 + t) % villageShadeTrees.Length];
                        GameObject tr = SpawnGrounded(treeAsset, compoundRoot, terrain, new Vector3(tp.x, GROUND_Y, tp.y),
                            new Vector3(0f, (hIdx * 73f + t * 137f) % 360f, 0f), Vector3.one * Random.Range(0.95f, 1.18f), -0.05f);
                        if (tr != null) tr.name = $"CayVuonNha_{hIdx + 1:00}_{t + 1:00}";
                        placedTreeTrunks.Add(tp);

                        // Cụm dương xỉ & nấm nhỏ dưới gốc cây vườn nhà
                        Vector2 fernPos = tp + right * (t == 0 ? 1.25f : -1.25f);
                        SpawnGrounded((t == 0) ? pFoliage1 : pFoliage2, compoundRoot, terrain,
                            new Vector3(fernPos.x, GROUND_Y, fernPos.y), new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.1f);
                    }
                }

                // F. Khóm tre cảnh nhỏ sau góc vườn nhà sàn
                Vector2 backyardBamboo = spec.pos - fwd * 6.6f + right * 5.2f;
                if (IsSafeVillagePropPos(backyardBamboo, houseSpecs, villageRoadSegments, courtyardCenter, forgeCenter2D, banyanCenter2D, wellCenter2D, 7.6f, 3.5f, 5.5f))
                {
                    GameObject smBamboo = (hIdx % 2 == 0) ? pBambooSmall1 : pBambooSmall2;
                    SpawnGrounded(smBamboo, compoundRoot, terrain, new Vector3(backyardBamboo.x, GROUND_Y, backyardBamboo.y),
                        new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.05f, -0.05f);
                }
            }

            // -----------------------------------------------------------------
            // 2. CỔNG LÀNG PHÙ ĐỔNG & CÂY ĐA CỔ THỤ - MIẾU THỔ THẦN ĐẦU LÀNG
            // -----------------------------------------------------------------
            GameObject gateSub = CreateGroup("CongLang_PhuDong", gateBanyanGroup);
            GameObject pVillageGateFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_CONG_LANG + ".fbx");
            if (pVillageGateFbx != null)
            {
                // Cổng Làng Đông Sơn Meshy AI (bề ngang 14.2m bắc theo trục Bắc-Nam, vòm giữa rộng 5.2m x cao 6.4m thông thoáng cho Ngựa Sắt phi qua!)
                SpawnGateArchitecture(
                    pVillageGateFbx, "CongLang_Co_PhuDong", gateSub, terrain,
                    gateCenter2D, true, 14.2f,
                    "M_CongLang_PhuDong", ARCH_CONG_LANG, true, -0.05f);

                // Tường đá ong thấp nối hai cánh ngoài Cổng Làng (Z = ±9.2m, nằm ngoài trụ cổng hoàn toàn không đè!)
                SpawnFittedWallOrSlab(pStoneWall2, gateSub, terrain, new Vector3(gateCenter2D.x, GROUND_Y, 154.2f), 90f, new Vector3(3.4f, 1.15f, 0.85f));
                SpawnFittedWallOrSlab(pStoneWall2, gateSub, terrain, new Vector3(gateCenter2D.x, GROUND_Y, 135.8f), 90f, new Vector3(3.4f, 1.15f, 0.85f));
            }
            else
            {
                SpawnFittedWallOrSlab(pStoneWall1, gateSub, terrain, new Vector3(gateCenter2D.x, GROUND_Y, 152.2f), 0f, new Vector3(4.2f, 1.25f, 0.90f));
                SpawnFittedWallOrSlab(pStoneWall1, gateSub, terrain, new Vector3(gateCenter2D.x, GROUND_Y, 137.8f), 0f, new Vector3(4.2f, 1.25f, 0.90f));
                SpawnFittedWallOrSlab(pStoneWall2, gateSub, terrain, new Vector3(gateCenter2D.x + 2.0f, GROUND_Y, 154.3f), 90f, new Vector3(2.8f, 1.10f, 0.85f));
                SpawnFittedWallOrSlab(pStoneWall2, gateSub, terrain, new Vector3(gateCenter2D.x + 2.0f, GROUND_Y, 135.7f), 90f, new Vector3(2.8f, 1.10f, 0.85f));
            }

            // Trụ đá lửa thiêng & biển gỗ tên làng phía trước hai trụ Cổng Làng (cách ra X = 107.0m không chạm mái cổng)
            SpawnGrounded(pRock3, gateSub, terrain, new Vector3(107.0f, GROUND_Y, 151.6f), new Vector3(0f, 15f, 0f), Vector3.one * 0.90f, -0.05f);
            SpawnGrounded(pCampfireLit, gateSub, terrain, new Vector3(107.0f, GROUND_Y, 151.6f), Vector3.zero, Vector3.one * 1.05f, -0.02f);
            SpawnGrounded(pRock3, gateSub, terrain, new Vector3(107.0f, GROUND_Y, 138.4f), new Vector3(0f, -25f, 0f), Vector3.one * 0.90f, -0.05f);
            SpawnGrounded(pCampfireLit, gateSub, terrain, new Vector3(107.0f, GROUND_Y, 138.4f), Vector3.zero, Vector3.one * 1.05f, -0.02f);
            SpawnGrounded(pSign1, gateSub, terrain, new Vector3(107.4f, GROUND_Y, 153.2f), new Vector3(0f, 90f, 0f), Vector3.one * 1.45f);

            // Khóm tre xanh & bụi hoa đón khách hai bên ngoài Cổng Làng
            SpawnGrounded(pBambooMid1, gateSub, terrain, new Vector3(102.5f, GROUND_Y, 158.2f), new Vector3(0f, 35f, 0f), Vector3.one * 1.08f, -0.06f);
            SpawnGrounded(pBambooSmall2, gateSub, terrain, new Vector3(105.2f, GROUND_Y, 159.0f), new Vector3(0f, 140f, 0f), Vector3.one * 1.05f, -0.05f);
            SpawnGrounded(pBambooMid2, gateSub, terrain, new Vector3(102.5f, GROUND_Y, 131.8f), new Vector3(0f, 210f, 0f), Vector3.one * 1.08f, -0.06f);
            SpawnGrounded(pBambooSmall1, gateSub, terrain, new Vector3(105.2f, GROUND_Y, 131.0f), new Vector3(0f, 75f, 0f), Vector3.one * 1.05f, -0.05f);
            SpawnGrounded(pFlower1, gateSub, terrain, new Vector3(105.8f, GROUND_Y, 154.6f), Vector3.zero, Vector3.one * 1.3f);
            SpawnGrounded(pFlower2, gateSub, terrain, new Vector3(105.8f, GROUND_Y, 135.4f), Vector3.zero, Vector3.one * 1.3f);

            // CÂY ĐA CỔ THỤ ĐẦU LÀNG & MIẾU THỔ THẦN (Tâm X=96, Z=163)
            GameObject banyanSub = CreateGroup("CayDaCoThu_MieuThoThan", gateBanyanGroup);
            GameObject mainBanyan = SpawnGrounded(pTree1, banyanSub, terrain, new Vector3(banyanCenter2D.x, GROUND_Y, banyanCenter2D.y),
                new Vector3(0f, 25f, 0f), new Vector3(1.75f, 1.65f, 1.75f), -0.08f);
            if (mainBanyan != null) mainBanyan.name = "CayDaCoThu_ThanChinh";

            GameObject crownBanyan = SpawnGrounded(pTreeGreen, banyanSub, terrain, new Vector3(banyanCenter2D.x + 0.85f, GROUND_Y, banyanCenter2D.y + 0.65f),
                new Vector3(0f, 145f, 0f), new Vector3(1.38f, 1.42f, 1.38f), -0.08f);
            if (crownBanyan != null) crownBanyan.name = "CayDaCoThu_TanPhu";

            // 4 Rễ đa cổ thụ uốn lượn nổi quanh gốc
            for (int r = 0; r < 4; r++)
            {
                float rAng = (r * 90f + 20f) * Mathf.Deg2Rad;
                Vector3 rootPos = new Vector3(banyanCenter2D.x + Mathf.Cos(rAng) * 1.35f, GROUND_Y, banyanCenter2D.y + Mathf.Sin(rAng) * 1.35f);
                SpawnGrounded(pRoot1, banyanSub, terrain, rootPos, new Vector3(0f, r * 90f + 20f, 0f), Vector3.one * 1.35f, -0.04f);
            }

            // Bệ đá cổ bao quanh gốc đa (4 cạnh tường đá thấp cao 0.48m tạo khuôn viên 5.3m x 5.3m)
            SpawnFittedWallOrSlab(pStoneWall3, banyanSub, terrain, new Vector3(banyanCenter2D.x, GROUND_Y, banyanCenter2D.y + 2.45f), 0f, new Vector3(5.38f, 0.48f, 0.48f));
            SpawnFittedWallOrSlab(pStoneWall3, banyanSub, terrain, new Vector3(banyanCenter2D.x, GROUND_Y, banyanCenter2D.y - 2.45f), 0f, new Vector3(5.38f, 0.48f, 0.48f));
            SpawnFittedWallOrSlab(pStoneWall3, banyanSub, terrain, new Vector3(banyanCenter2D.x + 2.45f, GROUND_Y, banyanCenter2D.y), 90f, new Vector3(5.38f, 0.48f, 0.48f));
            SpawnFittedWallOrSlab(pStoneWall3, banyanSub, terrain, new Vector3(banyanCenter2D.x - 2.45f, GROUND_Y, banyanCenter2D.y), 90f, new Vector3(5.38f, 0.48f, 0.48f));

            // Bia đá Miếu Thổ Thần, ghế gỗ nghỉ chân dưới bóng đa, nấm rừng & đom đóm
            GameObject shrine = SpawnGrounded(pRuneStone3, banyanSub, terrain, new Vector3(banyanCenter2D.x - 3.3f, GROUND_Y, banyanCenter2D.y - 2.6f),
                new Vector3(0f, -135f, 0f), Vector3.one * 1.25f, -0.04f);
            if (shrine != null) shrine.name = "MieuThoThan_DauLang";

            SpawnGrounded(pFallenTree, banyanSub, terrain, new Vector3(banyanCenter2D.x - 4.2f, GROUND_Y, banyanCenter2D.y + 1.6f),
                new Vector3(0f, 75f, 0f), Vector3.one * 0.95f, -0.04f);
            SpawnGrounded(pStump, banyanSub, terrain, new Vector3(banyanCenter2D.x - 3.6f, GROUND_Y, banyanCenter2D.y + 3.4f),
                Vector3.zero, Vector3.one * 1.05f, -0.03f);
            SpawnGrounded(pMushroom1, banyanSub, terrain, new Vector3(banyanCenter2D.x - 1.5f, GROUND_Y, banyanCenter2D.y - 1.4f), Vector3.zero, Vector3.one * 1.2f);
            SpawnGrounded(pMushroom2, banyanSub, terrain, new Vector3(banyanCenter2D.x + 1.6f, GROUND_Y, banyanCenter2D.y - 1.3f), new Vector3(0f, 65f, 0f), Vector3.one * 1.15f);
            SpawnGrounded(pFoliage1, banyanSub, terrain, new Vector3(banyanCenter2D.x + 1.5f, GROUND_Y, banyanCenter2D.y + 1.5f), Vector3.zero, Vector3.one * 1.2f);
            SpawnGrounded(pFlower2, banyanSub, terrain, new Vector3(banyanCenter2D.x - 2.6f, GROUND_Y, banyanCenter2D.y - 3.3f), Vector3.zero, Vector3.one * 1.25f);
            if (pFirefly != null)
            {
                Spawn(pFirefly, banyanSub, new Vector3(banyanCenter2D.x, GROUND_Y + 2.0f, banyanCenter2D.y), Vector3.zero, Vector3.one);
            }

            // -----------------------------------------------------------------
            // 3. GIẾNG NƯỚC ĐÁ CỔ SÂN ĐÌNH & BẾN NƯỚC VEN SÔNG PHÙ ĐỔNG
            // -----------------------------------------------------------------
            GameObject wellSub = CreateGroup("GiengNuocDaCo", wellDockGroup);
            // Thành giếng đá cổ khép kín (kích thước ngoài 2.56m x 2.56m, cao 0.84m)
            SpawnFittedWallOrSlab(pStoneWall2, wellSub, terrain, new Vector3(wellCenter2D.x, GROUND_Y, wellCenter2D.y + 1.05f), 0f, new Vector3(2.56f, 0.84f, 0.46f));
            SpawnFittedWallOrSlab(pStoneWall2, wellSub, terrain, new Vector3(wellCenter2D.x, GROUND_Y, wellCenter2D.y - 1.05f), 0f, new Vector3(2.56f, 0.84f, 0.46f));
            SpawnFittedWallOrSlab(pStoneWall2, wellSub, terrain, new Vector3(wellCenter2D.x + 1.05f, GROUND_Y, wellCenter2D.y), 90f, new Vector3(2.56f, 0.84f, 0.46f));
            SpawnFittedWallOrSlab(pStoneWall2, wellSub, terrain, new Vector3(wellCenter2D.x - 1.05f, GROUND_Y, wellCenter2D.y), 90f, new Vector3(2.56f, 0.84f, 0.46f));

            // Mặt nước giếng trong vắt nằm gọn bên trong lòng thành giếng đá (cao hơn mặt đất 0.50m, thấp hơn miệng giếng 0.30m)
            if (pWater != null)
            {
                GameObject wellWater = Spawn(pWater, wellSub, new Vector3(wellCenter2D.x, GROUND_Y + 0.50f, wellCenter2D.y),
                    Vector3.zero, new Vector3(0.19f, 1f, 0.19f));
                if (wellWater != null) wellWater.name = "MatNuoc_GiengLang";
            }

            // Nền đá lát quanh chân giếng, đôn gỗ đặt gàu nước, dương xỉ ưa ẩm & cây bóng mát che giếng
            for (int a = 0; a < 6; a++)
            {
                float ang = (a * 60f + 15f) * Mathf.Deg2Rad;
                Vector3 apronPos = new Vector3(wellCenter2D.x + Mathf.Cos(ang) * 2.15f, GROUND_Y, wellCenter2D.y + Mathf.Sin(ang) * 2.15f);
                GameObject st = SpawnGrounded((a % 2 == 0) ? pStepStone1 : pStepStone2, wellSub, terrain, apronPos,
                    new Vector3(0f, a * 60f, 0f), Vector3.one * 1.15f, -0.02f);
                EnsureMaterialsUrpLit(st);
            }
            SpawnGrounded(pStump, wellSub, terrain, new Vector3(wellCenter2D.x - 2.3f, GROUND_Y, wellCenter2D.y - 1.5f), Vector3.zero, Vector3.one * 0.90f, -0.03f);
            SpawnGrounded(pFoliage2, wellSub, terrain, new Vector3(wellCenter2D.x + 1.9f, GROUND_Y, wellCenter2D.y - 1.8f), Vector3.zero, Vector3.one * 1.15f);
            SpawnGrounded(pFlower1, wellSub, terrain, new Vector3(wellCenter2D.x - 1.8f, GROUND_Y, wellCenter2D.y + 2.0f), Vector3.zero, Vector3.one * 1.25f);

            Vector2 wellShadeTreePos = new Vector2(96.0f, 120.2f);
            GameObject wellTree = SpawnGrounded(pTree2, wellSub, terrain, new Vector3(wellShadeTreePos.x, GROUND_Y, wellShadeTreePos.y),
                new Vector3(0f, 65f, 0f), Vector3.one * 1.22f, -0.05f);
            if (wellTree != null) wellTree.name = "CayBongMat_BenGiengLang";
            placedTreeTrunks.Add(wellShadeTreePos);

            // BẾN NƯỚC VEN SÔNG (Nối từ Giếng Làng ra bờ Tây sông Phù Đổng tại Z = 123m)
            GameObject dockSub = CreateGroup("BenNuoc_VenSongPhuDong", wellDockGroup);
            float dockRiverCx = GetRiverCenterX(dockZ);
            for (int s = 0; s < 5; s++)
            {
                float t = (s + 1f) / 6f;
                Vector2 stepPos = Vector2.Lerp(new Vector2(wellCenter2D.x + 2.4f, wellCenter2D.y - 0.3f), new Vector2(dockRiverCx - 15.2f, dockZ), t);
                GameObject st = SpawnGrounded((s % 2 == 0) ? pStepStone1 : pStepStone2, dockSub, terrain,
                    new Vector3(stepPos.x, GROUND_Y, stepPos.y), new Vector3(0f, s * 37f, 0f), Vector3.one * 1.25f, -0.02f);
                EnsureMaterialsUrpLit(st);
            }

            // Bậc thềm đá 2 tầng dẫn từ bờ cát xuống sát mép nước sông (Y = 9.55m -> Y = 8.92m trên mặt nước 8.60m)
            SpawnFittedWallOrSlab(pPlatform3, dockSub, terrain, new Vector3(dockRiverCx - 13.6f, WATER_Y + 0.32f, dockZ),
                0f, new Vector3(3.2f, 0.62f, 2.8f), false);
            SpawnFittedWallOrSlab(pPlatform3, dockSub, terrain, new Vector3(dockRiverCx - 11.4f, WATER_Y - 0.25f, dockZ),
                0f, new Vector3(3.4f, 0.57f, 3.0f), false);

            // Cọc buộc thuyền, lan can gỗ, sỏi cuội & bụi tre rủ bóng bên Bến Nước
            SpawnGrounded(pStump, dockSub, terrain, new Vector3(dockRiverCx - 14.8f, GROUND_Y, dockZ + 2.1f), Vector3.zero, Vector3.one * 0.95f, -0.04f);
            SpawnGrounded(pFence2, dockSub, terrain, new Vector3(dockRiverCx - 15.5f, GROUND_Y, dockZ - 2.4f), new Vector3(0f, 12f, 0f), Vector3.one * 1.0f);
            SpawnGrounded(pPebble, dockSub, terrain, new Vector3(dockRiverCx - 14.2f, GROUND_Y, dockZ - 2.0f), Vector3.zero, Vector3.one * 1.3f);
            SpawnGrounded(pFoliageBush, dockSub, terrain, new Vector3(dockRiverCx - 16.2f, GROUND_Y, dockZ + 2.8f), Vector3.zero, Vector3.one * 1.15f);
            SpawnGrounded(pBambooMid2, dockSub, terrain, new Vector3(dockRiverCx - 17.5f, GROUND_Y, dockZ - 5.2f), new Vector3(0f, 45f, 0f), Vector3.one * 1.10f, -0.06f);

            // -----------------------------------------------------------------
            // 4. ĐÌNH LÀNG ĐÔNG SƠN LỚN, SÂN ĐÌNH TRUNG TÂM & CHÒI LÒ RÈN VŨ KHÍ
            // -----------------------------------------------------------------
            // A. ĐÌNH LÀNG ĐÔNG SƠN LỚN (Tâm X=76, Z=127, mặt tiền bậc thang hướng Bắc nhìn thẳng ra Sân Đình trung tâm!)
            GameObject pDinhLangFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_DINH_LANG + ".fbx");
            if (pDinhLangFbx != null)
            {
                SpawnFittedArchitecture(
                    pDinhLangFbx, "DinhLang_Lon_DongSon", yardForgeGroup, terrain,
                    dinhLangCenter2D, 0f, 13.8f, false,
                    "M_DinhLang_DongSon", ARCH_DINH_LANG, 0.04f, 0.22f, true, true, -0.05f);
            }

            // Hai đài lửa nghi lễ đứng hai bên trước bậc thềm Đình Làng
            SpawnGrounded(pCampfireLit, yardForgeGroup, terrain, new Vector3(69.5f, GROUND_Y, 133.5f), Vector3.zero, Vector3.one * 1.10f, -0.02f);
            SpawnGrounded(pCampfireLit, yardForgeGroup, terrain, new Vector3(82.5f, GROUND_Y, 133.5f), Vector3.zero, Vector3.one * 1.10f, -0.02f);

            // B. CHÒI LÒ RÈN VŨ KHÍ BÁC THỢ RÈN (Tâm X=74.5, Z=163.5, ngay đầu lối mòn từ Mỏ Quặng Sắt Chương 1 trở về!)
            Vector3 forgeCenter = new Vector3(forgeCenter2D.x, GROUND_Y, forgeCenter2D.y);
            GameObject pForgeHutFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_CHOI_LO_REN + ".fbx");
            if (pForgeHutFbx != null)
            {
                SpawnFittedArchitecture(
                    pForgeHutFbx, "ChoiLoRen_VuKhi_BacThoRen", yardForgeGroup, terrain,
                    forgeCenter2D, 165f, 7.8f, false,
                    "M_ChoiLoRen_PhuDong", ARCH_CHOI_LO_REN, 0.16f, 0.25f, false, true, -0.05f);

                // Đống gỗ củi & gốc đe phụ bên ngoài hiên Chòi Lò Rèn (cách tâm > 5.0m, không chạm mái chòi!)
                SpawnGrounded(pFallenTree, yardForgeGroup, terrain, forgeCenter + new Vector3(-5.8f, 0f, 1.8f), new Vector3(0f, 25f, 0f), Vector3.one * 1.05f, -0.04f);
                SpawnGrounded(pStump, yardForgeGroup, terrain, forgeCenter + new Vector3(5.0f, 0f, -1.6f), new Vector3(0f, 40f, 0f), Vector3.one * 1.15f, -0.03f);
                SpawnGrounded(pRock5, yardForgeGroup, terrain, forgeCenter + new Vector3(5.2f, 0f, 1.5f), new Vector3(0f, -20f, 0f), Vector3.one * 0.95f, -0.05f);
            }
            else
            {
                SpawnGrounded(pCampfireLit, yardForgeGroup, terrain, forgeCenter, Vector3.zero, Vector3.one * 1.45f, -0.02f);
                SpawnGrounded(pRock7, yardForgeGroup, terrain, forgeCenter + new Vector3(-2.8f, 0f, -1.0f), new Vector3(0f, 35f, 0f), Vector3.one * 1.15f, -0.05f);
                SpawnGrounded(pStump, yardForgeGroup, terrain, forgeCenter + new Vector3(2.8f, 0f, -1.7f), new Vector3(0f, 40f, 0f), Vector3.one * 1.18f, -0.03f);
            }

            // Cây cổ thụ tỏa bóng phía sau Chòi Lò Rèn (cách tâm 9.6m thoáng mát)
            Vector2 forgeTreePos = new Vector2(67.0f, 169.5f);
            GameObject forgeTree = SpawnGrounded(pTreeColor1, yardForgeGroup, terrain, new Vector3(forgeTreePos.x, GROUND_Y, forgeTreePos.y),
                new Vector3(0f, 110f, 0f), Vector3.one * 1.20f, -0.05f);
            if (forgeTree != null) forgeTree.name = "CayBongMat_XuongRen";
            placedTreeTrunks.Add(forgeTreePos);

            // Hàng rào gỗ dẫn lối đại lộ từ Sân Đình ra Cổng Làng (chừa lòng đường rộng 10m ở giữa Z = 139.5..150.5)
            SpawnGrounded(pFence1, yardForgeGroup, terrain, new Vector3(94.5f, GROUND_Y, 151.2f), Vector3.zero, Vector3.one * 1.18f);
            SpawnGrounded(pFence2, yardForgeGroup, terrain, new Vector3(98.5f, GROUND_Y, 151.2f), Vector3.zero, Vector3.one * 1.18f);
            SpawnGrounded(pFence1, yardForgeGroup, terrain, new Vector3(94.5f, GROUND_Y, 138.8f), Vector3.zero, Vector3.one * 1.18f);
            SpawnGrounded(pFence2, yardForgeGroup, terrain, new Vector3(98.5f, GROUND_Y, 138.8f), Vector3.zero, Vector3.one * 1.18f);

            // Đài lửa Bắc Sân Đình & ghế gỗ nghỉ chân rìa Đông Nam Sân Đình (không chắn tầm nhìn trung tâm)
            SpawnGrounded(pCampfireLit, yardForgeGroup, terrain, new Vector3(83.5f, GROUND_Y, 155.2f), Vector3.zero, Vector3.one * 1.05f, -0.02f);
            SpawnGrounded(pFallenTree, yardForgeGroup, terrain, new Vector3(87.5f, GROUND_Y, 134.8f), new Vector3(0f, 12f, 0f), Vector3.one * 0.88f, -0.04f);

            // -----------------------------------------------------------------
            // 5. NHÂN VẬT LỊCH SỬ TẠI SÂN ĐÌNH PHÙ ĐỔNG
            // -----------------------------------------------------------------
            if (emperorPrefab != null)
            {
                GameObject emperor = Spawn(emperorPrefab, charsGroup, new Vector3(73f, GROUND_Y, 145f), new Vector3(0f, 90f, 0f), Vector3.one * 1.55f);
                if (emperor != null)
                {
                    emperor.name = "HungVuong_HoangDe";
                    SetupModelMaterial(emperor, "M_HungVuong_HoangDe",
                        "Assets/Art/Models/Meshy_AI_Sun_Emperor_texture_remesh_tri_albedo.jpg",
                        "Assets/Art/Models/Meshy_AI_Sun_Emperor_texture_remesh_tri_normal.jpg",
                        "Assets/Art/Models/Meshy_AI_Sun_Emperor_texture_remesh_tri_metallic_roughness.jpg");
                    SnapToTerrain(emperor, terrain, -0.02f);
                }
            }

            if (envoyPrefab != null)
            {
                GameObject envoy = Spawn(envoyPrefab, charsGroup, new Vector3(79f, GROUND_Y, 149.5f), new Vector3(0f, 135f, 0f), Vector3.one * 1.45f);
                if (envoy != null)
                {
                    envoy.name = "SuGia_TrieuDinh_CauHien";
                    SetupModelMaterial(envoy, "M_SuGia_TrieuDinh_CauHien",
                        "Assets/Art/Models/Meshy_AI_Imperial_Envoy_texture_remesh_tri_albedo.jpg",
                        "Assets/Art/Models/Meshy_AI_Imperial_Envoy_texture_remesh_tri_normal.jpg",
                        "Assets/Art/Models/Meshy_AI_Imperial_Envoy_texture_remesh_tri_metallic_roughness.jpg");
                    SnapToTerrain(envoy, terrain, -0.02f);
                }
            }

            if (warriorPrefab != null)
            {
                GameObject giong = Spawn(warriorPrefab, charsGroup, new Vector3(91f, GROUND_Y, 146.5f), new Vector3(0f, 90f, 0f), Vector3.one * 1.75f);
                if (giong != null)
                {
                    giong.name = "ThanhGiong_DungSi";
                    SnapToTerrain(giong, terrain, -0.02f);
                }
            }

            if (horseWarriorPrefab != null)
            {
                GameObject horse = Spawn(horseWarriorPrefab, charsGroup, new Vector3(88f, GROUND_Y, 139.5f), new Vector3(0f, 82f, 0f), Vector3.one * 1.85f);
                if (horse != null)
                {
                    horse.name = "ThanhGiong_CuoiNguaSat";
                    SnapToTerrain(horse, terrain, -0.02f);
                }
            }

            // -----------------------------------------------------------------
            // 6. VƯỜN CÂY XANH XEN KẼ, LUỐNG HOA DẠI & THẢM CỎ TỰ NHIÊN TRONG LÀNG
            // -----------------------------------------------------------------
            Vector2[] pocketOrchardSpots =
            {
                new Vector2(44f, 159f), new Vector2(63.5f, 159.5f), // Khoảng vườn xanh giữa Nhà 01 & Nhà 02
                new Vector2(44f, 131f), new Vector2(63.5f, 130.5f), // Khoảng vườn xanh giữa Nhà 01 & Nhà 03
                new Vector2(41.5f, 145f),                           // Hậu viên phía Tây Nhà Mẹ Gióng
                new Vector2(67.5f, 182.5f), new Vector2(68.5f, 174f), // Vườn cây giữa Nhà 02 & Nhà 04
                new Vector2(67.5f, 107.5f), new Vector2(68.5f, 116f), // Vườn cây giữa Nhà 03 & Nhà 05
                new Vector2(95.5f, 180.5f), new Vector2(103.5f, 171.5f), // Vườn cây Đông Bắc sau Cây Đa
                new Vector2(94.5f, 108.5f),                         // Vườn cây Đông Nam sau Giếng Làng
                new Vector2(45.5f, 184.5f), new Vector2(45.5f, 105.5f)  // Hai góc vườn Tây Bắc & Tây Nam
            };

            for (int i = 0; i < pocketOrchardSpots.Length; i++)
            {
                Vector2 pt = pocketOrchardSpots[i];
                if (!IsSafeVillagePropPos(pt, houseSpecs, villageRoadSegments, courtyardCenter, forgeCenter2D, banyanCenter2D, wellCenter2D, 8.0f, 3.8f, 6.0f))
                    continue;
                if (!IsFarFromOtherTrees(pt, placedTreeTrunks, 5.2f))
                    continue;

                GameObject treePrefab = villageShadeTrees[i % villageShadeTrees.Length];
                GameObject tree = SpawnGrounded(treePrefab, orchardFloraGroup, terrain, new Vector3(pt.x, GROUND_Y, pt.y),
                    new Vector3(0f, (i * 67f + 25f) % 360f, 0f), Vector3.one * Random.Range(0.95f, 1.24f), -0.05f);
                if (tree != null) tree.name = $"CayXanh_NoiKhuLang_{i + 1:00}";
                placedTreeTrunks.Add(pt);

                // Điểm thêm bụi cây nhỏ / dương xỉ hoặc hoa dại sát gốc cây vườn làng
                Vector2 underPos = pt + new Vector2(Mathf.Cos(i * 1.7f) * 1.45f, Mathf.Sin(i * 1.7f) * 1.45f);
                GameObject underPrefab = (i % 3 == 0) ? pFoliageBush : ((i % 3 == 1) ? pFlower1 : pFoliage1);
                SpawnGrounded(underPrefab, orchardFloraGroup, terrain, new Vector3(underPos.x, GROUND_Y, underPos.y),
                    new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(1.0f, 1.25f));
            }

            // Rải bụi cây thấp & hoa dại dọc hai bên mép cỏ của các ngõ xóm (tuyệt đối không mọc đè lên mặt đường đất hay sân đình)
            GameObject[] bushAndFlowerPool = { pBush0, pBush1, pFoliageBush, pFoliage1, pFoliage2, pFlower1, pFlower2 };
            int plantedShrubs = 0;
            for (int attempt = 0; attempt < 95 && plantedShrubs < 28; attempt++)
            {
                Vector2 candidate = new Vector2(Random.Range(41f, 105f), Random.Range(97f, 193f));
                if (!IsSafeVillagePropPos(candidate, houseSpecs, villageRoadSegments, courtyardCenter, forgeCenter2D, banyanCenter2D, wellCenter2D, 6.8f, 3.2f, 4.8f))
                    continue;

                // Tránh vùng nhân vật Sân Đình trung tâm
                if (Vector2.Distance(candidate, courtyardCenter) < 14.5f) continue;

                GameObject prefab = bushAndFlowerPool[plantedShrubs % bushAndFlowerPool.Length];
                SpawnGrounded(prefab, orchardFloraGroup, terrain, new Vector3(candidate.x, GROUND_Y, candidate.y),
                    new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(0.95f, 1.28f));
                plantedShrubs++;
            }

            // Thảm cỏ tự nhiên điểm xuyết trên nền cỏ xanh quanh vườn làng
            GameObject[] grassPool = { pGrass1, pGrass2, pGrass3, pGrass4 };
            int plantedGrass = 0;
            for (int attempt = 0; attempt < 150 && plantedGrass < 48; attempt++)
            {
                Vector2 candidate = new Vector2(Random.Range(39f, 106f), Random.Range(95f, 195f));
                if (!IsSafeVillagePropPos(candidate, houseSpecs, villageRoadSegments, courtyardCenter, forgeCenter2D, banyanCenter2D, wellCenter2D, 6.0f, 2.9f, 3.8f))
                    continue;
                if (Vector2.Distance(candidate, courtyardCenter) < 15.0f) continue;

                GameObject gPrefab = grassPool[plantedGrass % grassPool.Length];
                SpawnGrounded(gPrefab, orchardFloraGroup, terrain, new Vector3(candidate.x, GROUND_Y, candidate.y),
                    new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(0.95f, 1.30f), -0.02f);
                plantedGrass++;
            }

            // Đom đóm ấm áp bay lượn trong vườn Nhà Mẹ Gióng
            if (pFirefly != null)
            {
                Spawn(pFirefly, orchardFloraGroup, new Vector3(46f, GROUND_Y + 1.6f, 152f), Vector3.zero, Vector3.one);
            }

            // -----------------------------------------------------------------
            // 7. LŨY TRE XANH 2 LỚP BAO BỌC LÀNG VIỆT CỔ (32 KHÓM TRE ĐAN XEN)
            // Mở thoáng mặt Đông hướng ra Cầu Gỗ, Bến Nước & Sông Phù Đổng
            // -----------------------------------------------------------------
            GameObject[] villageBamboos = { pBambooBig1, pBambooBig2, pBambooMid1, pBambooMid2, pBambooSmall1, pBambooSmall2 };

            Vector2[] bambooRampartOuter =
            {
                // Lũy tre phía Tây (2 lớp so le tại X = 34.5..38.5, cách lưng Nhà Mẹ Gióng 13m-16m, cách chân núi > 10m)
                new Vector2(35.0f, 108f), new Vector2(38.0f, 114f), new Vector2(34.5f, 121f), new Vector2(37.8f, 128f),
                new Vector2(34.5f, 136f), new Vector2(37.5f, 143f), new Vector2(34.5f, 150f), new Vector2(37.8f, 157f),
                new Vector2(34.5f, 165f), new Vector2(38.0f, 172f), new Vector2(35.0f, 180f), new Vector2(38.5f, 187f),

                // Lũy tre phía Bắc (2 lớp so le tại Z = 195..201, mở cổng rộng 11m tại X = 62.5..73.5 dẫn lên Mỏ Quặng Sắt Chương 1!)
                new Vector2(44.0f, 193f), new Vector2(50.5f, 198f), new Vector2(56.5f, 195f), new Vector2(61.5f, 199f),
                new Vector2(74.5f, 199f), new Vector2(81.0f, 201f), new Vector2(87.5f, 198f), new Vector2(94.5f, 200f),
                new Vector2(101.5f, 195f), new Vector2(107.0f, 188f),

                // Lũy tre phía Nam (2 lớp so le tại Z = 89..95, bao bọc phía Nam làng)
                new Vector2(44.0f, 97f),  new Vector2(50.5f, 92f),  new Vector2(57.5f, 95f),  new Vector2(64.5f, 90f),
                new Vector2(72.0f, 93f),  new Vector2(79.5f, 89f),  new Vector2(87.0f, 92f),  new Vector2(94.5f, 90f),
                new Vector2(100.5f, 95f), new Vector2(103.5f, 102f)
            };

            for (int i = 0; i < bambooRampartOuter.Length; i++)
            {
                Vector2 bp = bambooRampartOuter[i];
                // Đảm bảo không khóm tre nào lấn xuống bờ sông phía Đông Nam
                if (bp.x > GetRiverCenterX(bp.y) - 15.5f) continue;

                GameObject bPrefab = villageBamboos[i % 4]; // Thân tre lớn & trung làm trụ chính
                SpawnGrounded(bPrefab, hedgeGroup, terrain, new Vector3(bp.x, GROUND_Y, bp.y),
                    new Vector3(0f, (i * 53f) % 360f, 0f), Vector3.one * Random.Range(1.02f, 1.28f), -0.06f);

                // Cứ cách 3 khóm tre lớn lại điểm thêm 1 bụi tre non hoặc bụi cây chân lũy tre cho dày dặn tự nhiên
                if (i % 3 == 0)
                {
                    Vector2 subPos = bp + new Vector2(1.4f, (i % 2 == 0) ? 1.1f : -1.1f);
                    GameObject subPrefab = (i % 6 == 0) ? pBambooSmall1 : pBush1;
                    SpawnGrounded(subPrefab, hedgeGroup, terrain, new Vector3(subPos.x, GROUND_Y, subPos.y),
                        new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.10f, -0.04f);
                }
            }
        }

        private static bool IsSafeVillagePropPos(
            Vector2 pos,
            VillageHouseSpec[] houses,
            Vector4[] roadSegments,
            Vector2 courtyardCenter,
            Vector2 forgeCenter,
            Vector2 banyanCenter,
            Vector2 wellCenter,
            float minHouseDist,
            float minRoadDist,
            float minLandmarkDist)
        {
            // Giữ cách mép bờ sông Phù Đổng >= 16m để cây/cỏ luôn đứng trên thềm đất phẳng Y = 10m
            if (pos.x > GetRiverCenterX(pos.y) - 16.0f) return false;
            if (pos.x < 36.5f || pos.y < 93.5f || pos.y > 195.5f) return false;

            for (int i = 0; i < houses.Length; i++)
            {
                if (Vector2.Distance(pos, houses[i].pos) < minHouseDist) return false;
            }

            if (Vector2.Distance(pos, new Vector2(76.0f, 127.0f)) < 10.5f) return false; // Đình Làng Đông Sơn lớn
            if (Vector2.Distance(pos, forgeCenter) < Mathf.Max(minLandmarkDist, 7.2f)) return false; // Chòi Lò Rèn
            if (Vector2.Distance(pos, banyanCenter) < minLandmarkDist + 1.0f) return false;
            if (Vector2.Distance(pos, wellCenter) < minLandmarkDist) return false;
            if (Vector2.Distance(pos, new Vector2(103.5f, 145f)) < 8.5f) return false; // Cổng Làng cổ

            // Tránh lòng các ngõ xóm & đường trục chính
            for (int i = 0; i < roadSegments.Length; i++)
            {
                Vector4 seg = roadSegments[i];
                float extraRoadWidth = (i == 0) ? 1.6f : 0f; // Đại lộ chính rộng hơn ngõ nhỏ
                if (PointToSegmentDistance(pos, new Vector2(seg.x, seg.y), new Vector2(seg.z, seg.w)) < minRoadDist + extraRoadWidth)
                    return false;
            }

            // Tránh trung tâm Sân Đình nơi đặt Vua Hùng, Sứ Giả, Thánh Gióng & Ngựa Sắt
            if (Vector2.Distance(pos, courtyardCenter) < 12.8f) return false;

            return true;
        }

        private static bool IsFarFromOtherTrees(Vector2 candidate, List<Vector2> existingTrees, float minSpacing)
        {
            for (int i = 0; i < existingTrees.Count; i++)
            {
                if (Vector2.Distance(candidate, existingTrees[i]) < minSpacing) return false;
            }
            return true;
        }

        private static float PointToSegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            float t = lenSq > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }

        private static GameObject SpawnFittedWallOrSlab(
            GameObject prefab,
            GameObject parent,
            Terrain terrain,
            Vector3 targetPos,
            float yawAlongLongAxisDeg,
            Vector3 targetLongHeightThick,
            bool snapToGround = true,
            float sinkOffset = -0.05f)
        {
            if (prefab == null) return null;
            GameObject go = Spawn(prefab, parent, targetPos, Vector3.zero, Vector3.one);
            if (go == null) return null;

            EnsureMaterialsUrpLit(go);

            if (!TryGetCombinedMeshBounds(go, out Bounds b0) || b0.size.x < 0.01f || b0.size.y < 0.01f || b0.size.z < 0.01f)
            {
                if (snapToGround) SnapToTerrain(go, terrain, sinkOffset);
                return go;
            }

            float targetLong = targetLongHeightThick.x;
            float targetHeight = targetLongHeightThick.y;
            float targetThick = targetLongHeightThick.z;

            float sx, sy, sz, finalRotY;
            sy = targetHeight / b0.size.y;

            if (b0.size.x >= b0.size.z)
            {
                sx = targetLong / b0.size.x;
                sz = targetThick / b0.size.z;
                finalRotY = yawAlongLongAxisDeg;
            }
            else
            {
                sx = targetThick / b0.size.x;
                sz = targetLong / b0.size.z;
                finalRotY = yawAlongLongAxisDeg + 90f;
            }

            go.transform.localScale = new Vector3(sx, sy, sz);
            go.transform.eulerAngles = new Vector3(0f, finalRotY, 0f);

            if (TryGetCombinedMeshBounds(go, out Bounds b1))
            {
                go.transform.position += new Vector3(targetPos.x - b1.center.x, 0f, targetPos.z - b1.center.z);
                if (snapToGround)
                {
                    SnapToTerrain(go, terrain, sinkOffset);
                }
                else
                {
                    go.transform.position += new Vector3(0f, targetPos.y - b1.min.y, 0f);
                }
            }

            return go;
        }

        private static void EnsureMaterialsUrpLit(GameObject go)
        {
            if (go == null) return;
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null || r.sharedMaterials == null) continue;
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null || m.shader == null) continue;
                    string shName = m.shader.name;
                    if (shName == "Standard" || shName == "Hidden/InternalErrorShader")
                    {
                        Texture mainTex = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : m.mainTexture;
                        Color col = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                        Texture bump = m.HasProperty("_BumpMap") ? m.GetTexture("_BumpMap") : null;

                        m.shader = urpLit;
                        if (mainTex != null && m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", mainTex);
                        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
                        if (bump != null && m.HasProperty("_BumpMap"))
                        {
                            m.SetTexture("_BumpMap", bump);
                            m.EnableKeyword("_NORMALMAP");
                        }
                        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.18f);
                        EditorUtility.SetDirty(m);
                    }
                }
            }
        }

        // =====================================================================
        // CHƯƠNG 3: TRÁNG SĨ XUẤT TRẬN – GIẢI PHÓNG THÔN XÓM VEN RỪNG (ĐÔNG NAM SÔNG, X: 146..196, Z: 48..138)
        // Đường đất đỏ xuyên rừng tre từ đầu Cầu Đông xuôi Nam, 2 nhà dân bị cướp đốt, lồng cứu dân & bãi Tướng Tiên Phong
        // =====================================================================
        private static void BuildChapter3_RaidedHamletRoad(Terrain terrain)
        {
            GameObject ch3Root = CreateGroup(GRP_CH3_ROAD, null);
            GameObject roadGroup = CreateGroup("1_DUONG_XUAT_TRAN_VEN_RUNG_TRE", ch3Root);
            GameObject hamletGroup = CreateGroup("2_XOM_NHO_BI_GIAC_DOT_PHA_VA_LONG_DAN", ch3Root);
            GameObject bossSpotGroup = CreateGroup("3_BAI_DAU_BOSS_TUONG_TIEN_PHONG", ch3Root);

            GameObject housePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri.obj");
            GameObject pBambooBig1 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_01.prefab");
            GameObject pBambooMid1 = Load(BAMBOO_PATH + "Tree_Bamboo_Mid_Green_01.prefab");
            GameObject pCampfire = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pFence2 = Load(PREFAB_PATH + "SM_Fence_2.prefab");
            GameObject pSpikes = Load(PREFAB_PATH + "SM_Spikes_01.prefab");
            GameObject pFallenTree = Load(PREFAB_PATH + "SM_FallenTree.prefab");
            GameObject pRock3 = Load(PREFAB_PATH + "SM_Rock_03.prefab");
            GameObject pBush = Load(PREFAB_PATH + "SM_Bush_01.prefab");
            GameObject pTree1 = Load(PREFAB_PATH + "SM_Tree_01.prefab");

            // 1. Rặng tre xanh dọc theo con đường đất đỏ từ đầu Cầu Đông (146, 138) dẫn xuống Xóm Ven Rừng (170, 78)
            Vector2[] bambooRoadFlanks =
            {
                new Vector2(143f, 128f), new Vector2(145f, 114f), new Vector2(147f, 100f),
                new Vector2(158f, 132f), new Vector2(159f, 118f), new Vector2(166f, 104f)
            };
            for (int i = 0; i < bambooRoadFlanks.Length; i++)
            {
                Vector2 bp = bambooRoadFlanks[i];
                GameObject bPrefab = (i % 2 == 0) ? pBambooBig1 : pBambooMid1;
                SpawnGrounded(bPrefab, roadGroup, terrain, new Vector3(bp.x, GROUND_Y, bp.y),
                    new Vector3(0f, i * 63f, 0f), Vector3.one * 1.12f, -0.06f);
            }

            // Chốt chặn nhỏ đầu tiên trên đường rừng (cọc chông + lửa giặc)
            SpawnGrounded(pSpikes, roadGroup, terrain, new Vector3(149f, GROUND_Y, 108f), new Vector3(0f, 175f, 0f), Vector3.one * 1.15f, -0.05f);
            SpawnGrounded(pCampfire, roadGroup, terrain, new Vector3(162f, GROUND_Y, 108f), Vector3.zero, Vector3.one * 1.15f, -0.02f);

            // 2. Xóm nhỏ ven rừng đang bị quân Ân đốt phá & 2 Lồng gỗ giam giữ người dân
            if (housePrefab != null)
            {
                string houseAlbedo = "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri_albedo.jpg";
                string houseNormal = "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri_normal.jpg";
                string houseRough = "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri_metallic_roughness.jpg";

                GameObject h1 = Spawn(housePrefab, hamletGroup, new Vector3(154f, GROUND_Y, 86f), new Vector3(0f, 112f, 0f), Vector3.one * 3.75f);
                if (h1 != null)
                {
                    h1.name = "NhaDanVenRung_BiCuopPha_01";
                    SetupModelMaterial(h1, "M_NhaSan_ChimLac", houseAlbedo, houseNormal, houseRough);
                    SnapToTerrain(h1, terrain, -0.04f);
                    AddFittedBoxCollider(h1);
                }

                GameObject h2 = Spawn(housePrefab, hamletGroup, new Vector3(156f, GROUND_Y, 57f), new Vector3(0f, 52f, 0f), Vector3.one * 3.75f);
                if (h2 != null)
                {
                    h2.name = "NhaDanVenRung_BiCuopPha_02";
                    SetupModelMaterial(h2, "M_NhaSan_ChimLac", houseAlbedo, houseNormal, houseRough);
                    SnapToTerrain(h2, terrain, -0.04f);
                    AddFittedBoxCollider(h2);
                }
            }

            // Lửa cháy & đổ nát bên ngoài xóm bị cướp + 2 Lồng Gỗ Meshy AI nhốt dân làng cần giải cứu
            SpawnGrounded(pCampfire, hamletGroup, terrain, new Vector3(161.5f, GROUND_Y, 83.5f), Vector3.zero, Vector3.one * 1.45f, -0.02f);
            SpawnGrounded(pCampfire, hamletGroup, terrain, new Vector3(163.0f, GROUND_Y, 61.5f), Vector3.zero, Vector3.one * 1.35f, -0.02f);
            SpawnGrounded(pFallenTree, hamletGroup, terrain, new Vector3(162.5f, GROUND_Y, 93.0f), new Vector3(0f, 35f, 0f), Vector3.one * 1.05f, -0.04f);
            SpawnWoodenPrisonerCage("LongGo_GiamDanLang_01", hamletGroup, terrain, pFence2, new Vector2(169.5f, 89.5f), 20f);
            SpawnWoodenPrisonerCage("LongGo_GiamDanLang_02", hamletGroup, terrain, pFence2, new Vector2(169.5f, 60.5f), -25f);
            SpawnGrounded(pTree1, hamletGroup, terrain, new Vector3(149f, GROUND_Y, 72f), new Vector3(0f, 80f, 0f), Vector3.one * 1.15f, -0.05f);
            if (pBush != null)
            {
                SpawnGrounded(pBush, hamletGroup, terrain, new Vector3(180f, GROUND_Y, 55f), Vector3.zero, Vector3.one * 1.2f);
                SpawnGrounded(pBush, hamletGroup, terrain, new Vector3(183f, GROUND_Y, 97f), Vector3.zero, Vector3.one * 1.2f);
            }

            // 3. Khoảng sân phẳng giao chiến Boss Chương 3: Tướng Tiên Phong (Tâm X=184, Z=76) & Lều Chỉ Huy Tiên Phong
            GameObject pWarTent = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_LEU_TRAI + ".fbx");
            if (pWarTent != null)
            {
                SpawnFittedArchitecture(
                    pWarTent, "LeuChiHuy_TienPhong_GiacAn", bossSpotGroup, terrain,
                    new Vector2(188.5f, 90.0f), -135f, 5.8f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);
            }

            GameObject bossMarker = CreateGroup("DiemXuatHien_Boss_TuongTienPhong", bossSpotGroup);
            bossMarker.transform.position = new Vector3(184f, GROUND_Y, 76f);
            SpawnGrounded(pRock3, bossSpotGroup, terrain, new Vector3(193.5f, GROUND_Y, 83.5f), new Vector3(0f, 25f, 0f), Vector3.one * 1.2f);
            SpawnGrounded(pRock3, bossSpotGroup, terrain, new Vector3(191.5f, GROUND_Y, 66.5f), new Vector3(0f, -40f, 0f), Vector3.one * 1.2f);
        }

        // =====================================================================
        // CHƯƠNG 4: CÔNG PHÁ TIỀN ĐỒN GIẶC ÂN KIÊN CỐ (CỰC ĐÔNG NAM, X: 204..254, Z: 52..108)
        // Hàng rào cọc chông nhọn, 2 Tháp Canh Gỗ cao 8.4m, 3 Lều Doanh Trại, Lồng Gỗ & bãi đấu Tướng Phòng Tuyến
        // =====================================================================
        private static void BuildChapter4_AnInvaderFortress(Terrain terrain)
        {
            GameObject ch4Root = CreateGroup(GRP_CH4_FORT, null);
            GameObject wallTowerGroup = CreateGroup("1_TUONG_THANH_CHONG_NHON_VA_2_THAP_CANH", ch4Root);
            GameObject garrisonGroup = CreateGroup("2_DOANH_TRAI_VA_BOSS_TUONG_PHONG_TUYEN", ch4Root);

            GameObject pStoneWall1 = Load(PREFAB_PATH + "SM_StoneWall_01.prefab");
            GameObject pPlatform3 = Load(PREFAB_PATH + "SM_Platform_03.prefab");
            GameObject pFence1 = Load(PREFAB_PATH + "SM_Fence_1.prefab");
            GameObject pFence2 = Load(PREFAB_PATH + "SM_Fence_2.prefab");
            GameObject pSpikes = Load(PREFAB_PATH + "SM_Spikes_01.prefab");
            GameObject pCampfire = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pRock7 = Load(PREFAB_PATH + "SM_Rock_07.prefab");

            // 1. Hai Tháp Canh Gỗ Cung Thủ cao 8.4m gác hai bên Cổng Chính Tiền Đồn (X=208, Z=85.2 & Z=66.8 - cách đều Cổng Đồn 11.2m ở giữa!)
            BuildArcherWatchtower("ThapCanh_CungThu_Bac", wallTowerGroup, terrain, pPlatform3, pFence1, pCampfire, new Vector2(208.0f, 85.2f));
            BuildArcherWatchtower("ThapCanh_CungThu_Nam", wallTowerGroup, terrain, pPlatform3, pFence1, pCampfire, new Vector2(208.0f, 66.8f));

            // Tường đá & hàng rào cọc chông nhọn bao quanh Tiền Đồn (chừa Cổng Tây tại Z=64..88 & Cổng Hậu Bắc rộng 11m tại X=218.5..229.5)
            SpawnFittedWallOrSlab(pStoneWall1, wallTowerGroup, terrain, new Vector3(206.5f, GROUND_Y, 94.5f), 90f, new Vector3(13.0f, 1.85f, 1.1f));
            SpawnFittedWallOrSlab(pStoneWall1, wallTowerGroup, terrain, new Vector3(206.5f, GROUND_Y, 58.5f), 90f, new Vector3(11.0f, 1.85f, 1.1f));
            SpawnFittedWallOrSlab(pStoneWall1, wallTowerGroup, terrain, new Vector3(228.0f, GROUND_Y, 53.5f), 0f,  new Vector3(42.0f, 1.95f, 1.2f));
            SpawnFittedWallOrSlab(pStoneWall1, wallTowerGroup, terrain, new Vector3(250.5f, GROUND_Y, 80.5f), 90f, new Vector3(52.0f, 2.05f, 1.2f));
            SpawnFittedWallOrSlab(pStoneWall1, wallTowerGroup, terrain, new Vector3(212.5f, GROUND_Y, 108.0f), 0f, new Vector3(12.0f, 1.85f, 1.1f));
            SpawnFittedWallOrSlab(pStoneWall1, wallTowerGroup, terrain, new Vector3(240.0f, GROUND_Y, 108.0f), 0f, new Vector3(21.0f, 1.85f, 1.1f));

            // Dàn chông gỗ nhọn phòng thủ mặt trước doanh trại
            if (pSpikes != null)
            {
                SpawnGrounded(pSpikes, wallTowerGroup, terrain, new Vector3(203.2f, GROUND_Y, 89f), new Vector3(0f, -90f, 0f), Vector3.one * 1.45f, -0.05f);
                SpawnGrounded(pSpikes, wallTowerGroup, terrain, new Vector3(203.2f, GROUND_Y, 98f), new Vector3(0f, -90f, 0f), Vector3.one * 1.45f, -0.05f);
                SpawnGrounded(pSpikes, wallTowerGroup, terrain, new Vector3(203.2f, GROUND_Y, 63f), new Vector3(0f, -90f, 0f), Vector3.one * 1.45f, -0.05f);
                SpawnGrounded(pSpikes, wallTowerGroup, terrain, new Vector3(203.2f, GROUND_Y, 55f), new Vector3(0f, -90f, 0f), Vector3.one * 1.45f, -0.05f);
            }

            // 2. Doanh Trại Giặc Ân: 3 Lều Quân Sự + 1 Lồng Gỗ góc trại (chừa lòng sân giữa rộng 28m x 32m cho Boss Tướng Phòng Tuyến!)
            GameObject pWarTent = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_LEU_TRAI + ".fbx");
            if (pWarTent != null)
            {
                SpawnFittedArchitecture(
                    pWarTent, "LeuDoanhTrai_GiacAn_01_DongBac", garrisonGroup, terrain,
                    new Vector2(242.5f, 97.5f), -135f, 6.2f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);

                SpawnFittedArchitecture(
                    pWarTent, "LeuDoanhTrai_GiacAn_02_DongNam", garrisonGroup, terrain,
                    new Vector2(242.5f, 60.5f), -45f, 6.2f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);

                SpawnFittedArchitecture(
                    pWarTent, "LeuDoanhTrai_GiacAn_03_TayNam", garrisonGroup, terrain,
                    new Vector2(216.5f, 60.5f), 35f, 5.8f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);
            }

            SpawnWoodenPrisonerCage("LongGo_GiamDan_TienDon_Ch4", garrisonGroup, terrain, pFence2, new Vector2(216.5f, 97.5f), 145f);

            SpawnGrounded(pCampfire, garrisonGroup, terrain, new Vector3(236.0f, GROUND_Y, 95.0f), Vector3.zero, Vector3.one * 1.45f, -0.02f);
            SpawnGrounded(pCampfire, garrisonGroup, terrain, new Vector3(236.0f, GROUND_Y, 63.0f), Vector3.zero, Vector3.one * 1.45f, -0.02f);
            SpawnGrounded(pRock7, garrisonGroup, terrain, new Vector3(246.5f, GROUND_Y, 79.0f), new Vector3(0f, 45f, 0f), Vector3.one * 1.55f, -0.15f);

            GameObject boss4Marker = CreateGroup("DiemXuatHien_Boss_TuongPhongTuyen", garrisonGroup);
            boss4Marker.transform.position = new Vector3(230f, GROUND_Y, 79f);
        }

        private static void BuildArcherWatchtower(
            string name,
            GameObject parent,
            Terrain terrain,
            GameObject platformPrefab,
            GameObject fencePrefab,
            GameObject firePrefab,
            Vector2 center,
            float rotY = -90f)
        {
            GameObject pTowerFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_THAP_CANH + ".fbx");
            if (pTowerFbx != null)
            {
                SpawnFittedArchitecture(
                    pTowerFbx, name, parent, terrain,
                    center, rotY, 8.4f, true,
                    "M_ThapCanh_GiacAn", ARCH_THAP_CANH, 0.04f, 0.22f, false, true, -0.06f);
                return;
            }

            GameObject tower = CreateGroup(name, parent);
            float baseY = SampleTerrainY(terrain, center.x, center.y);
            tower.transform.position = new Vector3(center.x, baseY, center.y);

            // Fallback: Thân đài cao 2.6m (3.4m x 3.4m) + lan can chắn tên 4 mặt trên đỉnh đài
            SpawnFittedWallOrSlab(platformPrefab, tower, terrain, new Vector3(center.x, baseY - 0.1f, center.y),
                0f, new Vector3(3.4f, 2.7f, 3.4f), false);

            float topY = baseY + 2.55f;
            Vector3 railSize = new Vector3(3.1f, 0.95f, 0.25f);
            SpawnFittedWallOrSlab(fencePrefab, tower, terrain, new Vector3(center.x, topY, center.y + 1.45f), 0f, railSize, false);
            SpawnFittedWallOrSlab(fencePrefab, tower, terrain, new Vector3(center.x, topY, center.y - 1.45f), 0f, railSize, false);
            SpawnFittedWallOrSlab(fencePrefab, tower, terrain, new Vector3(center.x - 1.45f, topY, center.y), 90f, railSize, false);

            if (firePrefab != null)
            {
                Spawn(firePrefab, tower, new Vector3(center.x, topY + 0.05f, center.y), Vector3.zero, Vector3.one * 0.85f);
            }
        }

        // =====================================================================
        // CHƯƠNG 5: ĐẠI CHIẾN BÃI SẬY – ROI SẮT BỊ GÃY (TRUNG TÂM BỜ ĐÔNG, X: 168..250, Z: 114..176)
        // Sàn đấu phẳng lớn không vật cản + Tuyến Tháp Canh & Lều Chỉ Huy quân Ân rìa Đông + Vách đá chặn đi tắt từ Cầu Gỗ!
        // =====================================================================
        private static void BuildChapter5_BaiSayBattleArena(Terrain terrain)
        {
            GameObject ch5Root = CreateGroup(GRP_ARENA, null);
            GameObject dividerGroup = CreateGroup("1_VACH_DA_TRE_CHAN_DI_TAT_TU_CAU_GO", ch5Root);
            GameObject arenaGroup = CreateGroup("2_SAN_DAU_DAI_CHIEN_BAI_SAY", ch5Root);
            GameObject siegeLineGroup = CreateGroup("3_TUYEN_BAO_VAY_THAP_CANH_VA_LEU_CHI_HUY_DONG", ch5Root);

            GameObject pRock7 = Load(PREFAB_PATH + "SM_Rock_07.prefab");
            GameObject pRock5 = Load(PREFAB_PATH + "SM_Rock_05.prefab");
            GameObject pRock3 = Load(PREFAB_PATH + "SM_Rock_03.prefab");
            GameObject pPlatform3 = Load(PREFAB_PATH + "SM_Platform_03.prefab");
            GameObject pCampfire = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pFence1 = Load(PREFAB_PATH + "SM_Fence_1.prefab");
            GameObject pSpikes = Load(PREFAB_PATH + "SM_Spikes_01.prefab");
            GameObject pBambooBig1 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_01.prefab");
            GameObject pBambooBig2 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_02.prefab");

            // 1. Dải vách đá & lũy tre tự nhiên dọc X = 163 (Z = 112..180) ngăn tách hoàn toàn Đầu Cầu Đông (Chương 3) với Đại Chiến Trường (Chương 5)
            for (float z = 114f; z <= 178f; z += 8.0f)
            {
                GameObject rPrefab = (((int)z / 8) % 2 == 0) ? pRock7 : pRock5;
                SpawnGrounded(rPrefab, dividerGroup, terrain, new Vector3(163.0f, GROUND_Y, z),
                    new Vector3(0f, z * 23f, 0f), Vector3.one * 1.55f, -0.18f);

                GameObject bPrefab = (((int)z / 8) % 2 == 0) ? pBambooBig1 : pBambooBig2;
                SpawnGrounded(bPrefab, dividerGroup, terrain, new Vector3(160.2f, GROUND_Y, z + 3.5f),
                    new Vector3(0f, z * 41f, 0f), Vector3.one * 1.15f, -0.06f);
            }
            CreateInvisibleBlocker("Collider_ChanDiTat_CauGoVaoChienTruong", dividerGroup,
                new Vector3(162.5f, GROUND_Y + 3f, 146f), new Vector3(5.5f, 8f, 70f));

            // 2. Sàn Đấu Đại Chiến Bãi Sậy (Tâm X=204, Z=146, rộng 66m x 54m HOÀN TOÀN PHẲNG cho đại quân & Ngựa Sắt giao chiến)
            Vector3[] arenaCornerBraziers =
            {
                new Vector3(172f, GROUND_Y, 171f),
                new Vector3(236f, GROUND_Y, 171f),
                new Vector3(172f, GROUND_Y, 117f),
                new Vector3(236f, GROUND_Y, 117f)
            };

            for (int i = 0; i < arenaCornerBraziers.Length; i++)
            {
                SpawnGrounded(pCampfire, arenaGroup, terrain, arenaCornerBraziers[i], Vector3.zero, Vector3.one * 1.45f, -0.02f);
                SpawnGrounded(pRock3, arenaGroup, terrain, arenaCornerBraziers[i] + new Vector3(2.2f, 0f, 0f),
                    new Vector3(0f, i * 90f, 0f), Vector3.one * 1.1f);
            }

            // 3. Tuyến bao vây của đại quân Giặc Ân dọc rìa Đông chiến trường (X = 245.5m - nằm ngoài sàn đấu kỵ binh, không vướng combat!)
            BuildArcherWatchtower("ThapCanh_ChienTruong_Bac", siegeLineGroup, terrain, pPlatform3, pFence1, pCampfire, new Vector2(245.5f, 163.5f), -90f);
            BuildArcherWatchtower("ThapCanh_ChienTruong_Nam", siegeLineGroup, terrain, pPlatform3, pFence1, pCampfire, new Vector2(245.5f, 128.5f), -90f);

            GameObject pWarTent = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_LEU_TRAI + ".fbx");
            if (pWarTent != null)
            {
                SpawnFittedArchitecture(
                    pWarTent, "LeuChiHuy_BaiSay_01_Bac", siegeLineGroup, terrain,
                    new Vector2(245.0f, 152.5f), -95f, 6.2f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);

                SpawnFittedArchitecture(
                    pWarTent, "LeuChiHuy_BaiSay_02_Nam", siegeLineGroup, terrain,
                    new Vector2(245.0f, 139.5f), -85f, 6.2f, false,
                    "M_LeuTrai_GiacAn", ARCH_LEU_TRAI, 0.04f, 0.22f, false, true, -0.05f);
            }
            else if (pFence1 != null)
            {
                SpawnGrounded(pFence1, siegeLineGroup, terrain, new Vector3(246f, GROUND_Y, 132f), new Vector3(0f, 90f, 0f), Vector3.one * 1.3f);
                SpawnGrounded(pFence1, siegeLineGroup, terrain, new Vector3(246f, GROUND_Y, 146f), new Vector3(0f, 90f, 0f), Vector3.one * 1.3f);
                SpawnGrounded(pFence1, siegeLineGroup, terrain, new Vector3(246f, GROUND_Y, 160f), new Vector3(0f, 90f, 0f), Vector3.one * 1.3f);
            }

            SpawnGrounded(pCampfire, siegeLineGroup, terrain, new Vector3(240.2f, GROUND_Y, 146.0f), Vector3.zero, Vector3.one * 1.35f, -0.02f);
            if (pSpikes != null)
            {
                SpawnGrounded(pSpikes, siegeLineGroup, terrain, new Vector3(241.0f, GROUND_Y, 158.0f), new Vector3(0f, -90f, 0f), Vector3.one * 1.3f, -0.05f);
                SpawnGrounded(pSpikes, siegeLineGroup, terrain, new Vector3(241.0f, GROUND_Y, 134.0f), new Vector3(0f, -90f, 0f), Vector3.one * 1.3f, -0.05f);
            }

            GameObject whipBreakSpot = CreateGroup("TamSanDau_SuKienRoiSatGay_Ch5", arenaGroup);
            whipBreakSpot.transform.position = new Vector3(204f, GROUND_Y, 146f);
        }

        // =====================================================================
        // CHƯƠNG 6: TRE NGÀ QUẬT GIẶC – HẠ GỤC ĐẠI TƯỚNG & HỒ NƯỚC RỪNG TRE (ĐÔNG BẮC, X: 148..214, Z: 182..250)
        // =====================================================================
        private static void BuildZone3_SacredBambooAndPond(Terrain terrain)
        {
            GameObject bambooZoneRoot = CreateGroup(GRP_BAMBOO, null);
            GameObject sanctuaryGroup = CreateGroup("1_THANH_DIA_TRE_NGA_NHAN_E_NHO_TRE", bambooZoneRoot);
            GameObject forestGroup = CreateGroup("2_RUNG_TRE_XANH_DAI_NGAN", bambooZoneRoot);
            GameObject pondGroup = CreateGroup("3_HO_NUOC_RUNG_TRE", bambooZoneRoot);

            GameObject pBambooBigGreen1 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_01.prefab");
            GameObject pBambooBigGreen2 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Green_02.prefab");
            GameObject pBambooMidGreen1 = Load(BAMBOO_PATH + "Tree_Bamboo_Mid_Green_01.prefab");
            GameObject pBambooMidGreen2 = Load(BAMBOO_PATH + "Tree_Bamboo_Mid_Green_02.prefab");
            GameObject pBambooSmallGreen1 = Load(BAMBOO_PATH + "Tree_Bamboo_Small_Green_01.prefab");
            GameObject pBambooBigDry1 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Dry_01.prefab");
            GameObject pBambooBigDry2 = Load(BAMBOO_PATH + "Tree_Bamboo_Big_Dry_02.prefab");
            GameObject pBambooMidDry1 = Load(BAMBOO_PATH + "Tree_Bamboo_Mid_Dry_01.prefab");

            GameObject pRuneStone = Load(PREFAB_PATH + "SM_RuneStone_02.prefab");
            GameObject pSignPost = Load(PREFAB_PATH + "SM_SignPost_01.prefab");
            GameObject pPebble = Load(PREFAB_PATH + "SM_Pebble_01.prefab");
            GameObject pRock3 = Load(PREFAB_PATH + "SM_Rock_03.prefab");
            GameObject pWater = Load(PREFAB_PATH + "SM_Water.prefab");
            GameObject pFirefly = FindOptionalPrefab("Firefly");

            // 1. Thánh Địa Tre Ngà (Tâm X=188, Z=204 - thẳng hướng Bắc từ cổng thoát Đại Chiến Trường Chương 5!)
            Vector3 gladeCenter = new Vector3(188f, GROUND_Y, 204f);

            SpawnGrounded(pRuneStone, sanctuaryGroup, terrain, gladeCenter + new Vector3(-3.8f, 0f, -2.5f), new Vector3(0f, 35f, 0f), Vector3.one * 1.35f);
            SpawnGrounded(pSignPost, sanctuaryGroup, terrain, new Vector3(182f, GROUND_Y, 184f), new Vector3(0f, 15f, 0f), Vector3.one * 1.3f);

            GameObject g1 = SpawnGrounded(pBambooBigDry1, sanctuaryGroup, terrain, gladeCenter + new Vector3(0f, 0f, 1.5f), new Vector3(0f, 35f, 0f), Vector3.one * 1.30f, -0.06f);
            if (g1 != null) g1.name = "TreNga_ThanThanh_01";

            GameObject g2 = SpawnGrounded(pBambooBigDry2, sanctuaryGroup, terrain, gladeCenter + new Vector3(3.2f, 0f, 0.2f), new Vector3(0f, 120f, 0f), Vector3.one * 1.22f, -0.06f);
            if (g2 != null) g2.name = "TreNga_ThanThanh_02";

            GameObject g3 = SpawnGrounded(pBambooMidDry1, sanctuaryGroup, terrain, gladeCenter + new Vector3(-2.8f, 0f, 0.8f), new Vector3(0f, 215f, 0f), Vector3.one * 1.18f, -0.06f);
            if (g3 != null) g3.name = "TreNga_ThanThanh_03";

            if (pPebble != null)
            {
                for (int a = 0; a < 6; a++)
                {
                    float rad = a * 60f * Mathf.Deg2Rad;
                    Vector3 pebblePos = gladeCenter + new Vector3(Mathf.Cos(rad) * 5.2f, 0f, Mathf.Sin(rad) * 5.2f);
                    SpawnGrounded(pPebble, sanctuaryGroup, terrain, pebblePos, new Vector3(0f, a * 60f, 0f), Vector3.one * 1.25f);
                }
            }

            if (pFirefly != null)
            {
                Spawn(pFirefly, sanctuaryGroup, gladeCenter + new Vector3(0f, 1.8f, 0f), Vector3.zero, Vector3.one);
            }

            GameObject finalBossSpot = CreateGroup("DiemXuatHien_FinalBoss_DaiTuongGiacAn", sanctuaryGroup);
            finalBossSpot.transform.position = new Vector3(198f, GROUND_Y, 186f);

            // 2. Rừng Tre Xanh Đại Ngàn (Giới hạn X <= 213m để không một nhành tre nào lấn sang chân Dốc Núi Sóc Chương 7!)
            float[] ringRadii = { 14.0f, 21.5f, 29.0f };
            int[] ringCounts = { 9, 13, 16 };

            for (int rIdx = 0; rIdx < ringRadii.Length; rIdx++)
            {
                float radius = ringRadii[rIdx];
                int count = ringCounts[rIdx];

                for (int i = 0; i < count; i++)
                {
                    float angleDeg = Mathf.Lerp(-12f, 195f, (float)i / (count - 1));
                    float angleRad = angleDeg * Mathf.Deg2Rad;

                    float bx = gladeCenter.x + Mathf.Cos(angleRad) * radius + Random.Range(-1.2f, 1.2f);
                    float bz = gladeCenter.z + Mathf.Sin(angleRad) * radius + Random.Range(-1.2f, 1.2f);

                    if (GetPondNormalizedDist(bx, bz) < 1.18f) continue;
                    if (bx < 148f || bx > 213f || bz > 250f || bz < 190f) continue;

                    GameObject prefab;
                    int pick = (i + rIdx) % 4;
                    if (pick == 0) prefab = pBambooBigGreen1;
                    else if (pick == 1) prefab = pBambooBigGreen2;
                    else if (pick == 2) prefab = pBambooMidGreen1;
                    else prefab = pBambooMidGreen2;

                    SpawnGrounded(prefab, forestGroup, terrain, new Vector3(bx, GROUND_Y, bz),
                        new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * Random.Range(1.05f, 1.30f), -0.06f);
                }
            }

            if (pBambooSmallGreen1 != null)
            {
                Vector2[] sproutPositions =
                {
                    new Vector2(174f, 194f), new Vector2(202f, 194f),
                    new Vector2(172f, 212f), new Vector2(204f, 215f)
                };
                foreach (Vector2 sp in sproutPositions)
                {
                    if (GetPondNormalizedDist(sp.x, sp.y) > 1.18f)
                    {
                        SpawnGrounded(pBambooSmallGreen1, forestGroup, terrain, new Vector3(sp.x, GROUND_Y, sp.y),
                            new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.2f, -0.05f);
                    }
                }
            }

            // 3. Hồ Nước Rừng Tre (Tâm X=162, Z=232, riêng biệt hoàn toàn giữa bờ sông và rừng tre)
            if (pWater != null)
            {
                GameObject pondWater = Spawn(pWater, pondGroup, new Vector3(POND_CENTER.x, WATER_Y, POND_CENTER.y),
                    Vector3.zero, new Vector3(2.0f, 1f, 1.75f));
                if (pondWater != null) pondWater.name = "MatNuoc_HoRungTre";
            }

            int pondDecorCount = 10;
            for (int i = 0; i < pondDecorCount; i++)
            {
                float ang = (i * (360f / pondDecorCount)) * Mathf.Deg2Rad;
                float px = POND_CENTER.x + Mathf.Cos(ang) * (POND_RADIUS_X + 1.1f);
                float pz = POND_CENTER.y + Mathf.Sin(ang) * (POND_RADIUS_Z + 1.1f);

                if (pPebble != null && i % 2 == 0)
                {
                    SpawnGrounded(pPebble, pondGroup, terrain, new Vector3(px, GROUND_Y, pz),
                        new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.25f);
                }
                else if (pRock3 != null && i % 3 == 1)
                {
                    SpawnGrounded(pRock3, pondGroup, terrain, new Vector3(px, GROUND_Y, pz),
                        new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.05f);
                }

                if (Mathf.Sin(ang) > 0.1f && i % 2 == 1)
                {
                    float tx = POND_CENTER.x + Mathf.Cos(ang) * (POND_RADIUS_X + 3.4f);
                    float tz = POND_CENTER.y + Mathf.Sin(ang) * (POND_RADIUS_Z + 3.4f);
                    if (tx > 147f && tx < 212f)
                    {
                        SpawnGrounded(pBambooMidGreen2, pondGroup, terrain, new Vector3(tx, GROUND_Y, tz),
                            new Vector3(0f, Random.Range(0f, 360f), 0f), Vector3.one * 1.12f, -0.06f);
                    }
                }
            }

            if (pFirefly != null)
            {
                Spawn(pFirefly, pondGroup, new Vector3(POND_CENTER.x, WATER_Y + 1.4f, POND_CENTER.y), Vector3.zero, Vector3.one);
            }
        }

        // =====================================================================
        // CHƯƠNG 7: ĐỈNH NÚI SÓC – HUYỀN THOẠI BAY VỀ TRỜI (CAO ĐIỂM ĐÔNG BẮC, X: 220..262, Z: 184..258)
        // Con dốc cỏ xanh thoai thoải cao dần từ 10.0m -> 21.5m + Phiến đá để lại Nón Sắt & Giáp Sắt trên đỉnh núi
        // =====================================================================
        private static void BuildChapter7_SocMountainSummit(Terrain terrain)
        {
            GameObject ch7Root = CreateGroup(GRP_CH7_SOC, null);
            GameObject slopeGroup = CreateGroup("1_CON_DOC_CO_XANH_LEN_NUI_SOC", ch7Root);
            GameObject summitGroup = CreateGroup("2_DINH_NUI_SOC_PHIEN_DA_COI_GIAP_VE_TROI", ch7Root);

            GameObject pPlatform3 = Load(PREFAB_PATH + "SM_Platform_03.prefab");
            GameObject pRuneStone2 = Load(PREFAB_PATH + "SM_RuneStone_02.prefab");
            GameObject pCampfire = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pFir = Load(PREFAB_PATH + "Fir.prefab");
            GameObject pFlower1 = Load(PREFAB_PATH + "SM_Flower_01.prefab");
            GameObject pFlower2 = Load(PREFAB_PATH + "SM_Flower_02.prefab");
            GameObject pGrass1 = Load(PREFAB_PATH + "SM_Grass_01.prefab");
            GameObject pGrass3 = Load(PREFAB_PATH + "SM_Grass_03.prefab");
            GameObject pPebble = Load(PREFAB_PATH + "SM_Pebble_01.prefab");
            GameObject pRock3 = Load(PREFAB_PATH + "SM_Rock_03.prefab");
            GameObject pFirefly = FindOptionalPrefab("Firefly");

            // 1. Dọc hai bên sườn con dốc cỏ xanh lên Đỉnh Núi Sóc (chừa lòng đường dốc ở giữa rộng 10m thông thoáng)
            Vector2 dir = (SOC_SUMMIT_CENTER - SOC_RAMP_START).normalized;
            Vector2 perp = new Vector2(dir.y, -dir.x);

            for (int step = 1; step <= 5; step++)
            {
                float t = step / 6.2f;
                Vector2 center = Vector2.Lerp(SOC_RAMP_START, SOC_SUMMIT_CENTER, t);

                Vector2 leftFlank = center - perp * 5.6f;
                Vector2 rightFlank = center + perp * 5.6f;

                SpawnGrounded((step % 2 == 0) ? pFlower1 : pFlower2, slopeGroup, terrain,
                    new Vector3(leftFlank.x, GROUND_Y, leftFlank.y), new Vector3(0f, step * 55f, 0f), Vector3.one * 1.3f, -0.05f);
                SpawnGrounded((step % 2 == 0) ? pGrass1 : pGrass3, slopeGroup, terrain,
                    new Vector3(rightFlank.x, GROUND_Y, rightFlank.y), new Vector3(0f, step * 75f, 0f), Vector3.one * 1.25f, -0.05f);

                if (step % 2 == 1 && pFir != null)
                {
                    Vector2 pinePos = center + perp * ((step == 3) ? -7.2f : 7.2f);
                    SpawnGrounded(pFir, slopeGroup, terrain, new Vector3(pinePos.x, GROUND_Y, pinePos.y),
                        new Vector3(0f, step * 95f, 0f), Vector3.one * 1.15f, -0.18f);
                }
            }

            // 2. Đỉnh Núi Sóc Phẳng (Y = 21.5m tại X=245, Z=246, đường kính 21m) nhìn bao quát toàn bộ Thung Lũng Phù Đổng
            // Đài Đá Thiêng Đỉnh Núi Sóc Meshy AI (nơi Thánh Gióng cởi Nón Sắt & Bộ Giáp Sắt Hộ Tâm để lại cho non sông trước khi cưỡi Ngựa Sắt bay về trời!)
            Vector2 altarCenter2D = new Vector2(SOC_SUMMIT_CENTER.x + 0.6f, SOC_SUMMIT_CENTER.y + 2.2f);
            GameObject pAltarFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_DAI_DA_SOC + ".fbx");
            if (pAltarFbx != null)
            {
                SpawnFittedArchitecture(
                    pAltarFbx, "DaiDaThieng_CoiGiap_NuiSoc", summitGroup, terrain,
                    altarCenter2D, -163f, 7.4f, false,
                    "M_DaiDaThieng_NuiSoc", ARCH_DAI_DA_SOC, 0.05f, 0.24f, true, true, -0.06f);
            }
            else
            {
                GameObject armorStone = SpawnFittedWallOrSlab(pPlatform3, summitGroup, terrain,
                    new Vector3(SOC_SUMMIT_CENTER.x - 2.6f, SOC_SUMMIT_Y, SOC_SUMMIT_CENTER.y + 0.5f),
                    15f, new Vector3(3.4f, 0.72f, 2.4f), true, -0.06f);
                if (armorStone != null) armorStone.name = "PhienDaLon_DeLaiNonSat_Va_GiapSat";
            }

            // Bia đá tưởng niệm Phù Đổng Thiên Vương & 2 đài lửa bình minh chầu hai bên bậc thềm Đài Đá Thiêng (cách tâm đài > 6.5m, không đè!)
            GameObject stele = SpawnGrounded(pRuneStone2, summitGroup, terrain,
                new Vector3(SOC_SUMMIT_CENTER.x - 5.8f, SOC_SUMMIT_Y, SOC_SUMMIT_CENTER.y + 3.2f),
                new Vector3(0f, 135f, 0f), Vector3.one * 1.45f, -0.05f);
            if (stele != null) stele.name = "BiaDaThieng_PhuDongThienVuong";

            SpawnGrounded(pCampfire, summitGroup, terrain, new Vector3(SOC_SUMMIT_CENTER.x - 5.2f, SOC_SUMMIT_Y, SOC_SUMMIT_CENTER.y - 1.2f),
                Vector3.zero, Vector3.one * 1.15f, -0.03f);
            SpawnGrounded(pCampfire, summitGroup, terrain, new Vector3(SOC_SUMMIT_CENTER.x + 5.2f, SOC_SUMMIT_Y, SOC_SUMMIT_CENTER.y - 2.4f),
                Vector3.zero, Vector3.one * 1.15f, -0.03f);

            for (int a = 0; a < 10; a++)
            {
                float rad = a * 36f * Mathf.Deg2Rad;
                Vector2 ringPos2D = new Vector2(SOC_SUMMIT_CENTER.x + Mathf.Cos(rad) * 8.8f, SOC_SUMMIT_CENTER.y + Mathf.Sin(rad) * 8.8f);
                // Chừa lối dốc đi lên phía Tây Nam và tránh chạm vào chân Đài Đá Thiêng
                if (Vector2.Distance(ringPos2D, altarCenter2D) < 4.9f) continue;
                if (ringPos2D.y < SOC_SUMMIT_CENTER.y - 6.2f && Mathf.Abs(ringPos2D.x - (SOC_SUMMIT_CENTER.x - 1.5f)) < 4.5f) continue;

                SpawnGrounded((a % 2 == 0) ? pPebble : pFlower2, summitGroup, terrain,
                    new Vector3(ringPos2D.x, SOC_SUMMIT_Y, ringPos2D.y),
                    new Vector3(0f, a * 36f, 0f), Vector3.one * 1.25f, -0.04f);
            }
            SpawnGrounded(pRock3, summitGroup, terrain, new Vector3(SOC_SUMMIT_CENTER.x + 6.4f, SOC_SUMMIT_Y, SOC_SUMMIT_CENTER.y + 2.8f),
                new Vector3(0f, 30f, 0f), Vector3.one * 1.2f, -0.06f);

            // Điểm Trigger Kết Thúc Game ngay sân trước bậc thềm Đài Đá Thiêng: Ngựa sắt hí vang đạp mây bay thẳng về trời xanh
            Vector2 triggerPos2D = new Vector2(SOC_SUMMIT_CENTER.x - 0.8f, SOC_SUMMIT_CENTER.y - 3.6f);
            GameObject ascendTrigger = CreateGroup("TriggerKetThuc_NguaSatBayVeTroi_Victory", summitGroup);
            ascendTrigger.transform.position = new Vector3(triggerPos2D.x, SampleTerrainY(terrain, triggerPos2D.x, triggerPos2D.y), triggerPos2D.y);
            BoxCollider victoryCol = ascendTrigger.AddComponent<BoxCollider>();
            victoryCol.center = new Vector3(0f, 1.5f, 0f);
            victoryCol.size = new Vector3(6.5f, 3.5f, 6.5f);
            victoryCol.isTrigger = true;

            if (pFirefly != null)
            {
                Spawn(pFirefly, summitGroup, ascendTrigger.transform.position + new Vector3(0f, 1.5f, 0f), Vector3.zero, Vector3.one * 1.4f);
            }
        }

        // =====================================================================
        // HỆ THỐNG 6 CỔNG KHÓA CHUYỂN CHƯƠNG (BẮT BUỘC XONG MÀN TRƯỚC MỚI QUA MÀN SAU)
        // Mỗi Gate có sẵn mô hình 3D Cổng Đồn / Cánh Cổng chắn lối + BoxCollider; khi hoàn thành chương chỉ cần mở/tắt Gate!
        // =====================================================================
        private static void BuildAllChapterProgressionGates(Terrain terrain)
        {
            GameObject gatesRoot = CreateGroup(GRP_GATES, null);

            GameObject pFence1 = Load(PREFAB_PATH + "SM_Fence_1.prefab");
            GameObject pFence2 = Load(PREFAB_PATH + "SM_Fence_2.prefab");
            GameObject pSpikes = Load(PREFAB_PATH + "SM_Spikes_01.prefab");
            GameObject pStoneWall2 = Load(PREFAB_PATH + "SM_StoneWall_02.prefab");
            GameObject pCampfire = Load(PREFAB_PATH + "Campfire with fire.prefab");
            GameObject pFortGateFbx = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_CONG_DON + ".fbx");

            // GATE 1: Cổng Bắc Làng lên Mỏ Quặng Sắt (Mở sẵn ở Chương 1 để Gióng đi lấy quặng; có thể đóng lại khi bảo vệ Lò Rèn ở Chương 2)
            GameObject gate1 = CreateGroup("Gate_01_LoiLenMoQuangSat_MoSanTrongCh1", gatesRoot);
            gate1.transform.position = new Vector3(68f, GROUND_Y, 199f);
            SpawnFittedWallOrSlab(pStoneWall2, gate1, terrain, new Vector3(62.8f, GROUND_Y, 199f), 0f, new Vector3(2.2f, 1.4f, 0.9f));
            SpawnFittedWallOrSlab(pStoneWall2, gate1, terrain, new Vector3(73.2f, GROUND_Y, 199f), 0f, new Vector3(2.2f, 1.4f, 0.9f));

            // GATE 2: Hai Cánh Cửa Gỗ khóa giữa vòm Cổng Làng Phù Đổng (ĐÓNG trong Chương 1 & 2 -> Khi tắt Gate_02 cuối Chương 2, vòm Cổng Làng rộng 5.2m mở thông suốt ra Cầu Gỗ!)
            GameObject gate2 = CreateBlockingGate("Gate_02_KhoaCongLangRaCauGo_MoKhiXongCh2", gatesRoot,
                new Vector3(103.5f, GROUND_Y, 145f), 0f, new Vector3(2.0f, 5.0f, 5.8f));
            SpawnFittedWallOrSlab(pFence1, gate2, terrain, new Vector3(103.5f, GROUND_Y, 146.25f), 90f, new Vector3(2.42f, 2.05f, 0.34f));
            SpawnFittedWallOrSlab(pFence2, gate2, terrain, new Vector3(103.5f, GROUND_Y, 143.75f), 90f, new Vector3(2.42f, 2.05f, 0.34f));

            // GATE 3: Cổng Đồn Gỗ Cọc Nhọn Quân Tiên Phong (ĐÓNG trong Chương 3 -> Chỉ MỞ đường sang Tiền Đồn sau khi cứu dân & hạ Tướng Tiên Phong!)
            GameObject gate3 = CreateBlockingGate("Gate_03_ChotChanVaoTienDon_MoKhiXongCh3", gatesRoot,
                new Vector3(198.5f, GROUND_Y, 76f), 0f, new Vector3(2.6f, 5.5f, 16.0f));
            if (pFortGateFbx != null)
            {
                SpawnGateArchitecture(
                    pFortGateFbx, "CongDon_ChotTienPhong_Ch3", gate3, terrain,
                    new Vector2(198.5f, 76f), true, 11.6f,
                    "M_CongDon_GiacAn", ARCH_CONG_DON, false, -0.05f, true);
                SpawnGrounded(pSpikes, gate3, terrain, new Vector3(197.8f, GROUND_Y, 83.2f), new Vector3(0f, -90f, 0f), Vector3.one * 1.35f, -0.05f);
                SpawnGrounded(pSpikes, gate3, terrain, new Vector3(197.8f, GROUND_Y, 68.8f), new Vector3(0f, -90f, 0f), Vector3.one * 1.35f, -0.05f);
            }
            else
            {
                SpawnGrounded(pSpikes, gate3, terrain, new Vector3(198.5f, GROUND_Y, 79.2f), new Vector3(0f, -90f, 0f), Vector3.one * 1.4f, -0.05f);
                SpawnGrounded(pSpikes, gate3, terrain, new Vector3(198.5f, GROUND_Y, 72.8f), new Vector3(0f, -90f, 0f), Vector3.one * 1.4f, -0.05f);
                SpawnFittedWallOrSlab(pFence1, gate3, terrain, new Vector3(198.5f, GROUND_Y, 76.0f), 90f, new Vector3(3.6f, 1.6f, 0.35f));
            }

            // GATE 4A: Cổng Đồn Gỗ Cọc Nhọn Mặt Trước Tiền Đồn Giặc Ân (Gióng & Ngựa Sắt húc đổ khi bắt đầu công đồn Chương 4)
            GameObject gate4A = CreateBlockingGate("Gate_04A_CongGoTruocTienDon_HucDoTrongCh4", gatesRoot,
                new Vector3(206.5f, GROUND_Y, 76f), 0f, new Vector3(2.4f, 6.0f, 11.4f));
            if (pFortGateFbx != null)
            {
                SpawnGateArchitecture(
                    pFortGateFbx, "CongDon_MatTruoc_TienDon_Ch4", gate4A, terrain,
                    new Vector2(206.5f, 76f), true, 11.2f,
                    "M_CongDon_GiacAn", ARCH_CONG_DON, false, -0.05f, true);
            }
            else
            {
                SpawnFittedWallOrSlab(pFence2, gate4A, terrain, new Vector3(206.5f, GROUND_Y, 78.4f), 90f, new Vector3(4.6f, 2.1f, 0.40f));
                SpawnFittedWallOrSlab(pFence2, gate4A, terrain, new Vector3(206.5f, GROUND_Y, 73.6f), 90f, new Vector3(4.6f, 2.1f, 0.40f));
            }

            // GATE 4B: Cổng Đồn Hậu Tiền Đồn Ra Đại Chiến Trường (ĐÓNG trong Chương 4 -> Đổ sập mở đường vào Chương 5 khi hạ Tướng Phòng Tuyến!)
            GameObject gate4B = CreateBlockingGate("Gate_04B_CongHauRaDaiChienTruong_MoKhiXongCh4", gatesRoot,
                new Vector3(224f, GROUND_Y, 108f), 0f, new Vector3(12.2f, 6.0f, 2.4f));
            if (pFortGateFbx != null)
            {
                SpawnGateArchitecture(
                    pFortGateFbx, "CongDon_HauTrai_RaBaiSay_Ch4", gate4B, terrain,
                    new Vector2(224f, 108f), false, 11.2f,
                    "M_CongDon_GiacAn", ARCH_CONG_DON, false, -0.05f, true);
            }
            else
            {
                SpawnFittedWallOrSlab(pFence1, gate4B, terrain, new Vector3(221.3f, GROUND_Y, 108f), 0f, new Vector3(5.2f, 2.0f, 0.40f));
                SpawnFittedWallOrSlab(pFence1, gate4B, terrain, new Vector3(226.7f, GROUND_Y, 108f), 0f, new Vector3(5.2f, 2.0f, 0.40f));
            }

            // GATE 5: Cổng Đồn Vòng Vây Phía Bắc Đại Chiến Trường (ĐÓNG trong Chương 5 -> Chỉ MỞ lối chạy lên Rừng Tre Ngà ngay khoảnh khắc Roi Sắt Gãy!)
            GameObject gate5 = CreateBlockingGate("Gate_05_LoiLenRungTreNga_MoKhiRoiSatGay_Ch5", gatesRoot,
                new Vector3(188f, GROUND_Y, 178f), 0f, new Vector3(24.0f, 6.0f, 2.5f));
            if (pFortGateFbx != null)
            {
                SpawnGateArchitecture(
                    pFortGateFbx, "CongDon_VongVayBac_BaiSay_Ch5", gate5, terrain,
                    new Vector2(188f, 178f), false, 13.6f,
                    "M_CongDon_GiacAn", ARCH_CONG_DON, false, -0.05f, true);
                SpawnGrounded(pSpikes, gate5, terrain, new Vector3(179.0f, GROUND_Y, 177.2f), new Vector3(0f, 180f, 0f), Vector3.one * 1.35f, -0.05f);
                SpawnGrounded(pSpikes, gate5, terrain, new Vector3(197.0f, GROUND_Y, 177.2f), new Vector3(0f, 180f, 0f), Vector3.one * 1.35f, -0.05f);
            }
            else
            {
                SpawnFittedWallOrSlab(pFence1, gate5, terrain, new Vector3(184.5f, GROUND_Y, 178f), 0f, new Vector3(6.2f, 1.65f, 0.35f));
                SpawnFittedWallOrSlab(pFence2, gate5, terrain, new Vector3(191.5f, GROUND_Y, 178f), 0f, new Vector3(6.2f, 1.65f, 0.35f));
                SpawnGrounded(pSpikes, gate5, terrain, new Vector3(178.5f, GROUND_Y, 178f), new Vector3(0f, 180f, 0f), Vector3.one * 1.3f, -0.05f);
                SpawnGrounded(pSpikes, gate5, terrain, new Vector3(197.5f, GROUND_Y, 178f), new Vector3(0f, 180f, 0f), Vector3.one * 1.3f, -0.05f);
            }

            // GATE 6: Cổng Đá Chân Núi Sóc (ĐÓNG suốt Chương 5 & 6 -> Chỉ MỞ lối lên Đỉnh Núi Sóc Chương 7 sau khi hạ Final Boss Đại Tướng Giặc Ân!)
            GameObject gate6 = CreateBlockingGate("Gate_06_CongChanNuiSoc_MoKhiHaFinalBoss_Ch6", gatesRoot,
                new Vector3(225.5f, GROUND_Y, 183.5f), 17f, new Vector3(15.0f, 5.0f, 2.2f));
            SpawnFittedWallOrSlab(pStoneWall2, gate6, terrain, new Vector3(220.5f, GROUND_Y, 185.0f), 17f, new Vector3(2.4f, 1.6f, 1.0f));
            SpawnFittedWallOrSlab(pStoneWall2, gate6, terrain, new Vector3(230.5f, GROUND_Y, 182.0f), 17f, new Vector3(2.4f, 1.6f, 1.0f));
            SpawnFittedWallOrSlab(pFence1, gate6, terrain, new Vector3(225.5f, GROUND_Y, 183.5f), 17f, new Vector3(7.4f, 1.7f, 0.35f));
            SpawnGrounded(pCampfire, gate6, terrain, new Vector3(219.5f, GROUND_Y, 183.5f), Vector3.zero, Vector3.one * 1.05f, -0.02f);
            SpawnGrounded(pCampfire, gate6, terrain, new Vector3(231.5f, GROUND_Y, 180.5f), Vector3.zero, Vector3.one * 1.05f, -0.02f);
        }

        private static GameObject CreateBlockingGate(string name, GameObject parent, Vector3 worldPos, float rotY, Vector3 colliderSize)
        {
            GameObject gate = CreateGroup(name, parent);
            gate.transform.position = worldPos;
            gate.transform.eulerAngles = new Vector3(0f, rotY, 0f);
            BoxCollider bc = gate.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, colliderSize.y * 0.5f, 0f);
            bc.size = colliderSize;
            return gate;
        }

        // =====================================================================
        // KHU 5: DÃY NÚI HÙNG VĨ BAO BỌC 360 ĐỘ (KHÓA BIÊN NGOÀI VÙNG AN TOÀN 232m x 232m)
        // =====================================================================
        private static void BuildZone5_PerimeterMountains(Terrain terrain)
        {
            GameObject mountainGroup = CreateGroup(GRP_MOUNTAINS, null);
            GameObject pMountain = Load(PREFAB_PATH + "SM_Mountain_01.prefab");
            if (pMountain == null) return;

            // Vùng Thung Lũng Bảo Vệ Tuyệt Đối: X [34..266], Z [34..266] + đệm an toàn 6m => Không một đỉnh/chân núi nào được vượt vào [28..272]!
            Rect protectedValley = new Rect(34f, 34f, 232f, 232f);
            const float extraClearance = 6f;

            var perimeterConfigs = new[]
            {
                // Dãy núi phía Tây (sau lưng Làng Phù Đổng - lùi xa hoàn toàn khỏi Làng)
                new { Name = "Nui_Tay_01", Pos = new Vector2(6f,  70f),  RotY = -85f, Scale = new Vector3(1.55f, 1.35f, 1.45f) },
                new { Name = "Nui_Tay_02", Pos = new Vector2(4f,  145f), RotY = -95f, Scale = new Vector3(1.65f, 1.45f, 1.50f) },
                new { Name = "Nui_Tay_03", Pos = new Vector2(6f,  220f), RotY = -70f, Scale = new Vector3(1.55f, 1.35f, 1.45f) },

                // Dãy núi phía Bắc (hậu cảnh Thượng Nguồn Sông & Rừng Tre Thần, chừa khe sông tại X=126)
                new { Name = "Nui_Bac_01_TayBac",  Pos = new Vector2(55f,  294f), RotY = 15f,  Scale = new Vector3(1.65f, 1.45f, 1.50f) },
                new { Name = "Nui_Bac_02_ThuongNguonTay", Pos = new Vector2(96f, 296f), RotY = 30f, Scale = new Vector3(1.30f, 1.25f, 1.25f) },
                new { Name = "Nui_Bac_03_ThuongNguonDong", Pos = new Vector2(162f, 296f), RotY = -20f, Scale = new Vector3(1.45f, 1.35f, 1.35f) },
                new { Name = "Nui_Bac_04_SauRungTre", Pos = new Vector2(225f, 294f), RotY = 25f, Scale = new Vector3(1.70f, 1.55f, 1.55f) },

                // Dãy núi phía Đông (sau lưng Tiền đồn Giặc Ân)
                new { Name = "Nui_Dong_01", Pos = new Vector2(294f, 225f), RotY = 85f,  Scale = new Vector3(1.60f, 1.40f, 1.45f) },
                new { Name = "Nui_Dong_02", Pos = new Vector2(295f, 145f), RotY = 100f, Scale = new Vector3(1.70f, 1.50f, 1.50f) },
                new { Name = "Nui_Dong_03", Pos = new Vector2(294f, 70f),  RotY = 75f,  Scale = new Vector3(1.55f, 1.35f, 1.40f) },

                // Dãy núi phía Nam (thấp thoáng phía xa để không chắn góc nhìn Camera từ Nam nhìn lên Bắc)
                new { Name = "Nui_Nam_01_TayNam",  Pos = new Vector2(45f,  4f), RotY = 175f, Scale = new Vector3(1.40f, 1.10f, 1.25f) },
                new { Name = "Nui_Nam_02_DongNam", Pos = new Vector2(235f, 4f), RotY = 190f, Scale = new Vector3(1.45f, 1.15f, 1.30f) }
            };

            foreach (var cfg in perimeterConfigs)
            {
                GameObject mtn = Spawn(pMountain, mountainGroup, new Vector3(cfg.Pos.x, GROUND_Y, cfg.Pos.y),
                    new Vector3(0f, cfg.RotY, 0f), cfg.Scale);
                if (mtn == null) continue;

                mtn.name = cfg.Name;

                // 1. Kiểm tra kích thước thực tế (Renderer.bounds) của khối núi và ĐẨY LÙI RA NGOÀI nếu lấn dù chỉ 1cm vào thung lũng!
                PushBoundsOutsideRect(mtn, protectedValley, extraClearance);

                // 2. Hạ chân núi ăn khít xuống địa hình đồi ngoại vi
                SnapToTerrain(mtn, terrain, -1.8f);
            }
        }

        private static void PushBoundsOutsideRect(GameObject go, Rect safeRect, float clearance)
        {
            if (go == null) return;
            if (!TryGetCombinedMeshBounds(go, out Bounds b)) return;

            float minX = safeRect.xMin - clearance;
            float maxX = safeRect.xMax + clearance;
            float minZ = safeRect.yMin - clearance;
            float maxZ = safeRect.yMax + clearance;

            bool overlapsXZ = (b.max.x > minX) && (b.min.x < maxX) && (b.max.z > minZ) && (b.min.z < maxZ);
            if (!overlapsXZ) return;

            float pushWest = minX - b.max.x;   // < 0
            float pushEast = maxX - b.min.x;   // > 0
            float pushSouth = minZ - b.max.z;  // < 0
            float pushNorth = maxZ - b.min.z;  // > 0

            float bestDx = 0f;
            float bestDz = 0f;
            float minAbs = Mathf.Abs(pushWest);
            bestDx = pushWest;

            if (Mathf.Abs(pushEast) < minAbs)
            {
                minAbs = Mathf.Abs(pushEast);
                bestDx = pushEast;
                bestDz = 0f;
            }
            if (Mathf.Abs(pushSouth) < minAbs)
            {
                minAbs = Mathf.Abs(pushSouth);
                bestDx = 0f;
                bestDz = pushSouth;
            }
            if (Mathf.Abs(pushNorth) < minAbs)
            {
                bestDx = 0f;
                bestDz = pushNorth;
            }

            go.transform.position += new Vector3(bestDx, 0f, bestDz);
        }

        // =====================================================================
        // HỆ THỐNG VÙNG CẤM ĐÈ (EXCLUSION ZONES) & ĐẶT CHÂN VẬT THỂ CHUẨN ĐỊA HÌNH
        // =====================================================================
        private static void ResetExclusionZones()
        {
            s_Exclusions.Clear();
            s_ExclusionRects.Clear();

            // Chương 1: Khu Mỏ Quặng Sắt & lối mòn chân núi Tây Bắc
            s_ExclusionRects.Add(new Rect(44f, 198f, 58f, 60f));
            // Chương 2: Khu Làng Phù Đổng, Sân Đình, Vườn Nhà Sàn & Lũy Tre
            s_ExclusionRects.Add(new Rect(34f, 88f, 76f, 114f));
            // Khu Cầu Gỗ & hai đầu cầu bắc qua Sông Phù Đổng
            s_ExclusionRects.Add(new Rect(100f, BRIDGE_Z - 18f, 56f, 36f));
            // Chương 3: Lối xuất trận bờ Đông xuôi Nam & Xóm ven rừng giải cứu dân làng
            s_ExclusionRects.Add(new Rect(142f, 48f, 58f, 94f));
            // Chương 4: Tiền Đồn Giặc Ân kiên cố Đông Nam
            s_ExclusionRects.Add(new Rect(200f, 48f, 58f, 64f));
            // Chương 5: Đại Chiến Trường Bãi Sậy trung tâm bờ Đông
            s_ExclusionRects.Add(new Rect(158f, 112f, 94f, 68f));
            // Chương 6: Rừng Tre Ngà & Hồ Nước Rừng Tre Đông Bắc
            s_ExclusionRects.Add(new Rect(146f, 180f, 70f, 72f));
            // Chương 7: Dốc cỏ xanh & Đỉnh Núi Sóc cao 21.5m
            s_ExclusionRects.Add(new Rect(216f, 178f, 50f, 82f));
        }

        private static void AddExclusionCircle(Vector2 center, float radius)
        {
            s_Exclusions.Add(new ExclusionCircle { center = center, radius = radius });
        }

        private static bool IsClearForDecoration(Vector2 pos, float margin)
        {
            if (pos.x < 30f || pos.x > MAP_SIZE - 30f || pos.y < 30f || pos.y > MAP_SIZE - 30f) return false;
            if (Mathf.Abs(pos.x - GetRiverCenterX(pos.y)) < 15.2f + margin * 0.3f) return false;
            if (GetPondNormalizedDist(pos.x, pos.y) < 1.15f) return false;

            foreach (Rect r in s_ExclusionRects)
            {
                Rect expanded = new Rect(r.xMin - margin, r.yMin - margin, r.width + margin * 2f, r.height + margin * 2f);
                if (expanded.Contains(pos)) return false;
            }

            foreach (ExclusionCircle c in s_Exclusions)
            {
                if (Vector2.Distance(pos, c.center) < c.radius + margin) return false;
            }

            return true;
        }

        private static GameObject SpawnGrounded(GameObject prefab, GameObject parent, Terrain terrain, Vector3 pos, Vector3 rot, Vector3 scale, float sinkOffset = -0.03f)
        {
            GameObject instance = Spawn(prefab, parent, pos, rot, scale);
            if (instance != null)
            {
                SnapToTerrain(instance, terrain, sinkOffset);
            }
            return instance;
        }

        private static void SnapToTerrain(GameObject go, Terrain terrain, float verticalOffset = -0.03f)
        {
            if (go == null || terrain == null) return;

            if (!TryGetCombinedMeshBounds(go, out Bounds b))
            {
                float py = SampleTerrainY(terrain, go.transform.position.x, go.transform.position.z);
                go.transform.position = new Vector3(go.transform.position.x, py + 1.2f, go.transform.position.z);
                return;
            }

            // Lấy cao độ mặt đất tại tâm & 4 góc chân vật thể
            float hCenter = SampleTerrainY(terrain, b.center.x, b.center.z);
            float h1 = SampleTerrainY(terrain, b.min.x, b.min.z);
            float h2 = SampleTerrainY(terrain, b.max.x, b.min.z);
            float h3 = SampleTerrainY(terrain, b.min.x, b.max.z);
            float h4 = SampleTerrainY(terrain, b.max.x, b.max.z);

            float maxH = Mathf.Max(hCenter, Mathf.Max(Mathf.Max(h1, h2), Mathf.Max(h3, h4)));
            float minH = Mathf.Min(hCenter, Mathf.Min(Mathf.Min(h1, h2), Mathf.Min(h3, h4)));

            float groundY;
            if (b.size.x > 25f || b.size.z > 25f)
            {
                // Với núi rất lớn ở rìa map, ưu tiên điểm thấp nhất để chân núi không bị đẩy lơ lửng
                groundY = minH;
            }
            else if (maxH - minH > 0.25f)
            {
                // Trên sườn dốc (lối lên Mỏ Quặng Chương 1 hoặc dốc cỏ Đỉnh Núi Sóc Chương 7), cắm gốc ôm theo mặt dốc thay vì nổi lơ lửng mép dưới
                groundY = Mathf.Lerp(minH, hCenter, 0.65f);
            }
            else
            {
                // Trên mặt bằng phẳng (Làng Phù Đổng, Chiến Trường, Đỉnh Núi Sóc), dùng maxH để chân cột nhà sàn không bao giờ bị hụt đất
                groundY = maxH;
            }

            float deltaY = (groundY + verticalOffset) - b.min.y;
            go.transform.position += new Vector3(0f, deltaY, 0f);
        }

        private static float SampleTerrainY(Terrain terrain, float worldX, float worldZ)
        {
            if (terrain == null) return GROUND_Y;
            float clampedX = Mathf.Clamp(worldX, 0f, MAP_SIZE);
            float clampedZ = Mathf.Clamp(worldZ, 0f, MAP_SIZE);
            return terrain.SampleHeight(new Vector3(clampedX, 0f, clampedZ)) + terrain.transform.position.y;
        }

        private static bool TryGetCombinedMeshBounds(GameObject go, out Bounds combined)
        {
            combined = new Bounds();
            if (go == null) return false;

            Renderer[] allRenderers = go.GetComponentsInChildren<Renderer>(false);
            bool hasBounds = false;

            // Ưu tiên MeshRenderer/SkinnedMeshRenderer thật (bỏ qua ParticleSystemRenderer khói lửa & Billboard LOD xa)
            foreach (Renderer r in allRenderers)
            {
                if (r == null) continue;
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                if (r.gameObject.name.ToLowerInvariant().Contains("billboard")) continue;

                if (!hasBounds)
                {
                    combined = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(r.bounds);
                }
            }

            if (!hasBounds)
            {
                foreach (Renderer r in allRenderers)
                {
                    if (r == null || r is ParticleSystemRenderer) continue;
                    if (!hasBounds)
                    {
                        combined = r.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combined.Encapsulate(r.bounds);
                    }
                }
            }

            return hasBounds;
        }

        // =====================================================================
        // ÁNH SÁNG HOÀNG KIM VIỆT CỔ & CAMERA TOÀN CẢNH 5 KHU
        // =====================================================================
        private static void SetupCameraAndLighting()
        {
            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                // Góc nhìn 3/4 từ bờ Nam nhìn bao quát cả: Làng Phù Đổng (Trái) - Sông & Cầu Gỗ (Giữa) - Chiến Trường & Rừng Tre (Phải)
                cam.transform.position = new Vector3(122f, 54f, 56f);
                cam.transform.rotation = Quaternion.Euler(33f, 9f, 0f);
                cam.farClipPlane = 800f;
            }

            Light sun = RenderSettings.sun;
            if (sun == null) sun = Object.FindAnyObjectByType<Light>();
            if (sun != null && sun.type == LightType.Directional)
            {
                sun.transform.rotation = Quaternion.Euler(40f, -38f, 0f);
                sun.color = new Color(1.0f, 0.96f, 0.88f);
                sun.intensity = 1.35f;
                sun.shadows = LightShadows.Soft;
                RenderSettings.sun = sun;
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0018f; // Sương mỏng nhẹ ở dãy núi chân trời, nhìn rõ cả 5 khu!
            RenderSettings.fogColor = new Color(0.75f, 0.86f, 0.92f);
        }

        // =====================================================================
        // ĐỒNG BỘ 9 KIẾN TRÚC MESHY AI MỚI, PBR MATERIAL URP & SPAWN CHUẨN TỶ LỆ
        // =====================================================================
        private static void SyncExternalArchitectureAssets()
        {
            string destArchAbsRoot = ToAbsoluteProjectPath("Assets/Art/Models/Architecture");
            if (!Directory.Exists(destArchAbsRoot))
            {
                Directory.CreateDirectory(destArchAbsRoot);
            }

            string repoExternalAssetsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Assets"));
            string[] archFolders =
            {
                "CauGo_SongPhuDong",
                "ChoiLoRen_PhuDong",
                "CongDon_GiacAn",
                "CongLang_PhuDong",
                "DaiDaThieng_NuiSoc",
                "DinhLang_DongSon",
                "LeuTrai_GiacAn",
                "LongGo_GiamDan",
                "ThapCanh_GiacAn"
            };

            int copiedCount = 0;
            if (Directory.Exists(repoExternalAssetsDir))
            {
                foreach (string folder in archFolders)
                {
                    string srcDir = Path.Combine(repoExternalAssetsDir, folder);
                    if (!Directory.Exists(srcDir)) continue;

                    string dstDir = Path.Combine(destArchAbsRoot, folder);
                    if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);

                    string[] files = Directory.GetFiles(srcDir);
                    foreach (string srcFile in files)
                    {
                        string ext = Path.GetExtension(srcFile).ToLowerInvariant();
                        if (ext != ".fbx" && ext != ".obj" && ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                            continue;

                        string dstFile = Path.Combine(dstDir, Path.GetFileName(srcFile));
                        bool needCopy = !File.Exists(dstFile);
                        if (!needCopy)
                        {
                            long srcLen = new FileInfo(srcFile).Length;
                            long dstLen = new FileInfo(dstFile).Length;
                            if (srcLen != dstLen) needCopy = true;
                        }

                        if (needCopy)
                        {
                            File.Copy(srcFile, dstFile, true);
                            copiedCount++;
                        }
                    }
                }
            }

            bool needImportRefresh = copiedCount > 0
                || AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_DINH_LANG + ".fbx") == null
                || AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_CAU_GO + ".fbx") == null;

            if (needImportRefresh)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            EnsureAssetFolder("Assets", "Art");
            EnsureAssetFolder("Assets/Art", "Models");
            EnsureAssetFolder("Assets/Art/Models", "Architecture");
            EnsureAssetFolder("Assets/Art/Models/Architecture", "Materials");
        }

        private static void SetupArchitecturePbrMaterial(
            GameObject go,
            string matName,
            string assetBaseNoExt,
            float metallic,
            float smoothness)
        {
            if (go == null) return;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            EnsureAssetFolder("Assets", "Art");
            EnsureAssetFolder("Assets/Art", "Models");
            EnsureAssetFolder("Assets/Art/Models", "Architecture");
            EnsureAssetFolder("Assets/Art/Models/Architecture", "Materials");

            string albedoPath = assetBaseNoExt + ".png";
            string normalPath = assetBaseNoExt + "_normal.png";
            EnsureNormalMapType(normalPath);

            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");
            if (urpLit == null) return;

            string matPath = $"{ARCH_MAT_FOLDER}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            bool isNew = false;
            if (mat == null)
            {
                mat = new Material(urpLit);
                mat.name = matName;
                isNew = true;
            }
            else
            {
                mat.shader = urpLit;
            }

            // Không gán PNG _metallic opaque vào _MetallicGlossMap vì kênh Alpha = 1.0 sẽ biến gỗ/đá thành nhựa bóng loáng trong URP
            if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", null);
            mat.DisableKeyword("_METALLICSPECGLOSSMAP");

            mat.color = Color.white;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (albedo != null)
            {
                mat.mainTexture = albedo;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
            }

            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normal);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 1.0f);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", Mathf.Clamp01(metallic));
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));

            if (isNew) AssetDatabase.CreateAsset(mat, matPath);
            else EditorUtility.SetDirty(mat);

            foreach (Renderer r in renderers)
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                Material[] slots = r.sharedMaterials;
                if (slots == null || slots.Length <= 1)
                {
                    r.sharedMaterial = mat;
                }
                else
                {
                    for (int i = 0; i < slots.Length; i++) slots[i] = mat;
                    r.sharedMaterials = slots;
                }
            }
        }

        private static GameObject CreateUprightArchitectureWrapper(
            GameObject fbxPrefab,
            string instanceName,
            GameObject parent,
            Vector3 initialPos)
        {
            if (fbxPrefab == null) return null;

            GameObject wrapper = new GameObject(instanceName);
            if (parent != null)
            {
                wrapper.transform.SetParent(parent.transform, false);
            }
            wrapper.transform.position = initialPos;
            wrapper.transform.rotation = Quaternion.identity;
            wrapper.transform.localScale = Vector3.one;

            GameObject modelChild = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
            if (modelChild == null)
            {
                Object.DestroyImmediate(wrapper);
                return null;
            }

            modelChild.name = fbxPrefab.name;
            // Giữ nguyên localRotation (-90, 0, 0) & localScale gốc (100, 100, 100) của FBX Importer, không bị ghi đè bởi Vector3.zero!
            modelChild.transform.SetParent(wrapper.transform, false);
            modelChild.transform.localPosition = Vector3.zero;

            // Tắt mọi Camera / Light thừa nếu file FBX xuất kèm từ phần mềm 3D
            foreach (Camera extraCam in modelChild.GetComponentsInChildren<Camera>(true))
            {
                extraCam.enabled = false;
            }
            foreach (Light extraLight in modelChild.GetComponentsInChildren<Light>(true))
            {
                extraLight.enabled = false;
            }

            // Đảm bảo 100% mô hình đứng thẳng theo trục +Y của Unity:
            // Toàn bộ 9 model .fbx từ Meshy AI đều có đỉnh mái/chiều cao hướng theo trục +Z của Mesh gốc (mf.transform.forward).
            // Khi wrapper đang ở góc chuẩn Quaternion.identity, mf.transform.forward bắt buộc phải hướng thẳng lên trời (Vector3.up).
            MeshFilter mf = modelChild.GetComponentInChildren<MeshFilter>(false);
            if (mf != null)
            {
                float upDot = Vector3.Dot(mf.transform.forward.normalized, Vector3.up);
                if (upDot < 0.9f)
                {
                    modelChild.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                    if (Vector3.Dot(mf.transform.forward.normalized, Vector3.up) < -0.9f)
                    {
                        modelChild.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    }
                }
            }

            return wrapper;
        }

        private static GameObject SpawnFittedArchitecture(
            GameObject prefab,
            string instanceName,
            GameObject parent,
            Terrain terrain,
            Vector2 posXZ,
            float desiredYawDeg,
            float targetMeters,
            bool sizeIsHeight,
            string matName,
            string assetBaseNoExt,
            float metallic,
            float smoothness,
            bool autoAlignLowFrontEntrance = false,
            bool addSolidBoxCollider = true,
            float sinkOffset = -0.05f)
        {
            if (prefab == null) return null;

            GameObject go = CreateUprightArchitectureWrapper(
                prefab, instanceName, parent, new Vector3(posXZ.x, GROUND_Y, posXZ.y));
            if (go == null) return null;

            SetupArchitecturePbrMaterial(go, matName, assetBaseNoExt, metallic, smoothness);

            if (TryGetCombinedMeshBounds(go, out Bounds b0))
            {
                float refDim = sizeIsHeight ? b0.size.y : Mathf.Max(b0.size.x, b0.size.z);
                if (refDim > 0.005f)
                {
                    float uniformScale = targetMeters / refDim;
                    go.transform.localScale = Vector3.one * uniformScale;
                }
            }

            float finalYaw = desiredYawDeg;
            if (autoAlignLowFrontEntrance)
            {
                if (DetectLowEntranceForwardZSign(go) < 0)
                {
                    finalYaw += 180f;
                }
            }

            if (addSolidBoxCollider)
            {
                AddWorldBoundsBoxCollider(go);
            }

            go.transform.eulerAngles = new Vector3(0f, finalYaw, 0f);

            if (TryGetCombinedMeshBounds(go, out Bounds bRot))
            {
                go.transform.position += new Vector3(posXZ.x - bRot.center.x, 0f, posXZ.y - bRot.center.z);
            }

            SnapToTerrain(go, terrain, sinkOffset);
            return go;
        }

        private static GameObject SpawnGateArchitecture(
            GameObject prefab,
            string instanceName,
            GameObject parent,
            Terrain terrain,
            Vector2 posXZ,
            bool spanAlongZ,
            float targetSpanMeters,
            string matName,
            string assetBaseNoExt,
            bool keepCenterWalkthroughOpen,
            float sinkOffset = -0.05f,
            bool faceNegativeAxis = false)
        {
            if (prefab == null) return null;

            GameObject go = CreateUprightArchitectureWrapper(
                prefab, instanceName, parent, new Vector3(posXZ.x, GROUND_Y, posXZ.y));
            if (go == null) return null;

            SetupArchitecturePbrMaterial(go, matName, assetBaseNoExt, 0.04f, 0.22f);

            if (TryGetCombinedMeshBounds(go, out Bounds b0))
            {
                float rawSpan = Mathf.Max(b0.size.x, b0.size.z);
                if (rawSpan > 0.005f)
                {
                    go.transform.localScale = Vector3.one * (targetSpanMeters / rawSpan);
                }

                bool widerAlongX = b0.size.x >= b0.size.z;
                float yaw = 0f;
                if ((spanAlongZ && widerAlongX) || (!spanAlongZ && !widerAlongX))
                {
                    yaw = 90f;
                }
                if (faceNegativeAxis)
                {
                    yaw += 180f;
                }
                go.transform.eulerAngles = new Vector3(0f, yaw, 0f);
            }

            if (TryGetCombinedMeshBounds(go, out Bounds bVerify))
            {
                if ((spanAlongZ && bVerify.size.x > bVerify.size.z * 1.05f) ||
                    (!spanAlongZ && bVerify.size.z > bVerify.size.x * 1.05f))
                {
                    go.transform.eulerAngles += new Vector3(0f, 90f, 0f);
                }
            }

            if (TryGetCombinedMeshBounds(go, out Bounds bAligned))
            {
                go.transform.position += new Vector3(posXZ.x - bAligned.center.x, 0f, posXZ.y - bAligned.center.z);
            }

            SnapToTerrain(go, terrain, sinkOffset);

            if (!keepCenterWalkthroughOpen)
            {
                AddWorldBoundsBoxCollider(go);
            }
            else if (TryGetCombinedMeshBounds(go, out Bounds wb))
            {
                // Hai trụ BoxCollider hai bên Cổng Làng (chừa vòm cổng chính giữa rộng 40% ~ 5.7m x cao 6.8m thông thoáng cho Ngựa Sắt!)
                float pillarRatio = 0.30f;
                float pillarHeight = wb.size.y * 0.62f;
                float pillarCenterY = wb.min.y + pillarHeight * 0.5f;

                Vector3 cA, cB, pillarWorldSize, lintelCenter, lintelWorldSize;
                if (spanAlongZ)
                {
                    float offsetZ = wb.size.z * (0.5f - pillarRatio * 0.5f);
                    cA = new Vector3(wb.center.x, pillarCenterY, wb.center.z + offsetZ);
                    cB = new Vector3(wb.center.x, pillarCenterY, wb.center.z - offsetZ);
                    pillarWorldSize = new Vector3(wb.size.x * 0.72f, pillarHeight, wb.size.z * pillarRatio);
                    lintelCenter = new Vector3(wb.center.x, wb.max.y - wb.size.y * 0.19f, wb.center.z);
                    lintelWorldSize = new Vector3(wb.size.x * 0.75f, wb.size.y * 0.38f, wb.size.z * 0.94f);
                }
                else
                {
                    float offsetX = wb.size.x * (0.5f - pillarRatio * 0.5f);
                    cA = new Vector3(wb.center.x + offsetX, pillarCenterY, wb.center.z);
                    cB = new Vector3(wb.center.x - offsetX, pillarCenterY, wb.center.z);
                    pillarWorldSize = new Vector3(wb.size.x * pillarRatio, pillarHeight, wb.size.z * 0.72f);
                    lintelCenter = new Vector3(wb.center.x, wb.max.y - wb.size.y * 0.19f, wb.center.z);
                    lintelWorldSize = new Vector3(wb.size.x * 0.94f, wb.size.y * 0.38f, wb.size.z * 0.75f);
                }

                AddChildWorldBoxCollider(go, "Collider_TruCong_A", cA, pillarWorldSize);
                AddChildWorldBoxCollider(go, "Collider_TruCong_B", cB, pillarWorldSize);
                AddChildWorldBoxCollider(go, "Collider_MaiVomCong_TrenCao", lintelCenter, lintelWorldSize);
            }

            return go;
        }

        private static void AddChildWorldBoxCollider(GameObject owner, string childName, Vector3 worldCenter, Vector3 worldSize)
        {
            if (owner == null) return;
            GameObject child = new GameObject(childName);
            child.transform.SetParent(owner.transform, false);
            child.transform.rotation = Quaternion.identity;
            child.transform.position = worldCenter;

            Vector3 ls = child.transform.lossyScale;
            BoxCollider bc = child.AddComponent<BoxCollider>();
            bc.size = new Vector3(
                worldSize.x / Mathf.Max(0.0001f, Mathf.Abs(ls.x)),
                worldSize.y / Mathf.Max(0.0001f, Mathf.Abs(ls.y)),
                worldSize.z / Mathf.Max(0.0001f, Mathf.Abs(ls.z)));
        }

        private static int DetectLowEntranceForwardZSign(GameObject go)
        {
            if (go == null || !TryGetCombinedMeshBounds(go, out Bounds wb) || wb.size.z < 0.02f)
                return 1;

            float band = wb.size.z * 0.22f;
            float posThreshold = wb.max.z - band;
            float negThreshold = wb.min.z + band;

            double posSumY = 0.0;
            double negSumY = 0.0;
            int posCount = 0;
            int negCount = 0;

            MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>(false);
            foreach (MeshFilter mf in filters)
            {
                if (mf == null || mf.sharedMesh == null) continue;
                Vector3[] verts = mf.sharedMesh.vertices;
                if (verts == null || verts.Length == 0) continue;

                int stride = Mathf.Max(1, verts.Length / 3500);
                Transform tr = mf.transform;
                for (int i = 0; i < verts.Length; i += stride)
                {
                    Vector3 wp = tr.TransformPoint(verts[i]);
                    if (wp.z >= posThreshold)
                    {
                        posSumY += wp.y;
                        posCount++;
                    }
                    else if (wp.z <= negThreshold)
                    {
                        negSumY += wp.y;
                        negCount++;
                    }
                }
            }

            if (posCount > 5 && negCount > 5)
            {
                double avgPosY = posSumY / posCount;
                double avgNegY = negSumY / negCount;
                // Đầu nào có bậc tam cấp / thềm đá thấp vươn ra phía trước sẽ có cao độ trung bình thấp hơn đầu hậu tường/vách đá cao
                if (avgNegY < avgPosY - 0.015)
                {
                    return -1;
                }
            }

            return 1;
        }

        private static void AddWorldBoundsBoxCollider(GameObject go)
        {
            if (go == null) return;

            Quaternion savedRot = go.transform.rotation;
            go.transform.rotation = Quaternion.identity;

            BoxCollider col = go.GetComponent<BoxCollider>();
            if (col == null) col = go.AddComponent<BoxCollider>();

            if (TryGetCombinedMeshBounds(go, out Bounds wb))
            {
                col.center = go.transform.InverseTransformPoint(wb.center);
                Vector3 ls = go.transform.lossyScale;
                col.size = new Vector3(
                    wb.size.x / Mathf.Max(0.0001f, Mathf.Abs(ls.x)),
                    wb.size.y / Mathf.Max(0.0001f, Mathf.Abs(ls.y)),
                    wb.size.z / Mathf.Max(0.0001f, Mathf.Abs(ls.z)));
            }
            else
            {
                col.size = new Vector3(1.8f, 1.4f, 1.2f);
            }

            go.transform.rotation = savedRot;
        }

        // =====================================================================
        // QUẢN LÝ VẬT LIỆU MESHY AI, SHADER TRE & TIỆN ÍCH ASSET
        // =====================================================================
        private static void ForceReimportMeshyModels()
        {
            string[] modelPaths =
            {
                "Assets/Art/Models/Meshy_AI_Hornbill_Stilt_House_texture_remesh_tri.obj",
                "Assets/Art/Models/Meshy_AI_Imperial_Envoy_texture_remesh_tri.obj",
                "Assets/Art/Models/Meshy_AI_Sun_Emperor_texture_remesh_tri.obj"
            };

            foreach (string mp in modelPaths)
            {
                if (File.Exists(ToAbsoluteProjectPath(mp)))
                {
                    AssetDatabase.ImportAsset(mp, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private static void CleanRedundantModelMaterials()
        {
            string[] redundantMats =
            {
                "Assets/Art/Models/M_NhaSan_01_MeGiong_TrungTam.mat",
                "Assets/Art/Models/M_NhaSan_02_DanLang_PhiaBac.mat",
                "Assets/Art/Models/M_NhaSan_03_DanLang_PhiaNam.mat",
                "Assets/Art/Models/M_NhaSan_04_HauCanh_TayNam.mat",
                "Assets/Art/Models/M_NhaSan_05_HauCanh_TayBac.mat",
                "Assets/Art/Models/M_NhaSan_04_HauCanh_Tay.mat",
                "Assets/Art/Models/M_NhaSan_ChimLac_DongSon.mat"
            };

            foreach (string path in redundantMats)
            {
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void SetupModelMaterial(GameObject go, string sharedMatName, string albedoPath, string normalPath, string roughnessPath)
        {
            if (go == null) return;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            string matPath = $"Assets/Art/Models/{sharedMatName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            EnsureNormalMapType(normalPath);

            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

            bool isNew = false;
            if (mat == null)
            {
                mat = new Material(urpLit);
                mat.name = sharedMatName;
                isNew = true;
            }
            else
            {
                mat.shader = urpLit;
            }

            if (albedo != null)
            {
                mat.mainTexture = albedo;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            }

            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.30f);

            if (isNew)
            {
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                EditorUtility.SetDirty(mat);
            }

            foreach (Renderer r in renderers)
            {
                r.sharedMaterial = mat;
            }
        }

        private static void UpdateBambooMaterial(string path, Shader shader, Color color, bool isLeaves)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) return;
            mat.shader = shader;
            mat.color = color;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_HueVariationColor")) mat.SetColor("_HueVariationColor", new Color(color.r * 0.9f, color.g * 0.9f, 0.1f, 0.10f));
            if (mat.HasProperty("_HueVariationKwToggle")) mat.SetFloat("_HueVariationKwToggle", 1f);
            if (isLeaves)
            {
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.33f);
                if (mat.HasProperty("_TwoSided")) mat.SetFloat("_TwoSided", 2f);
            }
            EditorUtility.SetDirty(mat);
        }

        private static void EnsureNormalMapType(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && ti.textureType != TextureImporterType.NormalMap)
            {
                ti.textureType = TextureImporterType.NormalMap;
                ti.SaveAndReimport();
            }
        }

        private static void AddFittedBoxCollider(GameObject go)
        {
            AddWorldBoundsBoxCollider(go);
        }

        private static List<GameObject> FindForestTreePrefabs()
        {
            List<GameObject> list = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs" });
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                string fileName = Path.GetFileNameWithoutExtension(p);
                if ((fileName.Contains("Tree") || fileName.Contains("Willow") || fileName.Contains("Pine") || fileName.Contains("Oak"))
                    && !fileName.Contains("Fallen") && !fileName.Contains("Stump") && !fileName.Contains("Root"))
                {
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    if (asset != null) list.Add(asset);
                }
            }
            return list;
        }

        private static GameObject FindOptionalPrefab(string exactOrPartialName)
        {
            GameObject direct = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH + exactOrPartialName + ".prefab");
            if (direct != null) return direct;

            string[] guids = AssetDatabase.FindAssets($"{exactOrPartialName} t:Prefab", new[] { "Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs" });
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return null;
        }

        private static float SoftEllipseMask(Vector2 p, Vector2 center, float rx, float rz, float feather)
        {
            float dx = (p.x - center.x) / Mathf.Max(0.1f, rx);
            float dz = (p.y - center.y) / Mathf.Max(0.1f, rz);
            float r = Mathf.Sqrt(dx * dx + dz * dz);
            float inner = 1f - (feather / Mathf.Max(1f, (rx + rz) * 0.5f));
            if (r <= inner) return 1f;
            if (r >= 1.15f) return 0f;
            return 1f - Mathf.SmoothStep(0f, 1f, (r - inner) / (1.15f - inner));
        }

        private static float SoftSegmentMask(Vector2 p, Vector2 a, Vector2 b, float halfWidth, float feather)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            float t = lenSq > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq) : 0f;
            Vector2 closest = a + ab * t;
            float d = Vector2.Distance(p, closest);
            if (d <= halfWidth) return 1f;
            if (d >= halfWidth + feather) return 0f;
            return 1f - Mathf.SmoothStep(0f, 1f, (d - halfWidth) / feather);
        }

        private static void EnsureAssetFolder(string parentFolder, string childName)
        {
            string combined = $"{parentFolder}/{childName}";
            if (!AssetDatabase.IsValidFolder(combined))
            {
                AssetDatabase.CreateFolder(parentFolder, childName);
            }
        }

        private static void DestroyIfExists(string rootName)
        {
            GameObject old = GameObject.Find(rootName);
            if (old != null) Object.DestroyImmediate(old);
        }

        private static GameObject Load(string path)
        {
            GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (p == null) Debug.LogWarning($"[ThanhGiong] Không tìm thấy Prefab tại: {path}");
            return p;
        }

        private static GameObject Spawn(GameObject prefab, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            if (prefab == null) return null;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null) instance.transform.SetParent(parent.transform, true);
            instance.transform.position = pos;
            instance.transform.eulerAngles = rot;
            instance.transform.localScale = scale;
            return instance;
        }

        private static GameObject CreateGroup(string name, GameObject parent)
        {
            GameObject g = new GameObject(name);
            if (parent != null) g.transform.SetParent(parent.transform, false);
            return g;
        }
    }
}
