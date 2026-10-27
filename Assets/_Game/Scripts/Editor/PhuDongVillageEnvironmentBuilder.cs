using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds environment geometry, soaring karst mountains, winding river, rice terraces,
    /// and lush East Asian wuxia foliage.
    /// </summary>
    public static class PhuDongVillageEnvironmentBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string MatBase = "Assets/_Game/Art/Materials/";
        private const string OpenWorldBase = "Assets/_Game/Art/OpenWorld/";
        private const string KayKitBase = "Assets/ThirdParty/KayKit_Medieval_Hexagon_Pack/KayKit_Medieval_Hexagon_Pack_1.0_FREE/Assets/fbx/";

        public static void BuildTerrainAndRoads(GameObject env)
        {
            var root = CreateChild(env, "DiaHinh_Va_DuongLang");

            // Continuous undulating meadow terrain with BotW shader
            var groundMesh = AssetDatabase.LoadAssetAtPath<Mesh>(OpenWorldBase + "ContinuousGround.asset");
            var groundMat = AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "BiomeGround.mat");
            if (groundMesh != null)
            {
                var terrainGo = new GameObject("ThungLung_ContinuousGround_700m");
                terrainGo.transform.SetParent(root.transform, false);
                terrainGo.transform.position = new Vector3(-55f, 0f, 0f);
                terrainGo.AddComponent<MeshFilter>().sharedMesh = groundMesh;
                var mr = terrainGo.AddComponent<MeshRenderer>();
                if (groundMat != null) mr.sharedMaterial = groundMat;
                terrainGo.AddComponent<MeshCollider>().sharedMesh = groundMesh;
            }

            // Earthen road with cobblestones
            var roadMesh = AssetDatabase.LoadAssetAtPath<Mesh>(OpenWorldBase + "Road_VillageMainPath_PhuDong.asset");
            var roadMat = AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "ZeldaRoadEarth.mat");
            if (roadMesh != null)
            {
                var roadGo = new GameObject("DuongLang_PhuDong_Co");
                roadGo.transform.SetParent(root.transform, false);
                roadGo.transform.position = new Vector3(-55f, 0.05f, 0f);
                roadGo.AddComponent<MeshFilter>().sharedMesh = roadMesh;
                var rmr = roadGo.AddComponent<MeshRenderer>();
                if (roadMat != null) rmr.sharedMaterial = roadMat;
            }
        }

        public static void BuildMountains(GameObject env)
        {
            // Soaring karst limestone pinnacles (Ninh Binh / Wuxia style)
            PhuDongAsianCultureBuilder.BuildSoaringKarstPillars(env);
        }

        public static void BuildSkyAndClouds(GameObject env)
        {
            var root = CreateChild(env, "BauTroi_MayBay_3D");
            var skyMat = LoadMat("Sky/ThanhGiong_Sky.mat");
            if (skyMat != null) RenderSettings.skybox = skyMat;

            var cloudA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/cloud_big.fbx");
            var cloudB = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/cloud_small.fbx");
            var cloudMat = LoadMat("Sky/ThanhGiong_Cloud.mat");

            for (int i = 0; i < 18; i++)
            {
                float ang = i * Mathf.PI * 2f / 18;
                float r = Random.Range(35f, 85f);
                var asset = (i % 2 == 0) ? cloudA : cloudB;
                if (asset == null) continue;

                var c = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                c.name = $"DamMay_3D_{i + 1}";
                c.transform.position = new Vector3(Mathf.Cos(ang) * r, Random.Range(40f, 62f), Mathf.Sin(ang) * r);
                c.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                c.transform.localScale = Vector3.one * Random.Range(12f, 26f);
                if (cloudMat != null) SetMatRecursive(c, cloudMat);
            }
        }

        public static void BuildRiverAndBridges(GameObject env)
        {
            var root = CreateChild(env, "DongSong_CauGo_DamSen");
            var waterMat = LoadMat("Meshy/Environment/Mat_DongSong_NuocBiec.mat");

            // Broad serene river (85m x 15m) with jade turquoise Zelda water shader
            var river = GameObject.CreatePrimitive(PrimitiveType.Cube);
            river.name = "DongSong_SuoiLangGiong";
            river.transform.SetParent(root.transform, false);
            river.transform.position = new Vector3(0f, 0.05f, -15.5f);
            river.transform.localScale = new Vector3(88f, 0.08f, 10.5f);
            var riverCol = river.GetComponent<Collider>();
            if (riverCol != null) Object.DestroyImmediate(riverCol);
            if (waterMat != null) river.GetComponent<Renderer>().sharedMaterial = waterMat;

            // Two covered wooden bridges (Cầu gỗ cổ mái ngói)
            var bridgePrefab = LoadPrefab("Meshy_CauGo_CoDai.prefab");
            if (bridgePrefab != null)
            {
                var b1 = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab, root.transform);
                b1.name = "CauGo_QuaSong_Chinh";
                b1.transform.position = new Vector3(0f, -0.1f, -15.5f);
                b1.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                b1.transform.localScale = new Vector3(2.15f, 1f, 2.1f);

                var b2 = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab, root.transform);
                b2.name = "CauGo_Phu_Tay";
                b2.transform.position = new Vector3(-24f, -0.1f, -15.5f);
                b2.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                b2.transform.localScale = new Vector3(1.8f, 1f, 1.6f);
            }

            // Ancient Lotus Pond (Đầm sen cổ Văn Lang)
            var lotusPrefab = LoadPrefab("Meshy_DamSenCo_VanLang.prefab");
            if (lotusPrefab != null)
            {
                var lotus = (GameObject)PrefabUtility.InstantiatePrefab(lotusPrefab, root.transform);
                lotus.name = "DamSenCo_VanLang";
                lotus.transform.position = new Vector3(22f, 0.06f, -15.5f);
                lotus.transform.localScale = Vector3.one * 1.35f;
            }

            // Stepping stones across the lotus pond (Đá dặm bước như Nghịch Thủy Hàn ảnh 2 & 3)
            PhuDongAsianCultureBuilder.BuildLotusSteppingStones(root);

            // River dock, wooden sampans, cart, and pottery jars (Bến đò & thuyền nan như ảnh 1 & 2)
            PhuDongAsianCultureBuilder.BuildRiverDockAndSampans(root);

            // River shoreline details: rocks, water lilies, reeds
            var rockA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/rock_single_A.fbx");
            var lilyA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterlily_A.fbx");

            for (int i = 0; i < 14; i++)
            {
                float x = -36f + i * 5.5f;
                if (rockA != null)
                {
                    var rk = (GameObject)PrefabUtility.InstantiatePrefab(rockA, root.transform);
                    rk.name = $"DaBoSong_{i + 1}";
                    rk.transform.position = new Vector3(x, 0.08f, (i % 2 == 0) ? -10.5f : -20.5f);
                    rk.transform.localScale = Vector3.one * Random.Range(2.2f, 4f);
                }
                if (lilyA != null && i % 2 == 0)
                {
                    var lily = (GameObject)PrefabUtility.InstantiatePrefab(lilyA, root.transform);
                    lily.name = $"HoaSen_{i + 1}";
                    lily.transform.position = new Vector3(x + 1.8f, 0.12f, -15.5f + Random.Range(-3f, 3f));
                    lily.transform.localScale = Vector3.one * 2.2f;
                }
            }
        }

        public static void BuildRiceTerraces(GameObject env)
        {
            var root = CreateChild(env, "DongRuong_BacThang_LuaNuoc");
            root.transform.position = new Vector3(22f, 0f, 6f);

            var soilMat = AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "RiceFieldSoil.mat") ?? LoadMat("World/Farm_Soil.mat");
            var waterMat = LoadMat("Meshy/Environment/Mat_DongSong_NuocBiec.mat");
            var reedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_A.fbx");
            var riceMat = AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "GoldenRice.mat");

            for (int t = 0; t < 3; t++)
            {
                float elev = t * 0.4f;
                float x = t * 7.5f;
                var terrace = CreateChild(root, $"ThuaRuong_Bac_{t + 1}");
                terrace.transform.localPosition = new Vector3(x, elev, 0f);

                // Earthen dike
                var berm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                berm.name = "BoRuong_PhuSa";
                berm.transform.SetParent(terrace.transform, false);
                berm.transform.localPosition = Vector3.zero;
                berm.transform.localScale = new Vector3(7.2f, 0.42f, 28f);
                if (soilMat != null) berm.GetComponent<Renderer>().sharedMaterial = soilMat;

                // Flooded water mirror
                var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "MatNuoc_PhanChieu";
                water.transform.SetParent(terrace.transform, false);
                water.transform.localPosition = new Vector3(0f, 0.22f, 0f);
                water.transform.localScale = new Vector3(6.6f, 0.05f, 27f);
                var wCol = water.GetComponent<Collider>();
                if (wCol != null) Object.DestroyImmediate(wCol);
                if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;

                // 3D Rice Seedlings in neat agricultural rows
                for (int z = -11; z <= 11; z += 3)
                {
                    for (int off = -2; off <= 2; off += 2)
                    {
                        if (reedAsset != null)
                        {
                            var plant = (GameObject)PrefabUtility.InstantiatePrefab(reedAsset, terrace.transform);
                            plant.name = $"KhomLua_{z}_{off}";
                            plant.transform.localPosition = new Vector3(off * 1.3f, 0.24f, z);
                            plant.transform.localScale = Vector3.one * 1.65f;
                            if (riceMat != null) SetMatRecursive(plant, riceMat);
                        }
                    }
                }
            }
        }

        public static void BuildUndergrowthAndTrees(GameObject env)
        {
            // 1. Peach Blossom Trees (Cây Đào Hồng Cổ Thụ & Cánh hoa đào rơi bay)
            PhuDongAsianCultureBuilder.BuildPeachBlossomGroves(env);

            // 2. Dense 3D Ground Cover & White Pampas Reeds (Thảm cỏ lau trắng như ảnh 4)
            PhuDongAsianCultureBuilder.BuildDenseVegetationAndPampas(env);

            // 3. Ancient Banyan & Forest Trees
            var root = CreateChild(env, "ThamThucVat_CayCoThu");
            var bushPrefab = LoadPrefab("Foliage/FoliageCluster_Undergrowth_Wild.prefab");
            var treeA = LoadPrefab("Foliage/TreeCluster_LayeredCanopy_Grand.prefab");
            var treeB = LoadPrefab("Foliage/TreeCluster_Broadleaf_Oak.prefab");

            Vector3[] bushes = {
                new Vector3(-9f, 0f, 3f), new Vector3(-13f, 0f, -5f), new Vector3(-7f, 0f, 11f),
                new Vector3(7f, 0f, 3f), new Vector3(11f, 0f, -7f), new Vector3(15f, 0f, 19f),
                new Vector3(-5f, 0f, 21f), new Vector3(9f, 0f, 25f), new Vector3(-15f, 0f, 17f),
                new Vector3(5f, 0f, -19f), new Vector3(-9f, 0f, -13f), new Vector3(17f, 0f, -23f),
                new Vector3(-25f, 0f, -6f), new Vector3(25f, 0f, -8f)
            };
            if (bushPrefab != null)
            {
                for (int i = 0; i < bushes.Length; i++)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(bushPrefab, root.transform);
                    b.name = $"BuiCo_HoangDa_{i + 1}";
                    b.transform.position = bushes[i];
                    b.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    b.transform.localScale = Vector3.one * Random.Range(1.3f, 1.9f);
                }
            }

            Vector3[] trees = {
                new Vector3(-28f, 0f, 15f), new Vector3(-26f, 0f, 27f),
                new Vector3(24f, 0f, 25f), new Vector3(28f, 0f, 13f),
                new Vector3(30f, 0f, -5f), new Vector3(-18f, 0f, -25f)
            };
            for (int i = 0; i < trees.Length; i++)
            {
                var tAsset = (i % 2 == 0 && treeA != null) ? treeA : treeB;
                if (tAsset == null) continue;
                var tr = (GameObject)PrefabUtility.InstantiatePrefab(tAsset, root.transform);
                tr.name = $"CayDai_CanhLa_{i + 1}";
                tr.transform.position = trees[i];
                tr.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                tr.transform.localScale = Vector3.one * Random.Range(1.4f, 2f);
            }
        }

        private static GameObject LoadPrefab(string relPath) => AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + relPath);
        private static Material LoadMat(string relPath) => AssetDatabase.LoadAssetAtPath<Material>(MatBase + relPath);

        private static void SetMatRecursive(GameObject go, Material mat)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }
    }
}
