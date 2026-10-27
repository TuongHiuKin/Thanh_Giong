using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds village stilt houses, communal yard, ancient stone well, iron forge, bamboo groves,
    /// and living Vietnamese villagers.
    /// </summary>
    public static class PhuDongVillageArchitectureBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string KayKitBase = "Assets/ThirdParty/KayKit_Medieval_Hexagon_Pack/KayKit_Medieval_Hexagon_Pack_1.0_FREE/Assets/fbx/";
        private const string VillagerBase = "Assets/_Game/Art/Villagers/Rigged/";
        private const string MountBase = "Assets/_Game/Prefabs/Mounts/";

        public static void BuildStiltVillageAndFortress(GameObject env)
        {
            var root = CreateChild(env, "KhuLang_NhaSan_ChienLuy");
            var housePrefab = LoadPrefab("Meshy_NhaTranh_LangGiong.prefab");

            // Stilt houses cluster framing the northern clearing
            Vector3[] housePos = { new Vector3(-3f, 0f, 25f), new Vector3(13f, 0f, 27f), new Vector3(-17f, 0f, 21f) };
            float[] houseRot = { 180f, 160f, 195f };
            if (housePrefab != null)
            {
                for (int i = 0; i < housePos.Length; i++)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(housePrefab, root.transform);
                    h.name = $"NhaSan_VietCo_{i + 1}";
                    h.transform.position = housePos[i];
                    h.transform.rotation = Quaternion.Euler(0f, houseRot[i], 0f);
                    h.transform.localScale = Vector3.one * (1.15f + (i == 0 ? 0.15f : 0f));
                }
            }

            // Fortified Gate & Watchtowers
            var gatePrefab = LoadPrefab("Meshy_CongLang_ChienLuy.prefab");
            if (gatePrefab != null)
            {
                var gate = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab, root.transform);
                gate.name = "CongLang_ChienLuy";
                gate.transform.position = new Vector3(0f, 0f, 33f);
                gate.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var towerPrefab = LoadPrefab("Meshy_ThapCanh_DoanThanh.prefab");
            if (towerPrefab != null)
            {
                var t1 = (GameObject)PrefabUtility.InstantiatePrefab(towerPrefab, root.transform);
                t1.name = "ThapCanh_Tay";
                t1.transform.position = new Vector3(-12f, 0f, 32f);

                var t2 = (GameObject)PrefabUtility.InstantiatePrefab(towerPrefab, root.transform);
                t2.name = "ThapCanh_Dong";
                t2.transform.position = new Vector3(12f, 0f, 32f);
            }

            // Dong Son Bronze Drum Communal Plaza
            var plazaPrefab = LoadPrefab("Meshy_SanDinh_TrongDong.prefab");
            if (plazaPrefab != null)
            {
                var plaza = (GameObject)PrefabUtility.InstantiatePrefab(plazaPrefab, root.transform);
                plaza.name = "SanDinh_TrongDong_DongSon";
                plaza.transform.position = new Vector3(4f, 0.02f, 14f);
            }

            // Ceremonial War Flags along the main path
            var flagPrefab = LoadPrefab("Meshy_CoLenh_PhuDong.prefab");
            if (flagPrefab != null)
            {
                float[] flagZ = { 18f, 6f, -6f, -22f };
                for (int i = 0; i < flagZ.Length; i++)
                {
                    var fL = (GameObject)PrefabUtility.InstantiatePrefab(flagPrefab, root.transform);
                    fL.name = $"CoLenh_Tay_{i + 1}";
                    fL.transform.position = new Vector3(-6.2f, 0f, flagZ[i]);
                    fL.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                    var fR = (GameObject)PrefabUtility.InstantiatePrefab(flagPrefab, root.transform);
                    fR.name = $"CoLenh_Dong_{i + 1}";
                    fR.transform.position = new Vector3(6.2f, 0f, flagZ[i]);
                    fR.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                }
            }

            // Food Offering Basket
            var foodPrefab = LoadPrefab("Meshy_MamLuongThuc_LangGiong.prefab");
            if (foodPrefab != null)
            {
                var food = (GameObject)PrefabUtility.InstantiatePrefab(foodPrefab, root.transform);
                food.name = "MamCom_GomNuoiGiong";
                food.transform.position = new Vector3(-6f, 0f, 18f);
                food.transform.localScale = Vector3.one * 1.35f;
            }

            // Village storage props: barrels, crates, sacks
            var sackAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/props/sack.fbx");
            var barrelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/props/barrel.fbx");
            var crateAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBase + "decoration/props/crate_A_big.fbx");

            if (sackAsset != null)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(sackAsset, root.transform);
                s.transform.position = new Vector3(-8.2f, 0f, 21.5f);
                s.transform.localScale = Vector3.one * 2.2f;
            }
            if (barrelAsset != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(barrelAsset, root.transform);
                b.transform.position = new Vector3(-7f, 0f, 22.5f);
                b.transform.localScale = Vector3.one * 2.2f;
            }
            if (crateAsset != null)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(crateAsset, root.transform);
                c.transform.position = new Vector3(8.5f, 0f, 23.5f);
                c.transform.localScale = Vector3.one * 1.8f;
            }
        }

        public static void BuildBanyanStoneWell(GameObject env)
        {
            var root = CreateChild(env, "CayDa_GiengNuoc_CoDai");
            root.transform.position = new Vector3(-17f, 0f, 8f);

            // Grand Ancient Banyan Tree (Cây Đa Cổ Thụ Ngàn Năm)
            var banyanPrefab = LoadPrefab("Meshy_CayDaCoThu_LangViet.prefab");
            if (banyanPrefab != null)
            {
                var banyan = (GameObject)PrefabUtility.InstantiatePrefab(banyanPrefab, root.transform);
                banyan.name = "CayDaCoThu_LangViet";
                banyan.transform.localPosition = new Vector3(-2f, 0f, 2f);
                banyan.transform.localScale = Vector3.one * 1.3f;
            }

            // Authentic Vietnamese Ancient Stone Well (Giếng Ngọc Làng Phù Đổng - No European well!)
            PhuDongAsianCultureBuilder.BuildAncientStoneWell(root);
        }

        public static void BuildPastureAndMounts(GameObject env)
        {
            var root = CreateChild(env, "ChuongNgua_DongCo_LangViet");
            root.transform.position = new Vector3(10f, 0f, 2f);

            // Rustic timber posts corral (Rào cọc gỗ mộc mạc làng quê)
            var woodMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/World/Wood.mat");
            Vector3[] postPos = {
                new Vector3(-4f, 0f, -4f), new Vector3(0f, 0f, -4f), new Vector3(4f, 0f, -4f),
                new Vector3(4f, 0f, 0f), new Vector3(4f, 0f, 4f),
                new Vector3(0f, 0f, 4f), new Vector3(-4f, 0f, 4f), new Vector3(-4f, 0f, 0f)
            };
            for (int i = 0; i < postPos.Length; i++)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"CocRao_Go_{i + 1}";
                post.transform.SetParent(root.transform, false);
                post.transform.localPosition = postPos[i] + new Vector3(0f, 0.75f, 0f);
                post.transform.localScale = new Vector3(0.18f, 0.75f, 0.18f);
                if (woodMat != null) post.GetComponent<Renderer>().sharedMaterial = woodMat;
            }

            // Real 3D Grazing Mount in pasture (Ngựa chiến gặm cỏ)
            var horsePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MountBase + "KayKit_Horse_Modular.prefab");
            if (horsePrefab != null)
            {
                var horse = (GameObject)PrefabUtility.InstantiatePrefab(horsePrefab, root.transform);
                horse.name = "NguaChien_LangGiong_GamCo";
                horse.transform.localPosition = new Vector3(0f, 0f, 0.5f);
                horse.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
                horse.transform.localScale = Vector3.one * 1.55f;
            }
        }

        public static void BuildBambooGroves(GameObject env)
        {
            var root = CreateChild(env, "RungTre_LuyLang_PhuDong");
            var bambooPrefab = LoadPrefab("Meshy_KhomTreNga_ThanThanh.prefab");
            var foliagePrefab = LoadPrefab("Foliage/BambooGrove_Vietnamese_Legend.prefab");

            Vector3[] pos = {
                new Vector3(-24f, 0f, -16f), new Vector3(-26f, 0f, -8f),
                new Vector3(-25f, 0f, 0f), new Vector3(-23f, 0f, 8f),
                new Vector3(-19f, 0f, 19f), new Vector3(-11f, 0f, -25f),
                new Vector3(0f, 0f, -27f), new Vector3(11f, 0f, -27f),
                new Vector3(23f, 0f, -23f), new Vector3(28f, 0f, -10f)
            };

            for (int i = 0; i < pos.Length; i++)
            {
                var asset = (i % 2 == 0 && bambooPrefab != null) ? bambooPrefab : foliagePrefab;
                if (asset == null) continue;
                var b = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                b.name = $"RangTre_PhuDong_{i + 1}";
                b.transform.position = pos[i];
                b.transform.rotation = Quaternion.Euler(0f, i * 42f, 0f);
                b.transform.localScale = Vector3.one * (1.2f + (i % 3) * 0.2f);
            }

            // Sacred Bamboo Grove for Phase 3
            var sacred = CreateChild(root, "KhomTre_LinhThieng_Phase3");
            sacred.transform.position = new Vector3(-18f, 0f, -4f);
            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "HaoQuang_TreThan";
            glow.transform.SetParent(sacred.transform, false);
            glow.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            glow.transform.localScale = Vector3.one * 0.7f;
            var auraMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/Characters/SelectionRing.mat");
            if (auraMat != null) glow.GetComponent<Renderer>().sharedMaterial = auraMat;

            // Falling bamboo leaves VFX
            var pfxPrefab = LoadPrefab("Foliage/PFX_Forest_DriftingLeaves.prefab");
            if (pfxPrefab != null)
            {
                var pfx = (GameObject)PrefabUtility.InstantiatePrefab(pfxPrefab, root.transform);
                pfx.name = "VFX_LaTre_RoiBay";
                pfx.transform.position = new Vector3(-17f, 3.5f, -4f);
            }
        }

        public static void BuildIronForge(GameObject env)
        {
            var forgePrefab = LoadPrefab("Meshy_LoRenThep_VanLang.prefab");
            if (forgePrefab != null)
            {
                var forge = (GameObject)PrefabUtility.InstantiatePrefab(forgePrefab, env.transform);
                forge.name = "LoRenThep_PhuDong";
                forge.transform.position = new Vector3(9f, 0f, -14f);
                forge.transform.rotation = Quaternion.Euler(0f, -40f, 0f);
            }
        }

        public static void BuildLivingVillagers(GameObject env)
        {
            var root = CreateChild(env, "DanLang_VanLang_NhanVat");

            // Farmer by the rice field
            var farmerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Farmer_Tex_Rigged.prefab");
            if (farmerAsset != null)
            {
                var farmer = (GameObject)PrefabUtility.InstantiatePrefab(farmerAsset, root.transform);
                farmer.name = "BacNongDan_BoRuong";
                farmer.transform.position = new Vector3(18f, 0.42f, 8f);
                farmer.transform.rotation = Quaternion.Euler(0f, -70f, 0f);
            }

            // Village Elder under the banyan tree
            var elderAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Elder_Rigged.prefab");
            if (elderAsset != null)
            {
                var elder = (GameObject)PrefabUtility.InstantiatePrefab(elderAsset, root.transform);
                elder.name = "CuGia_TruongLang_GocDa";
                elder.transform.position = new Vector3(-14f, 0f, 7f);
                elder.transform.rotation = Quaternion.Euler(0f, 130f, 0f);
            }

            // Blacksmith at the forge
            var smithAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Blacksmith_Rigged.prefab");
            if (smithAsset != null)
            {
                var smith = (GameObject)PrefabUtility.InstantiatePrefab(smithAsset, root.transform);
                smith.name = "ThoRen_PhuDong";
                smith.transform.position = new Vector3(8.5f, 0f, -12.5f);
                smith.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
            }

            // Mother and Child by the stilt house
            var womanAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Woman_Rigged.prefab");
            var childAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Child_Rigged.prefab");
            if (womanAsset != null)
            {
                var woman = (GameObject)PrefabUtility.InstantiatePrefab(womanAsset, root.transform);
                woman.name = "MeGiong_NhaSan";
                woman.transform.position = new Vector3(-4.5f, 0f, 19f);
                woman.transform.rotation = Quaternion.Euler(0f, 150f, 0f);
            }
            if (childAsset != null)
            {
                var child = (GameObject)PrefabUtility.InstantiatePrefab(childAsset, root.transform);
                child.name = "DuaTre_LangGiong";
                child.transform.position = new Vector3(-3.2f, 0f, 18.5f);
                child.transform.rotation = Quaternion.Euler(0f, 120f, 0f);
            }

            // Fisherman near the river pier
            var fisherAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Fisherman_Rigged.prefab");
            if (fisherAsset != null)
            {
                var fisher = (GameObject)PrefabUtility.InstantiatePrefab(fisherAsset, root.transform);
                fisher.name = "NguDan_BenDo";
                fisher.transform.position = new Vector3(-4.2f, 0.12f, -14.2f); // Standing right on the wooden pier!
                fisher.transform.rotation = Quaternion.Euler(0f, 160f, 0f);
            }
        }

        private static GameObject LoadPrefab(string relPath) => AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + relPath);

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }
    }
}
