using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds environment terrain, meandering river, soaring karst mountains,
    /// 3D rice paddies, and wuxia foliage.
    /// </summary>
    public static class PhuDongVillageEnvironmentBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string MatBase = "Assets/_Game/Art/Materials/";
        private const string OpenWorldBase = "Assets/_Game/Art/OpenWorld/";
        private const string AsianBase = "Assets/_Game/Art/AsianCulture/";
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
            // Clean, unobstructed skybox without giant blocky polygon clouds blocking the village
        }

        public static void BuildRiverAndBridges(GameObject env)
        {
            // Fully procedural organic meandering river mesh (NO flat rectangular boxes!)
            PhuDongAsianCultureBuilder.BuildOrganicMeanderingRiver(env);
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
                float elev = t * 0.35f;
                float x = t * 7.5f;
                var terrace = CreateChild(root, $"ThuaRuong_Bac_{t + 1}");
                terrace.transform.localPosition = new Vector3(x, elev, 0f);

                // Earthen dike
                var berm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                berm.name = "BoRuong_PhuSa";
                berm.transform.SetParent(terrace.transform, false);
                berm.transform.localPosition = Vector3.zero;
                berm.transform.localScale = new Vector3(7.2f, 0.36f, 26f);
                if (soilMat != null) berm.GetComponent<Renderer>().sharedMaterial = soilMat;

                // Flooded water mirror
                var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = "MatNuoc_PhanChieu";
                water.transform.SetParent(terrace.transform, false);
                water.transform.localPosition = new Vector3(0f, 0.18f, 0f);
                water.transform.localScale = new Vector3(6.6f, 0.05f, 25f);
                var wCol = water.GetComponent<Collider>();
                if (wCol != null) Object.DestroyImmediate(wCol);
                if (waterMat != null) water.GetComponent<Renderer>().sharedMaterial = waterMat;

                // 3D Rice Seedlings in neat agricultural rows (Natural human scale: 0.65m - 0.75m)
                for (int z = -10; z <= 10; z += 3)
                {
                    for (int off = -2; off <= 2; off += 2)
                    {
                        if (reedAsset != null)
                        {
                            var plant = (GameObject)PrefabUtility.InstantiatePrefab(reedAsset, terrace.transform);
                            plant.name = $"KhomLua_{z}_{off}";
                            plant.transform.localPosition = new Vector3(off * 1.3f, 0.20f, z);
                            plant.transform.localScale = Vector3.one * 1.15f;
                            if (riceMat != null) SetMatRecursive(plant, riceMat);
                        }
                    }
                }
            }
        }

        public static void BuildUndergrowthAndTrees(GameObject env)
        {
            // 1. Blooming Peach Blossom Groves along riverbanks
            PhuDongAsianCultureBuilder.BuildPeachBlossomGroves(env);

            // 2. Dense 3D Volumetric Grass & Tall White Pampas Reeds
            PhuDongAsianCultureBuilder.BuildDenseVegetationAndPampas(env);

            // 3. Ancient Trees & Foliage Bushes
            var root = CreateChild(env, "ThamThucVat_CayCoThu");
            var treeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Tree02.glb")
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_Broadleaf_Oak.prefab");
            var bushAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Bush01.glb")
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/FoliageCluster_Undergrowth_Wild.prefab");

            Vector3[] bushes = {
                new Vector3(-9f, 0f, 3f), new Vector3(-13f, 0f, -5f), new Vector3(-7f, 0f, 11f),
                new Vector3(7f, 0f, 3f), new Vector3(11f, 0f, -7f), new Vector3(15f, 0f, 19f),
                new Vector3(-5f, 0f, 21f), new Vector3(9f, 0f, 25f), new Vector3(-15f, 0f, 17f),
                new Vector3(5f, 0f, -19f), new Vector3(-9f, 0f, -13f), new Vector3(17f, 0f, -23f)
            };
            if (bushAsset != null)
            {
                for (int i = 0; i < bushes.Length; i++)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(bushAsset, root.transform);
                    b.name = $"BuiCo_HoangDa_{i + 1}";
                    b.transform.position = bushes[i];
                    b.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    b.transform.localScale = Vector3.one * Random.Range(1.3f, 1.8f);
                }
            }

            Vector3[] trees = {
                new Vector3(-28f, 0f, 15f), new Vector3(-26f, 0f, 27f),
                new Vector3(24f, 0f, 25f), new Vector3(28f, 0f, 13f),
                new Vector3(30f, 0f, -5f), new Vector3(-18f, 0f, -25f)
            };
            if (treeAsset != null)
            {
                for (int i = 0; i < trees.Length; i++)
                {
                    var tr = (GameObject)PrefabUtility.InstantiatePrefab(treeAsset, root.transform);
                    tr.name = $"CayDai_CanhLa_{i + 1}";
                    tr.transform.position = trees[i];
                    tr.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    tr.transform.localScale = Vector3.one * Random.Range(1.3f, 1.8f);
                }
            }
        }

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
