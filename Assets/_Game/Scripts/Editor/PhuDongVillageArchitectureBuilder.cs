using UnityEditor;
using UnityEngine;

namespace ThanhGiong.Editor
{
    /// <summary>
    /// Builds village stilt houses, authentic East Asian communal drum pavilion, ancient stone well,
    /// timber pasture, bamboo groves, and living Vietnamese villagers.
    /// Eliminates all primitive placeholder capsules and outdated Meshy PRUDemo models.
    /// </summary>
    public static class PhuDongVillageArchitectureBuilder
    {
        private const string PrefabBase = "Assets/_Game/Prefabs/Environment/";
        private const string AsianBase = "Assets/_Game/Art/AsianCulture/";
        private const string VillagerBase = "Assets/_Game/Art/Villagers/Rigged/";
        private const string MountBase = "Assets/_Game/Prefabs/Mounts/";

        public static void BuildStiltVillageAndFortress(GameObject env)
        {
            var root = CreateChild(env, "KhuLang_NhaSan_ChienLuy");
            var housePrefab = LoadPrefab("Meshy_NhaTranh_LangGiong.prefab");
            var fenceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "lunar-year_Fence01.glb");
            var lampAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "lunar-year_OutterLamp.glb");

            // 1. Stilt houses cluster framing the northern clearing
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

            // 2. Fortified Gate & Watchtowers
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

            // 3. Authentic East Asian Communal Drum Pavilion & Bell Tower
            BuildCommunalDrumPavilion(root);

            // 4. Outdoor traditional wooden lanterns along the main village avenue
            if (lampAsset != null)
            {
                float[] lampZ = { 16f, 6f, -4f, -12f };
                for (int i = 0; i < lampZ.Length; i++)
                {
                    var lL = (GameObject)PrefabUtility.InstantiatePrefab(lampAsset, root.transform);
                    lL.name = $"DenLong_Go_Tay_{i + 1}";
                    lL.transform.position = new Vector3(-4.8f, 0f, lampZ[i]);
                    lL.transform.localScale = Vector3.one * 1.25f;

                    var lR = (GameObject)PrefabUtility.InstantiatePrefab(lampAsset, root.transform);
                    lR.name = $"DenLong_Go_Dong_{i + 1}";
                    lR.transform.position = new Vector3(4.8f, 0f, lampZ[i]);
                    lR.transform.localScale = Vector3.one * 1.25f;
                }
            }

            // 5. Traditional wooden fences along village homesteads
            if (fenceAsset != null)
            {
                Vector3[] fPos = { new Vector3(-9f, 0f, 19f), new Vector3(8.5f, 0f, 21f), new Vector3(-14f, 0f, 24f) };
                for (int i = 0; i < fPos.Length; i++)
                {
                    var f = (GameObject)PrefabUtility.InstantiatePrefab(fenceAsset, root.transform);
                    f.name = $"HangRao_Go_{i + 1}";
                    f.transform.position = fPos[i];
                    f.transform.localScale = Vector3.one * 1.4f;
                }
            }
        }

        private static void BuildCommunalDrumPavilion(GameObject parent)
        {
            var pavilion = CreateChild(parent, "DinhLang_SanDinh_PhuDong");
            pavilion.transform.position = new Vector3(6.5f, 0f, 14f);
            pavilion.transform.rotation = Quaternion.Euler(0f, -25f, 0f);

            // Authentic Vietnamese Communal Courtyard with Sacred Dong Son Bronze Drum
            var sanDinhPrefab = LoadPrefab("Meshy_SanDinh_TrongDong.prefab");
            if (sanDinhPrefab != null)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(sanDinhPrefab, pavilion.transform);
                s.name = "SanDinh_TrongDong_PhuDong";
                s.transform.localPosition = Vector3.zero;
                s.transform.localScale = Vector3.one * 1.1f;
            }
        }

        public static void BuildBanyanStoneWell(GameObject env)
        {
            var root = CreateChild(env, "CayDa_GiengNuoc_CoDai");
            root.transform.position = new Vector3(-16f, 0f, 8f);

            // Magnificent Vietnamese Ancient Banyan Tree shading the well
            var banyanPrefab = LoadPrefab("Meshy_CayDaCoThu_LangViet.prefab")
                            ?? LoadPrefab("Meshy/Environment/Meshy_CayDaCoThu_LangViet.prefab")
                            ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBase + "Foliage/TreeCluster_Broadleaf_Oak.prefab");
            if (banyanPrefab != null)
            {
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(banyanPrefab, root.transform);
                tree.name = "CayDaCoThu_BongMat";
                tree.transform.localPosition = new Vector3(-2.0f, 0f, 2.5f);
                tree.transform.localScale = Vector3.one * 1.85f;
            }

            // Authentic Vietnamese Ancient Stone Well (Giếng Ngọc Làng Phù Đổng)
            PhuDongAsianCultureBuilder.BuildAncientStoneWell(root);
        }

        public static void BuildPastureAndMounts(GameObject env)
        {
            var root = CreateChild(env, "ChuongNgua_DongCo_LangViet");
            root.transform.position = new Vector3(10f, 0f, 2f);

            var fenceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AsianBase + "lunar-year_Fence01.glb");
            if (fenceAsset != null)
            {
                Vector3[] fPos = {
                    new Vector3(-4f, 0f, -3.5f), new Vector3(0f, 0f, -3.5f), new Vector3(4f, 0f, -3.5f),
                    new Vector3(4f, 0f, 3.5f), new Vector3(0f, 0f, 3.5f), new Vector3(-4f, 0f, 3.5f)
                };
                for (int i = 0; i < fPos.Length; i++)
                {
                    var f = (GameObject)PrefabUtility.InstantiatePrefab(fenceAsset, root.transform);
                    f.name = $"RaoGo_Chuong_{i + 1}";
                    f.transform.localPosition = fPos[i];
                    f.transform.localScale = Vector3.one * 1.25f;
                }
            }

            // Real 3D Grazing War Horse in pasture
            var horsePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MountBase + "KayKit_Horse_Modular.prefab");
            if (horsePrefab != null)
            {
                var horse = (GameObject)PrefabUtility.InstantiatePrefab(horsePrefab, root.transform);
                horse.name = "NguaChien_LangGiong_GamCo";
                horse.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                horse.transform.localRotation = Quaternion.Euler(0f, -50f, 0f);
                horse.transform.localScale = Vector3.one * 1.55f;
            }
        }

        public static void BuildBambooGroves(GameObject env)
        {
            // Dense living bamboo hedge walls & sacred Phase 3 thicket (Reference Image 2)
            PhuDongAsianCultureBuilder.BuildDenseBambooWallAndGroves(env);
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

            // Fisherman standing right on the river pier
            var fisherAsset = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerBase + "Villager_Fisherman_Rigged.prefab");
            if (fisherAsset != null)
            {
                var fisher = (GameObject)PrefabUtility.InstantiatePrefab(fisherAsset, root.transform);
                fisher.name = "NguDan_BenDo";
                fisher.transform.position = new Vector3(-1.5f, 0.08f, -13.2f);
                fisher.transform.rotation = Quaternion.Euler(0f, 170f, 0f);
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
