using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Constructs a compact, highly detailed, realistic preview map of ancient Phù Đổng Village.
    /// Features: Stilt house homestead, ancient stone well under banyan tree, resting water buffalo,
    /// terraced rice fields with water reflection, sacred bamboo grove, and warm cinematic lighting.
    /// </summary>
    public static class PhuDongVillageMapBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Battlefield.unity";
        private const string SandboxPath = "Assets/_Game/Scenes/Sandbox/PhuDongVillage.unity";
        private const string PrefabBasePath = "Assets/_Game/Prefabs/Environment/";
        private const string MatBasePath = "Assets/_Game/Art/Materials/";

        [MenuItem("Thanh Giong/Build Phu Dong Village Map")]
        public static void BuildMapInActiveScene()
        {
            var root = new GameObject("PhuDong_Village_CompactShowcase");
            Undo.RegisterCreatedObjectUndo(root, "Build Phu Dong Village Map");

            var envRoot = CreateChild(root, "--- ENVIRONMENT ---");
            var gameplayRoot = CreateChild(root, "--- GAMEPLAY ---");
            var lightingRoot = CreateChild(root, "--- LIGHTING & ATMOSPHERE ---");

            BuildTerrainAndCourtyard(envRoot);
            BuildStiltHouseHomestead(envRoot);
            BuildAncientWellAndBanyan(envRoot);
            BuildWaterBuffaloAndPasture(envRoot);
            BuildTerracedRiceFields(envRoot);
            BuildBambooGroves(envRoot);
            BuildForgeAndLotusPond(envRoot);

            BuildGameplayAnchors(gameplayRoot);
            SetupRealisticAtmosphere(lightingRoot);
            SetupCinematicCamera(root);

            Debug.Log("[ThanhGiong] Compact realistic Phu Dong Village map successfully built!");
        }

        [MenuItem("Thanh Giong/Create and Save Village Scene")]
        public static void CreateAndSaveScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildMapInActiveScene();

            string dir = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string sandboxDir = Path.GetDirectoryName(SandboxPath);
            if (!string.IsNullOrEmpty(sandboxDir) && !Directory.Exists(sandboxDir))
            {
                Directory.CreateDirectory(sandboxDir);
            }

            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.CopyAsset(ScenePath, SandboxPath);
            AssetDatabase.Refresh();
            Debug.Log($"[ThanhGiong] Saved realistic village scene to {ScenePath} and {SandboxPath}");
        }

        private static void BuildTerrainAndCourtyard(GameObject parent)
        {
            var groundMat = LoadMaterial("World/Grass.mat");
            var soilMat = LoadMaterial("World/Farm_Soil.mat");
            var roadMat = LoadMaterial("World/Road.mat");

            // Base ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Terrain_Base_Grass";
            ground.transform.SetParent(parent.transform);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(6f, 1f, 6f); // 60m x 60m
            if (groundMat != null) ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // Compact central combat courtyard / path
            var courtyard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            courtyard.name = "San_Lang_DatNen";
            courtyard.transform.SetParent(parent.transform);
            courtyard.transform.position = new Vector3(0f, 0.02f, 0f);
            courtyard.transform.localScale = new Vector3(26f, 0.04f, 26f);
            if (soilMat != null) courtyard.GetComponent<Renderer>().sharedMaterial = soilMat;

            // Pathway connecting entrance gate to stilt house
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = "Duong_Lang_Co";
            path.transform.SetParent(parent.transform);
            path.transform.position = new Vector3(0f, 0.03f, 4f);
            path.transform.localScale = new Vector3(4f, 0.04f, 24f);
            if (roadMat != null) path.GetComponent<Renderer>().sharedMaterial = roadMat;
        }

        private static void BuildStiltHouseHomestead(GameObject parent)
        {
            var homestead = CreateChild(parent, "Khu_NhaSan_LangGiong");
            homestead.transform.position = new Vector3(0f, 0f, 16f);

            var housePrefab = LoadPrefab("Meshy_NhaTranh_LangGiong.prefab");
            if (housePrefab != null)
            {
                var house = (GameObject)PrefabUtility.InstantiatePrefab(housePrefab);
                house.name = "NhaSan_Chinh_Giong";
                house.transform.SetParent(homestead.transform);
                house.transform.localPosition = new Vector3(0f, 0f, 0f);
                house.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                house.transform.localScale = Vector3.one * 1.15f;
            }

            var platterPrefab = LoadPrefab("Meshy_MamLuongThuc_LangGiong.prefab");
            if (platterPrefab != null)
            {
                var platter = (GameObject)PrefabUtility.InstantiatePrefab(platterPrefab);
                platter.name = "MamCom_LangNuoiGiong";
                platter.transform.SetParent(homestead.transform);
                platter.transform.localPosition = new Vector3(-3.5f, 0f, -4f);
                platter.transform.localScale = Vector3.one * 1.3f;
            }

            var gatePrefab = LoadPrefab("Meshy_CongLang_ChienLuy.prefab");
            if (gatePrefab != null)
            {
                var gate = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab);
                gate.name = "CongLang_ChienLuy_North";
                gate.transform.SetParent(homestead.transform);
                gate.transform.localPosition = new Vector3(0f, 0f, 10f);
                gate.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        private static void BuildAncientWellAndBanyan(GameObject parent)
        {
            var area = CreateChild(parent, "GiengNuoc_CayDa_Area");
            area.transform.position = new Vector3(-13f, 0f, 8f);

            var stoneMat = LoadMaterial("World/Stone.mat");
            var woodMat = LoadMaterial("World/Wood.mat");
            var waterMat = LoadMaterial("World/Water.mat");

            // Giant Banyan Tree
            var banyanPrefab = LoadPrefab("Meshy_CayDaCoThu_LangViet.prefab");
            if (banyanPrefab != null)
            {
                var banyan = (GameObject)PrefabUtility.InstantiatePrefab(banyanPrefab);
                banyan.name = "CayDa_CoThu_LangViet";
                banyan.transform.SetParent(area.transform);
                banyan.transform.localPosition = new Vector3(-2f, 0f, 2f);
                banyan.transform.localRotation = Quaternion.Euler(0f, 40f, 0f);
                banyan.transform.localScale = Vector3.one * 1.1f;
            }

            // Ancient Stone Well
            var well = CreateChild(area, "GiengNuoc_CoDai");
            well.transform.localPosition = new Vector3(2.5f, 0f, -1.5f);

            var curb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            curb.name = "ThanhGieng_DaOng";
            curb.transform.SetParent(well.transform);
            curb.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            curb.transform.localScale = new Vector3(2.8f, 0.45f, 2.8f);
            if (stoneMat != null) curb.GetComponent<Renderer>().sharedMaterial = stoneMat;

            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "MatNuoc_GiengTrong";
            water.transform.SetParent(well.transform);
            water.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            water.transform.localScale = new Vector3(2.3f, 0.05f, 2.3f);
            if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;

            // Well wooden posts and crossbeam
            var postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postL.name = "CotGo_Trai";
            postL.transform.SetParent(well.transform);
            postL.transform.localPosition = new Vector3(-1.3f, 1.2f, 0f);
            postL.transform.localScale = new Vector3(0.14f, 1.2f, 0.14f);
            if (woodMat != null) postL.GetComponent<Renderer>().sharedMaterial = woodMat;

            var postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postR.name = "CotGo_Phai";
            postR.transform.SetParent(well.transform);
            postR.transform.localPosition = new Vector3(1.3f, 1.2f, 0f);
            postR.transform.localScale = new Vector3(0.14f, 1.2f, 0.14f);
            if (woodMat != null) postR.GetComponent<Renderer>().sharedMaterial = woodMat;

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "XaGo_Ngang";
            beam.transform.SetParent(well.transform);
            beam.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            beam.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            beam.transform.localScale = new Vector3(0.12f, 1.4f, 0.12f);
            if (woodMat != null) beam.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Stone platform around well
            var apron = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            apron.name = "ThemDa_KinhGieng";
            apron.transform.SetParent(well.transform);
            apron.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            apron.transform.localScale = new Vector3(4.5f, 0.04f, 4.5f);
            if (stoneMat != null) apron.GetComponent<Renderer>().sharedMaterial = stoneMat;
        }

        private static void BuildWaterBuffaloAndPasture(GameObject parent)
        {
            var pasture = CreateChild(parent, "BaiChanTrau_DunRom");
            pasture.transform.position = new Vector3(13f, 0f, 8f);

            var buffalo = CreateChild(pasture, "ConTrau_NghiNgoi");
            buffalo.transform.localPosition = new Vector3(0f, 0f, 0f);
            buffalo.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);

            var woodMat = LoadMaterial("World/Wood.mat");
            var buffaloMat = LoadMaterial("World/GrassDark.mat");

            // Buffalo stylized anatomy
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Than_Trau";
            body.transform.SetParent(buffalo.transform);
            body.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            body.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);
            body.transform.localScale = new Vector3(1.25f, 1.7f, 1.05f);
            if (buffaloMat != null) body.GetComponent<Renderer>().sharedMaterial = buffaloMat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Dau_Trau";
            head.transform.SetParent(buffalo.transform);
            head.transform.localPosition = new Vector3(0f, 1.15f, 1.4f);
            head.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            head.transform.localScale = new Vector3(0.65f, 0.65f, 0.9f);
            if (buffaloMat != null) head.GetComponent<Renderer>().sharedMaterial = buffaloMat;

            // Curved Horns
            var hornL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornL.name = "Sung_Trau_Trai";
            hornL.transform.SetParent(buffalo.transform);
            hornL.transform.localPosition = new Vector3(-0.65f, 1.45f, 1.35f);
            hornL.transform.localRotation = Quaternion.Euler(20f, -40f, 65f);
            hornL.transform.localScale = new Vector3(0.12f, 0.55f, 0.12f);

            var hornR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornR.name = "Sung_Trau_Phai";
            hornR.transform.SetParent(buffalo.transform);
            hornR.transform.localPosition = new Vector3(0.65f, 1.45f, 1.35f);
            hornR.transform.localRotation = Quaternion.Euler(20f, 40f, -65f);
            hornR.transform.localScale = new Vector3(0.12f, 0.55f, 0.12f);

            // Haystack (Đụn rơm)
            var haystack = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            haystack.name = "DunRom_Vang";
            haystack.transform.SetParent(pasture.transform);
            haystack.transform.localPosition = new Vector3(2.5f, 1.3f, 2.5f);
            haystack.transform.localScale = new Vector3(3.2f, 2.6f, 3.2f);
            var hayMat = LoadMaterial("World/Farm_Crops.mat");
            if (hayMat != null) haystack.GetComponent<Renderer>().sharedMaterial = hayMat;

            // Hitching post
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Coc_Buoc_Trau";
            post.transform.SetParent(pasture.transform);
            post.transform.localPosition = new Vector3(-1.8f, 0.6f, 1f);
            post.transform.localScale = new Vector3(0.16f, 0.6f, 0.16f);
            if (woodMat != null) post.GetComponent<Renderer>().sharedMaterial = woodMat;
        }

        private static void BuildTerracedRiceFields(GameObject parent)
        {
            var fields = CreateChild(parent, "DongRuong_LuaNuoc");
            fields.transform.position = new Vector3(16f, 0f, -6f);

            var soilMat = LoadMaterial("World/Farm_Soil.mat");
            var waterMat = LoadMaterial("World/Water.mat");
            var cropMat = LoadMaterial("World/Farm_Crops.mat");

            for (int t = 0; t < 2; t++)
            {
                float elevation = t * 0.35f;
                float x = t * 5.5f;

                var terrace = CreateChild(fields, $"ThuaRuong_Bac_{t + 1}");
                terrace.transform.localPosition = new Vector3(x, elevation, 0f);

                // Earth berm
                var berm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                berm.name = "BoRuong_DatAm";
                berm.transform.SetParent(terrace.transform);
                berm.transform.localPosition = Vector3.zero;
                berm.transform.localScale = new Vector3(5f, 0.35f, 18f);
                if (soilMat != null) berm.GetComponent<Renderer>().sharedMaterial = soilMat;

                // Water reflection layer
                var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "MatNuoc_PhanChieu";
                water.transform.SetParent(terrace.transform);
                water.transform.localPosition = new Vector3(0f, 0.14f, 0f);
                water.transform.localScale = new Vector3(4.5f, 0.05f, 17f);
                if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;

                // Rice shoot clusters
                for (int r = -6; r <= 6; r += 3)
                {
                    var sprout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    sprout.name = $"KhomMa_Lua_{r}";
                    sprout.transform.SetParent(terrace.transform);
                    sprout.transform.localPosition = new Vector3(0f, 0.3f, r);
                    sprout.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
                    if (cropMat != null) sprout.GetComponent<Renderer>().sharedMaterial = cropMat;
                }
            }
        }

        private static void BuildBambooGroves(GameObject parent)
        {
            var grove = CreateChild(parent, "LuyTreNga_PhuDong");
            var bambooPrefab = LoadPrefab("Meshy_KhomTreNga_ThanThanh.prefab");
            var foliagePrefab = LoadPrefab("Foliage/BambooGrove_Vietnamese_Legend.prefab");

            Vector3[] positions = {
                new Vector3(-16f, 0f, -4f),
                new Vector3(-17f, 0f, 3f),
                new Vector3(-14f, 0f, -12f),
                new Vector3(-6f, 0f, -17f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var prefab = (i % 2 == 0 && bambooPrefab != null) ? bambooPrefab : foliagePrefab;
                if (prefab != null)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    b.name = $"RangTre_LuyLang_{i + 1}";
                    b.transform.SetParent(grove.transform);
                    b.transform.position = positions[i];
                    b.transform.rotation = Quaternion.Euler(0f, i * 50f, 0f);
                    b.transform.localScale = Vector3.one * (1.1f + i * 0.1f);
                }
            }

            // Sacred Bamboo Grove for Phase 3 Interaction
            var sacredSpot = CreateChild(grove, "KhomTre_LinhThieng_Phase3");
            sacredSpot.transform.position = new Vector3(-14f, 0f, -4f);

            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "HaoQuang_TreNga";
            glow.transform.SetParent(sacredSpot.transform);
            glow.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            glow.transform.localScale = Vector3.one * 0.5f;

            // Drifting leaves VFX
            var leavesPrefab = LoadPrefab("Foliage/PFX_Forest_DriftingLeaves.prefab");
            if (leavesPrefab != null)
            {
                var pfx = (GameObject)PrefabUtility.InstantiatePrefab(leavesPrefab);
                pfx.name = "VFX_LaTre_Roi";
                pfx.transform.SetParent(grove.transform);
                pfx.transform.position = new Vector3(-12f, 3f, -5f);
            }
        }

        private static void BuildForgeAndLotusPond(GameObject parent)
        {
            var forgePrefab = LoadPrefab("Meshy_LoRenThep_VanLang.prefab");
            if (forgePrefab != null)
            {
                var forge = (GameObject)PrefabUtility.InstantiatePrefab(forgePrefab);
                forge.name = "LoRenThep_PhuDong";
                forge.transform.SetParent(parent.transform);
                forge.transform.position = new Vector3(8f, 0f, -14f);
                forge.transform.rotation = Quaternion.Euler(0f, -30f, 0f);
            }

            var lotusPrefab = LoadPrefab("Meshy_DamSenCo_VanLang.prefab");
            if (lotusPrefab != null)
            {
                var lotus = (GameObject)PrefabUtility.InstantiatePrefab(lotusPrefab);
                lotus.name = "DamSen_CoDai_VanLang";
                lotus.transform.SetParent(parent.transform);
                lotus.transform.position = new Vector3(-2f, 0f, -16f);
            }

            var bridgePrefab = LoadPrefab("Meshy_CauGo_CoDai.prefab");
            if (bridgePrefab != null)
            {
                var bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab);
                bridge.name = "CauGo_BacQuaDamSen";
                bridge.transform.SetParent(parent.transform);
                bridge.transform.position = new Vector3(3f, 0f, -14.5f);
                bridge.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
        }

        private static void BuildGameplayAnchors(GameObject parent)
        {
            var pSpawn = new GameObject("PlayerSpawnPoint_Giong");
            pSpawn.transform.SetParent(parent.transform);
            pSpawn.transform.position = new Vector3(0f, 0f, -6f);
            pSpawn.transform.rotation = Quaternion.identity;

            var eSpawns = CreateChild(parent, "EnemyWaveSpawners");
            Vector3[] spawns = {
                new Vector3(-8f, 0f, 6f),
                new Vector3(0f, 0f, 10f),
                new Vector3(8f, 0f, 6f)
            };
            for (int i = 0; i < spawns.Length; i++)
            {
                var sp = new GameObject($"EnemySpawnPoint_{i + 1}");
                sp.transform.SetParent(eSpawns.transform);
                sp.transform.position = spawns[i];
                sp.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var trigger = new GameObject("Trigger_NhoTre_Phase3");
            trigger.transform.SetParent(parent.transform);
            trigger.transform.position = new Vector3(-14f, 0f, -4f);
            var col = trigger.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 3.5f;
        }

        private static void SetupRealisticAtmosphere(GameObject parent)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 65f;
            RenderSettings.fogColor = new Color(0.76f, 0.78f, 0.72f);

            var sun = new GameObject("MatTroi_BanMai_Sun");
            sun.transform.SetParent(parent.transform);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.93f, 0.82f);
            light.intensity = 1.35f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
        }

        private static void SetupCinematicCamera(GameObject root)
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            camGo.transform.position = new Vector3(0f, 9.5f, -19f);
            camGo.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        }

        private static GameObject LoadPrefab(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBasePath + relativePath);
        }

        private static Material LoadMaterial(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MatBasePath + relativePath);
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }
    }
}
