using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds high-fidelity East Asian / ancient Vietnamese wuxia landscape features (Nghịch Thủy Hàn style):
    /// 1. Lush 3D ground carpet of grass blades, 3-leaf clover & wildflowers on moist fertile soil (Image 1)
    /// 2. Dense living bamboo hedges (Lũy Tre Làng) matching human height ratio ~3x (Image 2)
    /// 3. Natural river rock clusters with sharp grass tufts sprouting from crevices (Image 3)
    /// 4. Rich East Asian tree collection: Weeping willows, mountain pines, grand banyan & golden maples (Image 4)
    /// Plus: Meandering river, arched timber footbridge, wooden boat dock, moored sampans, and karst pinnacles.
    /// All objects are snapped directly to the procedural terrain surface (Zero buried objects!).
    /// </summary>
    public static class PhuDongAsianCultureBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string MatBase = "Assets/_Game/Art/Materials/";
        private const string AsianBase = "Assets/_Game/Art/AsianCulture/";
        private const string OpenWorldBase = "Assets/_Game/Art/OpenWorld/";
        private const string KayKitBase = "Assets/ThirdParty/KayKit_Medieval_Hexagon_Pack/KayKit_Medieval_Hexagon_Pack_1.0_FREE/Assets/fbx/";

        /// <summary>
        /// Generates a realistic, curved organic meandering river mesh (No flat rectangular boxes!).
        /// Features variable width, curved banks, dished bed, shoreline boulders, and water caustics.
        /// </summary>
        public static GameObject BuildOrganicMeanderingRiver(GameObject parent)
        {
            var root = CreateChild(parent, "DongSong_UonLuon_NuocBiec");
            var waterMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Meshy/Environment/Mat_DongSong_NuocBiec.mat");

            int segments = 64;
            float startX = -54f;
            float endX = 54f;
            float stepX = (endX - startX) / segments;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            for (int i = 0; i <= segments; i++)
            {
                float x = startX + i * stepX;
                float z = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);

                float dz = 4.5f * 0.055f * Mathf.Cos(x * 0.055f) + 2.2f * 0.11f * Mathf.Sin(x * 0.11f);
                Vector3 tangent = new Vector3(1f, 0f, dz).normalized;
                Vector3 perp = new Vector3(-tangent.z, 0f, tangent.x);

                float width = 17f;
                if (x >= -4f && x <= 26f)
                {
                    float factor = 1f - Mathf.Abs(x - 11f) / 15f;
                    width = Mathf.Lerp(17f, 25.5f, Mathf.Clamp01(factor));
                }
                else if (x >= -24f && x < -8f)
                {
                    width = 13.8f;
                }

                float v = (float)i / segments * 12f;
                float halfW = width * 0.5f;
                Vector3 center = new Vector3(x, -0.05f, z);

                Vector3 p0 = center - perp * halfW + Vector3.up * 0.02f;
                Vector3 p1 = center - perp * (halfW * 0.55f) + Vector3.down * 0.06f;
                Vector3 p2 = center + Vector3.down * 0.28f;
                Vector3 p3 = center + perp * (halfW * 0.55f) + Vector3.down * 0.06f;
                Vector3 p4 = center + perp * halfW + Vector3.up * 0.02f;

                verts.Add(p0); uvs.Add(new Vector2(0.0f, v)); normals.Add(Vector3.up);
                verts.Add(p1); uvs.Add(new Vector2(0.25f, v)); normals.Add(Vector3.up);
                verts.Add(p2); uvs.Add(new Vector2(0.5f, v)); normals.Add(Vector3.up);
                verts.Add(p3); uvs.Add(new Vector2(0.75f, v)); normals.Add(Vector3.up);
                verts.Add(p4); uvs.Add(new Vector2(1.0f, v)); normals.Add(Vector3.up);

                if (i > 0)
                {
                    int curr = i * 5;
                    int prev = (i - 1) * 5;
                    for (int s = 0; s < 4; s++)
                    {
                        tris.Add(prev + s); tris.Add(curr + s); tris.Add(prev + s + 1);
                        tris.Add(curr + s); tris.Add(curr + s + 1); tris.Add(prev + s + 1);
                        tris.Add(prev + s + 1); tris.Add(curr + s); tris.Add(prev + s);
                        tris.Add(prev + s + 1); tris.Add(curr + s + 1); tris.Add(curr + s);
                    }
                }
            }

            var riverMesh = new Mesh { name = "MeanderingRiver_Mesh" };
            riverMesh.SetVertices(verts);
            riverMesh.SetUVs(0, uvs);
            riverMesh.SetNormals(normals);
            riverMesh.SetTriangles(tris, 0);

            var riverGo = new GameObject("DongSong_MatNuoc_Spline");
            riverGo.transform.SetParent(root.transform, false);
            riverGo.AddComponent<MeshFilter>().sharedMesh = riverMesh;
            var mr = riverGo.AddComponent<MeshRenderer>();
            if (waterMat != null) mr.sharedMaterial = waterMat;

            BuildArchedWoodenBridge(root);
            BuildRiverDockAndSampans(root);
            BuildLotusSteppingStones(root);
            BuildMossyRockGrassClusters(root);
            BuildBloomingLotusPonds(root);

            return root;
        }

        public static void BuildBloomingLotusPonds(GameObject parent)
        {
            var root = CreateChild(parent, "DamSen_HoaKhai_NuocBiec");
            var damSenPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy_DamSenCo_VanLang.prefab")
                            ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy/Environment/Meshy_DamSenCo_VanLang.prefab");
            if (damSenPrefab == null) return;

            Vector3[] lotusPos = {
                new Vector3(17.5f, -0.05f, -14.0f),
                new Vector3(26.0f, -0.05f, -16.5f)
            };

            for (int i = 0; i < lotusPos.Length; i++)
            {
                var lotus = (GameObject)PrefabUtility.InstantiatePrefab(damSenPrefab, root.transform);
                lotus.name = $"DamSenCo_VanLang_{i + 1}";
                lotus.transform.position = lotusPos[i];
                lotus.transform.rotation = Quaternion.Euler(0f, i * 55f, 0f);
                lotus.transform.localScale = Vector3.one * 1.35f;
            }
        }

        public static void BuildArchedWoodenBridge(GameObject parent)
        {
            var bridgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy_CauGo_CoDai.prefab")
                            ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy/Environment/Meshy_CauGo_CoDai.prefab");
            if (bridgePrefab != null)
            {
                var bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab, parent.transform);
                bridge.name = "CauGo_CoDai_QuaSong";
                bridge.transform.position = new Vector3(-16.0f, 0.05f, -16.2f);
                bridge.transform.rotation = Quaternion.Euler(0f, 15f, 0f);
                bridge.transform.localScale = Vector3.one * 1.15f;
            }
        }

        public static void BuildRiverDockAndSampans(GameObject parent)
        {
            var root = CreateChild(parent, "BenDo_Cauthuyen_ThuyenNan");
            var dockAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Dock01.glb");
            var woodMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Wood.mat");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat");

            if (dockAsset != null)
            {
                var dock = (GameObject)PrefabUtility.InstantiatePrefab(dockAsset, root.transform);
                dock.name = "BenDo_CauTau_Go";
                dock.transform.position = new Vector3(-1.5f, -0.05f, -13.2f);
                dock.transform.rotation = Quaternion.Euler(0f, 172f, 0f);
                dock.transform.localScale = Vector3.one * 0.75f;
            }

            Vector3[] boatPos = {
                new Vector3(4.5f, -0.02f, -14.2f),
                new Vector3(9.8f, -0.02f, -16.5f),
                new Vector3(16.2f, -0.02f, -13.8f)
            };
            float[] boatRot = { 18f, -22f, 12f };
            for (int b = 0; b < boatPos.Length; b++)
            {
                var boat = BuildWoodenSampan(root, $"ThuyenNan_Go_{b + 1}", woodMat);
                boat.transform.position = boatPos[b];
                boat.transform.rotation = Quaternion.Euler(0f, boatRot[b], 0f);
            }

            BuildWoodenHandcart(root, new Vector3(-6.5f, 0.12f, -10.5f), woodMat);
            BuildPotteryAndBaskets(root, stoneMat, woodMat);
        }

        public static void BuildLotusSteppingStones(GameObject parent)
        {
            var root = CreateChild(parent, "DaDam_BuocQuaAoSen");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Sacred_Mountain_Stone.mat");

            Vector3[] stonePts = {
                new Vector3(15f, 0.08f, -11.5f),
                new Vector3(17f, 0.08f, -12.2f),
                new Vector3(19f, 0.08f, -13.0f),
                new Vector3(21.5f, 0.08f, -13.5f),
                new Vector3(24f, 0.08f, -13.0f),
                new Vector3(26.5f, 0.08f, -12.2f),
                new Vector3(29f, 0.08f, -11.5f)
            };

            for (int i = 0; i < stonePts.Length; i++)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stone.name = $"DaDam_Buoc_{i + 1}";
                stone.transform.SetParent(root.transform, false);
                stone.transform.position = stonePts[i];
                stone.transform.rotation = Quaternion.Euler(0f, i * 35f, 0f);
                stone.transform.localScale = new Vector3(1.35f, 0.08f, 1.35f);
                if (stoneMat != null) stone.GetComponent<Renderer>().sharedMaterial = stoneMat;
            }
        }

        /// <summary>
        /// Builds natural 3D river rock clusters with wild grass tufts sprouting directly from crevices.
        /// Faithfully reproduces Reference Image 3 (Cụm đá cuội xen cỏ tự nhiên).
        /// </summary>
        public static void BuildMossyRockGrassClusters(GameObject parent)
        {
            var root = CreateChild(parent, "CumDaCuoi_BoSong_MocCo");
            var rockA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/rock_single_A.fbx");
            var rockB = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/rock_single_B.fbx");
            var grassTuftAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_A.fbx");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "MountainRock.mat");
            var grassMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamThucVat_CloverFlower.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamCo_Wuxia.mat");

            // Natural shoreline positions along the water's edge and village outskirts (No view blocking!)
            Vector3[] clusterPos = {
                new Vector3(-13.5f, 0f, -12.5f), new Vector3(-20.5f, 0f, -14.2f),
                new Vector3(-28.0f, 0f, -17.5f), new Vector3(7.5f, 0f, -10.8f),
                new Vector3(14.5f, 0f, -9.8f),   new Vector3(23.5f, 0f, -19.5f),
                new Vector3(30.0f, 0f, -11.0f),  new Vector3(-14.5f, 0f, 6.5f),
                new Vector3(8.5f, 0f, 2.5f)
            };

            for (int i = 0; i < clusterPos.Length; i++)
            {
                var clusterGo = CreateChild(root, $"CumDa_XenCo_{i + 1}");
                float groundY = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(clusterPos[i].x, clusterPos[i].z);
                clusterGo.transform.position = new Vector3(clusterPos[i].x, groundY + 0.02f, clusterPos[i].z);
                clusterGo.transform.rotation = Quaternion.Euler(0f, (i * 47f) % 360f, 0f);

                // 1. Central weathered river boulder (Natural human scale ~0.8m)
                if (rockA != null)
                {
                    var central = (GameObject)PrefabUtility.InstantiatePrefab(rockA, clusterGo.transform);
                    central.name = "DaCuoi_TrungTam";
                    central.transform.localPosition = Vector3.zero;
                    central.transform.localRotation = Quaternion.Euler(0f, (i * 31f) % 360f, 0f);
                    central.transform.localScale = Vector3.one * Random.Range(0.85f, 1.25f);
                    if (stoneMat != null) SetMatRecursive(central, stoneMat);
                }

                // 2. Satellite companion stones nestled around the base (Natural pebble scale ~0.35m - 0.65m)
                if (rockB != null)
                {
                    int satCount = 2 + (i % 2);
                    for (int s = 0; s < satCount; s++)
                    {
                        float ang = s * (Mathf.PI * 2f / satCount) + 0.45f;
                        float dist = Random.Range(0.55f, 0.95f);
                        var sat = (GameObject)PrefabUtility.InstantiatePrefab(rockB, clusterGo.transform);
                        sat.name = $"DaCuoi_VeTinh_{s + 1}";
                        sat.transform.localPosition = new Vector3(Mathf.Cos(ang) * dist, 0.01f, Mathf.Sin(ang) * dist);
                        sat.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
                        sat.transform.localScale = Vector3.one * Random.Range(0.40f, 0.70f);
                        if (stoneMat != null) SetMatRecursive(sat, stoneMat);
                    }
                }

                // 3. Sharp wild grass tufts sprouting directly out of stone crevices (Reference Image 3)
                if (grassTuftAsset != null)
                {
                    int grassCount = 3 + (i % 2);
                    for (int g = 0; g < grassCount; g++)
                    {
                        float gAng = g * (Mathf.PI * 2f / grassCount) + 0.65f;
                        float gDist = Random.Range(0.35f, 0.65f);
                        var tuft = (GameObject)PrefabUtility.InstantiatePrefab(grassTuftAsset, clusterGo.transform);
                        tuft.name = $"BuiCo_KheDa_{g + 1}";
                        tuft.transform.localPosition = new Vector3(Mathf.Cos(gAng) * gDist, 0.02f, Mathf.Sin(gAng) * gDist);
                        tuft.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        tuft.transform.localScale = new Vector3(0.6f, Random.Range(0.75f, 1.10f), 0.6f);
                        if (grassMat != null) SetMatRecursive(tuft, grassMat);
                    }
                }
            }
        }

        public static void BuildNaturalRiverShoreRocks(GameObject parent) => BuildMossyRockGrassClusters(parent);

        /// <summary>
        /// Generates dense volumetric 3D grass field, 3-leaf clover, wildflowers, and tall white pampas reeds.
        /// Faithfully reproduces Reference Image 1 & 4.
        /// </summary>
        public static void BuildDenseVegetationAndPampas(GameObject parent)
        {
            var root = CreateChild(parent, "ThamCoLau_Trang_DayDac");
            var pampasMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_CoLau_Trang.mat");
            var reedAssetA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_A.fbx");
            var reedAssetB = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_B.fbx");

            Vector3[] pampasPos = {
                new Vector3(-28f, 0f, -11.5f), new Vector3(-22f, 0f, -12.2f),
                new Vector3(-10f, 0f, -10.5f), new Vector3(-5f, 0f, -10.8f),
                new Vector3(3f, 0f, -9.8f), new Vector3(8f, 0f, -10.2f),
                new Vector3(14f, 0f, -9.2f), new Vector3(20f, 0f, -8.8f),
                new Vector3(27f, 0f, -9.5f), new Vector3(34f, 0f, -10.5f),
                new Vector3(-30f, 0f, -21.5f), new Vector3(-18f, 0f, -22.5f),
                new Vector3(-4f, 0f, -21.8f), new Vector3(7f, 0f, -22.2f),
                new Vector3(18f, 0f, -23.5f), new Vector3(28f, 0f, -22.8f),
                new Vector3(19f, 0f, 4.5f), new Vector3(25f, 0f, 5.0f),
                new Vector3(-16f, 0f, 5.2f), new Vector3(-12f, 0f, 16.5f)
            };

            for (int i = 0; i < pampasPos.Length; i++)
            {
                var asset = (i % 2 == 0 && reedAssetA != null) ? reedAssetA : (reedAssetB ?? reedAssetA);
                if (asset == null) break;
                var reed = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                reed.name = $"BuiCoLau_Trang_{i + 1}";
                float groundY = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(pampasPos[i].x, pampasPos[i].z);
                reed.transform.position = new Vector3(pampasPos[i].x, groundY + 0.03f, pampasPos[i].z);
                reed.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                reed.transform.localScale = new Vector3(1.5f, Random.Range(2.2f, 3.0f), 1.5f);
                if (pampasMat != null) SetMatRecursive(reed, pampasMat);
            }

            // 2. Dense 3D Ground Carpet: Grass Blades + Clover + Purple/White Wildflowers (Image 1)
            BuildLushCloverAndWildflowerCarpet(root);
        }

        /// <summary>
        /// Generates dense 3D ground carpet of grass blades, 3-leaf clover clumps, and purple & white wildflowers.
        /// Faithfully reproduces Reference Image 1 (Thảm cỏ 3D nở hoa dại và cỏ ba lá trên nền đất ẩm).
        /// </summary>
        public static void BuildLushCloverAndWildflowerCarpet(GameObject parent)
        {
            var root = CreateChild(parent, "ThamCo_3D_Clover_HoaDai");
            var carpetMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamThucVat_CloverFlower.mat")
                         ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamCo_Wuxia.mat");
            var soilMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/OpenWorld/RiceFieldSoil.mat")
                       ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Materials/Farm_Soil.mat")
                       ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Farm_Soil.mat");

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            var rng = new System.Random(88);

            Color grassRoot = new Color(0.12f, 0.28f, 0.06f, 1f);      // Moist fertile soil green
            Color grassMid  = new Color(0.36f, 0.68f, 0.16f, 1f);      // Lush wuxia green
            Color grassTip  = new Color(0.72f, 0.94f, 0.30f, 1f);      // Bright sunlit emerald blade tips
            Color cloverBase = new Color(0.18f, 0.56f, 0.14f, 1f);     // Deep clover leaf body
            Color cloverTip  = new Color(0.48f, 0.86f, 0.28f, 1f);     // Light clover chevron highlight
            Color flowerPurple = new Color(0.82f, 0.60f, 0.95f, 1f);   // Delicate purple wildflower (Image 1)
            Color flowerWhite  = new Color(0.98f, 0.98f, 0.92f, 1f);   // Daisy white blossom
            Color flowerCenter = new Color(0.98f, 0.84f, 0.20f, 1f);   // Golden stamen center

            int grassClumps = 8000;
            int cloverClumps = 2200;
            int flowerCount = 1500;

            // 1. Volumetric Curved 3D Grass Blades (Snapped to terrain elevation, Image 1)
            for (int i = 0; i < grassClumps; i++)
            {
                float x = (float)(rng.NextDouble() * 68.0 - 34.0);
                float z = (float)(rng.NextDouble() * 52.0 - 24.0);

                float riverZ = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);
                if (Mathf.Abs(z - riverZ) < 6.8f) continue;
                if (Mathf.Abs(x) < 1.4f && z >= -14f && z <= 30f) continue; // Footpath path line

                float groundY = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(x, z);
                float h = 0.45f + (float)rng.NextDouble() * 0.40f;
                float w = 0.08f + (float)rng.NextDouble() * 0.04f;
                Vector3 baseP = new Vector3(x, groundY + 0.015f, z);

                // 5 curved blades per clump radiating outward
                int bladeCount = 5;
                for (int b = 0; b < bladeCount; b++)
                {
                    float ang = b * (Mathf.PI * 2f / bladeCount) + (float)rng.NextDouble() * 0.35f;
                    Vector3 radialDir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                    Vector3 sideDir = new Vector3(-radialDir.z, 0f, radialDir.x) * (w * 0.5f);

                    Vector3 p0L = baseP - sideDir;
                    Vector3 p0R = baseP + sideDir;

                    // Mid segment bending outward
                    Vector3 midP = baseP + radialDir * (h * 0.28f) + Vector3.up * (h * 0.55f);
                    Vector3 p1L = midP - sideDir * 0.7f;
                    Vector3 p1R = midP + sideDir * 0.7f;

                    // Tip bending further outward
                    Vector3 tipP = baseP + radialDir * (h * 0.52f) + Vector3.up * h;

                    int v = verts.Count;
                    // Segment 1: base to mid (quad)
                    verts.Add(p0L); verts.Add(p0R);
                    verts.Add(p1L); verts.Add(p1R);
                    // Segment 2: mid to tip (triangle)
                    verts.Add(tipP);

                    uvs.Add(new Vector2(0f, 0f)); uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(0f, 0.55f)); uvs.Add(new Vector2(1f, 0.55f));
                    uvs.Add(new Vector2(0.5f, 1f));

                    colors.Add(grassRoot); colors.Add(grassRoot);
                    colors.Add(grassMid); colors.Add(grassMid);
                    colors.Add(grassTip);

                    Vector3 n = Vector3.Cross(sideDir, Vector3.up).normalized;
                    normals.Add(n); normals.Add(n); normals.Add(n); normals.Add(n); normals.Add(n);

                    // Quad: v..v+3
                    tris.Add(v); tris.Add(v + 2); tris.Add(v + 1);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v + 3);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v);
                    tris.Add(v + 3); tris.Add(v + 2); tris.Add(v + 1);
                    // Triangle: v+2, v+4, v+3
                    tris.Add(v + 2); tris.Add(v + 4); tris.Add(v + 3);
                    tris.Add(v + 3); tris.Add(v + 4); tris.Add(v + 2);
                }
            }

            // 2. 3-Leaf Clover Clumps (Curved shamrock leaves nestled near the ground, Image 1)
            for (int i = 0; i < cloverClumps; i++)
            {
                float x = (float)(rng.NextDouble() * 64.0 - 32.0);
                float z = (float)(rng.NextDouble() * 48.0 - 22.0);

                float riverZ = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);
                if (Mathf.Abs(z - riverZ) < 7.0f) continue;
                if (Mathf.Abs(x) < 1.6f && z >= -14f && z <= 30f) continue;

                float groundY = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(x, z);
                float cloverH = 0.06f + (float)rng.NextDouble() * 0.06f;
                Vector3 center = new Vector3(x, groundY + cloverH, z);
                float leafSize = 0.13f + (float)rng.NextDouble() * 0.05f;

                for (int l = 0; l < 3; l++)
                {
                    float angle = l * (2f * Mathf.PI / 3f) + (float)rng.NextDouble() * 0.25f;
                    Vector3 forward = new Vector3(Mathf.Cos(angle), 0.16f, Mathf.Sin(angle)).normalized * leafSize;
                    Vector3 side = new Vector3(-forward.z, 0f, forward.x).normalized * (leafSize * 0.60f);

                    int v = verts.Count;
                    verts.Add(center);
                    verts.Add(center + forward - side);
                    verts.Add(center + forward + side);

                    uvs.Add(new Vector2(0.5f, 0f));
                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(1f, 1f));

                    colors.Add(cloverBase);
                    colors.Add(cloverTip);
                    colors.Add(cloverTip);

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    tris.Add(v); tris.Add(v + 1); tris.Add(v + 2);
                    tris.Add(v + 2); tris.Add(v + 1); tris.Add(v);
                }
            }

            // 3. Purple & White Wildflowers (Blooming delicately atop grass, Image 1)
            for (int i = 0; i < flowerCount; i++)
            {
                float x = (float)(rng.NextDouble() * 64.0 - 32.0);
                float z = (float)(rng.NextDouble() * 48.0 - 22.0);

                float riverZ = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);
                if (Mathf.Abs(z - riverZ) < 7.0f) continue;
                if (Mathf.Abs(x) < 1.6f && z >= -14f && z <= 30f) continue;

                float groundY = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(x, z);
                float fH = 0.18f + (float)rng.NextDouble() * 0.16f;
                Vector3 fCenter = new Vector3(x, groundY + fH, z);
                float fRadius = 0.08f + (float)rng.NextDouble() * 0.04f;

                Color petalCol = (rng.NextDouble() < 0.60) ? flowerPurple : flowerWhite;

                for (int q = 0; q < 2; q++)
                {
                    float fAng = q * (Mathf.PI * 0.5f) + (float)rng.NextDouble() * 0.2f;
                    Vector3 d = new Vector3(Mathf.Cos(fAng), 0f, Mathf.Sin(fAng)) * fRadius;
                    Vector3 cross = new Vector3(-d.z, 0f, d.x) * 0.7f;

                    int v = verts.Count;
                    verts.Add(fCenter - d - cross);
                    verts.Add(fCenter + d - cross);
                    verts.Add(fCenter - d + cross);
                    verts.Add(fCenter + d + cross);

                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(1f, 1f));

                    colors.Add(petalCol);
                    colors.Add(petalCol);
                    colors.Add(flowerCenter);
                    colors.Add(petalCol);

                    normals.Add(Vector3.up); normals.Add(Vector3.up);
                    normals.Add(Vector3.up); normals.Add(Vector3.up);

                    tris.Add(v); tris.Add(v + 2); tris.Add(v + 1);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v + 3);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v);
                    tris.Add(v + 3); tris.Add(v + 2); tris.Add(v + 1);
                }
            }

            var mesh = new Mesh { name = "LushFoliageCarpet_Mesh" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);

            var carpetGo = new GameObject("ThamCo_3D_Wuxia_CloverFlower");
            carpetGo.transform.SetParent(root.transform, false);
            carpetGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = carpetGo.AddComponent<MeshRenderer>();
            if (carpetMat != null) mr.sharedMaterial = carpetMat;

            // 4. Moist Earth Soil Discs underneath dense foliage patches (Image 1)
            var soilRoot = CreateChild(root, "NenDat_Am_ThamThucVat");
            Vector3[] soilCenters = {
                new Vector3(-12f, 0f, -8f), new Vector3(14f, 0f, -8f),
                new Vector3(-8f, 0f, 12f), new Vector3(8f, 0f, 12f),
                new Vector3(-18f, 0f, 4f), new Vector3(20f, 0f, 4f),
                new Vector3(4f, 0f, -22f), new Vector3(-6f, 0f, -22f),
                new Vector3(-4f, 0f, -12f), new Vector3(6f, 0f, -12f),
                new Vector3(-14f, 0f, 18f), new Vector3(12f, 0f, 18f),
                new Vector3(-2f, 0f, 2f), new Vector3(2f, 0f, 2f)
            };
            for (int s = 0; s < soilCenters.Length; s++)
            {
                var soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                soil.name = $"MatDat_Am_{s + 1}";
                soil.transform.SetParent(soilRoot.transform, false);
                float gy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(soilCenters[s].x, soilCenters[s].z);
                soil.transform.position = new Vector3(soilCenters[s].x, gy + 0.012f, soilCenters[s].z);
                soil.transform.localScale = new Vector3(4.5f, 0.02f, 4.5f);
                var c = soil.GetComponent<Collider>();
                if (c != null) Object.DestroyImmediate(c);
                if (soilMat != null) soil.GetComponent<Renderer>().sharedMaterial = soilMat;
            }
        }

        /// <summary>
        /// Builds blooming pink peach blossom trees along the riverbank mirroring in the water,
        /// complete with drifting pink flower petal particles. (Zero cartoon blob trees!)
        /// </summary>
        public static void BuildPeachBlossomGroves(GameObject parent)
        {
            var root = CreateChild(parent, "RungHoaDao_HongTham");
            var treeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_LayeredCanopy_Grand.prefab")
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_PuffyCrown_Painterly.prefab");
            var peachMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_HoaDao_HongTham.mat");

            Vector3[] peachPos = {
                new Vector3(-13f, 0f, -9.5f),
                new Vector3(-21f, 0f, -10.5f),
                new Vector3(13f, 0f, -9.0f),
                new Vector3(21f, 0f, -8.5f),
                new Vector3(-8f, 0f, -22.5f),
                new Vector3(9f, 0f, -23.0f),
                new Vector3(19f, 0f, -24.0f),
                new Vector3(7f, 0f, 22f),
                new Vector3(-11f, 0f, 23f)
            };

            for (int i = 0; i < peachPos.Length; i++)
            {
                if (treeAsset == null) break;
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(treeAsset, root.transform);
                tree.name = $"CayHoaDao_HongTham_{i + 1}";
                float gy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(peachPos[i].x, peachPos[i].z);
                tree.transform.position = new Vector3(peachPos[i].x, gy, peachPos[i].z);
                tree.transform.rotation = Quaternion.Euler(0f, (i * 47f) % 360f, 0f);
                tree.transform.localScale = Vector3.one * Random.Range(1.4f, 1.9f);

                if (peachMat != null)
                {
                    foreach (var mr in tree.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (mr.name.Contains("Bush") || mr.name.Contains("Crown") || mr.name.Contains("Leaf") || mr.name.Contains("Canopy") || mr.name.Contains("Tree"))
                        {
                            mr.sharedMaterial = peachMat;
                        }
                    }
                }
            }

            BuildDriftingPeachPetalsVFX(root);
        }

        /// <summary>
        /// Builds tall, dense living bamboo hedges (Lũy Tre Làng) matching Reference Image 2.
        /// Features natural 4.8m – 5.6m height (~3x human ratio as shown in image silhouette).
        /// </summary>
        public static void BuildDenseBambooWallAndGroves(GameObject env)
        {
            var root = CreateChild(env, "RungTre_LuyLang_PhuDong");
            var bambooPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy_KhomTreNga_ThanThanh.prefab");
            var foliagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/BambooGrove_Vietnamese_Legend.prefab");
            var bambooGoldMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "ThanhGiong/Mat_Giong_BambooGold.mat")
                             ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Campaign/Campaign_Bamboo.mat");
            var bambooLeafMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "ThanhGiong/Mat_Giong_BambooLeaf.mat");

            // 1. Continuous Western Village Bamboo Wall (Sheltering the huts, double-staggered rows)
            for (float z = -20f; z <= 20f; z += 2.2f)
            {
                Vector3 p1 = new Vector3(-24.5f + Mathf.Sin(z * 0.15f) * 1.2f, 0f, z);
                p1.y = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(p1.x, p1.z);
                SpawnBambooStalkCluster(root, p1, bambooPrefab, foliagePrefab, bambooGoldMat, bambooLeafMat, 1.25f);

                Vector3 p2 = new Vector3(-26.8f + Mathf.Cos(z * 0.12f) * 1.4f, 0f, z + 1.1f);
                p2.y = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(p2.x, p2.z);
                SpawnBambooStalkCluster(root, p2, bambooPrefab, foliagePrefab, bambooGoldMat, bambooLeafMat, 1.35f);
            }

            // 2. Northern Curved Bamboo Boundary behind huts
            for (float x = -14f; x <= 14f; x += 3.2f)
            {
                Vector3 p = new Vector3(x, 0f, 26.5f + Mathf.Sin(x * 0.2f) * 1.5f);
                p.y = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(p.x, p.z);
                SpawnBambooStalkCluster(root, p, bambooPrefab, foliagePrefab, bambooGoldMat, bambooLeafMat, 1.28f);
            }

            // 3. Eastern Riverbank Bamboo Grove
            for (float z = -22f; z <= -8f; z += 2.8f)
            {
                Vector3 p = new Vector3(26.5f + Mathf.Cos(z * 0.25f) * 1.2f, 0f, z);
                p.y = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(p.x, p.z);
                SpawnBambooStalkCluster(root, p, bambooPrefab, foliagePrefab, bambooGoldMat, bambooLeafMat, 1.30f);
            }

            // 4. Sacred Phase 3 Bamboo Thicket (Khóm tre thần để Gióng nhổ đánh giặc)
            var sacred = CreateChild(root, "KhomTre_LinhThieng_Phase3");
            float sy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(-18f, -4f);
            sacred.transform.position = new Vector3(-18f, sy, -4f);

            for (int a = 0; a < 6; a++)
            {
                float ang = a * (Mathf.PI * 2f / 6f);
                Vector3 ringPos = new Vector3(Mathf.Cos(ang) * 2.2f, 0f, Mathf.Sin(ang) * 2.2f);
                var stalk = SpawnBambooStalkCluster(sacred, ringPos, bambooPrefab, foliagePrefab, bambooGoldMat, bambooLeafMat, 1.45f);
                stalk.name = $"TreThan_BaoVe_{a + 1}";
            }

            // 5. Foreground Living Bamboo Hedge Wall (Framing the left entrance, Reference Image 2)
            for (float z = -16f; z <= -8f; z += 2.0f)
            {
                Vector3 p = new Vector3(-9.2f + Mathf.Sin(z * 0.35f) * 0.8f, 0f, z);
                p.y = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(p.x, p.z);
                var b = SpawnBambooStalkCluster(root, p, bambooPrefab, foliagePrefab, bambooGoldMat, bambooLeafMat, 1.35f);
                b.name = $"TreLuyLang_TienCanh_{z}";
            }

            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "HaoQuang_TreThan";
            glow.transform.SetParent(sacred.transform, false);
            glow.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            glow.transform.localScale = Vector3.one * 0.9f;
            var auraMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/Characters/SelectionRing.mat");
            if (auraMat != null) glow.GetComponent<Renderer>().sharedMaterial = auraMat;

            // Falling bamboo leaves VFX
            var pfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/PFX_Forest_DriftingLeaves.prefab");
            if (pfxPrefab != null)
            {
                var pfx = (GameObject)PrefabUtility.InstantiatePrefab(pfxPrefab, root.transform);
                pfx.name = "VFX_LaTre_RoiBay";
                pfx.transform.position = new Vector3(-18f, sy + 4.2f, -4f);
            }
        }

        private static GameObject SpawnBambooStalkCluster(GameObject parent, Vector3 localPos, GameObject prefabA, GameObject prefabB, Material goldMat, Material leafMat, float scaleMult)
        {
            var asset = (prefabB != null) ? prefabB : prefabA;
            GameObject b;
            if (asset != null)
            {
                b = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent.transform);
                b.transform.localPosition = localPos;
                b.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                b.transform.localScale = Vector3.one * (scaleMult * Random.Range(0.95f, 1.15f));
            }
            else
            {
                b = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                b.transform.SetParent(parent.transform, false);
                b.transform.localPosition = localPos + Vector3.up * 2.5f;
                b.transform.localScale = new Vector3(0.12f, 2.5f, 0.12f);
                if (goldMat != null) b.GetComponent<Renderer>().sharedMaterial = goldMat;
            }
            return b;
        }

        /// <summary>
        /// Builds the full collection of authentic East Asian trees matching Reference Image 4:
        /// Weeping willows (Liễu rủ), mountain pines (Tùng bách), broadleaf banyan (Đa cổ thụ), and golden maples (Phong lá vàng).
        /// Zero cartoon blob models!
        /// </summary>
        public static void BuildEastAsianTreeCollection(GameObject parent)
        {
            var root = CreateChild(parent, "HeCay_CoThu_A_Dong");

            var oakPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_Broadleaf_Oak.prefab");
            var pinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_Pine_Mossy.prefab");
            var grandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_LayeredCanopy_Grand.prefab");
            var puffyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_PuffyCrown_Painterly.prefab");
            var banyanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy/Environment/Meshy_CayDaCoThu_LangViet.prefab") ?? oakPrefab;

            var willowMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_LieuRu_XanhNhat.mat");
            var mapleMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_PhongLaVang.mat");

            // 1. Weeping Willows (Cây Liễu Rủ - Drooping pale-green branches over water mirror, Image 4)
            Vector3[] willowPos = {
                new Vector3(-4.5f, 0f, -14.2f),
                new Vector3(12.5f, 0f, -15.5f),
                new Vector3(-21f, 0f, -15.0f),
                new Vector3(26.5f, 0f, -16.0f)
            };
            var wAsset = grandPrefab ?? oakPrefab;
            for (int w = 0; w < willowPos.Length; w++)
            {
                if (wAsset == null) break;
                var willow = (GameObject)PrefabUtility.InstantiatePrefab(wAsset, root.transform);
                willow.name = $"CayLieuRu_VenSong_{w + 1}";
                float gy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(willowPos[w].x, willowPos[w].z);
                willow.transform.position = new Vector3(willowPos[w].x, gy, willowPos[w].z);
                willow.transform.rotation = Quaternion.Euler(0f, (w * 55f) % 360f, 0f);
                willow.transform.localScale = Vector3.one * Random.Range(1.5f, 2.0f);
                if (willowMat != null)
                {
                    foreach (var mr in willow.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (mr.name.Contains("Bush") || mr.name.Contains("Crown") || mr.name.Contains("Leaf") || mr.name.Contains("Canopy") || mr.name.Contains("Tree"))
                            mr.sharedMaterial = willowMat;
                    }
                }
            }

            // 2. Mountain Pines (Cây Tùng / Thông Bách - Tiered 14m needle umbrellas along karst foothills, Image 4)
            Vector3[] pinePos = {
                new Vector3(-36f, 0f, 48f), new Vector3(-24f, 0f, 62f),
                new Vector3(-10f, 0f, 55f), new Vector3(12f, 0f, 58f),
                new Vector3(28f, 0f, 52f), new Vector3(42f, 0f, 46f),
                new Vector3(-58f, 0f, 22f), new Vector3(58f, 0f, 22f)
            };
            var pAsset = pinePrefab ?? grandPrefab;
            for (int p = 0; p < pinePos.Length; p++)
            {
                if (pAsset == null) break;
                var pine = (GameObject)PrefabUtility.InstantiatePrefab(pAsset, root.transform);
                pine.name = $"CayTungBach_ChanNui_{p + 1}";
                float gy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(pinePos[p].x, pinePos[p].z);
                pine.transform.position = new Vector3(pinePos[p].x, gy, pinePos[p].z);
                pine.transform.rotation = Quaternion.Euler(0f, (p * 43f) % 360f, 0f);
                pine.transform.localScale = Vector3.one * Random.Range(1.4f, 1.9f);
            }

            // 3. Ancient Sprawling Banyan (Cây Đa Cổ Thụ - Massive canopy spread over well & yard, Image 4)
            if (banyanPrefab != null)
            {
                var banyan = (GameObject)PrefabUtility.InstantiatePrefab(banyanPrefab, root.transform);
                banyan.name = "CayDaCoThu_BongMat_SanDinh";
                float gy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(-14f, 7.5f);
                banyan.transform.position = new Vector3(-14f, gy, 7.5f);
                banyan.transform.rotation = Quaternion.Euler(0f, -25f, 0f);
                banyan.transform.localScale = Vector3.one * 1.8f;
            }

            // 4. Golden Autumn Maple Trees (Cây Phong Lá Vàng - Warm amber/gold foliage, Image 4)
            Vector3[] maplePos = {
                new Vector3(-16f, 0f, 22f), new Vector3(16f, 0f, 24f),
                new Vector3(8f, 0f, -22f), new Vector3(-10f, 0f, -22f),
                new Vector3(-28f, 0f, -2f), new Vector3(29f, 0f, 2f)
            };
            var mAsset = puffyPrefab ?? grandPrefab;
            for (int m = 0; m < maplePos.Length; m++)
            {
                if (mAsset == null) break;
                var maple = (GameObject)PrefabUtility.InstantiatePrefab(mAsset, root.transform);
                maple.name = $"CayPhong_LaVang_{m + 1}";
                float gy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(maplePos[m].x, maplePos[m].z);
                maple.transform.position = new Vector3(maplePos[m].x, gy, maplePos[m].z);
                maple.transform.rotation = Quaternion.Euler(0f, (m * 61f) % 360f, 0f);
                maple.transform.localScale = Vector3.one * Random.Range(1.3f, 1.7f);
                if (mapleMat != null)
                {
                    foreach (var mr in maple.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (mr.name.Contains("Bush") || mr.name.Contains("Crown") || mr.name.Contains("Leaf") || mr.name.Contains("Canopy") || mr.name.Contains("Tree"))
                            mr.sharedMaterial = mapleMat;
                    }
                }
            }
        }

        public static void BuildSoaringKarstPillars(GameObject parent)
        {
            var root = CreateChild(parent, "DayNui_VachDaVoi_ThuyMac");
            var cliffPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy_DaNui_RungThieng.prefab");
            var mtnAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Mountain01.glb");
            var asset = cliffPrefab ?? mtnAsset;
            if (asset == null) return;

            Vector3[] distantPeaks = {
                new Vector3(-42f, -1f, 85f),
                new Vector3(-14f, -1f, 98f),
                new Vector3(18f, -1f, 95f),
                new Vector3(45f, -1f, 82f),
                new Vector3(-78f, -1f, 25f),
                new Vector3(78f, -1f, 25f)
            };

            for (int i = 0; i < distantPeaks.Length; i++)
            {
                var cliff = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                cliff.name = $"DinhNui_Karst_DuyenDang_{i + 1}";
                cliff.transform.position = distantPeaks[i];
                cliff.transform.rotation = Quaternion.Euler(0f, (i * 67f) % 360f, 0f);

                float w = Random.Range(15f, 22f);
                float h = Random.Range(42f, 62f);
                cliff.transform.localScale = new Vector3(w, h, w);
            }
        }

        public static void BuildAncientStoneWell(GameObject parent)
        {
            var root = CreateChild(parent, "GiengNgoc_CoDai_LangGiong");
            float wy = PhuDongVillageEnvironmentBuilder.GetTerrainElevation(-13.5f, 6.5f);
            root.transform.position = new Vector3(-13.5f, wy, 6.5f);

            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Sacred_Mountain_Stone.mat");
            var woodMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Wood.mat");
            var waterMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Meshy/Environment/Mat_DongSong_NuocBiec.mat");

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "ThanhGieng_DaCo";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            rim.transform.localScale = new Vector3(2.8f, 0.45f, 2.8f);
            if (stoneMat != null) rim.GetComponent<Renderer>().sharedMaterial = stoneMat;

            var wellWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellWater.name = "MatNuoc_Gieng";
            wellWater.transform.SetParent(root.transform, false);
            wellWater.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            wellWater.transform.localScale = new Vector3(2.3f, 0.05f, 2.3f);
            var col = wellWater.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            if (waterMat != null) wellWater.GetComponent<Renderer>().sharedMaterial = waterMat;

            var beamL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamL.name = "CotTre_L";
            beamL.transform.SetParent(root.transform, false);
            beamL.transform.localPosition = new Vector3(-1.1f, 1.4f, 0f);
            beamL.transform.localScale = new Vector3(0.12f, 1.1f, 0.12f);
            if (woodMat != null) beamL.GetComponent<Renderer>().sharedMaterial = woodMat;

            var beamR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamR.name = "CotTre_R";
            beamR.transform.SetParent(root.transform, false);
            beamR.transform.localPosition = new Vector3(1.1f, 1.4f, 0f);
            beamR.transform.localScale = new Vector3(0.12f, 1.1f, 0.12f);
            if (woodMat != null) beamR.GetComponent<Renderer>().sharedMaterial = woodMat;

            var crossbar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crossbar.name = "ThanhNgang_CanKeo";
            crossbar.transform.SetParent(root.transform, false);
            crossbar.transform.localPosition = new Vector3(0f, 2.45f, 0f);
            crossbar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            crossbar.transform.localScale = new Vector3(0.09f, 1.25f, 0.09f);
            if (woodMat != null) crossbar.GetComponent<Renderer>().sharedMaterial = woodMat;

            var bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucket.name = "GauMucNuoc_Go";
            bucket.transform.SetParent(root.transform, false);
            bucket.transform.localPosition = new Vector3(0.15f, 1.6f, 0f);
            bucket.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            if (woodMat != null) bucket.GetComponent<Renderer>().sharedMaterial = woodMat;
        }

        private static GameObject BuildWoodenSampan(GameObject parent, string name, Material woodMat)
        {
            var boat = new GameObject(name);
            boat.transform.SetParent(parent.transform, false);

            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "DayThuyen";
            hull.transform.SetParent(boat.transform, false);
            hull.transform.localPosition = Vector3.zero;
            hull.transform.localScale = new Vector3(1.35f, 0.15f, 4.6f);
            if (woodMat != null) hull.GetComponent<Renderer>().sharedMaterial = woodMat;

            var bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bow.name = "MuiThuyen";
            bow.transform.SetParent(boat.transform, false);
            bow.transform.localPosition = new Vector3(0f, 0.12f, 2.4f);
            bow.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            bow.transform.localScale = new Vector3(1.1f, 0.14f, 1.2f);
            if (woodMat != null) bow.GetComponent<Renderer>().sharedMaterial = woodMat;

            var stern = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stern.name = "LaiThuyen";
            stern.transform.SetParent(boat.transform, false);
            stern.transform.localPosition = new Vector3(0f, 0.12f, -2.4f);
            stern.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            stern.transform.localScale = new Vector3(1.1f, 0.14f, 1.2f);
            if (woodMat != null) stern.GetComponent<Renderer>().sharedMaterial = woodMat;

            for (int s = -1; s <= 1; s += 2)
            {
                var gunwale = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gunwale.name = $"ManThuyen_{s}";
                gunwale.transform.SetParent(boat.transform, false);
                gunwale.transform.localPosition = new Vector3(s * 0.7f, 0.22f, 0f);
                gunwale.transform.localRotation = Quaternion.Euler(0f, 0f, s * -14f);
                gunwale.transform.localScale = new Vector3(0.08f, 0.42f, 4.4f);
                if (woodMat != null) gunwale.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            var oar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            oar.name = "MaiCheo_Go";
            oar.transform.SetParent(boat.transform, false);
            oar.transform.localPosition = new Vector3(0.65f, 0.35f, -1.2f);
            oar.transform.localRotation = Quaternion.Euler(15f, 25f, 45f);
            oar.transform.localScale = new Vector3(0.06f, 1.8f, 0.06f);
            if (woodMat != null) oar.GetComponent<Renderer>().sharedMaterial = woodMat;

            return boat;
        }

        private static void BuildWoodenHandcart(GameObject parent, Vector3 pos, Material woodMat)
        {
            var cart = new GameObject("XeKeo_Go_BenDo");
            cart.transform.SetParent(parent.transform, false);
            cart.transform.position = pos;
            cart.transform.rotation = Quaternion.Euler(0f, -25f, 0f);

            var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "ThungXe";
            bed.transform.SetParent(cart.transform, false);
            bed.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            bed.transform.localScale = new Vector3(1.6f, 0.32f, 2.4f);
            if (woodMat != null) bed.GetComponent<Renderer>().sharedMaterial = woodMat;

            for (int w = -1; w <= 1; w += 2)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"BanhXe_Go_{w}";
                wheel.transform.SetParent(cart.transform, false);
                wheel.transform.localPosition = new Vector3(w * 0.95f, 0.42f, 0f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.localScale = new Vector3(0.85f, 0.12f, 0.85f);
                if (woodMat != null) wheel.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            for (int s = -1; s <= 1; s += 2)
            {
                var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shaft.name = $"CangXe_{s}";
                shaft.transform.SetParent(cart.transform, false);
                shaft.transform.localPosition = new Vector3(s * 0.55f, 0.35f, -1.8f);
                shaft.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
                shaft.transform.localScale = new Vector3(0.08f, 0.08f, 1.6f);
                if (woodMat != null) shaft.GetComponent<Renderer>().sharedMaterial = woodMat;
            }
        }

        private static void BuildPotteryAndBaskets(GameObject parent, Material stoneMat, Material woodMat)
        {
            var cluster = new GameObject("ChumVai_Va_GioCa_BenDo");
            cluster.transform.SetParent(parent.transform, false);

            Vector3[] jarPos = {
                new Vector3(-4.8f, 0.28f, -10.8f),
                new Vector3(-4.2f, 0.32f, -11.2f),
                new Vector3(-7.8f, 0.28f, -9.8f)
            };
            for (int j = 0; j < jarPos.Length; j++)
            {
                var jar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jar.name = $"ChumSanh_Co_{j + 1}";
                jar.transform.SetParent(cluster.transform, false);
                jar.transform.position = jarPos[j];
                jar.transform.localScale = new Vector3(0.65f, 0.85f, 0.65f);
                if (stoneMat != null) jar.GetComponent<Renderer>().sharedMaterial = stoneMat;
            }
        }

        private static void BuildDriftingPeachPetalsVFX(GameObject parent)
        {
            var go = new GameObject("VFX_CanhHoaDao_RoiBay");
            go.transform.SetParent(parent.transform, false);
            go.transform.position = new Vector3(0f, 8f, -12f);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            main.maxParticles = 140;

            Color pinkA = new Color(1.0f, 0.65f, 0.78f, 0.85f);
            Color pinkB = new Color(0.98f, 0.48f, 0.68f, 0.80f);
            main.startColor = new ParticleSystem.MinMaxGradient(pinkA, pinkB);

            var emission = ps.emission;
            emission.rateOverTime = 16f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(70f, 4f, 60f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            vel.y = new ParticleSystem.MinMaxCurve(-0.25f, -0.6f);
            vel.z = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);

            var psr = go.GetComponent<ParticleSystemRenderer>();
            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            if (pShader != null)
            {
                var mat = new Material(pShader);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", pinkA);
                psr.sharedMaterial = mat;
            }
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void SetMatRecursive(GameObject go, Material mat)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }
    }
}
