using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Editor utility to construct the ancient Vietnamese Phù Đổng Village map
    /// combining combat arena, stilt houses, water well, buffalo pasture, rice fields, and bamboo groves.
    /// </summary>
    public static class PhuDongVillageMapBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Battlefield.unity";
        private const string SandboxPath = "Assets/_Game/Scenes/Sandbox/PhuDongVillage.unity";
        private const string ModelBasePath = "Assets/_Game/Art/Models/Environment/";

        [MenuItem("Thanh Giong/Build Phu Dong Village Map")]
        public static void BuildMapInActiveScene()
        {
            var root = new GameObject("PhuDong_Village_Environment");
            Undo.RegisterCreatedObjectUndo(root, "Build Phu Dong Village Map");

            var envRoot = CreateChild(root, "--- ENVIRONMENT ---");
            var gameplayRoot = CreateChild(root, "--- GAMEPLAY ---");
            var lightingRoot = CreateChild(root, "--- LIGHTING & ATMOSPHERE ---");

            BuildTerrainAndArena(envRoot);
            BuildStiltVillage(envRoot);
            BuildAncientWell(envRoot);
            BuildBuffaloPasture(envRoot);
            BuildRiceFields(envRoot);
            BuildBambooGroves(envRoot);
            BuildCommunalPlazaAndForge(envRoot);

            BuildGameplayAnchors(gameplayRoot);
            SetupLightingAndAtmosphere(lightingRoot);

            Debug.Log("[ThanhGiong] Phu Dong Village map successfully constructed!");
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

            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.CopyAsset(ScenePath, SandboxPath);
            AssetDatabase.Refresh();
            Debug.Log($"[ThanhGiong] Saved village scene to {ScenePath} and {SandboxPath}");
        }

        private static void BuildTerrainAndArena(GameObject parent)
        {
            var arena = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arena.name = "Arena_Battlefield_Ground";
            arena.transform.SetParent(parent.transform);
            arena.transform.position = new Vector3(0f, -0.05f, 0f);
            arena.transform.localScale = new Vector3(60f, 0.1f, 60f);

            var boundary = CreateChild(parent, "Arena_Wooden_Palisade");
            int fencePoles = 24;
            float radius = 29f;
            for (int i = 0; i < fencePoles; i++)
            {
                float angle = i * Mathf.PI * 2f / fencePoles;
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = $"Palisade_Stake_{i}";
                pole.transform.SetParent(boundary.transform);
                pole.transform.position = new Vector3(Mathf.Cos(angle) * radius, 0.8f, Mathf.Sin(angle) * radius);
                pole.transform.localScale = new Vector3(0.35f, 0.8f, 0.35f);
            }
        }

        private static void BuildStiltVillage(GameObject parent)
        {
            var village = CreateChild(parent, "KhuLang_NhaSan_Village");
            string stiltHousePath = ModelBasePath + "NhaTranh_LangGiong/NhaTranh_LangGiong.fbx";
            var stiltHouseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(stiltHousePath);

            Vector3[] housePositions = {
                new Vector3(-25f, 0f, 32f),
                new Vector3(-12f, 0f, 40f),
                new Vector3(5f, 0f, 38f),
                new Vector3(20f, 0f, 30f)
            };
            float[] rotations = { 15f, -10f, 25f, -20f };

            for (int i = 0; i < housePositions.Length; i++)
            {
                GameObject house = stiltHouseAsset != null
                    ? (GameObject)PrefabUtility.InstantiatePrefab(stiltHouseAsset)
                    : CreateStiltHouseBlockout($"NhaSan_VietCo_{i}");

                house.name = $"NhaSan_VietCo_{i + 1}";
                house.transform.SetParent(village.transform);
                house.transform.position = housePositions[i];
                house.transform.rotation = Quaternion.Euler(0f, rotations[i], 0f);
            }

            var gateAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelBasePath + "CongLang_ChienLuy/CongLang_ChienLuy.fbx");
            if (gateAsset != null)
            {
                var gate = (GameObject)PrefabUtility.InstantiatePrefab(gateAsset);
                gate.name = "CongLang_ChienLuy_North";
                gate.transform.SetParent(village.transform);
                gate.transform.position = new Vector3(0f, 0f, 48f);
                gate.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        private static void BuildAncientWell(GameObject parent)
        {
            var wellGroup = CreateChild(parent, "GiengNuoc_CayDa_Area");
            wellGroup.transform.position = new Vector3(-28f, 0f, 12f);

            var wellRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellRing.name = "GiengNuoc_ThanhDa";
            wellRing.transform.SetParent(wellGroup.transform);
            wellRing.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            wellRing.transform.localScale = new Vector3(3.2f, 0.45f, 3.2f);

            var wellWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellWater.name = "GiengNuoc_MatNuoc";
            wellWater.transform.SetParent(wellGroup.transform);
            wellWater.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            wellWater.transform.localScale = new Vector3(2.6f, 0.05f, 2.6f);

            var banyanAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelBasePath + "CayDaCoThu_LangViet/CayDaCoThu_LangViet.fbx");
            if (banyanAsset != null)
            {
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(banyanAsset);
                tree.name = "CayDaCoThu_GocGieng";
                tree.transform.SetParent(wellGroup.transform);
                tree.transform.localPosition = new Vector3(-5f, 0f, 4f);
                tree.transform.localScale = Vector3.one * 1.2f;
            }
        }

        private static void BuildBuffaloPasture(GameObject parent)
        {
            var pasture = CreateChild(parent, "BaiChanTrau_BuffaloPasture");
            pasture.transform.position = new Vector3(26f, 0f, 15f);

            var buffalo = CreateChild(pasture, "ConTrau_LangViet");
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Trau_Body";
            body.transform.SetParent(buffalo.transform);
            body.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(1.1f, 1.6f, 0.95f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Trau_Head";
            head.transform.SetParent(buffalo.transform);
            head.transform.localPosition = new Vector3(0f, 1.4f, 1.4f);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.8f);

            var hornL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornL.name = "Sung_Trau_L";
            hornL.transform.SetParent(buffalo.transform);
            hornL.transform.localPosition = new Vector3(-0.6f, 1.7f, 1.3f);
            hornL.transform.localRotation = Quaternion.Euler(20f, -45f, 60f);
            hornL.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);

            var hornR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hornR.name = "Sung_Trau_R";
            hornR.transform.SetParent(buffalo.transform);
            hornR.transform.localPosition = new Vector3(0.6f, 1.7f, 1.3f);
            hornR.transform.localRotation = Quaternion.Euler(20f, 45f, -60f);
            hornR.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);

            var haystack = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            haystack.name = "DunRom_LangViet";
            haystack.transform.SetParent(pasture.transform);
            haystack.transform.localPosition = new Vector3(4f, 1.2f, 3f);
            haystack.transform.localScale = new Vector3(3.5f, 2.5f, 3.5f);
        }

        private static void BuildRiceFields(GameObject parent)
        {
            var fieldsRoot = CreateChild(parent, "DongRuong_BacThang_Fields");
            fieldsRoot.transform.position = new Vector3(36f, 0f, -8f);

            for (int terrace = 0; terrace < 3; terrace++)
            {
                float elevation = terrace * 0.45f;
                float xOffset = terrace * 7f;

                var terraceBerm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                terraceBerm.name = $"BoRuong_Thua_{terrace + 1}";
                terraceBerm.transform.SetParent(fieldsRoot.transform);
                terraceBerm.transform.localPosition = new Vector3(xOffset, elevation, 0f);
                terraceBerm.transform.localScale = new Vector3(6f, 0.4f, 26f);

                var waterSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
                waterSurface.name = $"MatNuocRuong_{terrace + 1}";
                waterSurface.transform.SetParent(fieldsRoot.transform);
                waterSurface.transform.localPosition = new Vector3(xOffset, elevation + 0.15f, 0f);
                waterSurface.transform.localScale = new Vector3(0.55f, 1f, 2.4f);
            }
        }

        private static void BuildBambooGroves(GameObject parent)
        {
            var grove = CreateChild(parent, "LuyTre_PhuDong_Grove");
            string bambooPath = ModelBasePath + "KhomTreNga_ThanThanh/KhomTreNga_ThanThanh.fbx";
            var bambooAsset = AssetDatabase.LoadAssetAtPath<GameObject>(bambooPath);

            Vector3[] bambooPositions = {
                new Vector3(-32f, 0f, -10f),
                new Vector3(-34f, 0f, 5f),
                new Vector3(-30f, 0f, 22f),
                new Vector3(-20f, 0f, -28f),
                new Vector3(-5f, 0f, -34f),
                new Vector3(15f, 0f, -32f)
            };

            for (int i = 0; i < bambooPositions.Length; i++)
            {
                GameObject b = bambooAsset != null
                    ? (GameObject)PrefabUtility.InstantiatePrefab(bambooAsset)
                    : CreateBambooBlockout($"KhomTre_{i}");

                b.name = $"KhomTre_PhuDong_{i + 1}";
                b.transform.SetParent(grove.transform);
                b.transform.position = bambooPositions[i];
                b.transform.rotation = Quaternion.Euler(0f, i * 45f, 0f);
                b.transform.localScale = Vector3.one * (1.1f + (i % 3) * 0.2f);
            }

            var sacredBamboo = CreateChild(grove, "KhomTre_LinhThieng_Phase3");
            sacredBamboo.transform.position = new Vector3(-26f, 0f, -5f);
            var sacredMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sacredMarker.name = "Sacred_Glow_Anchor";
            sacredMarker.transform.SetParent(sacredBamboo.transform);
            sacredMarker.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            sacredMarker.transform.localScale = Vector3.one * 0.6f;
        }

        private static void BuildCommunalPlazaAndForge(GameObject parent)
        {
            var plazaAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelBasePath + "SanDinh_TrongDong/SanDinh_TrongDong.fbx");
            if (plazaAsset != null)
            {
                var plaza = (GameObject)PrefabUtility.InstantiatePrefab(plazaAsset);
                plaza.name = "SanDinh_TrongDong_Plaza";
                plaza.transform.SetParent(parent.transform);
                plaza.transform.position = new Vector3(-10f, 0f, 20f);
            }

            var forgeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelBasePath + "LoRenThep_VanLang/LoRenThep_VanLang.fbx");
            if (forgeAsset != null)
            {
                var forge = (GameObject)PrefabUtility.InstantiatePrefab(forgeAsset);
                forge.name = "LoRenThep_PhuDong";
                forge.transform.SetParent(parent.transform);
                forge.transform.position = new Vector3(25f, 0f, -24f);
            }

            var lotusAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelBasePath + "DamSenCo_VanLang/DamSenCo_VanLang.fbx");
            if (lotusAsset != null)
            {
                var lotus = (GameObject)PrefabUtility.InstantiatePrefab(lotusAsset);
                lotus.name = "DamSenCo_VanLang";
                lotus.transform.SetParent(parent.transform);
                lotus.transform.position = new Vector3(2f, 0f, -38f);
            }
        }

        private static void BuildGameplayAnchors(GameObject parent)
        {
            var pSpawn = new GameObject("PlayerSpawnPoint");
            pSpawn.transform.SetParent(parent.transform);
            pSpawn.transform.position = new Vector3(0f, 0f, -12f);
            pSpawn.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var enemySpawns = CreateChild(parent, "EnemyWaveSpawners");
            Vector3[] spawnPoints = {
                new Vector3(-20f, 0f, 20f),
                new Vector3(0f, 0f, 25f),
                new Vector3(20f, 0f, 20f)
            };
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                var sp = new GameObject($"EnemySpawn_{i + 1}");
                sp.transform.SetParent(enemySpawns.transform);
                sp.transform.position = spawnPoints[i];
                sp.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var trigger = new GameObject("BambooInteractionTrigger");
            trigger.transform.SetParent(parent.transform);
            trigger.transform.position = new Vector3(-26f, 0f, -5f);
            var col = trigger.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 3.5f;
        }

        private static void SetupLightingAndAtmosphere(GameObject parent)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = 85f;
            RenderSettings.fogColor = new Color(0.78f, 0.76f, 0.68f);

            var sun = new GameObject("DirectionalLight_MorningSun");
            sun.transform.SetParent(parent.transform);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.94f, 0.82f);
            light.intensity = 1.35f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        }

        private static GameObject CreateStiltHouseBlockout(string name)
        {
            var house = new GameObject(name);
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "House_Body";
            body.transform.SetParent(house.transform);
            body.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            body.transform.localScale = new Vector3(8f, 3f, 5f);
            return house;
        }

        private static GameObject CreateBambooBlockout(string name)
        {
            var cluster = new GameObject(name);
            for (int i = 0; i < 5; i++)
            {
                var stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stalk.name = $"Bamboo_Stalk_{i}";
                stalk.transform.SetParent(cluster.transform);
                stalk.transform.localPosition = new Vector3((i - 2) * 0.4f, 4f, Random.Range(-0.3f, 0.3f));
                stalk.transform.localScale = new Vector3(0.2f, 4f, 0.2f);
            }
            return cluster;
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

