using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds a sprawling, highly authentic AAA-inspired ancient Vietnamese Phù Đổng Village map.
    /// Completely clears any duplicate or legacy test objects before building.
    /// Features: Contoured mountain valley, dynamic drifting 3D clouds, procedural sky,
    /// natural lake with lotus and waterlilies, realistic terraced rice paddies, traditional stilt houses,
    /// stone well under giant banyan, resting water buffalo, and dense bamboo forests.
    /// </summary>
    public static class PhuDongVillageMapBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Battlefield.unity";
        private const string SandboxPath = "Assets/_Game/Scenes/Sandbox/PhuDongVillage.unity";
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string MatBase = "Assets/_Game/Art/Materials/";
        private const string NatureBase = "Assets/ThirdParty/KayKit_Medieval_Hexagon_Pack/KayKit_Medieval_Hexagon_Pack_1.0_FREE/Assets/fbx/decoration/nature/";

        [MenuItem("Thanh Giong/Build Phu Dong Village Map (Clean Rebuild)")]
        public static void BuildMapInActiveScene()
        {
            ClearExistingObjects();

            var root = new GameObject("PhuDong_Village_Environment");
            Undo.RegisterCreatedObjectUndo(root, "Build Phu Dong Village Map");

            var env = CreateChild(root, "--- ENVIRONMENT ---");
            var game = CreateChild(root, "--- GAMEPLAY ---");
            var light = CreateChild(root, "--- LIGHTING & ATMOSPHERE ---");

            BuildValleyTerrainAndPaths(env);
            BuildMountainBackdrop(env);
            BuildSkyAndDriftingClouds(env);
            BuildLakeAndLotusPond(env);
            BuildTerracedRiceFields(env);
            BuildStiltVillageHomesteads(env);
            BuildBanyanAndAncientWell(env);
            BuildWaterBuffaloPasture(env);
            BuildBambooForest(env);

            BuildGameplayAnchors(game);
            SetupAtmosphereAndLighting(light);
            SetupCinematicCamera();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ThanhGiong] Clean AAA Phu Dong Village map successfully constructed!");
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
            Debug.Log($"[ThanhGiong] Saved AAA village scene to {ScenePath} and {SandboxPath}");
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
                    n.Contains("Directional Light") || n.Contains("MatTroi_") || n.Contains("DirectionalLight"))
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }
        }

        private static void BuildValleyTerrainAndPaths(GameObject parent)
        {
            var root = CreateChild(parent, "DiaHinh_ThungLung_PhuDong");
            var grassMat = LoadMat("World/GrassDark.mat");
            var roadMat = LoadMat("World/Road.mat");

            var baseMeadow = GameObject.CreatePrimitive(PrimitiveType.Plane);
            baseMeadow.name = "ThungLung_CoXanh_Base";
            baseMeadow.transform.SetParent(root.transform);
            baseMeadow.transform.position = Vector3.zero;
            baseMeadow.transform.localScale = new Vector3(14f, 1f, 14f);
            if (grassMat != null) baseMeadow.GetComponent<Renderer>().sharedMaterial = grassMat;

            Vector3[] pathPoints = {
                new Vector3(0f, 0.04f, -22f), new Vector3(-2f, 0.04f, -12f),
                new Vector3(0f, 0.04f, 0f), new Vector3(3f, 0.04f, 11f),
                new Vector3(-1f, 0.04f, 22f)
            };
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"DuongLang_Mon_{i + 1}";
                seg.transform.SetParent(root.transform);
                Vector3 mid = (pathPoints[i] + pathPoints[i + 1]) * 0.5f;
                Vector3 d = pathPoints[i + 1] - pathPoints[i];
                seg.transform.position = mid;
                seg.transform.rotation = Quaternion.LookRotation(d);
                seg.transform.localScale = new Vector3(4.2f, 0.04f, d.magnitude + 0.4f);
                if (roadMat != null) seg.GetComponent<Renderer>().sharedMaterial = roadMat;
            }
        }

        private static void BuildMountainBackdrop(GameObject parent)
        {
            var mtnRoot = CreateChild(parent, "DayNui_BaoBoc_ThungLung");
            string[] mtnFiles = { "mountain_A.fbx", "mountain_B.fbx", "mountain_C.fbx", "hills_A_trees.fbx", "hills_B_trees.fbx" };
            int count = 18;
            float radius = 68f;

            for (int i = 0; i < count; i++)
            {
                float ang = i * Mathf.PI * 2f / count;
                string file = mtnFiles[i % mtnFiles.Length];
                var mtnAsset = AssetDatabase.LoadAssetAtPath<GameObject>(NatureBase + file);
                if (mtnAsset == null) continue;

                var mtn = (GameObject)PrefabUtility.InstantiatePrefab(mtnAsset);
                mtn.name = $"Nui_PhuDong_{i + 1}";
                mtn.transform.SetParent(mtnRoot.transform);
                float dist = radius + (i % 3) * 6f;
                mtn.transform.position = new Vector3(Mathf.Cos(ang) * dist, -1.5f, Mathf.Sin(ang) * dist);
                mtn.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                mtn.transform.localScale = Vector3.one * Random.Range(5.5f, 9.5f);
            }
        }

        private static void BuildSkyAndDriftingClouds(GameObject parent)
        {
            var skyRoot = CreateChild(parent, "BauTroi_MayBay");
            var skyMat = LoadMat("Sky/ThanhGiong_Sky.mat");
            if (skyMat != null) RenderSettings.skybox = skyMat;

            var cloudA = AssetDatabase.LoadAssetAtPath<GameObject>(NatureBase + "cloud_big.fbx");
            var cloudB = AssetDatabase.LoadAssetAtPath<GameObject>(NatureBase + "cloud_small.fbx");
            var cloudMat = LoadMat("Sky/ThanhGiong_Cloud.mat");

            int clouds = 14;
            for (int i = 0; i < clouds; i++)
            {
                float a = i * Mathf.PI * 2f / clouds;
                float r = Random.Range(35f, 65f);
                var asset = (i % 2 == 0) ? cloudA : cloudB;
                if (asset == null) continue;

                var c = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                c.name = $"May_Trang_{i + 1}";
                c.transform.SetParent(skyRoot.transform);
                c.transform.position = new Vector3(Mathf.Cos(a) * r, Random.Range(32f, 48f), Mathf.Sin(a) * r);
                c.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                c.transform.localScale = Vector3.one * Random.Range(7f, 14f);
                if (cloudMat != null) SetMatRecursive(c, cloudMat);
            }
        }

        private static void BuildLakeAndLotusPond(GameObject parent)
        {
            var waterRoot = CreateChild(parent, "AoHo_DamSen_NuocTrong");
            waterRoot.transform.position = new Vector3(22f, 0f, -14f);
            var waterMat = LoadMat("World/Water.mat");

            var lakeSurface = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lakeSurface.name = "MatNuoc_HoSen_PhuDong";
            lakeSurface.transform.SetParent(waterRoot.transform);
            lakeSurface.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            lakeSurface.transform.localScale = new Vector3(32f, 0.05f, 24f);
            if (waterMat != null) lakeSurface.GetComponent<Renderer>().sharedMaterial = waterMat;

            var lotusPrefab = LoadPrefab("Meshy_DamSenCo_VanLang.prefab");
            if (lotusPrefab != null)
            {
                var lotus = (GameObject)PrefabUtility.InstantiatePrefab(lotusPrefab);
                lotus.name = "DamSenCo_VanLang";
                lotus.transform.SetParent(waterRoot.transform);
                lotus.transform.localPosition = new Vector3(-2f, 0f, -1f);
                lotus.transform.localScale = Vector3.one * 1.3f;
            }

            var bridgePrefab = LoadPrefab("Meshy_CauGo_CoDai.prefab");
            if (bridgePrefab != null)
            {
                var bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab);
                bridge.name = "CauGo_BacQuaHo";
                bridge.transform.SetParent(waterRoot.transform);
                bridge.transform.localPosition = new Vector3(-12f, 0f, 1f);
                bridge.transform.localRotation = Quaternion.Euler(0f, 85f, 0f);
                bridge.transform.localScale = Vector3.one * 1.1f;
            }

            var lilyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(NatureBase + "waterlily_A.fbx");
            var reedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(NatureBase + "waterplant_A.fbx");
            for (int i = 0; i < 6; i++)
            {
                float ang = i * 1.0f;
                if (lilyAsset != null)
                {
                    var lily = (GameObject)PrefabUtility.InstantiatePrefab(lilyAsset);
                    lily.name = $"HoaSen_{i + 1}";
                    lily.transform.SetParent(waterRoot.transform);
                    lily.transform.localPosition = new Vector3(Mathf.Cos(ang) * 9f, 0.16f, Mathf.Sin(ang) * 7f);
                    lily.transform.localScale = Vector3.one * 1.8f;
                }
                if (reedAsset != null)
                {
                    var reed = (GameObject)PrefabUtility.InstantiatePrefab(reedAsset);
                    reed.name = $"LauSay_{i + 1}";
                    reed.transform.SetParent(waterRoot.transform);
                    reed.transform.localPosition = new Vector3(Mathf.Cos(ang + 0.5f) * 14f, 0.1f, Mathf.Sin(ang + 0.5f) * 10f);
                    reed.transform.localScale = Vector3.one * 2.2f;
                }
            }
        }

        private static void BuildTerracedRiceFields(GameObject parent)
        {
            var paddyRoot = CreateChild(parent, "DongRuong_BacThang_PhuDong");
            paddyRoot.transform.position = new Vector3(18f, 0f, 8f);

            var soilMat = LoadMat("World/Farm_Soil.mat");
            var waterMat = LoadMat("World/Water.mat");
            var cropMat = LoadMat("World/Farm_Crops.mat");

            for (int t = 0; t < 3; t++)
            {
                float elevation = t * 0.45f;
                float x = t * 6.2f;
                var terrace = CreateChild(paddyRoot, $"ThuaRuong_Bac_{t + 1}");
                terrace.transform.localPosition = new Vector3(x, elevation, 0f);

                var berm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                berm.name = "BoRuong_DatSet";
                berm.transform.SetParent(terrace.transform);
                berm.transform.localPosition = Vector3.zero;
                berm.transform.localScale = new Vector3(5.8f, 0.45f, 26f);
                if (soilMat != null) berm.GetComponent<Renderer>().sharedMaterial = soilMat;

                var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "MatNuoc_RuongLua";
                water.transform.SetParent(terrace.transform);
                water.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                water.transform.localScale = new Vector3(5.2f, 0.05f, 25f);
                if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;

                for (int z = -10; z <= 10; z += 2)
                {
                    for (int off = -1; off <= 1; off += 2)
                    {
                        var shoot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        shoot.name = $"KhomLua_{z}_{off}";
                        shoot.transform.SetParent(terrace.transform);
                        shoot.transform.localPosition = new Vector3(off * 1.3f, 0.38f, z);
                        shoot.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
                        if (cropMat != null) shoot.GetComponent<Renderer>().sharedMaterial = cropMat;
                    }
                }
            }
        }

        private static void BuildStiltVillageHomesteads(GameObject parent)
        {
            var village = CreateChild(parent, "KhuLang_NhaSan_PhuDong");
            var housePrefab = LoadPrefab("Meshy_NhaTranh_LangGiong.prefab");

            Vector3[] housePos = { new Vector3(-3f, 0f, 24f), new Vector3(12f, 0f, 26f), new Vector3(-16f, 0f, 20f) };
            float[] houseRot = { 180f, 160f, 195f };
            float[] houseScale = { 1.25f, 1.1f, 1.15f };

            if (housePrefab != null)
            {
                for (int i = 0; i < housePos.Length; i++)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(housePrefab);
                    h.name = $"NhaSan_VietCo_{i + 1}";
                    h.transform.SetParent(village.transform);
                    h.transform.position = housePos[i];
                    h.transform.rotation = Quaternion.Euler(0f, houseRot[i], 0f);
                    h.transform.localScale = Vector3.one * houseScale[i];
                }
            }

            var gatePrefab = LoadPrefab("Meshy_CongLang_ChienLuy.prefab");
            if (gatePrefab != null)
            {
                var gate = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab);
                gate.name = "CongLang_ChienLuy";
                gate.transform.SetParent(village.transform);
                gate.transform.position = new Vector3(0f, 0f, 32f);
                gate.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var plazaPrefab = LoadPrefab("Meshy_SanDinh_TrongDong.prefab");
            if (plazaPrefab != null)
            {
                var plaza = (GameObject)PrefabUtility.InstantiatePrefab(plazaPrefab);
                plaza.name = "SanDinh_TrongDong";
                plaza.transform.SetParent(village.transform);
                plaza.transform.position = new Vector3(4f, 0.02f, 14f);
            }

            var foodPrefab = LoadPrefab("Meshy_MamLuongThuc_LangGiong.prefab");
            if (foodPrefab != null)
            {
                var food = (GameObject)PrefabUtility.InstantiatePrefab(foodPrefab);
                food.name = "MamCom_GomNuoiGiong";
                food.transform.SetParent(village.transform);
                food.transform.position = new Vector3(-6f, 0f, 18f);
                food.transform.localScale = Vector3.one * 1.35f;
            }
        }

        private static void BuildBanyanAndAncientWell(GameObject parent)
        {
            var area = CreateChild(parent, "CayDa_GiengNuoc_CoDai");
            area.transform.position = new Vector3(-16f, 0f, 8f);

            var stoneMat = LoadMat("World/Stone.mat");
            var woodMat = LoadMat("World/Wood.mat");
            var waterMat = LoadMat("World/Water.mat");

            var banyanPrefab = LoadPrefab("Meshy_CayDaCoThu_LangViet.prefab");
            if (banyanPrefab != null)
            {
                var banyan = (GameObject)PrefabUtility.InstantiatePrefab(banyanPrefab);
                banyan.name = "CayDaCoThu_LangViet";
                banyan.transform.SetParent(area.transform);
                banyan.transform.localPosition = new Vector3(-2f, 0f, 2f);
                banyan.transform.localScale = Vector3.one * 1.25f;
            }

            var well = CreateChild(area, "GiengNuoc_DaOng");
            well.transform.localPosition = new Vector3(3f, 0f, -2f);

            var curb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            curb.name = "ThanhGieng_DaOng";
            curb.transform.SetParent(well.transform);
            curb.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            curb.transform.localScale = new Vector3(3f, 0.45f, 3f);
            if (stoneMat != null) curb.GetComponent<Renderer>().sharedMaterial = stoneMat;

            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "MatNuoc_Gieng";
            water.transform.SetParent(well.transform);
            water.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            water.transform.localScale = new Vector3(2.4f, 0.05f, 2.4f);
            if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;

            var post1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post1.transform.SetParent(well.transform);
            post1.transform.localPosition = new Vector3(-1.3f, 1.2f, 0f);
            post1.transform.localScale = new Vector3(0.14f, 1.2f, 0.14f);
            if (woodMat != null) post1.GetComponent<Renderer>().sharedMaterial = woodMat;

            var post2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post2.transform.SetParent(well.transform);
            post2.transform.localPosition = new Vector3(1.3f, 1.2f, 0f);
            post2.transform.localScale = new Vector3(0.14f, 1.2f, 0.14f);
            if (woodMat != null) post2.GetComponent<Renderer>().sharedMaterial = woodMat;

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.transform.SetParent(well.transform);
            beam.transform.localPosition = new Vector3(0f, 2.38f, 0f);
            beam.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            beam.transform.localScale = new Vector3(0.12f, 1.4f, 0.12f);
            if (woodMat != null) beam.GetComponent<Renderer>().sharedMaterial = woodMat;

            var bucketAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/KayKit_Medieval_Hexagon_Pack/KayKit_Medieval_Hexagon_Pack_1.0_FREE/Assets/fbx/decoration/props/bucket_water.fbx");
            if (bucketAsset != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(bucketAsset);
                b.name = "GauMucNuoc";
                b.transform.SetParent(well.transform);
                b.transform.localPosition = new Vector3(0.5f, 0.8f, 0.4f);
                b.transform.localScale = Vector3.one * 1.5f;
            }
        }

        private static void BuildWaterBuffaloPasture(GameObject parent)
        {
            var pasture = CreateChild(parent, "BaiChanTrau_DongCo");
            pasture.transform.position = new Vector3(10f, 0f, 4f);

            var hideMat = LoadMat("World/GrassDark.mat");
            var hornMat = LoadMat("World/Wood.mat");
            var strawMat = LoadMat("World/Farm_Crops.mat");

            var buffalo = CreateChild(pasture, "ConTrau_NghiNgoi");
            buffalo.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);

            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Than_Trau";
            torso.transform.SetParent(buffalo.transform);
            torso.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            torso.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);
            torso.transform.localScale = new Vector3(1.3f, 1.75f, 1.1f);
            if (hideMat != null) torso.GetComponent<Renderer>().sharedMaterial = hideMat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Dau_Trau";
            head.transform.SetParent(buffalo.transform);
            head.transform.localPosition = new Vector3(0f, 1.15f, 1.4f);
            head.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            head.transform.localScale = new Vector3(0.65f, 0.6f, 0.85f);
            if (hideMat != null) head.GetComponent<Renderer>().sharedMaterial = hideMat;

            var hornL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornL.name = "Sung_Trau_L";
            hornL.transform.SetParent(buffalo.transform);
            hornL.transform.localPosition = new Vector3(-0.68f, 1.42f, 1.35f);
            hornL.transform.localRotation = Quaternion.Euler(20f, -40f, 65f);
            hornL.transform.localScale = new Vector3(0.13f, 0.6f, 0.13f);
            if (hornMat != null) hornL.GetComponent<Renderer>().sharedMaterial = hornMat;

            var hornR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornR.name = "Sung_Trau_R";
            hornR.transform.SetParent(buffalo.transform);
            hornR.transform.localPosition = new Vector3(0.68f, 1.42f, 1.35f);
            hornR.transform.localRotation = Quaternion.Euler(20f, 40f, -65f);
            hornR.transform.localScale = new Vector3(0.13f, 0.6f, 0.13f);
            if (hornMat != null) hornR.GetComponent<Renderer>().sharedMaterial = hornMat;

            var haystack = CreateChild(pasture, "DunRom_Vang");
            haystack.transform.localPosition = new Vector3(3.2f, 0f, 2.5f);
            var hayBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hayBase.transform.SetParent(haystack.transform);
            hayBase.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            hayBase.transform.localScale = new Vector3(3.4f, 0.8f, 3.4f);
            if (strawMat != null) hayBase.GetComponent<Renderer>().sharedMaterial = strawMat;

            var hayDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hayDome.transform.SetParent(haystack.transform);
            hayDome.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            hayDome.transform.localScale = new Vector3(2.9f, 2.3f, 2.9f);
            if (strawMat != null) hayDome.GetComponent<Renderer>().sharedMaterial = strawMat;
        }

        private static void BuildBambooForest(GameObject parent)
        {
            var grove = CreateChild(parent, "RungTre_LuyLang_PhuDong");
            var bambooPrefab = LoadPrefab("Meshy_KhomTreNga_ThanThanh.prefab");
            var foliagePrefab = LoadPrefab("Foliage/BambooGrove_Vietnamese_Legend.prefab");

            Vector3[] pos = {
                new Vector3(-22f, 0f, -16f), new Vector3(-24f, 0f, -8f),
                new Vector3(-24f, 0f, 0f), new Vector3(-22f, 0f, 8f),
                new Vector3(-18f, 0f, 18f), new Vector3(-10f, 0f, -24f),
                new Vector3(0f, 0f, -26f), new Vector3(10f, 0f, -26f)
            };

            for (int i = 0; i < pos.Length; i++)
            {
                var asset = (i % 2 == 0 && bambooPrefab != null) ? bambooPrefab : foliagePrefab;
                if (asset == null) continue;
                var b = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                b.name = $"RangTre_PhuDong_{i + 1}";
                b.transform.SetParent(grove.transform);
                b.transform.position = pos[i];
                b.transform.rotation = Quaternion.Euler(0f, i * 42f, 0f);
                b.transform.localScale = Vector3.one * (1.2f + (i % 3) * 0.2f);
            }

            var sacred = CreateChild(grove, "KhomTre_LinhThieng_Phase3");
            sacred.transform.position = new Vector3(-18f, 0f, -4f);
            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "HaoQuang_TreThan";
            glow.transform.SetParent(sacred.transform);
            glow.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            glow.transform.localScale = Vector3.one * 0.7f;
            var auraMat = LoadMat("Characters/SelectionRing.mat");
            if (auraMat != null) glow.GetComponent<Renderer>().sharedMaterial = auraMat;

            var pfxPrefab = LoadPrefab("Foliage/PFX_Forest_DriftingLeaves.prefab");
            if (pfxPrefab != null)
            {
                var pfx = (GameObject)PrefabUtility.InstantiatePrefab(pfxPrefab);
                pfx.name = "VFX_LaTre_RoiBay";
                pfx.transform.SetParent(grove.transform);
                pfx.transform.position = new Vector3(-16f, 3.5f, -4f);
            }
        }

        private static void BuildGameplayAnchors(GameObject parent)
        {
            var pSpawn = new GameObject("PlayerSpawnPoint_Giong");
            pSpawn.transform.SetParent(parent.transform);
            pSpawn.transform.position = new Vector3(0f, 0.05f, -8f);

            var eSpawns = CreateChild(parent, "EnemyWaveSpawners");
            Vector3[] spawns = { new Vector3(-10f, 0.05f, 8f), new Vector3(0f, 0.05f, 14f), new Vector3(10f, 0.05f, 8f) };
            for (int i = 0; i < spawns.Length; i++)
            {
                var s = new GameObject($"EnemySpawnPoint_{i + 1}");
                s.transform.SetParent(eSpawns.transform);
                s.transform.position = spawns[i];
                s.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var trig = new GameObject("Trigger_NhoTre_Phase3");
            trig.transform.SetParent(parent.transform);
            trig.transform.position = new Vector3(-18f, 0f, -4f);
            trig.AddComponent<SphereCollider>().isTrigger = true;
        }

        private static void SetupAtmosphereAndLighting(GameObject parent)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 110f;
            RenderSettings.fogColor = new Color(0.72f, 0.80f, 0.82f);

            var sun = new GameObject("MatTroi_BanMai_Sun");
            sun.transform.SetParent(parent.transform);
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.95f, 0.88f);
            l.intensity = 1.05f;
            l.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(34f, -30f, 0f);
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
            cam.transform.position = new Vector3(0f, 10f, -25f);
            cam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        }

        private static Material LoadMat(string relPath)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MatBase + relPath);
        }

        private static GameObject LoadPrefab(string relPath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + relPath);
        }

        private static void SetMatRecursive(GameObject go, Material mat)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }

        private static void EnsureDir(string path)
        {
            string d = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d);
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
