using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Constructs an authentic, richly detailed, compact preview map of ancient Phù Đổng Village.
    /// Combines realistic stilt houses, village gates, stone well under ancient banyan,
    /// resting water buffalo, terraced rice paddies with water reflection, sacred bamboo grove,
    /// blacksmith forge, lotus pond, and cinematic golden-hour atmospheric lighting.
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
            var root = new GameObject("PhuDong_Village_Environment");
            Undo.RegisterCreatedObjectUndo(root, "Build Phu Dong Village Map");

            var envRoot = CreateChild(root, "--- ENVIRONMENT ---");
            var gameplayRoot = CreateChild(root, "--- GAMEPLAY ---");
            var lightingRoot = CreateChild(root, "--- LIGHTING & ATMOSPHERE ---");

            BuildNaturalTerrain(envRoot);
            BuildStiltVillage(envRoot);
            BuildBanyanAndAncientWell(envRoot);
            BuildWaterBuffaloPasture(envRoot);
            BuildTerracedRicePaddies(envRoot);
            BuildBambooGroveWall(envRoot);
            BuildForgeLotusAndRocks(envRoot);

            BuildGameplayAnchors(gameplayRoot);
            SetupAtmosphereAndLighting(lightingRoot);
            SetupCinematicCamera(root);

            Debug.Log("[ThanhGiong] Authentic realistic Phu Dong Village map successfully constructed!");
        }

        [MenuItem("Thanh Giong/Create and Save Village Scene")]
        public static void CreateAndSaveScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildMapInActiveScene();

            EnsureDirectory(ScenePath);
            EnsureDirectory(SandboxPath);

            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.CopyAsset(ScenePath, SandboxPath);
            AssetDatabase.Refresh();
            Debug.Log($"[ThanhGiong] Saved realistic village scene to {ScenePath} and {SandboxPath}");
        }

        private static void BuildNaturalTerrain(GameObject parent)
        {
            var terrainRoot = CreateChild(parent, "DiaHinh_LangQue");
            var grassMat = GetValidMaterial("World/Grass.mat", new Color(0.24f, 0.40f, 0.16f), 0.85f);
            var soilMat = GetValidMaterial("World/Farm_Soil.mat", new Color(0.42f, 0.32f, 0.20f), 0.9f);
            var roadMat = GetValidMaterial("World/Road.mat", new Color(0.52f, 0.44f, 0.32f), 0.8f);

            // Main landscape meadow base
            var baseMeadow = GameObject.CreatePrimitive(PrimitiveType.Plane);
            baseMeadow.name = "ThamCo_LangQue_Base";
            baseMeadow.transform.SetParent(terrainRoot.transform);
            baseMeadow.transform.position = Vector3.zero;
            baseMeadow.transform.localScale = new Vector3(5.5f, 1f, 5.5f); // 55m x 55m
            baseMeadow.GetComponent<Renderer>().sharedMaterial = grassMat;

            // Central village clearing / combat court (organic ground patch)
            var court = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            court.name = "SanDat_TrungTam";
            court.transform.SetParent(terrainRoot.transform);
            court.transform.position = new Vector3(0f, 0.02f, 2f);
            court.transform.localScale = new Vector3(24f, 0.04f, 24f);
            court.GetComponent<Renderer>().sharedMaterial = soilMat;

            // Curving village road from south gate to stilt house
            Vector3[] pathWaypoints = {
                new Vector3(0f, 0.03f, -14f),
                new Vector3(-1f, 0.03f, -7f),
                new Vector3(0f, 0.03f, 2f),
                new Vector3(1f, 0.03f, 9f),
                new Vector3(0f, 0.03f, 15f)
            };
            for (int i = 0; i < pathWaypoints.Length - 1; i++)
            {
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"DuongMon_Doan_{i + 1}";
                seg.transform.SetParent(terrainRoot.transform);
                Vector3 mid = (pathWaypoints[i] + pathWaypoints[i + 1]) * 0.5f;
                Vector3 dir = pathWaypoints[i + 1] - pathWaypoints[i];
                seg.transform.position = mid;
                seg.transform.rotation = Quaternion.LookRotation(dir);
                seg.transform.localScale = new Vector3(3.6f, 0.04f, dir.magnitude + 0.3f);
                seg.GetComponent<Renderer>().sharedMaterial = roadMat;
            }
        }

        private static void BuildStiltVillage(GameObject parent)
        {
            var village = CreateChild(parent, "KhuLang_NhaSan_PhuDong");

            // Main Stilt House (Gióng's homestead)
            var housePrefab = LoadPrefab("Meshy_NhaTranh_LangGiong.prefab");
            if (housePrefab != null)
            {
                var mainHouse = (GameObject)PrefabUtility.InstantiatePrefab(housePrefab);
                mainHouse.name = "NhaSan_GiaDinh_Giong";
                mainHouse.transform.SetParent(village.transform);
                mainHouse.transform.position = new Vector3(-2f, 0f, 16f);
                mainHouse.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                mainHouse.transform.localScale = Vector3.one * 1.2f;

                // Secondary Stilt House (Storehouse / Neighbor)
                var secondHouse = (GameObject)PrefabUtility.InstantiatePrefab(housePrefab);
                secondHouse.name = "NhaSan_LangXom";
                secondHouse.transform.SetParent(village.transform);
                secondHouse.transform.position = new Vector3(14f, 0f, 17f);
                secondHouse.transform.rotation = Quaternion.Euler(0f, 165f, 0f);
                secondHouse.transform.localScale = Vector3.one * 1.05f;
            }

            // Traditional Village Defense Gate
            var gatePrefab = LoadPrefab("Meshy_CongLang_ChienLuy.prefab");
            if (gatePrefab != null)
            {
                var gate = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab);
                gate.name = "CongLang_ChienLuy_North";
                gate.transform.SetParent(village.transform);
                gate.transform.position = new Vector3(0f, 0f, 22f);
                gate.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            // Communal Bronze Drum Court
            var plazaPrefab = LoadPrefab("Meshy_SanDinh_TrongDong.prefab");
            if (plazaPrefab != null)
            {
                var plaza = (GameObject)PrefabUtility.InstantiatePrefab(plazaPrefab);
                plaza.name = "SanDinh_TrongDong_DongSon";
                plaza.transform.SetParent(village.transform);
                plaza.transform.position = new Vector3(6f, 0.02f, 6f);
                plaza.transform.rotation = Quaternion.Euler(0f, 20f, 0f);
            }

            // Feast Platter (Dân làng gom cơm cà nuôi Gióng)
            var foodPrefab = LoadPrefab("Meshy_MamLuongThuc_LangGiong.prefab");
            if (foodPrefab != null)
            {
                var food = (GameObject)PrefabUtility.InstantiatePrefab(foodPrefab);
                food.name = "MamCom_GomNuoiGiong";
                food.transform.SetParent(village.transform);
                food.transform.position = new Vector3(-5.5f, 0f, 12f);
                food.transform.localScale = Vector3.one * 1.3f;
            }

            // Battle Flags (Cờ Lệnh Phù Đổng)
            var flagPrefab = LoadPrefab("Meshy_CoLenh_PhuDong.prefab");
            if (flagPrefab != null)
            {
                var flagL = (GameObject)PrefabUtility.InstantiatePrefab(flagPrefab);
                flagL.name = "CoLenh_PhuDong_L";
                flagL.transform.SetParent(village.transform);
                flagL.transform.position = new Vector3(-6f, 0f, 20f);

                var flagR = (GameObject)PrefabUtility.InstantiatePrefab(flagPrefab);
                flagR.name = "CoLenh_PhuDong_R";
                flagR.transform.SetParent(village.transform);
                flagR.transform.position = new Vector3(6f, 0f, 20f);
            }
        }

        private static void BuildBanyanAndAncientWell(GameObject parent)
        {
            var area = CreateChild(parent, "CayDa_GiengNuoc_CoDai");
            area.transform.position = new Vector3(-13f, 0f, 6f);

            var stoneMat = GetValidMaterial("World/Stone.mat", new Color(0.48f, 0.49f, 0.46f), 0.7f);
            var woodMat = GetValidMaterial("World/Wood.mat", new Color(0.38f, 0.28f, 0.18f), 0.85f);
            var waterMat = GetValidMaterial("World/Water.mat", new Color(0.22f, 0.45f, 0.52f), 0.1f);

            // Grand Ancient Banyan Tree
            var banyanPrefab = LoadPrefab("Meshy_CayDaCoThu_LangViet.prefab");
            if (banyanPrefab != null)
            {
                var banyan = (GameObject)PrefabUtility.InstantiatePrefab(banyanPrefab);
                banyan.name = "CayDaCoThu_LangViet";
                banyan.transform.SetParent(area.transform);
                banyan.transform.localPosition = new Vector3(-2f, 0f, 2f);
                banyan.transform.localScale = Vector3.one * 1.15f;
            }

            // Authentic Ancient Stone Well
            var well = CreateChild(area, "GiengNuoc_DaOng");
            well.transform.localPosition = new Vector3(2.8f, 0f, -1.8f);

            // Outer stone curb (8 stone blocks arranged circular)
            var stoneCurb = CreateChild(well, "ThanhGieng_VanhDa");
            int stones = 10;
            float wellR = 1.35f;
            for (int i = 0; i < stones; i++)
            {
                float ang = i * Mathf.PI * 2f / stones;
                var blk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blk.name = $"TangDa_{i + 1}";
                blk.transform.SetParent(stoneCurb.transform);
                blk.transform.localPosition = new Vector3(Mathf.Cos(ang) * wellR, 0.4f, Mathf.Sin(ang) * wellR);
                blk.transform.localRotation = Quaternion.Euler(0f, -ang * Mathf.Rad2Deg + 90f, 0f);
                blk.transform.localScale = new Vector3(0.9f, 0.75f, 0.45f);
                blk.GetComponent<Renderer>().sharedMaterial = stoneMat;
            }

            // Inside water
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "MatNuoc_Gieng";
            water.transform.SetParent(well.transform);
            water.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            water.transform.localScale = new Vector3(2.2f, 0.05f, 2.2f);
            water.GetComponent<Renderer>().sharedMaterial = waterMat;

            // Wooden gallows & beam
            var post1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post1.name = "CotGo_1";
            post1.transform.SetParent(well.transform);
            post1.transform.localPosition = new Vector3(-1.25f, 1.25f, 0f);
            post1.transform.localScale = new Vector3(0.15f, 1.25f, 0.15f);
            post1.GetComponent<Renderer>().sharedMaterial = woodMat;

            var post2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post2.name = "CotGo_2";
            post2.transform.SetParent(well.transform);
            post2.transform.localPosition = new Vector3(1.25f, 1.25f, 0f);
            post2.transform.localScale = new Vector3(0.15f, 1.25f, 0.15f);
            post2.GetComponent<Renderer>().sharedMaterial = woodMat;

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "XaNgang_TreoGau";
            beam.transform.SetParent(well.transform);
            beam.transform.localPosition = new Vector3(0f, 2.45f, 0f);
            beam.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            beam.transform.localScale = new Vector3(0.12f, 1.35f, 0.12f);
            beam.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Bucket
            var bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucket.name = "GauMucNuoc_Go";
            bucket.transform.SetParent(well.transform);
            bucket.transform.localPosition = new Vector3(0.4f, 0.78f, 0.3f);
            bucket.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            bucket.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Paved stone apron
            var apron = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            apron.name = "SanDa_QuanhGieng";
            apron.transform.SetParent(well.transform);
            apron.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            apron.transform.localScale = new Vector3(4.2f, 0.03f, 4.2f);
            apron.GetComponent<Renderer>().sharedMaterial = stoneMat;
        }

        private static void BuildWaterBuffaloPasture(GameObject parent)
        {
            var pasture = CreateChild(parent, "BaiChanTrau_DongCo");
            pasture.transform.position = new Vector3(13f, 0f, 6f);

            var buffaloMat = GetValidMaterial("World/GrassDark.mat", new Color(0.18f, 0.19f, 0.20f), 0.5f);
            var hornMat = GetValidMaterial("World/Wood.mat", new Color(0.10f, 0.10f, 0.10f), 0.3f);
            var strawMat = GetValidMaterial("World/Farm_Crops.mat", new Color(0.78f, 0.65f, 0.30f), 0.9f);
            var woodMat = GetValidMaterial("World/Wood.mat", new Color(0.38f, 0.28f, 0.18f), 0.85f);

            // Water Buffalo (Con trâu nằm nghỉ bên đụn rơm)
            var buffalo = CreateChild(pasture, "ConTrau_Mong_NghiNgoi");
            buffalo.transform.localPosition = new Vector3(0f, 0f, 0f);
            buffalo.transform.localRotation = Quaternion.Euler(0f, -35f, 0f);

            // Body
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Than_Trau";
            torso.transform.SetParent(buffalo.transform);
            torso.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            torso.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);
            torso.transform.localScale = new Vector3(1.3f, 1.75f, 1.1f);
            torso.GetComponent<Renderer>().sharedMaterial = buffaloMat;

            // Rump
            var rump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rump.name = "Mong_Trau";
            rump.transform.SetParent(buffalo.transform);
            rump.transform.localPosition = new Vector3(0f, 0.72f, -0.9f);
            rump.transform.localScale = new Vector3(1.25f, 1.05f, 1.1f);
            rump.GetComponent<Renderer>().sharedMaterial = buffaloMat;

            // Head & Neck
            var neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Co_Trau";
            neck.transform.SetParent(buffalo.transform);
            neck.transform.localPosition = new Vector3(0f, 0.9f, 0.9f);
            neck.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
            neck.transform.localScale = new Vector3(0.7f, 0.55f, 0.7f);
            neck.GetComponent<Renderer>().sharedMaterial = buffaloMat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Dau_Trau";
            head.transform.SetParent(buffalo.transform);
            head.transform.localPosition = new Vector3(0f, 1.15f, 1.4f);
            head.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            head.transform.localScale = new Vector3(0.65f, 0.6f, 0.85f);
            head.GetComponent<Renderer>().sharedMaterial = buffaloMat;

            // Curved Horns (Sừng trâu cong vút đặc trưng)
            var hornL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornL.name = "Sung_Trau_L";
            hornL.transform.SetParent(buffalo.transform);
            hornL.transform.localPosition = new Vector3(-0.68f, 1.42f, 1.35f);
            hornL.transform.localRotation = Quaternion.Euler(20f, -40f, 65f);
            hornL.transform.localScale = new Vector3(0.13f, 0.6f, 0.13f);
            hornL.GetComponent<Renderer>().sharedMaterial = hornMat;

            var hornR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornR.name = "Sung_Trau_R";
            hornR.transform.SetParent(buffalo.transform);
            hornR.transform.localPosition = new Vector3(0.68f, 1.42f, 1.35f);
            hornR.transform.localRotation = Quaternion.Euler(20f, 40f, -65f);
            hornR.transform.localScale = new Vector3(0.13f, 0.6f, 0.13f);
            hornR.GetComponent<Renderer>().sharedMaterial = hornMat;

            // Realistic Haystack (Đụn rơm vàng nhiều tầng)
            var haystack = CreateChild(pasture, "DunRom_Vang_LangQue");
            haystack.transform.localPosition = new Vector3(2.8f, 0f, 2.6f);

            var hayBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hayBase.name = "Chan_DunRom";
            hayBase.transform.SetParent(haystack.transform);
            hayBase.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            hayBase.transform.localScale = new Vector3(3.2f, 0.8f, 3.2f);
            hayBase.GetComponent<Renderer>().sharedMaterial = strawMat;

            var hayDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hayDome.name = "Chop_DunRom";
            hayDome.transform.SetParent(haystack.transform);
            hayDome.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            hayDome.transform.localScale = new Vector3(2.8f, 2.2f, 2.8f);
            hayDome.GetComponent<Renderer>().sharedMaterial = strawMat;

            // Wooden hitching stake
            var stake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stake.name = "CocBuoc_Thung";
            stake.transform.SetParent(pasture.transform);
            stake.transform.localPosition = new Vector3(-1.6f, 0.5f, 1f);
            stake.transform.localScale = new Vector3(0.14f, 0.5f, 0.14f);
            stake.GetComponent<Renderer>().sharedMaterial = woodMat;
        }

        private static void BuildTerracedRicePaddies(GameObject parent)
        {
            var paddies = CreateChild(parent, "DongRuong_BacThang_LuaNuoc");
            paddies.transform.position = new Vector3(17f, 0f, -5f);

            var soilMat = GetValidMaterial("World/Farm_Soil.mat", new Color(0.35f, 0.28f, 0.18f), 0.9f);
            var waterMat = GetValidMaterial("World/Water.mat", new Color(0.25f, 0.48f, 0.50f), 0.08f);
            var cropMat = GetValidMaterial("World/Farm_Crops.mat", new Color(0.32f, 0.65f, 0.18f), 0.7f);

            for (int t = 0; t < 3; t++)
            {
                float elevation = t * 0.4f;
                float x = t * 5.2f;

                var terrace = CreateChild(paddies, $"ThuaRuong_Bac_{t + 1}");
                terrace.transform.localPosition = new Vector3(x, elevation, 0f);

                // Terraced earth dyke
                var berm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                berm.name = "BoRuong_DatSet";
                berm.transform.SetParent(terrace.transform);
                berm.transform.localPosition = Vector3.zero;
                berm.transform.localScale = new Vector3(4.8f, 0.38f, 20f);
                berm.GetComponent<Renderer>().sharedMaterial = soilMat;

                // Reflective water surface
                var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "MatNuoc_RuongLua";
                water.transform.SetParent(terrace.transform);
                water.transform.localPosition = new Vector3(0f, 0.16f, 0f);
                water.transform.localScale = new Vector3(4.3f, 0.05f, 19f);
                water.GetComponent<Renderer>().sharedMaterial = waterMat;

                // Green rice shoot clusters
                for (int z = -8; z <= 8; z += 2)
                {
                    for (int off = -1; off <= 1; off += 2)
                    {
                        var shoot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        shoot.name = $"KhomMa_{z}_{off}";
                        shoot.transform.SetParent(terrace.transform);
                        shoot.transform.localPosition = new Vector3(off * 1.1f, 0.32f, z);
                        shoot.transform.localScale = new Vector3(0.45f, 0.28f, 0.45f);
                        shoot.GetComponent<Renderer>().sharedMaterial = cropMat;
                    }
                }
            }
        }

        private static void BuildBambooGroveWall(GameObject parent)
        {
            var grove = CreateChild(parent, "LuyTreNga_XanhMat");
            var bambooPrefab = LoadPrefab("Meshy_KhomTreNga_ThanThanh.prefab");
            var foliagePrefab = LoadPrefab("Foliage/BambooGrove_Vietnamese_Legend.prefab");

            Vector3[] bambooPositions = {
                new Vector3(-17f, 0f, -12f),
                new Vector3(-18f, 0f, -5f),
                new Vector3(-18f, 0f, 2f),
                new Vector3(-16f, 0f, 9f),
                new Vector3(-10f, 0f, -18f),
                new Vector3(-2f, 0f, -20f)
            };

            for (int i = 0; i < bambooPositions.Length; i++)
            {
                var prefab = (i % 2 == 0 && bambooPrefab != null) ? bambooPrefab : foliagePrefab;
                if (prefab != null)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    b.name = $"BuiTre_Xanh_{i + 1}";
                    b.transform.SetParent(grove.transform);
                    b.transform.position = bambooPositions[i];
                    b.transform.rotation = Quaternion.Euler(0f, i * 45f + 15f, 0f);
                    b.transform.localScale = Vector3.one * (1.15f + (i % 3) * 0.15f);
                }
            }

            // Sacred Bamboo Grove for Phase 3 Interaction
            var sacredSpot = CreateChild(grove, "KhomTre_LinhThieng_Phase3");
            sacredSpot.transform.position = new Vector3(-15f, 0f, -5f);

            var auraMat = GetValidMaterial("Characters/SelectionRing.mat", new Color(1f, 0.85f, 0.3f), 0.1f);
            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "HaoQuang_TreThan";
            glow.transform.SetParent(sacredSpot.transform);
            glow.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            glow.transform.localScale = Vector3.one * 0.6f;
            glow.GetComponent<Renderer>().sharedMaterial = auraMat;

            // Falling leaves particle system
            var leavesPrefab = LoadPrefab("Foliage/PFX_Forest_DriftingLeaves.prefab");
            if (leavesPrefab != null)
            {
                var pfx = (GameObject)PrefabUtility.InstantiatePrefab(leavesPrefab);
                pfx.name = "VFX_LaTre_RoiBay";
                pfx.transform.SetParent(grove.transform);
                pfx.transform.position = new Vector3(-13f, 3f, -5f);
            }
        }

        private static void BuildForgeLotusAndRocks(GameObject parent)
        {
            // Blacksmith Forge
            var forgePrefab = LoadPrefab("Meshy_LoRenThep_VanLang.prefab");
            if (forgePrefab != null)
            {
                var forge = (GameObject)PrefabUtility.InstantiatePrefab(forgePrefab);
                forge.name = "LoRenThep_PhuDong";
                forge.transform.SetParent(parent.transform);
                forge.transform.position = new Vector3(9f, 0f, -14f);
                forge.transform.rotation = Quaternion.Euler(0f, -40f, 0f);
            }

            // Sacred Mountain Rocks around the Forge
            var rockPrefab = LoadPrefab("Meshy_DaNui_RungThieng.prefab");
            if (rockPrefab != null)
            {
                var rock = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                rock.name = "DaNui_PhuDong";
                rock.transform.SetParent(parent.transform);
                rock.transform.position = new Vector3(14f, 0f, -17f);
                rock.transform.localScale = Vector3.one * 1.3f;
            }

            // Lotus Pond & Wooden Bridge
            var lotusPrefab = LoadPrefab("Meshy_DamSenCo_VanLang.prefab");
            if (lotusPrefab != null)
            {
                var lotus = (GameObject)PrefabUtility.InstantiatePrefab(lotusPrefab);
                lotus.name = "DamSenCo_VanLang";
                lotus.transform.SetParent(parent.transform);
                lotus.transform.position = new Vector3(-2f, 0f, -16f);
            }

            var bridgePrefab = LoadPrefab("Meshy_CauGo_CoDai.prefab");
            if (bridgePrefab != null)
            {
                var bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab);
                bridge.name = "CauGo_BacQuaDamSen";
                bridge.transform.SetParent(parent.transform);
                bridge.transform.position = new Vector3(3f, 0f, -15f);
                bridge.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
        }

        private static void BuildGameplayAnchors(GameObject parent)
        {
            var pSpawn = new GameObject("PlayerSpawnPoint_Giong");
            pSpawn.transform.SetParent(parent.transform);
            pSpawn.transform.position = new Vector3(0f, 0f, -5f);
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
            trigger.transform.position = new Vector3(-15f, 0f, -5f);
            var col = trigger.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 3.5f;
        }

        private static void SetupAtmosphereAndLighting(GameObject parent)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 16f;
            RenderSettings.fogEndDistance = 65f;
            RenderSettings.fogColor = new Color(0.78f, 0.77f, 0.70f);

            var sun = new GameObject("MatTroi_BanMai_Sun");
            sun.transform.SetParent(parent.transform);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.92f, 0.80f);
            light.intensity = 1.35f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(36f, -32f, 0f);
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
            camGo.transform.position = new Vector3(0f, 8.5f, -16f);
            camGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        }

        private static Material GetValidMaterial(string path, Color fallbackColor, float smoothness)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MatBasePath + path);
            if (mat != null && mat.shader != null && mat.shader.isSupported && !mat.shader.name.Contains("Error"))
            {
                return mat;
            }

            var urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null) urpShader = Shader.Find("Standard");

            var newMat = new Material(urpShader)
            {
                color = fallbackColor
            };
            if (newMat.HasProperty("_Smoothness")) newMat.SetFloat("_Smoothness", smoothness);
            return newMat;
        }

        private static GameObject LoadPrefab(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBasePath + relativePath);
        }

        private static void EnsureDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
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
