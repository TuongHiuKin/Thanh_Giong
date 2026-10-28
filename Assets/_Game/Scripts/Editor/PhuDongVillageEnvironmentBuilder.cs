using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds environment terrain, meandering river, soaring karst mountains,
    /// 3D rice paddies, and wuxia foliage.
    /// Completely eliminates 8m rolling mound clipping and stark white road shaders.
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

            // 1. Procedural Level Village Meadow (180m x 180m, level at y=0 in village, carved river, gentle outer slopes)
            BuildProceduralVillageMeadow(root);

            // 2. Procedural Earthen Village Main Road (Warm brown packed dirt/cobblestones, NO stark white ribbons!)
            BuildProceduralVillageRoad(root);
        }

        public static float GetTerrainElevation(float x, float z)
        {
            // 1. River channel carving (smoothly dished down)
            float riverZ = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);
            float rDist = Mathf.Abs(z - riverZ);
            float riverHalfW = 9.5f;
            if (x >= -4f && x <= 26f) riverHalfW = 13.5f;
            else if (x >= -24f && x < -8f) riverHalfW = 8.0f;

            if (rDist < riverHalfW)
            {
                float t = rDist / riverHalfW;
                return Mathf.Lerp(-0.45f, 0.02f, t * t);
            }

            // 2. Village central plateau is level at y = 0.0m (Zero buried buildings or grass!)
            if (Mathf.Abs(x) < 36f && z >= -30f && z <= 34f)
            {
                return 0.0f;
            }

            // 3. Gentle outer foothills framing the landscape
            float dist = Mathf.Max(0f, Mathf.Sqrt(x * x + (z - 5f) * (z - 5f)) - 38f);
            float hillHeight = Mathf.Clamp(dist * 0.10f, 0f, 5.0f);
            if (z > 42f) hillHeight += (z - 42f) * 0.14f;
            return hillHeight;
        }

        private static void BuildProceduralVillageMeadow(GameObject parent)
        {
            var groundGo = new GameObject("ThungLung_DongCo_PhuDong_180m");
            groundGo.transform.SetParent(parent.transform, false);

            var groundMat = AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "BiomeGround.mat")
                         ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamCo_Wuxia.mat")
                         ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Grass.mat");

            int resolution = 72; // 72x72 grid
            float size = 180f;
            float step = size / resolution;
            float half = size * 0.5f;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            for (int iz = 0; iz <= resolution; iz++)
            {
                float z = -half + iz * step;
                for (int ix = 0; ix <= resolution; ix++)
                {
                    float x = -half + ix * step;
                    float y = GetTerrainElevation(x, z);

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(x * 0.12f, z * 0.12f));

                    // Multi-tone vertex colors for OpenWorldBiomeGround shader
                    float riverZ = -15.5f + 4.5f * Mathf.Sin(x * 0.055f) - 2.2f * Mathf.Cos(x * 0.11f);
                    float rDist = Mathf.Abs(z - riverZ);
                    float riverHalfW = (x >= -4f && x <= 26f) ? 13.5f : ((x >= -24f && x < -8f) ? 8.0f : 9.5f);

                    Color col;
                    if (rDist < riverHalfW * 0.75f)
                    {
                        // Riverbed: moist river silt, wet sand and pebbles
                        col = new Color(0.40f, 0.35f, 0.26f, 1f);
                    }
                    else if (rDist < riverHalfW + 2.5f)
                    {
                        // Shoreline bank: rich fertile loam & damp soil
                        col = new Color(0.36f, 0.42f, 0.22f, 1f);
                    }
                    else if (Mathf.Abs(x) < 2.5f && z >= -14f && z <= 32f)
                    {
                        // Village main packed earthen footpath
                        col = new Color(0.46f, 0.37f, 0.24f, 1f);
                    }
                    else if (Mathf.Abs(x) < 14f && z >= 6f && z <= 22f)
                    {
                        // Village central gathering courtyard
                        col = new Color(0.42f, 0.35f, 0.22f, 1f);
                    }
                    else if (x >= 16f && x <= 34f && z >= 0f && z <= 22f)
                    {
                        // Rice terrace waterlogged fertile mud
                        col = new Color(0.28f, 0.38f, 0.18f, 1f);
                    }
                    else
                    {
                        // Vibrant East Asian meadow with natural Perlin noise variation
                        float noise = Mathf.PerlinNoise(x * 0.08f + 15.3f, z * 0.08f + 27.8f);
                        Color c1 = new Color(0.28f, 0.48f, 0.15f, 1f); // Deep emerald grass
                        Color c2 = new Color(0.38f, 0.58f, 0.18f, 1f); // Sunlit vibrant clover
                        col = Color.Lerp(c1, c2, noise);
                    }

                    // Slope-based karst rock weathering on foothills
                    if (y > 1.0f)
                    {
                        float slopeT = Mathf.Clamp01((y - 1.0f) / 3.8f);
                        Color rockCol = new Color(0.48f, 0.46f, 0.42f, 1f);
                        col = Color.Lerp(col, rockCol, slopeT * 0.70f);
                    }

                    colors.Add(col);

                    // Numerical normal calculation
                    float eps = 0.5f;
                    float yR = GetTerrainElevation(x + eps, z);
                    float yL = GetTerrainElevation(x - eps, z);
                    float yU = GetTerrainElevation(x, z + eps);
                    float yD = GetTerrainElevation(x, z - eps);
                    Vector3 n = new Vector3(yL - yR, eps * 2f, yD - yU).normalized;
                    normals.Add(n);
                }
            }

            for (int iz = 0; iz < resolution; iz++)
            {
                for (int ix = 0; ix < resolution; ix++)
                {
                    int i0 = iz * (resolution + 1) + ix;
                    int i1 = i0 + 1;
                    int i2 = (iz + 1) * (resolution + 1) + ix;
                    int i3 = i2 + 1;

                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            }

            var mesh = new Mesh { name = "PhuDong_VillageMeadow_Mesh" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);

            groundGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = groundGo.AddComponent<MeshRenderer>();
            if (groundMat != null) mr.sharedMaterial = groundMat;
            groundGo.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private static void BuildProceduralVillageRoad(GameObject parent)
        {
            var roadGo = new GameObject("DuongLang_DatNen_PhuDong");
            roadGo.transform.SetParent(parent.transform, false);

            var roadMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Road.mat")
                       ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Farm_Soil.mat");

            // Road centerline points through village
            Vector3[] pathWaypoints = {
                new Vector3(0f, 0.03f, -14f),    // Pier / River crossing
                new Vector3(0.5f, 0.03f, -6f),   // South village entry
                new Vector3(-0.2f, 0.03f, 4f),   // Pasture & forge fork
                new Vector3(0.8f, 0.03f, 14f),   // Communal pavilion entrance
                new Vector3(0f, 0.03f, 24f),     // Stilt house yard
                new Vector3(0f, 0.03f, 33f)      // Fortified village gate
            };

            int segments = 32;
            float roadWidth = 3.6f;
            float halfW = roadWidth * 0.5f;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float segIndex = t * (pathWaypoints.Length - 1);
                int idx = Mathf.Min(Mathf.FloorToInt(segIndex), pathWaypoints.Length - 2);
                float frac = segIndex - idx;

                Vector3 p = Vector3.Lerp(pathWaypoints[idx], pathWaypoints[idx + 1], frac);
                Vector3 next = (idx + 1 < pathWaypoints.Length) ? pathWaypoints[idx + 1] : pathWaypoints[idx];
                Vector3 prev = (idx > 0) ? pathWaypoints[idx - 1] : pathWaypoints[idx];
                Vector3 forward = (next - prev).normalized;
                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                Vector3 right = new Vector3(forward.z, 0f, -forward.x).normalized;

                verts.Add(p - right * halfW);
                verts.Add(p + right * halfW);
                uvs.Add(new Vector2(0f, t * 8f));
                uvs.Add(new Vector2(1f, t * 8f));
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                if (i > 0)
                {
                    int curr = i * 2;
                    int pIdx = (i - 1) * 2;
                    tris.Add(pIdx); tris.Add(curr); tris.Add(pIdx + 1);
                    tris.Add(pIdx + 1); tris.Add(curr); tris.Add(curr + 1);
                }
            }

            var mesh = new Mesh { name = "PhuDong_VillageRoad_Mesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);

            roadGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            var rmr = roadGo.AddComponent<MeshRenderer>();
            if (roadMat != null) rmr.sharedMaterial = roadMat;
        }

        public static void BuildMountains(GameObject env)
        {
            PhuDongAsianCultureBuilder.BuildSoaringKarstPillars(env);
        }

        public static void BuildSkyAndClouds(GameObject env)
        {
            var skyMat = LoadMat("Sky/ThanhGiong_Sky.mat");
            if (skyMat != null) RenderSettings.skybox = skyMat;
        }

        public static void BuildRiverAndBridges(GameObject env)
        {
            PhuDongAsianCultureBuilder.BuildOrganicMeanderingRiver(env);
        }

        public static void BuildRiceTerraces(GameObject env)
        {
            var root = CreateChild(env, "DongRuong_BacThang_LuaNuoc");
            root.transform.position = new Vector3(22f, 0f, 6f);

            var soilMat = AssetDatabase.LoadAssetAtPath<Material>(OpenWorldBase + "RiceFieldSoil.mat") ?? LoadMat("World/Farm_Soil.mat");
            var waterMat = LoadMat("Meshy/Environment/Mat_DongSong_NuocBiec.mat");
            var reedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_A.fbx");
            var riceMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_ThamCo_Wuxia.mat")
                       ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Grass.mat");

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

                // Vibrant green 3D rice seedlings
                for (int z = -10; z <= 10; z += 3)
                {
                    for (int off = -2; off <= 2; off += 2)
                    {
                        if (reedAsset != null)
                        {
                            var plant = (GameObject)PrefabUtility.InstantiatePrefab(reedAsset, terrace.transform);
                            plant.name = $"KhomLua_{z}_{off}";
                            plant.transform.localPosition = new Vector3(off * 1.3f, 0.20f, z);
                            plant.transform.localScale = new Vector3(0.85f, 0.70f, 0.85f); // Knee-height young rice plants
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

            // 2. Dense 3D Ground Carpet: Grass Blades, Clover, Wildflowers & Pampas Reeds (Reference Image 1)
            PhuDongAsianCultureBuilder.BuildDenseVegetationAndPampas(env);

            // 3. Full East Asian Tree Collection: Weeping Willows, Mountain Pines, Grand Banyan & Golden Maples (Reference Image 4)
            PhuDongAsianCultureBuilder.BuildEastAsianTreeCollection(env);

            // 4. Authentic wild foliage clusters along paths
            var root = CreateChild(env, "ThamThucVat_BuiCo_HoangDa");
            var bushAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/FoliageCluster_Undergrowth_Wild.prefab")
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_PuffyCrown_Painterly.prefab");

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
                    float gy = GetTerrainElevation(bushes[i].x, bushes[i].z);
                    b.transform.position = new Vector3(bushes[i].x, gy, bushes[i].z);
                    b.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    b.transform.localScale = Vector3.one * Random.Range(0.85f, 1.25f);
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
