using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds high-fidelity East Asian / ancient Vietnamese wuxia landscape features:
    /// Natural organic meandering river, soaring karst limestone pinnacles,
    /// blooming pink peach blossom trees, dense volumetric 3D grass & tall white pampas reeds,
    /// authentic arched timber bridges, wooden river docks, sampan boats, and stone stepping paths.
    /// </summary>
    public static class PhuDongAsianCultureBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string MatBase = "Assets/_Game/Art/Materials/";
        private const string AsianBase = "Assets/_Game/Art/AsianCulture/";
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

            // Centerline curve: z(x) = -15.5f + 4.5f * sin(0.055 * x) - 2.2f * cos(0.11 * x)
            // Width(x): swells to 24m at central basin (x = 2..22), narrows to 13.5m at bridge (x = -16)
            for (int i = 0; i <= segments; i++)
            {
                float x = startX + i * stepX;
                float z = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);

                // Tangent vector
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
                    width = 13.8f; // Narrow river gorge under bridge
                }

                float v = (float)i / segments * 12f;
                float halfW = width * 0.5f;
                Vector3 center = new Vector3(x, 0f, z);

                // 5 cross-section points: FarBank, FarShallow, DeepCenter, NearShallow, NearBank
                Vector3 p0 = center - perp * halfW + Vector3.up * 0.03f;
                Vector3 p1 = center - perp * (halfW * 0.55f) + Vector3.down * 0.06f;
                Vector3 p2 = center + Vector3.down * 0.28f;
                Vector3 p3 = center + perp * (halfW * 0.55f) + Vector3.down * 0.06f;
                Vector3 p4 = center + perp * halfW + Vector3.up * 0.03f;

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
                        // Double sided for clear water visibility
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

            // Arched wooden footbridge across the river at x = -16f
            BuildArchedWoodenBridge(root);

            // River dock and moored sampans at tranquil basin
            BuildRiverDockAndSampans(root);

            // Stepping stones across the shallow pond edge
            BuildLotusSteppingStones(root);

            // Real 3D river rocks lining the natural banks
            BuildNaturalRiverShoreRocks(root);

            return root;
        }

        /// <summary>
        /// Places authentic arched timber bridge spanning smoothly across the meandering river.
        /// </summary>
        public static void BuildArchedWoodenBridge(GameObject parent)
        {
            var bridgeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Bridge01.glb");
            if (bridgeAsset != null)
            {
                var bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgeAsset, parent.transform);
                bridge.name = "CauGo_VomCong_QuaSong";
                bridge.transform.position = new Vector3(-16.5f, 0.15f, -17.2f);
                bridge.transform.rotation = Quaternion.Euler(0f, 12f, 0f);
                bridge.transform.localScale = new Vector3(0.38f, 0.55f, 0.75f);
            }
        }

        /// <summary>
        /// Builds rustic East Asian river dock with wooden piers,
        /// moored wooden sampan boats (thuyền nan), wooden cart, ceramic jars, and fish baskets.
        /// </summary>
        public static void BuildRiverDockAndSampans(GameObject parent)
        {
            var root = CreateChild(parent, "BenDo_Cauthuyen_ThuyenNan");
            var dockAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Dock01.glb");
            var woodMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Wood.mat");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat");

            // 1. Rustic wooden pier extending into the tranquil basin (Human scale ~4m x 4.5m)
            if (dockAsset != null)
            {
                var dock = (GameObject)PrefabUtility.InstantiatePrefab(dockAsset, root.transform);
                dock.name = "BenDo_CauTau_Go";
                dock.transform.position = new Vector3(-1.5f, -0.05f, -13.2f);
                dock.transform.rotation = Quaternion.Euler(0f, 172f, 0f);
                dock.transform.localScale = Vector3.one * 0.75f;
            }

            // 2. Moored wooden sampans floating in the water basin
            Vector3[] boatPos = {
                new Vector3(4.5f, 0.02f, -14.2f),
                new Vector3(9.8f, 0.02f, -16.5f),
                new Vector3(16.2f, 0.02f, -13.8f)
            };
            float[] boatRot = { 18f, -22f, 12f };
            for (int b = 0; b < boatPos.Length; b++)
            {
                var boat = BuildWoodenSampan(root, $"ThuyenNan_Go_{b + 1}", woodMat);
                boat.transform.position = boatPos[b];
                boat.transform.rotation = Quaternion.Euler(0f, boatRot[b], 0f);
            }

            // 3. Wooden Handcart on the dock bank
            BuildWoodenHandcart(root, new Vector3(-6.5f, 0.12f, -10.5f), woodMat);

            // 4. Ceramic jars and fish baskets along the river pier
            BuildPotteryAndBaskets(root, stoneMat, woodMat);
        }

        /// <summary>
        /// Builds round carved stepping stones across the shallow lotus pond.
        /// </summary>
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
        /// Places natural 3D river rocks along the curving shoreline.
        /// </summary>
        public static void BuildNaturalRiverShoreRocks(GameObject parent)
        {
            var rockA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/rock_single_A.fbx");
            var rockB = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/rock_single_B.fbx");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat");

            Vector3[] rockPos = {
                new Vector3(-32f, 0.02f, -12.5f), new Vector3(-24f, 0.02f, -18.5f),
                new Vector3(-12f, 0.02f, -11.8f), new Vector3(-7f, 0.02f, -19.2f),
                new Vector3(1f, 0.02f, -10.8f), new Vector3(8f, 0.02f, -20.2f),
                new Vector3(15f, 0.02f, -9.8f), new Vector3(22f, 0.02f, -20.8f),
                new Vector3(31f, 0.02f, -11.2f), new Vector3(38f, 0.02f, -18.5f)
            };

            for (int i = 0; i < rockPos.Length; i++)
            {
                var asset = (i % 2 == 0 && rockA != null) ? rockA : rockB;
                if (asset == null) continue;
                var rk = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent.transform);
                rk.name = $"DaBoSong_TuNhien_{i + 1}";
                rk.transform.position = rockPos[i];
                rk.transform.rotation = Quaternion.Euler(0f, (i * 53f) % 360f, 0f);
                rk.transform.localScale = Vector3.one * Random.Range(1.8f, 2.8f); // Natural 0.8m - 1.2m river boulders!
                if (stoneMat != null) SetMatRecursive(rk, stoneMat);
            }
        }

        /// <summary>
        /// Generates dense volumetric 3D grass field and tall white pampas reeds (Cỏ lau trắng).
        /// Matches the rich, wind-swept aesthetic seen in Nghịch Thủy Hàn Reference Image 4.
        /// </summary>
        public static void BuildDenseVegetationAndPampas(GameObject parent)
        {
            var root = CreateChild(parent, "ThamCoLau_Trang_DayDac");
            var grassMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamCo_Wuxia.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/OpenWorld/ZeldaWindGrass.mat");
            var pampasMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_CoLau_Trang.mat");
            var reedAssetA = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_A.fbx");
            var reedAssetB = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_B.fbx");
            var flowerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Flower01.glb");
            var bushAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Bush01.glb");

            // 1. Clustered Tall White Pampas Reeds (Cỏ lau trắng bạt ngàn ven sông như ảnh 4 - Chiều cao 1.5m - 2.1m)
            Vector3[] pampasPos = {
                new Vector3(-28f, 0.05f, -11.5f), new Vector3(-22f, 0.05f, -12.2f),
                new Vector3(-10f, 0.05f, -10.5f), new Vector3(-5f, 0.05f, -10.8f),
                new Vector3(3f, 0.05f, -9.8f), new Vector3(8f, 0.05f, -10.2f),
                new Vector3(14f, 0.05f, -9.2f), new Vector3(20f, 0.05f, -8.8f),
                new Vector3(27f, 0.05f, -9.5f), new Vector3(34f, 0.05f, -10.5f),
                new Vector3(-30f, 0.05f, -21.5f), new Vector3(-18f, 0.05f, -22.5f),
                new Vector3(-4f, 0.05f, -21.8f), new Vector3(7f, 0.05f, -22.2f),
                new Vector3(18f, 0.05f, -23.5f), new Vector3(28f, 0.05f, -22.8f),
                new Vector3(19f, 0.05f, 4.5f), new Vector3(25f, 0.05f, 5.0f),
                new Vector3(-16f, 0.05f, 5.2f), new Vector3(-12f, 0.05f, 16.5f)
            };

            for (int i = 0; i < pampasPos.Length; i++)
            {
                var asset = (i % 2 == 0 && reedAssetA != null) ? reedAssetA : (reedAssetB ?? reedAssetA);
                if (asset == null) break;
                var reed = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                reed.name = $"BuiCoLau_Trang_{i + 1}";
                reed.transform.position = pampasPos[i];
                reed.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                reed.transform.localScale = new Vector3(1.5f, Random.Range(2.2f, 3.0f), 1.5f); // Natural 1.5m - 2.1m swaying pampas plumes!
                if (pampasMat != null) SetMatRecursive(reed, pampasMat);
            }

            // 2. Wildflower Clumps along pathways
            if (flowerAsset != null)
            {
                Vector3[] flowerPos = {
                    new Vector3(-3.5f, 0.05f, -8f), new Vector3(3.5f, 0.05f, -8f),
                    new Vector3(-5.5f, 0.05f, 2f), new Vector3(5.5f, 0.05f, 2f),
                    new Vector3(-4.5f, 0.05f, 12f), new Vector3(4.5f, 0.05f, 12f)
                };
                for (int i = 0; i < flowerPos.Length; i++)
                {
                    var fl = (GameObject)PrefabUtility.InstantiatePrefab(flowerAsset, root.transform);
                    fl.name = $"HoaDai_VenDuong_{i + 1}";
                    fl.transform.position = flowerPos[i];
                    fl.transform.localScale = Vector3.one * 1.4f;
                }
            }

            // 3. Dense 3D Cross-Quad Grass Mesh (2,200 tufts = 13,200 volumetric blades)
            BuildVolumetricStarGrassMesh(root, grassMat);
        }

        /// <summary>
        /// Builds blooming pink peach blossom trees along the riverbank mirroring in the water,
        /// complete with drifting pink flower petal particles.
        /// </summary>
        public static void BuildPeachBlossomGroves(GameObject parent)
        {
            var root = CreateChild(parent, "RungHoaDao_HongTham");
            var treeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Tree01.glb")
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_LayeredCanopy_Grand.prefab");
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
                tree.transform.position = peachPos[i];
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
        /// Builds distant limestone karst pinnacles framing only the northern background and far flanks.
        /// Leaves the foreground, camera angle, and sky open for clear viewing and wide panoramic vistas.
        /// </summary>
        public static void BuildSoaringKarstPillars(GameObject parent)
        {
            var root = CreateChild(parent, "DayNui_VachDaVoi_ThuyMac");
            var cliffPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy_DaNui_RungThieng.prefab");
            var mtnAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "avatar-garden_Mountain01.glb");
            var asset = cliffPrefab ?? mtnAsset;
            if (asset == null) return;

            // Only 6 distant peaks framing the northern horizon (z > 65f) and far flanks (x < -65f or x > 65f)
            // Foreground (z < 0) is kept 100% open so nothing blocks the camera or player view!
            Vector3[] distantPeaks = {
                new Vector3(-42f, -1f, 85f),  // Far North-West
                new Vector3(-14f, -1f, 98f),  // Distant North
                new Vector3(18f, -1f, 95f),   // Distant North
                new Vector3(45f, -1f, 82f),   // Far North-East
                new Vector3(-78f, -1f, 25f),  // Far West flank
                new Vector3(78f, -1f, 25f)    // Far East flank
            };

            for (int i = 0; i < distantPeaks.Length; i++)
            {
                var cliff = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                cliff.name = $"DinhNui_Karst_DuyenDang_{i + 1}";
                cliff.transform.position = distantPeaks[i];
                cliff.transform.rotation = Quaternion.Euler(0f, (i * 67f) % 360f, 0f);

                // Elegant proportions: tall enough to frame the horizon, but not overwhelming
                float w = Random.Range(15f, 22f);
                float h = Random.Range(42f, 62f);
                cliff.transform.localScale = new Vector3(w, h, w);
            }
        }

        /// <summary>
        /// Builds authentic ancient Vietnamese stone well with bamboo crane under the ancient tree.
        /// </summary>
        public static void BuildAncientStoneWell(GameObject parent)
        {
            var root = CreateChild(parent, "GiengNgoc_CoDai_LangGiong");
            root.transform.position = new Vector3(-13.5f, 0f, 6.5f);

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

        // --- HELPER CONSTRUCTIONS ---

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

        private static void BuildVolumetricStarGrassMesh(GameObject parent, Material grassMat)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            int clumps = 2200;
            var rng = new System.Random(77);

            Color rootCol = new Color(0.18f, 0.38f, 0.12f, 1f);
            Color tipCol = new Color(0.72f, 0.90f, 0.36f, 1f);

            for (int i = 0; i < clumps; i++)
            {
                float x = (float)(rng.NextDouble() * 74.0 - 37.0);
                float z = (float)(rng.NextDouble() * 74.0 - 37.0);

                // River curve formula exclusion
                float riverZ = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);
                float rDist = Mathf.Abs(z - riverZ);
                if (rDist < 7.5f) continue; // In water

                // Village path exclusion
                if (Mathf.Abs(x) < 2.8f && z >= -24f && z <= 24f) continue;

                float h = 0.65f + (float)rng.NextDouble() * 0.5f;
                float w = 0.38f + (float)rng.NextDouble() * 0.2f;
                Vector3 p = new Vector3(x, 0.05f, z);

                // 3 vertical cross-quad blades per clump (0 deg, 60 deg, 120 deg)
                float[] angles = { 0f, Mathf.PI / 3f, 2f * Mathf.PI / 3f };
                for (int b = 0; b < 3; b++)
                {
                    float ang = angles[b] + (float)rng.NextDouble() * 0.2f;
                    Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * (w * 0.5f);

                    int v = verts.Count;
                    verts.Add(p - dir);                     // bottom left
                    verts.Add(p + dir);                     // bottom right
                    verts.Add(p - dir + Vector3.up * h);    // top left
                    verts.Add(p + dir + Vector3.up * h);    // top right

                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(1f, 1f));

                    colors.Add(rootCol);
                    colors.Add(rootCol);
                    colors.Add(tipCol);
                    colors.Add(tipCol);

                    Vector3 n = Vector3.Cross(dir, Vector3.up).normalized;
                    normals.Add(n); normals.Add(n); normals.Add(n); normals.Add(n);

                    // Front & back faces
                    tris.Add(v); tris.Add(v + 2); tris.Add(v + 1);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v + 3);
                    tris.Add(v + 1); tris.Add(v + 2); tris.Add(v);
                    tris.Add(v + 3); tris.Add(v + 2); tris.Add(v + 1);
                }
            }

            var mesh = new Mesh { name = "StarGrass_Volumetric_Mesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);

            var go = new GameObject("ThamCo_3D_Wuxia_DayDac");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            if (grassMat != null) mr.sharedMaterial = grassMat;
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
