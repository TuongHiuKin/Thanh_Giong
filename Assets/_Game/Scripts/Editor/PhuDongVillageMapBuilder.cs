using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Master orchestrator for constructing the living, cinematic AAA ancient Vietnamese
    /// Phù Đổng Village landscape in Unity (Nghịch Thủy Hàn / Wuxia East Asian style).
    /// </summary>
    public static class PhuDongVillageMapBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Battlefield.unity";
        public const string SandboxPath = "Assets/_Game/Scenes/Sandbox/PhuDongVillage.unity";

        [MenuItem("Thanh Giong/Build Phu Dong Village Map (Clean Rebuild)")]
        public static void BuildMapInActiveScene()
        {
            ClearExistingObjects();

            var root = new GameObject("PhuDong_Village_Environment");
            Undo.RegisterCreatedObjectUndo(root, "Build Phu Dong Village Map");

            var env = CreateChild(root, "--- ENVIRONMENT & NATURE ---");
            var arch = CreateChild(root, "--- ARCHITECTURE & VILLAGERS ---");
            var game = CreateChild(root, "--- GAMEPLAY ---");
            var light = CreateChild(root, "--- LIGHTING & ATMOSPHERE ---");

            // 1. Nature, Soaring Karsts, Meandering River, Docks & Dense Wuxia Vegetation
            PhuDongVillageEnvironmentBuilder.BuildTerrainAndRoads(env);
            PhuDongVillageEnvironmentBuilder.BuildMountains(env);
            PhuDongVillageEnvironmentBuilder.BuildSkyAndClouds(env);
            PhuDongVillageEnvironmentBuilder.BuildRiverAndBridges(env);
            PhuDongVillageEnvironmentBuilder.BuildRiceTerraces(env);
            PhuDongVillageEnvironmentBuilder.BuildUndergrowthAndTrees(env);

            // 2. East Asian Architecture, Living Villagers, Mounts
            PhuDongVillageArchitectureBuilder.BuildStiltVillageAndFortress(arch);
            PhuDongVillageArchitectureBuilder.BuildBanyanStoneWell(arch);
            PhuDongVillageArchitectureBuilder.BuildPastureAndMounts(arch);
            PhuDongVillageArchitectureBuilder.BuildBambooGroves(arch);
            PhuDongVillageArchitectureBuilder.BuildIronForge(arch);
            PhuDongVillageArchitectureBuilder.BuildLivingVillagers(arch);

            // 3. Gameplay Anchors, Cinematic Atmosphere & Camera
            BuildGameplayAnchors(game);
            SetupAtmosphereAndLighting(light);
            SetupCinematicCamera();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ThanhGiong] Wuxia East Asian Living Phu Dong Village successfully constructed!");
        }

        [MenuItem("Thanh Giong/Create and Save Village Scene")]
        public static void CreateAndSaveScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildMapInActiveScene();

            EnsureDir(ScenePath);
            EnsureDir(SandboxPath);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.CopyAsset(ScenePath, SandboxPath);
            AssetDatabase.Refresh();
            Debug.Log($"[ThanhGiong] Saved Wuxia AAA village scene to {ScenePath} and {SandboxPath}");
        }

        private static void ClearExistingObjects()
        {
            var all = Object.FindObjectsByType<GameObject>();
            foreach (var go in all)
            {
                if (go == null) continue;
                string n = go.name;
                if (n.Contains("PhuDong_Village") || n.Contains("Arena_") || n.Contains("San_Lang") ||
                    n.Contains("ThamCo_") || n.Contains("ThungLung_") || n.Contains("DiaHinh_") ||
                    n.Contains("Directional Light") || n.Contains("MatTroi_") || n.Contains("DirectionalLight") ||
                    n.Contains("KhuLang_") || n.Contains("DayNui_") || n.Contains("DongSong_") ||
                    n.Contains("BauTroi_") || n.Contains("DongRuong_") || n.Contains("ThamThucVat_") ||
                    n.Contains("RungHoaDao_") || n.Contains("BenDo_") || n.Contains("DaDam_") ||
                    n.Contains("ThamCoLau_") || n.Contains("GiengNgoc_") || n.Contains("ChuongNgua_") ||
                    n.Contains("RungTre_") || n.Contains("LoRenThep_") || n.Contains("DanLang_") ||
                    n.Contains("DinhLang_") || n.Contains("CayDa_") || n.Contains("CauGo_") ||
                    n.Contains("DinhNui_") || n.Contains("VachDa_") || n.Contains("DamMay_") || n.Contains("cloud_") ||
                    n.Contains("PlayerSpawnPoint") || n.Contains("EnemyWaveSpawners") || n.Contains("Trigger_NhoTre"))
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }
        }

        private static void BuildGameplayAnchors(GameObject parent)
        {
            var pSpawn = new GameObject("PlayerSpawnPoint_Giong");
            pSpawn.transform.SetParent(parent.transform, false);
            pSpawn.transform.position = new Vector3(0f, 0.05f, -8f);

            var eSpawns = CreateChild(parent, "EnemyWaveSpawners");
            Vector3[] spawns = { new Vector3(-10f, 0.05f, 8f), new Vector3(0f, 0.05f, 14f), new Vector3(10f, 0.05f, 8f) };
            for (int i = 0; i < spawns.Length; i++)
            {
                var s = new GameObject($"EnemySpawnPoint_{i + 1}");
                s.transform.SetParent(eSpawns.transform, false);
                s.transform.position = spawns[i];
                s.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var trig = new GameObject("Trigger_NhoTre_Phase3");
            trig.transform.SetParent(parent.transform, false);
            trig.transform.position = new Vector3(-18f, 0f, -4f);
            trig.AddComponent<SphereCollider>().isTrigger = true;
        }

        private static void SetupAtmosphereAndLighting(GameObject parent)
        {
            // Soft river mist / mountain atmosphere (Sương khói bồng bềnh thủy mặc)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 28f;
            RenderSettings.fogEndDistance = 150f;
            RenderSettings.fogColor = new Color(0.74f, 0.84f, 0.90f);

            // Morning dawn golden sunlight with soft shadows
            var sun = new GameObject("MatTroi_BanMai_Sun");
            sun.transform.SetParent(parent.transform, false);
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1.0f, 0.95f, 0.88f);
            l.intensity = 1.15f;
            l.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(34f, -32f, 0f);
        }

        private static void SetupCinematicCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = GameObject.Find("Main Camera");
                if (camGo == null) camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cam = camGo.GetComponent<Camera>();
            }
            // Positioned overlooking the river dock, peach blossom trees, wooden boats, and towering karsts
            cam.transform.position = new Vector3(-4f, 9.5f, -27f);
            cam.transform.rotation = Quaternion.Euler(16f, 15f, 0f);
            cam.farClipPlane = 650f;
        }

        private static void EnsureDir(string path)
        {
            string d = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d);
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }
    }
}
