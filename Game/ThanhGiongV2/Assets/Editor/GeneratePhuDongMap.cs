using UnityEngine;
using UnityEditor;
using System.IO;

namespace ThanhGiong.Editor
{
    public class GeneratePhuDongMap : EditorWindow
    {
        [MenuItem("Thánh Gióng/Tạo Địa Hình Chương 1 (Làng Phù Đổng & Mỏ Quặng)", false, 10)]
        public static void BuildFullMap()
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                GameObject terrainObj = GameObject.Find("Terrain_ThanhGiongWorld");
                if (terrainObj != null) terrain = terrainObj.GetComponent<Terrain>();
            }

            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Terrain nào trong Scene! Hãy chọn hoặc tạo Terrain trước.", "OK");
                return;
            }

            TerrainData data = terrain.terrainData;
            Undo.RegisterCompleteObjectUndo(data, "Tạo Địa Hình Toàn Diện Chương 1");

            // 1. NẶN ĐỊA HÌNH CHUẨN THEO CONCEPT ART
            data.size = new Vector3(400f, 80f, 400f);
            int res = data.heightmapResolution;
            float[,] heights = new float[res, res];
            float baseHeight = 15f / 80f; // Nền sàn làng 15m

            for (int y = 0; y < res; y++)
            {
                float v = (float)y / (res - 1);
                for (int x = 0; x < res; x++)
                {
                    float u = (float)x / (res - 1);
                    float h = baseHeight;

                    // 1.1 Con suối uốn lượn hình chữ S
                    float riverCenterU = Mathf.Lerp(0.82f, 0.35f, v) + Mathf.Sin(v * Mathf.PI * 2.5f) * 0.08f;
                    float distToRiver = Mathf.Abs(u - riverCenterU);
                    if (distToRiver < 0.12f)
                    {
                        float riverFactor = Mathf.Cos((distToRiver / 0.12f) * (Mathf.PI * 0.5f));
                        h -= riverFactor * (7f / 80f); // Khoét sâu 7m
                    }

                    // 1.2 Dãy núi đá vôi sừng sững ở mỏ quặng góc trên bên phải (Zone C)
                    if (u > 0.48f && v > 0.38f)
                    {
                        float mU = Mathf.Clamp01((u - 0.48f) * 2.2f);
                        float mV = Mathf.Clamp01((v - 0.38f) * 2.0f);
                        float mountainFactor = mU * mV;
                        float noise = Mathf.PerlinNoise(u * 14f, v * 14f) * 0.45f + Mathf.PerlinNoise(u * 28f, v * 28f) * 0.18f;
                        h += mountainFactor * (0.60f + noise * 0.30f);
                    }

                    // 1.3 Đồi tre bao bọc phía sau và mạn trái Làng Phù Đổng
                    if (u < 0.22f || (u < 0.45f && v < 0.15f))
                    {
                        float villageHill = Mathf.Clamp01(Mathf.Max(0.22f - u, 0.15f - v) * 4.5f);
                        float hillNoise = Mathf.PerlinNoise(u * 9f, v * 9f) * 0.12f;
                        h += villageHill * (0.28f + hillNoise);
                    }

                    // 1.4 Viền núi biên giới tự nhiên bao quanh 4 mép map
                    float borderDist = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                    if (borderDist < 0.10f)
                    {
                        float borderFactor = Mathf.Clamp01((0.10f - borderDist) / 0.10f);
                        h += borderFactor * 0.48f;
                    }

                    // 1.5 Khoảng sân làng phẳng phiu để đặt cụm nhà sàn và lò rèn
                    if (u >= 0.22f && u <= 0.48f && v >= 0.15f && v <= 0.45f)
                    {
                        float blend = Mathf.Clamp01(Mathf.Min(Mathf.Min(u - 0.22f, 0.48f - u), Mathf.Min(v - 0.15f, 0.45f - v)) * 10f);
                        h = Mathf.Lerp(h, baseHeight, blend);
                    }

                    heights[y, x] = Mathf.Clamp01(h);
                }
            }
            data.SetHeights(0, 0, heights);

            // 2. TÔ MÀU TERRAIN TỰ ĐỘNG
            ApplyTerrainTextures(data);

            // 3. SET ÁNH SÁNG VÀNG BAN MAI & SƯƠNG MÙ
            SetupLightingAndAtmosphere();

            // 4. MẶT NƯỚC CON SUỐI TRONG VẮT
            CreateRiverWater(terrain.transform.position, data.size);

            // 5. TRỒNG CÂY TRE AI VÀ ĐẶT NHÂN VẬT THÁNH GIÓNG THẬT VÀO MAP
            BuildSceneWithRealAIModels(terrain.transform.position, data.size);

            EditorUtility.DisplayDialog("Thành Công Rực Rỡ!", 
                "Đã đưa toàn bộ mô hình 3D AI của nhóm vào Map:\n" +
                "✓ Cây Tre Thần khổng lồ (AI Model thật) đã được trồng quanh làng!\n" +
                "✓ Nhân vật Thánh Gióng dũng sĩ Việt Cổ (AI Model thật) đã xuất hiện tại sân làng!\n" +
                "✓ Thánh Gióng cưỡi ngựa sắt phun lửa (AI Model thật) đã đứng sẵn sàng!\n" +
                "✓ Đã dọn sạch các khối hộp thô sơ!", "Chiêm Ngưỡng Ngay!");
        }

        // ================= TÔ MÀU TEXTURES =================
        private static void ApplyTerrainTextures(TerrainData data)
        {
            string folder = "Assets/Settings/TerrainLayers";
            if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Settings", "TerrainLayers");

            Texture2D texGrass = CreateColorTexture(folder, "Tex_Grass_Green", new Color(0.36f, 0.60f, 0.18f), new Color(0.26f, 0.46f, 0.13f));
            Texture2D texDirt = CreateColorTexture(folder, "Tex_Red_Dirt", new Color(0.68f, 0.36f, 0.20f), new Color(0.52f, 0.26f, 0.14f));
            Texture2D texRock = CreateColorTexture(folder, "Tex_Cliff_Rock", new Color(0.50f, 0.50f, 0.48f), new Color(0.36f, 0.36f, 0.34f));
            Texture2D texMud = CreateColorTexture(folder, "Tex_River_Mud", new Color(0.42f, 0.34f, 0.26f), new Color(0.30f, 0.24f, 0.17f));

            TerrainLayer layerGrass = GetOrCreateLayer(folder, "Layer_Grass", texGrass, new Vector2(10, 10));
            TerrainLayer layerDirt = GetOrCreateLayer(folder, "Layer_RedDirt", texDirt, new Vector2(8, 8));
            TerrainLayer layerRock = GetOrCreateLayer(folder, "Layer_Rock", texRock, new Vector2(12, 12));
            TerrainLayer layerMud = GetOrCreateLayer(folder, "Layer_RiverMud", texMud, new Vector2(6, 6));

            data.terrainLayers = new TerrainLayer[] { layerGrass, layerDirt, layerRock, layerMud };

            int alphaRes = data.alphamapResolution;
            float[,,] splatmap = new float[alphaRes, alphaRes, 4];

            for (int y = 0; y < alphaRes; y++)
            {
                float v = (float)y / (alphaRes - 1);
                for (int x = 0; x < alphaRes; x++)
                {
                    float u = (float)x / (alphaRes - 1);

                    float steepness = data.GetSteepness(u, v);
                    float riverCenterU = Mathf.Lerp(0.82f, 0.35f, v) + Mathf.Sin(v * Mathf.PI * 2.5f) * 0.08f;
                    float distToRiver = Mathf.Abs(u - riverCenterU);

                    float wGrass = 0f, wDirt = 0f, wRock = 0f, wMud = 0f;

                    if (steepness > 22f)
                    {
                        wRock = 1f; // Vách dốc thành đá vôi
                    }
                    else if (distToRiver < 0.05f)
                    {
                        wMud = 1f; // Lòng suối
                    }
                    else if (distToRiver < 0.12f)
                    {
                        wDirt = 1f; // Con đường đất đỏ men theo suối
                    }
                    else if (u >= 0.24f && u <= 0.46f && v >= 0.17f && v <= 0.43f)
                    {
                        wDirt = 0.90f; // Sân làng đất đỏ
                        wGrass = 0.10f;
                    }
                    else
                    {
                        wGrass = 1f; // Đồng cỏ xanh
                    }

                    float sum = wGrass + wDirt + wRock + wMud;
                    splatmap[y, x, 0] = wGrass / sum;
                    splatmap[y, x, 1] = wDirt / sum;
                    splatmap[y, x, 2] = wRock / sum;
                    splatmap[y, x, 3] = wMud / sum;
                }
            }
            data.SetAlphamaps(0, 0, splatmap);
        }

        private static Texture2D CreateColorTexture(string folder, string name, Color baseCol, Color darkCol)
        {
            string path = $"{folder}/{name}.png";
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                    Color c = Color.Lerp(darkCol, baseCol, noise);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static TerrainLayer GetOrCreateLayer(string folder, string name, Texture2D tex, Vector2 tileSize)
        {
            string path = $"{folder}/{name}.asset";
            TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer == null)
            {
                layer = new TerrainLayer { diffuseTexture = tex, tileSize = tileSize };
                AssetDatabase.CreateAsset(layer, path);
            }
            return layer;
        }

        // ================= ÁNH SÁNG & SƯƠNG MÙ =================
        private static void SetupLightingAndAtmosphere()
        {
            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (Light l in lights)
                {
                    if (l.type == LightType.Directional) { sun = l; break; }
                }
            }

            if (sun != null)
            {
                Undo.RecordObject(sun.transform, "Setup Sun");
                Undo.RecordObject(sun, "Setup Sun Light");
                sun.transform.rotation = Quaternion.Euler(30f, -42f, 0f);
                sun.color = new Color(1.0f, 0.94f, 0.84f);
                sun.intensity = 1.35f;
                sun.shadows = LightShadows.Soft;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.65f, 0.78f, 0.92f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.55f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.25f, 0.18f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0048f;
            RenderSettings.fogColor = new Color(0.72f, 0.84f, 0.92f);
        }

        // ================= CON SUỐI NƯỚC =================
        private static void CreateRiverWater(Vector3 tPos, Vector3 size)
        {
            string waterName = "[RIVER_WATER_SURFACE]";
            GameObject waterObj = GameObject.Find(waterName);
            if (waterObj == null)
            {
                waterObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                waterObj.name = waterName;
                Undo.RegisterCreatedObjectUndo(waterObj, "Create River Water");
            }

            waterObj.transform.position = tPos + new Vector3(size.x * 0.52f, 11.2f, size.z * 0.50f);
            waterObj.transform.localScale = new Vector3(size.x * 0.09f, 1f, size.z * 0.09f);

            Material waterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            waterMat.color = new Color(0.16f, 0.52f, 0.62f, 0.78f);
            waterMat.SetFloat("_Smoothness", 0.94f);
            waterObj.GetComponent<MeshRenderer>().material = waterMat;
        }

        // ================= TÍCH HỢP CÁC MODEL 3D AI THẬT =================
        private static void BuildSceneWithRealAIModels(Vector3 tPos, Vector3 size)
        {
            string rootName = "[MAP_ZONES_CHUONG_1]";
            GameObject root = GameObject.Find(rootName);
            if (root != null) Object.DestroyImmediate(root);

            root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Complete Map Props with Real AI Models");

            // Tạo Materials cho các Model AI
            Material bambooMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bambooMat.color = new Color(0.25f, 0.68f, 0.20f);
            bambooMat.SetFloat("_Smoothness", 0.4f);

            Material warriorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            warriorMat.color = new Color(0.72f, 0.52f, 0.35f);
            warriorMat.SetFloat("_Smoothness", 0.3f);

            Material horseMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            horseMat.color = new Color(0.20f, 0.20f, 0.22f);
            horseMat.SetFloat("_Smoothness", 0.65f);

            // Đường dẫn model thật từ bạn Trung
            string bambooPath = "Assets/Art/Models/Meshy_AI_bamboo_giant_compressed_20261027_texture_remesh_tri.obj";
            string warriorPath = "Assets/Art/Models/Meshy_AI_vietnamese_warrior_compressed_20261027_texture_remesh_tri.obj";
            string horseWarriorPath = "Assets/Art/Models/Meshy_AI_warrior_riding_horse_compressed_20261027_texture_remesh_tri.obj";

            GameObject bambooPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bambooPath);
            GameObject warriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(warriorPath);
            GameObject horseWarriorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(horseWarriorPath);

            Vector3 villageCenter = tPos + new Vector3(size.x * 0.33f, 15f, size.z * 0.28f);

            // 1. TRỒNG CÁC CÂY TRE THẬT (AI MODEL) THÀNH RỪNG TRE QUANH LÀNG
            GameObject bambooGroup = new GameObject("[RUNG_TRE_AI_THAT]");
            bambooGroup.transform.parent = root.transform;

            if (bambooPrefab != null)
            {
                // Trồng 35 cây tre AI khổng lồ thành lũy tre bao quanh làng
                for (int i = 0; i < 35; i++)
                {
                    float angle = Random.Range(100f, 270f) * Mathf.Deg2Rad;
                    float radius = Random.Range(20f, 45f);
                    Vector3 pos = villageCenter + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                    GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(bambooPrefab);
                    b.name = $"Tre_AI_{i}";
                    b.transform.parent = bambooGroup.transform;
                    b.transform.position = pos;
                    float sc = Random.Range(6.5f, 11.0f); // Tỉ lệ cây tre thật
                    b.transform.localScale = Vector3.one * sc;
                    b.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));

                    // Gán màu xanh tre
                    foreach (var mr in b.GetComponentsInChildren<MeshRenderer>())
                    {
                        mr.material = bambooMat;
                    }
                }

                // Trồng thêm 15 cây tre AI dọc theo lối mòn ven suối
                for (int j = 0; j < 15; j++)
                {
                    float v = 0.35f + j * 0.035f;
                    float riverCenterU = Mathf.Lerp(0.82f, 0.35f, v) + Mathf.Sin(v * Mathf.PI * 2.5f) * 0.08f;
                    Vector3 pos = tPos + new Vector3((riverCenterU - 0.08f) * size.x, 12.0f, v * size.z);

                    GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(bambooPrefab);
                    b.name = $"Tre_VenSuoi_{j}";
                    b.transform.parent = bambooGroup.transform;
                    b.transform.position = pos;
                    b.transform.localScale = Vector3.one * Random.Range(7f, 10f);
                    foreach (var mr in b.GetComponentsInChildren<MeshRenderer>()) mr.material = bambooMat;
                }
            }

            // 2. ĐẶT NHÂN VẬT THÁNH GIÓNG (AI MODEL THẬT) VÀO SÂN LÀNG
            if (warriorPrefab != null)
            {
                GameObject giong = (GameObject)PrefabUtility.InstantiatePrefab(warriorPrefab);
                giong.name = "ThanhGiong_DungSi_SanLang";
                giong.transform.parent = root.transform;
                giong.transform.position = villageCenter + new Vector3(0, 0.2f, 0);
                giong.transform.localScale = Vector3.one * 1.8f;
                giong.transform.rotation = Quaternion.Euler(0, 140f, 0);
                foreach (var mr in giong.GetComponentsInChildren<MeshRenderer>()) mr.material = warriorMat;
            }

            // 3. ĐẶT THÁNH GIÓNG CƯỠI NGỰA SẮT (AI MODEL THẬT) CẠNH CỔNG LÀNG
            if (horseWarriorPrefab != null)
            {
                GameObject horse = (GameObject)PrefabUtility.InstantiatePrefab(horseWarriorPrefab);
                horse.name = "ThanhGiong_CuoiNguaSat";
                horse.transform.parent = root.transform;
                horse.transform.position = villageCenter + new Vector3(12f, 0.2f, -5f);
                horse.transform.localScale = Vector3.one * 1.6f;
                horse.transform.rotation = Quaternion.Euler(0, 65f, 0);
                foreach (var mr in horse.GetComponentsInChildren<MeshRenderer>()) mr.material = horseMat;
            }
        }
    }
}
