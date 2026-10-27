using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds high-fidelity East Asian / ancient Vietnamese wuxia landscape features:
    /// Soaring limestone karst peaks, pink peach blossom trees, dense swaying grass & pampas reeds,
    /// river docks, wooden sampan boats, stepping stones, ceramic jars, and wooden carts.
    /// </summary>
    public static class PhuDongAsianCultureBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string MatBase = "Assets/_Game/Art/Materials/";
        private const string KayKitBase = "Assets/ThirdParty/KayKit_Medieval_Hexagon_Pack/KayKit_Medieval_Hexagon_Pack_1.0_FREE/Assets/fbx/";

        /// <summary>
        /// Builds towering monolithic limestone karst pinnacles (Ninh Binh / Ha Long / Wuxia style).
        /// Replaces all low-poly European hex mountains with photorealistic sacred karst cliffs.
        /// </summary>
        public static void BuildSoaringKarstPillars(GameObject parent)
        {
            var root = CreateChild(parent, "DayNui_VachDaVoi_ThuyMac");
            var cliffPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Meshy_DaNui_RungThieng.prefab");
            if (cliffPrefab == null) return;

            // 1. Outer mountain ring (18 soaring karst towers soaring 55m - 95m)
            int outerCount = 18;
            float outerRadius = 82f;
            for (int i = 0; i < outerCount; i++)
            {
                float ang = i * Mathf.PI * 2f / outerCount;
                float dist = outerRadius + (i % 3) * 7f;
                Vector3 pos = new Vector3(Mathf.Cos(ang) * dist, -2f, Mathf.Sin(ang) * dist);

                var cliff = (GameObject)PrefabUtility.InstantiatePrefab(cliffPrefab, root.transform);
                cliff.name = $"DinhNui_Karst_SocSon_{i + 1}";
                cliff.transform.position = pos;
                cliff.transform.rotation = Quaternion.Euler(0f, (i * 47f) % 360f, 0f);

                // Vertical scaling creates soaring karst pillars
                float w = Random.Range(14f, 22f);
                float h = Random.Range(55f, 95f);
                cliff.transform.localScale = new Vector3(w, h, w);
            }

            // 2. Scenic River Gorge Karsts (rising dramatically right out of the river mist!)
            Vector3[] riverKarsts = {
                new Vector3(-42f, -1.5f, -15f),
                new Vector3(42f, -1.5f, -16f),
                new Vector3(-35f, -1.5f, -28f),
                new Vector3(36f, -1.5f, -28f),
                new Vector3(-28f, -1.5f, 32f),
                new Vector3(28f, -1.5f, 32f)
            };
            for (int i = 0; i < riverKarsts.Length; i++)
            {
                var k = (GameObject)PrefabUtility.InstantiatePrefab(cliffPrefab, root.transform);
                k.name = $"VachDa_BoSong_Gorge_{i + 1}";
                k.transform.position = riverKarsts[i];
                k.transform.rotation = Quaternion.Euler(0f, i * 65f, 0f);
                k.transform.localScale = new Vector3(Random.Range(10f, 15f), Random.Range(38f, 65f), Random.Range(10f, 15f));
            }
        }

        /// <summary>
        /// Builds blooming pink peach blossom trees (Cây Đào Hồng Cổ Thụ) lining the river and plaza,
        /// complete with drifting pink flower petal particles.
        /// </summary>
        public static void BuildPeachBlossomGroves(GameObject parent)
        {
            var root = CreateChild(parent, "RungHoaDao_HongTham");
            var treeBase = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_LayeredCanopy_Grand.prefab")
                        ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_Broadleaf_Oak.prefab");
            var peachMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_HoaDao_HongTham.mat");

            // Positions along the riverbank, mirroring in the water as in Reference Image 1 & 2
            Vector3[] peachPos = {
                new Vector3(-14f, 0f, -10f),
                new Vector3(-20f, 0f, -11f),
                new Vector3(14f, 0f, -10f),
                new Vector3(20f, 0f, -10f),
                new Vector3(-8f, 0f, -21f),
                new Vector3(8f, 0f, -21f),
                new Vector3(18f, 0f, -22f),
                new Vector3(6f, 0f, 22f),
                new Vector3(-12f, 0f, 23f)
            };

            for (int i = 0; i < peachPos.Length; i++)
            {
                if (treeBase == null) break;
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(treeBase, root.transform);
                tree.name = $"CayHoaDao_HongTham_{i + 1}";
                tree.transform.position = peachPos[i];
                tree.transform.rotation = Quaternion.Euler(0f, i * 53f, 0f);
                tree.transform.localScale = Vector3.one * Random.Range(1.35f, 1.85f);

                // Apply radiant pink peach blossom foliage material to tree canopies
                if (peachMat != null)
                {
                    foreach (var mr in tree.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (mr.name.Contains("Bush") || mr.name.Contains("Crown") || mr.name.Contains("Leaf") || mr.name.Contains("Canopy"))
                        {
                            mr.sharedMaterial = peachMat;
                        }
                    }
                }
            }

            // Drifting pink peach blossom petal particles (Cánh hoa đào rơi bay)
            BuildDriftingPeachPetalsVFX(root);
        }

        /// <summary>
        /// Builds rustic East Asian river dock (Bến đò ven sông) with wooden piers,
        /// moored wooden sampan boats (thuyền nan), wooden cart, ceramic jars, and fish baskets.
        /// </summary>
        public static void BuildRiverDockAndSampans(GameObject parent)
        {
            var root = CreateChild(parent, "BenDo_Cauthuyen_ThuyenNan");
            var woodMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Wood.mat");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat");

            // 1. Wooden River Pier / Jetty extending into the water (at x = -6f, z = -15.5f)
            var pier = CreateChild(root, "CauTau_Go_BenDo");
            pier.transform.position = new Vector3(-6f, 0.12f, -15.5f);

            // Pier decking
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = "San_CauTau";
            deck.transform.SetParent(pier.transform, false);
            deck.transform.localPosition = new Vector3(0f, 0f, 0f);
            deck.transform.localScale = new Vector3(4.5f, 0.12f, 7.5f);
            if (woodMat != null) deck.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Pier support timber piles
            Vector3[] pileOffsets = {
                new Vector3(-2f, -0.6f, -3.2f), new Vector3(2f, -0.6f, -3.2f),
                new Vector3(-2f, -0.6f, 0f), new Vector3(2f, -0.6f, 0f),
                new Vector3(-2f, -0.6f, 3.2f), new Vector3(2f, -0.6f, 3.2f)
            };
            for (int i = 0; i < pileOffsets.Length; i++)
            {
                var pile = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pile.name = $"CotTru_Go_{i + 1}";
                pile.transform.SetParent(pier.transform, false);
                pile.transform.localPosition = pileOffsets[i];
                pile.transform.localScale = new Vector3(0.24f, 0.7f, 0.24f);
                if (woodMat != null) pile.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // Mooring bollards on pier
            for (int i = -1; i <= 1; i += 2)
            {
                var bollard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bollard.name = $"CocNeo_Thuyen_{i}";
                bollard.transform.SetParent(pier.transform, false);
                bollard.transform.localPosition = new Vector3(2.1f, 0.3f, i * 2.8f);
                bollard.transform.localScale = new Vector3(0.18f, 0.35f, 0.18f);
                if (woodMat != null) bollard.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // 2. Authentic Wooden Sampan Boats (Thuyền nan gỗ tam bản) floating in the water
            Vector3[] boatPos = {
                new Vector3(-3.2f, 0.08f, -14.8f),
                new Vector3(-9.2f, 0.08f, -16.2f),
                new Vector3(12.5f, 0.08f, -15.2f)
            };
            float[] boatRot = { 15f, -25f, 10f };
            for (int b = 0; b < boatPos.Length; b++)
            {
                var boat = BuildWoodenSampan(root, $"ThuyenNan_Go_{b + 1}", woodMat);
                boat.transform.position = boatPos[b];
                boat.transform.rotation = Quaternion.Euler(0f, boatRot[b], 0f);
            }

            // 3. Wooden Handcart (Xe kéo gỗ như ảnh 1)
            BuildWoodenHandcart(root, new Vector3(-8.5f, 0.15f, -11.5f), woodMat);

            // 4. Ceramic Jars & Fish Baskets along the riverbank (Chum vại sành & giỏ cá)
            BuildPotteryAndBaskets(root, stoneMat, woodMat);
        }

        /// <summary>
        /// Builds round carved stepping stones across the lotus pond (Đá dặm bước như ảnh 2 & 3).
        /// </summary>
        public static void BuildLotusSteppingStones(GameObject parent)
        {
            var root = CreateChild(parent, "DaDam_BuocQuaAoSen");
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Sacred_Mountain_Stone.mat");

            // S-curve stepping path across the shallow lotus pond
            Vector3[] stonePts = {
                new Vector3(15f, 0.14f, -13.5f),
                new Vector3(17f, 0.14f, -14.2f),
                new Vector3(19f, 0.14f, -15.0f),
                new Vector3(21.5f, 0.14f, -15.5f),
                new Vector3(24f, 0.14f, -15.0f),
                new Vector3(26f, 0.14f, -14.2f),
                new Vector3(28f, 0.14f, -13.5f)
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
        /// Generates dense 3D swaying grass blades and tall white pampas reeds (Cỏ lau trắng).
        /// Gives the volumetric grass field aesthetic seen in Nghịch Thủy Hàn reference image 4.
        /// </summary>
        public static void BuildDenseVegetationAndPampas(GameObject parent)
        {
            var root = CreateChild(parent, "ThamCoLau_Trang_DayDac");
            var grassMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "OpenWorld/ZeldaWindGrass.mat");
            var pampasMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Foliage/Mat_CoLau_Trang.mat");
            var reedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/nature/waterplant_A.fbx");

            // 1. Clustered Pampas Reeds (Bụi Cỏ Lau Trắng nở rộ ven bờ sông và bờ ruộng)
            Vector3[] pampasPos = {
                new Vector3(-4f, 0.08f, -11.5f), new Vector3(-11f, 0.08f, -11.5f),
                new Vector3(-16f, 0.08f, -11.0f), new Vector3(4f, 0.08f, -11.5f),
                new Vector3(11f, 0.08f, -11.5f), new Vector3(17f, 0.08f, -11.0f),
                new Vector3(-5f, 0.08f, -19.5f), new Vector3(-12f, 0.08f, -19.5f),
                new Vector3(5f, 0.08f, -19.5f), new Vector3(12f, 0.08f, -19.5f),
                new Vector3(21f, 0.08f, 4.5f), new Vector3(25f, 0.08f, 5.0f),
                new Vector3(29f, 0.08f, 5.5f), new Vector3(-18f, 0.08f, 5.0f)
            };

            for (int i = 0; i < pampasPos.Length; i++)
            {
                if (reedAsset == null) break;
                var reed = (GameObject)PrefabUtility.InstantiatePrefab(reedAsset, root.transform);
                reed.name = $"BuiCoLau_Trang_{i + 1}";
                reed.transform.position = pampasPos[i];
                reed.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                reed.transform.localScale = new Vector3(1.6f, Random.Range(2.4f, 3.5f), 1.6f); // Tall swaying reeds!
                if (pampasMat != null) SetMatRecursive(reed, pampasMat);
            }

            // 2. Procedural Batched 3D Grass Cover Mesh (1,400 blades covering the meadow)
            BuildBatchedGrassMesh(root, grassMat);
        }

        /// <summary>
        /// Builds authentic ancient Vietnamese stone well with bamboo crane under the Banyan tree.
        /// Replaces the blue European well with a cultural Vietnamese Giếng Ngọc.
        /// </summary>
        public static void BuildAncientStoneWell(GameObject parent)
        {
            var root = CreateChild(parent, "GiengNgoc_CoDai_LangGiong");
            root.transform.position = new Vector3(-13.5f, 0f, 6.5f);

            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Stone.mat")
                        ?? AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Sacred_Mountain_Stone.mat");
            var woodMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "World/Wood.mat");
            var waterMat = AssetDatabase.LoadAssetAtPath<Material>(MatBase + "Meshy/Environment/Mat_DongSong_NuocBiec.mat");

            // Ancient circular stone well rim (Thành giếng đá cổ)
            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "ThanhGieng_DaCo";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            rim.transform.localScale = new Vector3(2.8f, 0.45f, 2.8f);
            if (stoneMat != null) rim.GetComponent<Renderer>().sharedMaterial = stoneMat;

            // Well water interior (Mặt nước giếng phản chiếu)
            var wellWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellWater.name = "MatNuoc_Gieng";
            wellWater.transform.SetParent(root.transform, false);
            wellWater.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            wellWater.transform.localScale = new Vector3(2.3f, 0.05f, 2.3f);
            var col = wellWater.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            if (waterMat != null) wellWater.GetComponent<Renderer>().sharedMaterial = waterMat;

            // Bamboo/timber gallows for bucket (Cần múc nước bằng tre)
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

            // Wooden bucket
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

            // Hull floor
            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "DayThuyen";
            hull.transform.SetParent(boat.transform, false);
            hull.transform.localPosition = Vector3.zero;
            hull.transform.localScale = new Vector3(1.35f, 0.15f, 4.6f);
            if (woodMat != null) hull.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Bow & stern tapered points
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

            // Gunwales (mạn thuyền)
            for (int s = -1; s <= 1; s += 2)
            {
                var gunwale = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gunwale.name = $"ManThuyen_{s}";
                gunwale.transform.SetParent(boat.transform, false);
                gunwale.transform.localPosition = new Vector3(s * 0.68f, 0.22f, 0f);
                gunwale.transform.localScale = new Vector3(0.12f, 0.35f, 4.5f);
                if (woodMat != null) gunwale.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // Wooden Oar (Mái chèo)
            var oar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            oar.name = "MaiCheo";
            oar.transform.SetParent(boat.transform, false);
            oar.transform.localPosition = new Vector3(0.55f, 0.45f, -0.6f);
            oar.transform.localRotation = Quaternion.Euler(35f, 25f, -20f);
            oar.transform.localScale = new Vector3(0.06f, 1.5f, 0.06f);
            if (woodMat != null) oar.GetComponent<Renderer>().sharedMaterial = woodMat;

            return boat;
        }

        private static void BuildWoodenHandcart(GameObject parent, Vector3 pos, Material woodMat)
        {
            var cart = new GameObject("XeKeo_Go_Co");
            cart.transform.SetParent(parent.transform, false);
            cart.transform.position = pos;
            cart.transform.rotation = Quaternion.Euler(0f, -30f, 0f);

            // Bed of cart
            var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "ThungXe";
            bed.transform.SetParent(cart.transform, false);
            bed.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            bed.transform.localScale = new Vector3(1.6f, 0.15f, 2.4f);
            if (woodMat != null) bed.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Spoked wheels
            for (int w = -1; w <= 1; w += 2)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"BanhXe_{w}";
                wheel.transform.SetParent(cart.transform, false);
                wheel.transform.localPosition = new Vector3(w * 0.95f, 0.42f, 0f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.localScale = new Vector3(0.85f, 0.12f, 0.85f);
                if (woodMat != null) wheel.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // Pull shafts (Càng xe)
            for (int s = -1; s <= 1; s += 2)
            {
                var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shaft.name = $"CangXe_{s}";
                shaft.transform.SetParent(cart.transform, false);
                shaft.transform.localPosition = new Vector3(s * 0.55f, 0.32f, 1.8f);
                shaft.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
                shaft.transform.localScale = new Vector3(0.1f, 0.1f, 1.8f);
                if (woodMat != null) shaft.GetComponent<Renderer>().sharedMaterial = woodMat;
            }
        }

        private static void BuildPotteryAndBaskets(GameObject parent, Material stoneMat, Material woodMat)
        {
            Vector3[] jarPos = {
                new Vector3(-4.5f, 0.25f, -12.2f),
                new Vector3(-4.8f, 0.2f, -12.8f),
                new Vector3(4.5f, 0.25f, -12.5f),
                new Vector3(-7f, 0.25f, 19.5f),
                new Vector3(-7.5f, 0.2f, 20f)
            };

            for (int i = 0; i < jarPos.Length; i++)
            {
                var jar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jar.name = $"ChumSanh_Nuoc_{i + 1}";
                jar.transform.SetParent(parent.transform, false);
                jar.transform.position = jarPos[i];
                jar.transform.localScale = new Vector3(0.65f, 0.85f, 0.65f); // Earthenware urn shape
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
            main.maxParticles = 120;

            // Vibrant soft pink flower petal gradient
            Color pinkA = new Color(1.0f, 0.65f, 0.78f, 0.85f);
            Color pinkB = new Color(0.98f, 0.48f, 0.68f, 0.80f);
            main.startColor = new ParticleSystem.MinMaxGradient(pinkA, pinkB);

            var emission = ps.emission;
            emission.rateOverTime = 14f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(70f, 4f, 60f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            vel.y = new ParticleSystem.MinMaxCurve(-0.25f, -0.6f); // gentle downward flutter
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

        private static void BuildBatchedGrassMesh(GameObject parent, Material grassMat)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var tris = new List<int>();

            int blades = 900;
            var rng = new System.Random(42);

            for (int i = 0; i < blades; i++)
            {
                float x = (float)(rng.NextDouble() * 70.0 - 35.0);
                float z = (float)(rng.NextDouble() * 70.0 - 35.0);

                // Skip river bed and village road
                if (z >= -19.5f && z <= -11.5f) continue;
                if (Mathf.Abs(x) < 3.2f && z >= -24f && z <= 24f) continue;

                float h = 0.45f + (float)rng.NextDouble() * 0.4f;
                float w = 0.12f;
                Vector3 p = new Vector3(x, 0.05f, z);

                int v = verts.Count;
                verts.Add(p + new Vector3(-w, 0f, 0f));
                verts.Add(p + new Vector3(w, 0f, 0f));
                verts.Add(p + new Vector3(0f, h, 0.08f));

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(0.5f, 1f));

                Color tipCol = (i % 7 == 0) ? new Color(1.0f, 0.82f, 0.90f) : new Color(0.72f, 0.88f, 0.45f);
                colors.Add(new Color(0.35f, 0.58f, 0.22f));
                colors.Add(new Color(0.35f, 0.58f, 0.22f));
                colors.Add(tipCol);

                tris.Add(v); tris.Add(v + 1); tris.Add(v + 2);
                tris.Add(v + 2); tris.Add(v + 1); tris.Add(v); // double sided
            }

            var mesh = new Mesh { name = "GrassBlades_Batched" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();

            var go = new GameObject("ThamCo_3D_GiongSong");
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
