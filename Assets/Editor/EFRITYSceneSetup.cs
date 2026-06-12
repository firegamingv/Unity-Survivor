using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.IO;

/// <summary>
/// Script Editor — configure automatiquement la scène EFRITY.
/// Une fois compilé, va dans le menu Unity : EFRITY → Setup Scène
/// </summary>
public static class EFRITYSceneSetup
{
    // ─── Entry Point ──────────────────────────────────────────────────────────
    [MenuItem("EFRITY/🎮 Setup Scène Automatique")]
    public static void SetupScene()
    {
        SetupManagers();
        SetupPlayer();
        SetupCanvas();
        SetupCamera();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[EFRITY] ✅ Setup scène terminé !");

        EditorUtility.DisplayDialog("EFRITY Setup ✅",
            "Hiérarchie créée !\n\n" +
            "Reste à faire dans l'Inspector :\n\n" +
            "1. Créer GameConfig SO → assigner sur GameManager\n" +
            "2. Créer WeaponData + ProjectileData SOs\n" +
            "   → assigner sur WeaponSystem du Player\n" +
            "3. Créer EnemyData_Bits SO\n" +
            "   → ajouter dans EnemyManager > Phases\n" +
            "4. Créer les 3 Prefabs (Projectile, Bits, XPOrb)\n" +
            "5. Glisser le sprite Evil Wizard sur Player > Sprite",
            "C'est noté !");
    }

    // ─── UpgradeCard Prefab ───────────────────────────────────────────────────
    [MenuItem("EFRITY/🃏 Créer Prefab UpgradeCard")]
    public static void CreateUpgradeCardPrefab()
    {
        EnsureFolder("Assets/Prefabs/UI");

        // ── Root — Button + Image + UpgradeCardUI ─────────────────────────────
        var root = new GameObject("UpgradeCard", typeof(RectTransform));
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(260f, 380f);

        var rootBg = root.AddComponent<Image>();
        rootBg.color = new Color(0.12f, 0.12f, 0.18f); // fond sombre

        var btn = root.AddComponent<Button>();
        btn.targetGraphic = rootBg;

        // Couleur hover
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f);
        colors.pressedColor     = new Color(0.08f, 0.08f, 0.12f);
        btn.colors = colors;

        var cardUI = root.AddComponent<UpgradeCardUI>();

        // ── Bordure colorée (rareté) ──────────────────────────────────────────
        var borderGO = new GameObject("Border", typeof(RectTransform));
        borderGO.transform.SetParent(root.transform, false);
        var borderRT = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero; borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-3f, -3f); borderRT.offsetMax = new Vector2(3f, 3f);
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(0.54f, 0.54f, 0.54f); // Common par défaut
        borderImg.raycastTarget = false;
        // Met la bordure derrière le fond
        borderGO.transform.SetAsFirstSibling();

        // ── Icône ─────────────────────────────────────────────────────────────
        var iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(root.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.5f, 1f); iconRT.anchorMax = new Vector2(0.5f, 1f);
        iconRT.pivot     = new Vector2(0.5f, 1f);
        iconRT.anchoredPosition = new Vector2(0f, -20f);
        iconRT.sizeDelta = new Vector2(100f, 100f);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        iconImg.preserveAspect = true;

        // ── Rareté ────────────────────────────────────────────────────────────
        var rarityGO = new GameObject("TXT_Rarity", typeof(RectTransform));
        rarityGO.transform.SetParent(root.transform, false);
        var rarityRT = rarityGO.GetComponent<RectTransform>();
        rarityRT.anchorMin = new Vector2(0f, 1f); rarityRT.anchorMax = new Vector2(1f, 1f);
        rarityRT.pivot     = new Vector2(0.5f, 1f);
        rarityRT.anchoredPosition = new Vector2(0f, -130f);
        rarityRT.sizeDelta = new Vector2(0f, 28f);
        var rarityTMP = rarityGO.AddComponent<TextMeshProUGUI>();
        rarityTMP.text = "Common"; rarityTMP.fontSize = 14;
        rarityTMP.fontStyle = FontStyles.Bold;
        rarityTMP.color = new Color(0.54f, 0.54f, 0.54f);
        rarityTMP.alignment = TextAlignmentOptions.Center;

        // ── Nom ───────────────────────────────────────────────────────────────
        var nameGO = new GameObject("TXT_Name", typeof(RectTransform));
        nameGO.transform.SetParent(root.transform, false);
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 1f); nameRT.anchorMax = new Vector2(1f, 1f);
        nameRT.pivot     = new Vector2(0.5f, 1f);
        nameRT.anchoredPosition = new Vector2(0f, -168f);
        nameRT.sizeDelta = new Vector2(-20f, 40f);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Nom Upgrade"; nameTMP.fontSize = 20;
        nameTMP.fontStyle = FontStyles.Bold; nameTMP.color = Color.white;
        nameTMP.alignment = TextAlignmentOptions.Center;

        // ── Description ───────────────────────────────────────────────────────
        var descGO = new GameObject("TXT_Description", typeof(RectTransform));
        descGO.transform.SetParent(root.transform, false);
        var descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0f, 0f); descRT.anchorMax = new Vector2(1f, 1f);
        descRT.pivot     = new Vector2(0.5f, 0.5f);
        descRT.offsetMin = new Vector2(15f, 50f); descRT.offsetMax = new Vector2(-15f, -220f);
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.text = "Description de l'upgrade."; descTMP.fontSize = 15;
        descTMP.color = new Color(0.85f, 0.85f, 0.85f);
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.textWrappingMode = TextWrappingModes.Normal;

        // ── Bouton "Choisir" ──────────────────────────────────────────────────
        var chooseGO = new GameObject("BTN_Choose_BG", typeof(RectTransform));
        chooseGO.transform.SetParent(root.transform, false);
        var chooseRT = chooseGO.GetComponent<RectTransform>();
        chooseRT.anchorMin = new Vector2(0.1f, 0f); chooseRT.anchorMax = new Vector2(0.9f, 0f);
        chooseRT.pivot     = new Vector2(0.5f, 0f);
        chooseRT.anchoredPosition = new Vector2(0f, 15f);
        chooseRT.sizeDelta = new Vector2(0f, 38f);
        var chooseBg = chooseGO.AddComponent<Image>();
        chooseBg.color = new Color(0.18f, 0.45f, 0.18f);
        chooseBg.raycastTarget = false;
        var chooseLabel = new GameObject("Label", typeof(RectTransform));
        chooseLabel.transform.SetParent(chooseGO.transform, false);
        var chooseLabelRT = chooseLabel.GetComponent<RectTransform>();
        chooseLabelRT.anchorMin = Vector2.zero; chooseLabelRT.anchorMax = Vector2.one;
        chooseLabelRT.offsetMin = Vector2.zero; chooseLabelRT.offsetMax = Vector2.zero;
        var chooseTMP = chooseLabel.AddComponent<TextMeshProUGUI>();
        chooseTMP.text = "Choisir"; chooseTMP.fontSize = 17;
        chooseTMP.color = Color.white; chooseTMP.alignment = TextAlignmentOptions.Center;

        // ── Câblage UpgradeCardUI ─────────────────────────────────────────────
        Wire(cardUI, "_background",      borderImg);
        Wire(cardUI, "_icon",            iconImg);
        Wire(cardUI, "_nameText",        nameTMP);
        Wire(cardUI, "_descriptionText", descTMP);
        Wire(cardUI, "_rarityText",      rarityTMP);

        // ── Sauvegarde prefab ─────────────────────────────────────────────────
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/UI/UpgradeCard.prefab");
        Object.DestroyImmediate(root);
        Debug.Log("[EFRITY] Prefab UpgradeCard ✓");

        // ── Assigne dans UpgradeMenuUI de la scène ────────────────────────────
        var menuUI = Object.FindAnyObjectByType<UpgradeMenuUI>();
        if (menuUI != null)
        {
            Wire(menuUI, "_cardPrefab", prefab);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[EFRITY] UpgradeCard assigné dans UpgradeMenuUI ✓");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("EFRITY 🃏 ✅",
            "Prefab UpgradeCard créé et assigné !\n\n" +
            "Monte de niveau en jeu → 3 cartes apparaissent.\n" +
            "Clique sur une carte pour choisir l'upgrade.",
            "Let's go !");
    }

    // ─── ScriptableObjects ────────────────────────────────────────────────────
    [MenuItem("EFRITY/📦 Créer ScriptableObjects")]
    public static void CreateScriptableObjects()
    {
        EnsureFolder("Assets/ScriptableObjects/Config");
        EnsureFolder("Assets/ScriptableObjects/Weapons");
        EnsureFolder("Assets/ScriptableObjects/Enemies");
        EnsureFolder("Assets/ScriptableObjects/Upgrades");

        // ── GameConfig ────────────────────────────────────────────────────────
        var config = CreateSO<GameConfig>("Assets/ScriptableObjects/Config/GameConfig.asset");
        config.BaseXP      = 100f;
        config.XPExponent  = 1.4f;
        config.BossTimers  = new float[] { 300f, 600f };
        config.SpawnRadius = 15f;
        EditorUtility.SetDirty(config);

        // ── WeaponData ────────────────────────────────────────────────────────
        var weapon = CreateSO<WeaponData>("Assets/ScriptableObjects/Weapons/WeaponData_AutoFire.asset");
        weapon.BaseDamage      = 20f;
        weapon.BaseAttackSpeed = 1.2f;
        weapon.BaseRange       = 8f;
        EditorUtility.SetDirty(weapon);

        // ── ProjectileData ────────────────────────────────────────────────────
        var proj = CreateSO<ProjectileData>("Assets/ScriptableObjects/Weapons/ProjectileData_Basic.asset");
        proj.Speed    = 12f;
        proj.Lifetime = 3f;
        EditorUtility.SetDirty(proj);

        // ── EnemyData — Bits (Fodder mêlée) ──────────────────────────────────
        var bits = CreateSO<EnemyData>("Assets/ScriptableObjects/Enemies/EnemyData_Bits.asset");
        bits.EnemyName      = "Bits";
        bits.MaxHP          = 30f;
        bits.MoveSpeed      = 3f;
        bits.Damage         = 5f;
        bits.XPReward       = 10f;
        bits.AttackRange    = 0.6f;
        bits.AttackCooldown = 1f;
        EditorUtility.SetDirty(bits);

        // ── EnemyData — Corrupts (Ranged) ─────────────────────────────────────
        var corrupts = CreateSO<EnemyData>("Assets/ScriptableObjects/Enemies/EnemyData_Corrupts.asset");
        corrupts.EnemyName      = "Corrupts";
        corrupts.MaxHP          = 50f;
        corrupts.MoveSpeed      = 2f;
        corrupts.Damage         = 10f;
        corrupts.XPReward       = 20f;
        corrupts.AttackRange    = 0.5f;
        corrupts.AttackCooldown = 2f;
        EditorUtility.SetDirty(corrupts);

        // ── Upgrades ──────────────────────────────────────────────────────────
        CreateUpgrade("Tir Rapide",
            "Vitesse d'attaque +20%.",
            UpgradeType.Weapon, UpgradeRarity.Common,
            new StatModifier { TargetStat = StatType.AttackSpeedMultiplier, ModType = ModifierType.Multiplicative, Value = 0.20f });

        CreateUpgrade("Blindage Léger",
            "Armure +10. PV max +20.",
            UpgradeType.Stat, UpgradeRarity.Common,
            new StatModifier { TargetStat = StatType.Armor,  ModType = ModifierType.Additive, Value = 10f },
            new StatModifier { TargetStat = StatType.MaxHP,  ModType = ModifierType.Additive, Value = 20f });

        CreateUpgrade("Amplificateur XP",
            "Gain d'XP +25%.",
            UpgradeType.Stat, UpgradeRarity.Common,
            new StatModifier { TargetStat = StatType.XPMultiplier, ModType = ModifierType.Multiplicative, Value = 0.25f });

        CreateUpgrade("Aimant à Gemmes",
            "Rayon de collecte ×2.",
            UpgradeType.Stat, UpgradeRarity.Common,
            new StatModifier { TargetStat = StatType.PickupRadius, ModType = ModifierType.Multiplicative, Value = 1.0f });

        CreateUpgrade("Sprint Cybernétique",
            "Vitesse de déplacement +30%.",
            UpgradeType.Stat, UpgradeRarity.Common,
            new StatModifier { TargetStat = StatType.MoveSpeed, ModType = ModifierType.Multiplicative, Value = 0.30f });

        CreateUpgrade("Drain d'Âme",
            "Soin 5% PV max par kill.",
            UpgradeType.Passive, UpgradeRarity.Rare,
            new StatModifier { TargetStat = StatType.MaxHP, ModType = ModifierType.Additive, Value = 15f });

        CreateUpgrade("Surcharge",
            "Dégâts +40%.",
            UpgradeType.Weapon, UpgradeRarity.Rare,
            new StatModifier { TargetStat = StatType.DamageMultiplier, ModType = ModifierType.Multiplicative, Value = 0.40f });

        CreateUpgrade("Bouclier de Données",
            "Armure +25. PV max +50.",
            UpgradeType.Stat, UpgradeRarity.Rare,
            new StatModifier { TargetStat = StatType.Armor, ModType = ModifierType.Additive, Value = 25f },
            new StatModifier { TargetStat = StatType.MaxHP, ModType = ModifierType.Additive, Value = 50f });

        CreateUpgrade("Nova de Données",
            "Dégâts +60%. Vitesse d'attaque +30%.",
            UpgradeType.Weapon, UpgradeRarity.Epic,
            new StatModifier { TargetStat = StatType.DamageMultiplier,      ModType = ModifierType.Multiplicative, Value = 0.60f },
            new StatModifier { TargetStat = StatType.AttackSpeedMultiplier, ModType = ModifierType.Multiplicative, Value = 0.30f });

        CreateUpgrade("Exosquelette",
            "Toutes les stats +15%.",
            UpgradeType.Stat, UpgradeRarity.Epic,
            new StatModifier { TargetStat = StatType.MaxHP,                 ModType = ModifierType.Multiplicative, Value = 0.15f },
            new StatModifier { TargetStat = StatType.MoveSpeed,             ModType = ModifierType.Multiplicative, Value = 0.15f },
            new StatModifier { TargetStat = StatType.DamageMultiplier,      ModType = ModifierType.Multiplicative, Value = 0.15f },
            new StatModifier { TargetStat = StatType.AttackSpeedMultiplier, ModType = ModifierType.Multiplicative, Value = 0.15f });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assigne GameConfig sur GameManager dans la scène
        var gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            Wire(gm, "_config", config);
            Debug.Log("[EFRITY] GameConfig assigné sur GameManager ✓");
        }

        // Assigne WeaponData sur WeaponSystem du Player
        var ws = Object.FindAnyObjectByType<WeaponSystem>();
        if (ws != null)
        {
            Wire(ws, "_weaponData", weapon);
            Debug.Log("[EFRITY] WeaponData assigné sur WeaponSystem ✓");
        }

        // Assigne les upgrades dans UpgradeManager
        var um = Object.FindAnyObjectByType<UpgradeManager>();
        if (um != null)
        {
            var allUpgrades = new System.Collections.Generic.List<UpgradeData>();
            var guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { "Assets/ScriptableObjects/Upgrades" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var u    = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                if (u != null) allUpgrades.Add(u);
            }
            var soUM = new SerializedObject(um);
            var list = soUM.FindProperty("_allUpgrades");
            list.ClearArray();
            for (int i = 0; i < allUpgrades.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = allUpgrades[i];
            }
            soUM.ApplyModifiedProperties();
            Debug.Log($"[EFRITY] {allUpgrades.Count} upgrades assignés dans UpgradeManager ✓");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[EFRITY] ✅ Tous les ScriptableObjects créés !");
        EditorUtility.DisplayDialog("EFRITY 📦 ✅",
            "ScriptableObjects créés et assignés !\n\n" +
            "Reste à faire :\n" +
            "1. Créer le Prefab Projectile joueur\n" +
            "2. Créer le Prefab Bits (ennemi)\n" +
            "3. Créer le Prefab XPOrb\n" +
            "4. Les assigner dans EnemyData et WeaponData\n" +
            "5. Glisser le sprite Evil Wizard sur Player > Sprite",
            "Compris !");
    }

    // ── Helpers ScriptableObjects ──────────────────────────────────────────────
    static T CreateSO<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var so = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    static void CreateUpgrade(string upgradeName, string description,
                              UpgradeType type, UpgradeRarity rarity,
                              params StatModifier[] modifiers)
    {
        string safeName = upgradeName.Replace(" ", "_").Replace("'", "");
        string path     = $"Assets/ScriptableObjects/Upgrades/Upgrade_{safeName}.asset";
        var    u        = CreateSO<UpgradeData>(path);
        u.UpgradeName   = upgradeName;
        u.Description   = description;
        u.Type          = type;
        u.Rarity        = rarity;
        u.Modifiers     = modifiers;
        u.IsUnique      = (rarity == UpgradeRarity.Epic);
        EditorUtility.SetDirty(u);
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    // ─── Prefabs ──────────────────────────────────────────────────────────────
    [MenuItem("EFRITY/🎯 Créer Prefabs")]
    public static void CreatePrefabs()
    {
        EnsureFolder("Assets/Prefabs/Enemies");
        EnsureFolder("Assets/Prefabs/Projectiles");
        EnsureFolder("Assets/Prefabs/VFX");

        // ── Projectile joueur ─────────────────────────────────────────────────
        var projData = AssetDatabase.LoadAssetAtPath<ProjectileData>(
            "Assets/ScriptableObjects/Weapons/ProjectileData_Basic.asset");

        var projGO = new GameObject("Projectile_Player");
        projGO.layer = LayerMask.NameToLayer("PlayerProjectile");
        // ProjectilePlayer ajoute automatiquement Rigidbody2D + CircleCollider2D via [RequireComponent]
        var projComp = projGO.AddComponent<ProjectilePlayer>();
        if (projData != null) Wire(projComp, "_data", projData);
        var projSR  = projGO.AddComponent<SpriteRenderer>();
        projSR.color = new Color(0.3f, 0.8f, 1f);
        var projCol  = Ensure<CircleCollider2D>(projGO);
        projCol.isTrigger = true;
        projCol.radius    = 0.2f;
        var projRB = Ensure<Rigidbody2D>(projGO);
        projRB.bodyType        = RigidbodyType2D.Kinematic;
        projRB.gravityScale    = 0f;
        projRB.freezeRotation  = true;
        var projPrefab = PrefabUtility.SaveAsPrefabAsset(projGO, "Assets/Prefabs/Projectiles/Projectile_Player.prefab");
        Object.DestroyImmediate(projGO);
        Debug.Log("[EFRITY] Prefab Projectile_Player ✓");

        // ── Ennemi Bits (mêlée) ───────────────────────────────────────────────
        var bitsData = AssetDatabase.LoadAssetAtPath<EnemyData>(
            "Assets/ScriptableObjects/Enemies/EnemyData_Bits.asset");

        var bitsGO = new GameObject("Enemy_Bits");
        bitsGO.layer = LayerMask.NameToLayer("Enemy");
        // EnemyMelee ajoute automatiquement Rigidbody2D via [RequireComponent]
        var bitsComp = bitsGO.AddComponent<EnemyMelee>();
        if (bitsData != null) Wire(bitsComp, "_data", bitsData);
        bitsGO.AddComponent<SpriteRenderer>().color = new Color(0.9f, 0.2f, 0.2f);
        var bitsCol = Ensure<CircleCollider2D>(bitsGO);
        bitsCol.radius = 0.4f;
        var bitsRB = Ensure<Rigidbody2D>(bitsGO);
        bitsRB.bodyType       = RigidbodyType2D.Dynamic;
        bitsRB.gravityScale   = 0f;
        bitsRB.freezeRotation = true;
        bitsRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var bitsPrefab = PrefabUtility.SaveAsPrefabAsset(bitsGO, "Assets/Prefabs/Enemies/Enemy_Bits.prefab");
        Object.DestroyImmediate(bitsGO);
        Debug.Log("[EFRITY] Prefab Enemy_Bits ✓");

        // ── XPOrb ─────────────────────────────────────────────────────────────
        var orbGO = new GameObject("XPOrb");
        orbGO.AddComponent<SpriteRenderer>().color = new Color(0.2f, 1f, 0.4f);
        orbGO.AddComponent<XPOrb>();
        var orbCol = orbGO.AddComponent<CircleCollider2D>();
        orbCol.isTrigger = true;
        orbCol.radius    = 0.3f;
        var orbPrefab = PrefabUtility.SaveAsPrefabAsset(orbGO, "Assets/Prefabs/VFX/XPOrb.prefab");
        Object.DestroyImmediate(orbGO);
        Debug.Log("[EFRITY] Prefab XPOrb ✓");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Assigne les prefabs dans les ScriptableObjects ────────────────────
        if (bitsData != null)
        {
            Wire(bitsData, "Prefab",      bitsPrefab);
            Wire(bitsData, "XPOrbPrefab", orbPrefab);
            EditorUtility.SetDirty(bitsData);
        }

        var weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(
            "Assets/ScriptableObjects/Weapons/WeaponData_AutoFire.asset");
        if (weaponData != null)
        {
            Wire(weaponData, "ProjectilePrefab", projPrefab);
            EditorUtility.SetDirty(weaponData);
        }

        // ── Configure EnemyManager — Phase 0 ─────────────────────────────────
        var em = Object.FindAnyObjectByType<EnemyManager>();
        if (em != null && bitsData != null)
        {
            var soEM   = new SerializedObject(em);
            var phases = soEM.FindProperty("_phases");
            if (phases.arraySize == 0) phases.InsertArrayElementAtIndex(0);

            var phase0 = phases.GetArrayElementAtIndex(0);
            phase0.FindPropertyRelative("StartTime").floatValue    = 0f;
            phase0.FindPropertyRelative("SpawnInterval").floatValue = 1.5f;
            phase0.FindPropertyRelative("MaxEnemies").intValue      = 15;

            var pool = phase0.FindPropertyRelative("EnemyPool");
            pool.ClearArray();
            pool.InsertArrayElementAtIndex(0);
            pool.GetArrayElementAtIndex(0).objectReferenceValue = bitsData;
            soEM.ApplyModifiedProperties();
            Debug.Log("[EFRITY] EnemyManager Phase 0 configurée ✓");
        }

        // ── Configure WeaponSystem — LayerMask Enemy ──────────────────────────
        var ws = Object.FindAnyObjectByType<WeaponSystem>();
        if (ws != null)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                var soWS = new SerializedObject(ws);
                soWS.FindProperty("_enemyLayer").intValue = 1 << enemyLayer;
                soWS.ApplyModifiedProperties();
                Debug.Log("[EFRITY] WeaponSystem LayerMask Enemy ✓");
            }
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[EFRITY] ✅ Tous les Prefabs créés et assignés !");
        EditorUtility.DisplayDialog("EFRITY 🎯 ✅",
            "Prefabs créés !\n\n" +
            "Le jeu est maintenant jouable (rectangles colorés).\n\n" +
            "Dernière étape :\n" +
            "Glisser tes sprites sur les SpriteRenderers :\n" +
            "• Evil Wizard → Player/Sprite\n" +
            "• Enemy Galore → Enemy_Bits prefab\n" +
            "• Gemme verte → XPOrb prefab\n" +
            "• Boule bleue → Projectile_Player prefab",
            "Let's go !");
    }

    // ─── Managers ─────────────────────────────────────────────────────────────
    static void SetupManagers()
    {
        var managers = GetOrCreate("_Managers");

        Ensure<GameManager>      (GetOrCreateChild(managers, "GameManager"));
        Ensure<ObjectPoolManager>(GetOrCreateChild(managers, "ObjectPoolManager"));
        Ensure<EnemyManager>     (GetOrCreateChild(managers, "EnemyManager"));
        Ensure<XPSystem>         (GetOrCreateChild(managers, "XPSystem"));
        Ensure<UpgradeManager>   (GetOrCreateChild(managers, "UpgradeManager"));
        Ensure<LeaderboardManager>(GetOrCreateChild(managers, "LeaderboardManager"));
        Ensure<AchievementManager>(GetOrCreateChild(managers, "AchievementManager"));

        var audioGO = GetOrCreateChild(managers, "AudioManager");
        Ensure<AudioManager>(audioGO);
        // 2 AudioSources : index 0 = Music, index 1 = SFX
        var sources = audioGO.GetComponents<AudioSource>();
        if (sources.Length < 1) audioGO.AddComponent<AudioSource>();
        if (sources.Length < 2) audioGO.AddComponent<AudioSource>();

        Debug.Log("[EFRITY] _Managers ✓");
    }

    // ─── Player ───────────────────────────────────────────────────────────────
    static void SetupPlayer()
    {
        var player = GetOrCreate("Player");
        player.tag = "Player";

        var rb = Ensure<Rigidbody2D>(player);
        rb.gravityScale           = 0f;
        rb.freezeRotation         = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = Ensure<CircleCollider2D>(player);
        col.radius    = 0.4f;
        col.isTrigger = false;

        Ensure<PlayerStats>     (player);
        Ensure<PlayerHealth>    (player);
        Ensure<PlayerController>(player);
        Ensure<WeaponSystem>    (player);

        // Enfant : Sprite
        var spriteGO = GetOrCreateChild(player, "Sprite");
        Ensure<SpriteRenderer>(spriteGO);

        // Enfant : Zone de collecte XP
        var xpZone = GetOrCreateChild(player, "XPPickupZone");
        Ensure<XPPickupZone>(xpZone);
        var xpCol = Ensure<CircleCollider2D>(xpZone);
        xpCol.isTrigger = true;
        xpCol.radius    = 2f;

        Debug.Log("[EFRITY] Player ✓");
    }

    // ─── Canvas ───────────────────────────────────────────────────────────────
    static void SetupCanvas()
    {
        // EventSystem (si absent)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>(); // New Input System
        }

        // Canvas racine
        var canvasGO = GetOrCreate("Canvas");
        var cv = Ensure<Canvas>(canvasGO);
        cv.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = Ensure<CanvasScaler>(canvasGO);
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        Ensure<GraphicRaycaster>(canvasGO);

        SetupHUD(canvasGO);
        SetupLevelUpPanel(canvasGO);
        SetupGameOverPanel(canvasGO);
        SetupPausePanel(canvasGO);
        SetupAchievementToast(canvasGO);

        Debug.Log("[EFRITY] Canvas ✓");
    }

    // ── HUD ───────────────────────────────────────────────────────────────────
    static void SetupHUD(GameObject canvas)
    {
        var hudGO = UIChild(canvas, "HUD_Panel");
        Stretch(hudGO);
        var hud = Ensure<HUDController>(hudGO);

        // Flash dégâts plein écran
        var flashGO  = UIChild(hudGO, "IMG_DamageFlash");
        Stretch(flashGO);
        var flashImg = Ensure<Image>(flashGO);
        flashImg.color         = new Color(1f, 0f, 0f, 0f);
        flashImg.raycastTarget = false;

        // Barre de vie (top-left)
        var hpBar = MakeSlider(hudGO, "HealthBar", new Color(0.8f, 0.15f, 0.15f));
        Rect(hpBar, 0f, 1f, 0f, 1f, 0f, 1f, 20f, -20f, 300f, 28f);

        // Texte PV
        var hpTxtGO = UIChild(hudGO, "TXT_HP");
        Rect(hpTxtGO, 0f, 1f, 0f, 1f, 0f, 1f, 20f, -55f, 200f, 24f);
        var hpTMP = Ensure<TextMeshProUGUI>(hpTxtGO);
        hpTMP.text = "100 / 100"; hpTMP.fontSize = 15; hpTMP.color = Color.white;

        // Barre XP (bottom-center)
        var xpBar = MakeSlider(hudGO, "XPBar", new Color(0.1f, 0.5f, 0.9f));
        Rect(xpBar, 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 0f, 12f, 700f, 18f);

        // Niveau
        var lvlGO  = UIChild(hudGO, "TXT_Level");
        Rect(lvlGO, 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 0f, 36f, 140f, 28f);
        var lvlTMP = Ensure<TextMeshProUGUI>(lvlGO);
        lvlTMP.text = "Niv. 1"; lvlTMP.fontSize = 20; lvlTMP.color = Color.white;
        lvlTMP.alignment = TextAlignmentOptions.Center;

        // Timer (top-center)
        var timerGO  = UIChild(hudGO, "TXT_Timer");
        Rect(timerGO, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0f, -18f, 180f, 45f);
        var timerTMP = Ensure<TextMeshProUGUI>(timerGO);
        timerTMP.text = "00:00"; timerTMP.fontSize = 32;
        timerTMP.fontStyle = FontStyles.Bold; timerTMP.color = Color.white;
        timerTMP.alignment = TextAlignmentOptions.Center;

        // Kills (top-right)
        var killsGO  = UIChild(hudGO, "TXT_Kills");
        Rect(killsGO, 1f, 1f, 1f, 1f, 1f, 1f, -20f, -20f, 160f, 28f);
        var killsTMP = Ensure<TextMeshProUGUI>(killsGO);
        killsTMP.text = "Kills : 0"; killsTMP.fontSize = 18; killsTMP.color = Color.white;
        killsTMP.alignment = TextAlignmentOptions.Right;

        // Câblage des références dans HUDController
        Wire(hud, "_healthBar",   hpBar.GetComponent<Slider>());
        Wire(hud, "_healthText",  hpTMP);
        Wire(hud, "_xpBar",       xpBar.GetComponent<Slider>());
        Wire(hud, "_levelText",   lvlTMP);
        Wire(hud, "_timerText",   timerTMP);
        Wire(hud, "_killsText",   killsTMP);
        Wire(hud, "_damageFlash", flashImg);
    }

    // ── LevelUp Panel ─────────────────────────────────────────────────────────
    static void SetupLevelUpPanel(GameObject canvas)
    {
        // Panneau visuel (commence inactif)
        var panelGO = UIChild(canvas, "LevelUp_Panel");
        Stretch(panelGO);
        var bg = Ensure<Image>(panelGO);
        bg.color = new Color(0f, 0f, 0f, 0.75f);

        // Container des 3 cartes
        var container = UIChild(panelGO, "CardContainer");
        Rect(container, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 0f, 920f, 420f);
        var hlg = Ensure<HorizontalLayoutGroup>(container);
        hlg.spacing               = 20f;
        hlg.childAlignment        = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight= false;

        // Titre
        var titleGO  = UIChild(panelGO, "TXT_Title");
        Rect(titleGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 230f, 600f, 60f);
        var titleTMP = Ensure<TextMeshProUGUI>(titleGO);
        titleTMP.text = "CHOISISSEZ UN UPGRADE"; titleTMP.fontSize = 32;
        titleTMP.color = Color.white; titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;

        // UpgradeMenuUI DOIT être sur un GO toujours actif (pas sur le panel).
        // Si le composant est sur panelGO, son Start() appelle _panel.SetActive(false)
        // ce qui déclenche OnDisable → unsubscribe → LevelUpEvent jamais reçu.
        var controllerGO = UIChild(canvas, "LevelUp_Controller");
        var ui = Ensure<UpgradeMenuUI>(controllerGO);

        Wire(ui, "_cardContainer", container.transform);
        Wire(ui, "_panel",         panelGO);

        panelGO.SetActive(false);
        // controllerGO reste actif
    }

    // ── GameOver Panel ────────────────────────────────────────────────────────
    static void SetupGameOverPanel(GameObject canvas)
    {
        var panelGO = UIChild(canvas, "GameOver_Panel");
        Stretch(panelGO);
        var bg = Ensure<Image>(panelGO);
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        var ui = Ensure<GameOverUI>(panelGO);

        // Titre GAME OVER
        var titleGO  = UIChild(panelGO, "TXT_GameOver");
        Rect(titleGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 150f, 500f, 80f);
        var titleTMP = Ensure<TextMeshProUGUI>(titleGO);
        titleTMP.text = "GAME OVER"; titleTMP.fontSize = 60;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(0.9f, 0.15f, 0.15f);
        titleTMP.alignment = TextAlignmentOptions.Center;

        // Stats
        var timeGO  = UIChild(panelGO, "TXT_Time");
        Rect(timeGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 55f, 420f, 36f);
        var timeTMP = Ensure<TextMeshProUGUI>(timeGO);
        timeTMP.text = "Temps : 00:00"; timeTMP.fontSize = 24;
        timeTMP.color = Color.white; timeTMP.alignment = TextAlignmentOptions.Center;

        var killsGO  = UIChild(panelGO, "TXT_Kills");
        Rect(killsGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 10f, 420f, 36f);
        var killsTMP = Ensure<TextMeshProUGUI>(killsGO);
        killsTMP.text = "Kills : 0"; killsTMP.fontSize = 24;
        killsTMP.color = Color.white; killsTMP.alignment = TextAlignmentOptions.Center;

        var levelGO  = UIChild(panelGO, "TXT_LevelReached");
        Rect(levelGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -35f, 420f, 36f);
        var levelTMP = Ensure<TextMeshProUGUI>(levelGO);
        levelTMP.text = "Niveau atteint : 1"; levelTMP.fontSize = 24;
        levelTMP.color = Color.white; levelTMP.alignment = TextAlignmentOptions.Center;

        var scoreGO  = UIChild(panelGO, "TXT_Score");
        Rect(scoreGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -80f, 420f, 40f);
        var scoreTMP = Ensure<TextMeshProUGUI>(scoreGO);
        scoreTMP.text = "Score : 0"; scoreTMP.fontSize = 28;
        scoreTMP.fontStyle = FontStyles.Bold;
        scoreTMP.color = new Color(1f, 0.85f, 0f); scoreTMP.alignment = TextAlignmentOptions.Center;

        var bestGO  = UIChild(panelGO, "TXT_BestScore");
        Rect(bestGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -118f, 420f, 28f);
        var bestTMP = Ensure<TextMeshProUGUI>(bestGO);
        bestTMP.text = ""; bestTMP.fontSize = 17;
        bestTMP.color = new Color(0.65f, 0.65f, 0.65f); bestTMP.alignment = TextAlignmentOptions.Center;

        // Boutons
        var restartGO = MakeButton(panelGO, "BTN_Restart", "Rejouer",       new Color(0.1f, 0.55f, 0.1f));
        Rect(restartGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, -115f, -165f, 210f, 55f);

        var menuGO = MakeButton(panelGO, "BTN_Menu", "Menu Principal", new Color(0.2f, 0.2f, 0.55f));
        Rect(menuGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 115f, -165f, 210f, 55f);

        Wire(ui, "_panel",         panelGO);
        Wire(ui, "_timeText",      timeTMP);
        Wire(ui, "_killsText",     killsTMP);
        Wire(ui, "_levelText",     levelTMP);
        Wire(ui, "_scoreText",     scoreTMP);
        Wire(ui, "_bestScoreText", bestTMP);
        Wire(ui, "_restartButton", restartGO.GetComponent<Button>());
        Wire(ui, "_menuButton",    menuGO.GetComponent<Button>());

        panelGO.SetActive(false);
    }

    // ── Pause Panel ───────────────────────────────────────────────────────────
    static void SetupPausePanel(GameObject canvas)
    {
        var panelGO = UIChild(canvas, "Pause_Panel");
        Stretch(panelGO);
        Ensure<Image>(panelGO).color = new Color(0f, 0f, 0f, 0.72f);
        var ui = Ensure<PauseMenuUI>(panelGO);

        var titleGO  = UIChild(panelGO, "TXT_Pause");
        Rect(titleGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 100f, 400f, 70f);
        var titleTMP = Ensure<TextMeshProUGUI>(titleGO);
        titleTMP.text = "PAUSE"; titleTMP.fontSize = 52;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white; titleTMP.alignment = TextAlignmentOptions.Center;

        var resumeGO = MakeButton(panelGO, "BTN_Resume", "Reprendre", new Color(0.1f, 0.55f, 0.1f));
        Rect(resumeGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 10f, 250f, 58f);

        var restartGO = MakeButton(panelGO, "BTN_Restart", "Recommencer", new Color(0.5f, 0.38f, 0.08f));
        Rect(restartGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -60f, 250f, 58f);

        var menuGO = MakeButton(panelGO, "BTN_Menu", "Menu Principal", new Color(0.2f, 0.2f, 0.55f));
        Rect(menuGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -130f, 250f, 58f);

        Wire(ui, "_panel",         panelGO);
        Wire(ui, "_resumeButton",  resumeGO.GetComponent<Button>());
        Wire(ui, "_restartButton", restartGO.GetComponent<Button>());
        Wire(ui, "_menuButton",    menuGO.GetComponent<Button>());

        panelGO.SetActive(false);
    }

    // ── Achievement Toast ─────────────────────────────────────────────────────
    static void SetupAchievementToast(GameObject canvas)
    {
        var toastGO = UIChild(canvas, "Achievement_Toast");
        Rect(toastGO, 1f, 0f, 1f, 0f, 1f, 0f, 450f, 90f, 390f, 100f);
        Ensure<Image>(toastGO).color = new Color(0.07f, 0.13f, 0.07f, 0.95f);
        var ui = Ensure<AchievementToastUI>(toastGO);

        var headGO  = UIChild(toastGO, "TXT_Title");
        Rect(headGO, 0f, 1f, 1f, 1f, 0.5f, 1f, 0f, -9f, -20f, 24f);
        var headTMP = Ensure<TextMeshProUGUI>(headGO);
        headTMP.text = "SUCCES DEBLOQUE !"; headTMP.fontSize = 13;
        headTMP.fontStyle = FontStyles.Bold;
        headTMP.color = new Color(0.3f, 1f, 0.3f); headTMP.alignment = TextAlignmentOptions.Center;

        var nameGO  = UIChild(toastGO, "TXT_Name");
        Rect(nameGO, 0f, 0.42f, 1f, 1f, 0.5f, 1f, 0f, -38f, -20f, 34f);
        var nameTMP = Ensure<TextMeshProUGUI>(nameGO);
        nameTMP.text = "Nom"; nameTMP.fontSize = 22;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color = Color.white; nameTMP.alignment = TextAlignmentOptions.Center;

        var descGO  = UIChild(toastGO, "TXT_Desc");
        Rect(descGO, 0f, 0f, 1f, 0.42f, 0.5f, 0.5f, 0f, 0f, -20f, 0f);
        var descTMP = Ensure<TextMeshProUGUI>(descGO);
        descTMP.text = "Description"; descTMP.fontSize = 13;
        descTMP.color = new Color(0.75f, 0.75f, 0.75f); descTMP.alignment = TextAlignmentOptions.Center;

        Wire(ui, "_toastPanel", toastGO.GetComponent<RectTransform>());
        Wire(ui, "_titleText",  headTMP);
        Wire(ui, "_nameText",   nameTMP);
        Wire(ui, "_descText",   descTMP);
    }

    // ─── Caméra ───────────────────────────────────────────────────────────────
    static void SetupCamera()
    {
        if (Camera.main == null) return;
        Camera.main.orthographic     = true;
        Camera.main.orthographicSize = 8f;
        Camera.main.backgroundColor  = new Color(0.05f, 0.05f, 0.1f);
        Camera.main.clearFlags       = CameraClearFlags.SolidColor;
        Debug.Log("[EFRITY] Caméra orthographique ✓");
    }

    // ─── Helpers généraux ─────────────────────────────────────────────────────

    static GameObject GetOrCreate(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        return go;
    }

    static GameObject GetOrCreateChild(GameObject parent, string childName)
    {
        var t = parent.transform.Find(childName);
        if (t != null) return t.gameObject;
        var child = new GameObject(childName);
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    /// <summary>Crée un enfant UI (RectTransform) sous un parent.</summary>
    static GameObject UIChild(GameObject parent, string childName)
    {
        var t = parent.transform.Find(childName);
        if (t != null) return t.gameObject;
        var child = new GameObject(childName, typeof(RectTransform));
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    /// <summary>Ajoute un composant si absent, sinon retourne celui qui existe.</summary>
    static T Ensure<T>(GameObject go) where T : Component
    {
        // On utilise if/else et non ?? car Unity surcharge l'opérateur ==
        // mais pas ??, ce qui cause des MissingComponentException.
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    /// <summary>Câble une référence sérialisée via SerializedObject.
    /// Accepte Component ET ScriptableObject (tous deux héritent de UnityEngine.Object).</summary>
    static void Wire(Object target, string fieldName, Object value)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null) { Debug.LogWarning($"[EFRITY] Champ '{fieldName}' introuvable sur {target.GetType().Name}"); return; }
        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    /// <summary>Étire un RectTransform sur tout son parent.</summary>
    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>Configure un RectTransform : ancres, pivot, position, taille.</summary>
    static void Rect(GameObject go,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float pivotX, float pivotY,
        float posX,   float posY,
        float w,      float h)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(aMinX, aMinY);
        rt.anchorMax        = new Vector2(aMaxX, aMaxY);
        rt.pivot            = new Vector2(pivotX, pivotY);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta        = new Vector2(w, h);
    }

    /// <summary>Crée un Slider fonctionnel avec fond + fill coloré.</summary>
    static GameObject MakeSlider(GameObject parent, string name, Color fillColor)
    {
        var go = UIChild(parent, name);
        Ensure<Image>(go).color = new Color(0.12f, 0.12f, 0.12f);

        var fillArea = UIChild(go, "FillArea");
        Stretch(fillArea);
        var fill = UIChild(fillArea, "Fill");
        Stretch(fill);
        Ensure<Image>(fill).color = fillColor;

        var slider = Ensure<Slider>(go);
        slider.fillRect    = fill.GetComponent<RectTransform>();
        slider.direction   = Slider.Direction.LeftToRight;
        slider.minValue    = 0f;
        slider.maxValue    = 1f;
        slider.value       = 1f;
        slider.interactable= false;
        return go;
    }

    /// <summary>Crée un bouton avec texte TMP.</summary>
    static GameObject MakeButton(GameObject parent, string name, string label, Color bgColor)
    {
        var go  = UIChild(parent, name);
        var img = Ensure<Image>(go);
        img.color = bgColor;
        var btn = Ensure<Button>(go);
        btn.targetGraphic = img;

        var txtGO = UIChild(go, "Label");
        Stretch(txtGO);
        var tmp = Ensure<TextMeshProUGUI>(txtGO);
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    // ─── MainMenu Scene Builder ───────────────────────────────────────────────

    [MenuItem("EFRITY/🏠 Créer Scène MainMenu")]
    public static void BuildMainMenuScene()
    {
        EnsureFolder("Assets/Scenes");

        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single);

        // ── Caméra ────────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.03f, 0.03f, 0.06f);
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        camGO.AddComponent<AudioListener>();

        // ── EventSystem ───────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // ── Managers persistants ──────────────────────────────────────────────
        var mgrsGO = new GameObject("_PersistentManagers");
        var lbGO   = new GameObject("LeaderboardManager"); lbGO.transform.SetParent(mgrsGO.transform);
        lbGO.AddComponent<LeaderboardManager>();
        var achGO  = new GameObject("AchievementManager"); achGO.transform.SetParent(mgrsGO.transform);
        achGO.AddComponent<AchievementManager>();
        var audioMGO = new GameObject("AudioManager"); audioMGO.transform.SetParent(mgrsGO.transform);
        audioMGO.AddComponent<AudioManager>();
        audioMGO.AddComponent<AudioSource>();
        audioMGO.AddComponent<AudioSource>();

        // ── Canvas ────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        var menuUI = canvasGO.AddComponent<MainMenuUI>();

        // ── Fond sombre ───────────────────────────────────────────────────────
        var bgGO = UIChild(canvasGO, "Background");
        Stretch(bgGO);
        Ensure<Image>(bgGO).color = new Color(0.03f, 0.03f, 0.06f);

        // ── Panneau gauche (navigation) ───────────────────────────────────────
        var navGO = new GameObject("NavPanel", typeof(RectTransform));
        navGO.transform.SetParent(canvasGO.transform, false);
        var navRT = navGO.GetComponent<RectTransform>();
        navRT.anchorMin = new Vector2(0, 0); navRT.anchorMax = new Vector2(0, 1);
        navRT.pivot     = new Vector2(0, 0.5f);
        navRT.offsetMin = Vector2.zero; navRT.offsetMax = Vector2.zero;
        navRT.sizeDelta = new Vector2(360, 0);
        Ensure<Image>(navGO).color = new Color(0.06f, 0.06f, 0.10f, 1f);

        // Titre
        var titleGO  = UIChild(navGO, "TXT_Title");
        Rect(titleGO, 0f, 1f, 1f, 1f, 0.5f, 1f, 0f, -55f, 0f, 90f);
        var titleTMP = Ensure<TextMeshProUGUI>(titleGO);
        titleTMP.text      = "EFRITY"; titleTMP.fontSize = 58;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = new Color(0.9f, 0.15f, 0.15f);
        titleTMP.alignment = TextAlignmentOptions.Center;

        var subGO  = UIChild(navGO, "TXT_Sub");
        Rect(subGO, 0f, 1f, 1f, 1f, 0.5f, 1f, 0f, -128f, 0f, 32f);
        var subTMP = Ensure<TextMeshProUGUI>(subGO);
        subTMP.text           = "SURVIVE"; subTMP.fontSize = 19;
        subTMP.fontStyle      = FontStyles.Bold | FontStyles.SmallCaps;
        subTMP.color          = new Color(0.55f, 0.55f, 0.55f);
        subTMP.alignment      = TextAlignmentOptions.Center;
        subTMP.characterSpacing = 12f;

        // Container boutons
        var btnContGO = UIChild(navGO, "ButtonContainer");
        Rect(btnContGO, 0.08f, 0f, 0.92f, 1f, 0.5f, 0.5f, 0f, -30f, 0f, -280f);
        var vlg = Ensure<VerticalLayoutGroup>(btnContGO);
        vlg.spacing               = 14f;
        vlg.childAlignment        = TextAnchor.MiddleCenter;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight= false;

        var btnPlay = MakeMenuNavButton(btnContGO, "BTN_Play", "JOUER",       new Color(0.12f, 0.48f, 0.12f));
        MakeMenuNavButton(btnContGO, "BTN_LB",   "CLASSEMENT",  new Color(0.12f, 0.12f, 0.25f));
        MakeMenuNavButton(btnContGO, "BTN_Ach",  "SUCCES",      new Color(0.12f, 0.12f, 0.25f));
        MakeMenuNavButton(btnContGO, "BTN_Set",  "PARAMETRES",  new Color(0.12f, 0.12f, 0.25f));
        MakeMenuNavButton(btnContGO, "BTN_Quit", "QUITTER",     new Color(0.40f, 0.08f, 0.08f));

        // Version
        var verGO  = UIChild(navGO, "TXT_Version");
        Rect(verGO, 0f, 0f, 1f, 0f, 0.5f, 0f, 0f, 12f, 0f, 22f);
        var verTMP = Ensure<TextMeshProUGUI>(verGO);
        verTMP.text = "v0.1 - Alpha"; verTMP.fontSize = 13;
        verTMP.color = new Color(0.38f, 0.38f, 0.38f); verTMP.alignment = TextAlignmentOptions.Center;

        // ── Panneau droit (contenu) ───────────────────────────────────────────
        var contentGO = new GameObject("ContentPanel", typeof(RectTransform));
        contentGO.transform.SetParent(canvasGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.offsetMin = new Vector2(360, 0); contentRT.offsetMax = Vector2.zero;

        // Panel Accueil
        var panelHome = UIChild(contentGO, "Panel_Home");
        Stretch(panelHome);
        SetupHomeContent(panelHome);

        // Panel Classement
        var panelLB = UIChild(contentGO, "Panel_Leaderboard");
        Stretch(panelLB);
        TMP_Text lbEmpty;
        Transform lbContent = SetupScrollContent(panelLB, "CLASSEMENT", out lbEmpty);

        // Panel Succès
        var panelAch = UIChild(contentGO, "Panel_Achievements");
        Stretch(panelAch);
        TMP_Text achEmpty;
        Transform achContent = SetupScrollContent(panelAch, "SUCCES", out achEmpty);

        // Panel Paramètres
        var panelSet = UIChild(contentGO, "Panel_Settings");
        Stretch(panelSet);
        Slider musicSlider, sfxSlider;
        TMP_Text musicLabel, sfxLabel;
        SetupSettingsContent(panelSet, out musicSlider, out sfxSlider, out musicLabel, out sfxLabel);

        // ── Câblage MainMenuUI ────────────────────────────────────────────────
        var so = new SerializedObject(menuUI);

        // Buttons
        WireMenuBtn(so, "_btnPlay",         btnContGO, "BTN_Play");
        WireMenuBtn(so, "_btnLeaderboard",  btnContGO, "BTN_LB");
        WireMenuBtn(so, "_btnAchievements", btnContGO, "BTN_Ach");
        WireMenuBtn(so, "_btnSettings",     btnContGO, "BTN_Set");
        WireMenuBtn(so, "_btnQuit",         btnContGO, "BTN_Quit");

        so.FindProperty("_panelHome").objectReferenceValue          = panelHome;
        so.FindProperty("_panelLeaderboard").objectReferenceValue   = panelLB;
        so.FindProperty("_panelAchievements").objectReferenceValue  = panelAch;
        so.FindProperty("_panelSettings").objectReferenceValue      = panelSet;
        so.FindProperty("_leaderboardContent").objectReferenceValue = lbContent;
        if (lbEmpty != null) so.FindProperty("_leaderboardEmptyText").objectReferenceValue = lbEmpty;
        so.FindProperty("_achievementsContent").objectReferenceValue = achContent;
        if (musicSlider != null) so.FindProperty("_musicSlider").objectReferenceValue = musicSlider;
        if (sfxSlider   != null) so.FindProperty("_sfxSlider").objectReferenceValue   = sfxSlider;
        if (musicLabel  != null) so.FindProperty("_musicLabel").objectReferenceValue  = musicLabel;
        if (sfxLabel    != null) so.FindProperty("_sfxLabel").objectReferenceValue    = sfxLabel;
        so.ApplyModifiedProperties();

        // ── Cacher les panneaux non-accueil ───────────────────────────────────
        panelLB.SetActive(false);
        panelAch.SetActive(false);
        panelSet.SetActive(false);

        // ── Sauvegarde ────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        AssetDatabase.Refresh();

        // Ajoute les scènes au Build Settings
        AddToBuildSettings("Assets/Scenes/MainMenu.unity",  0);
        AddToBuildSettings("Assets/Scenes/SampleScene.unity", 1);

        Debug.Log("[EFRITY] ✅ Scène MainMenu créée : Assets/Scenes/MainMenu.unity");
        EditorUtility.DisplayDialog("EFRITY 🏠 ✅",
            "Scène MainMenu créée !\n\n" +
            "Dans Build Settings :\n" +
            "  Index 0 = MainMenu\n" +
            "  Index 1 = SampleScene\n\n" +
            "Lance MainMenu comme scène de départ.",
            "Parfait !");
    }

    // ─── Helpers MainMenu ─────────────────────────────────────────────────────

    static GameObject MakeMenuNavButton(GameObject parent, string name, string label, Color bg)
    {
        var go  = UIChild(parent, name);
        var le  = go.AddComponent<LayoutElement>();
        le.preferredHeight = 58f; le.flexibleWidth = 1f;
        var img = Ensure<Image>(go);
        img.color = bg;
        var btn = Ensure<Button>(go);
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(bg.r * 1.5f, 1f), Mathf.Min(bg.g * 1.5f, 1f), Mathf.Min(bg.b * 1.5f, 1f));
        colors.pressedColor = new Color(bg.r * 0.6f, bg.g * 0.6f, bg.b * 0.6f);
        btn.colors = colors;
        var lblGO = UIChild(go, "Label");
        Stretch(lblGO);
        var rt = lblGO.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(18, 0);
        var tmp = Ensure<TextMeshProUGUI>(lblGO);
        tmp.text             = label;
        tmp.fontSize         = 21;
        tmp.fontStyle        = FontStyles.Bold;
        tmp.color            = Color.white;
        tmp.alignment        = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode     = TextOverflowModes.Truncate;
        return go;
    }

    static void SetupHomeContent(GameObject panel)
    {
        var msgGO  = UIChild(panel, "TXT_Welcome");
        Rect(msgGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 80f, 700f, 80f);
        var msgTMP = Ensure<TextMeshProUGUI>(msgGO);
        msgTMP.text      = "Pret a survivre ?";
        msgTMP.fontSize  = 42; msgTMP.fontStyle = FontStyles.Bold;
        msgTMP.color     = Color.white; msgTMP.alignment = TextAlignmentOptions.Center;

        var subGO  = UIChild(panel, "TXT_Sub");
        Rect(subGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 10f, 600f, 50f);
        var subTMP = Ensure<TextMeshProUGUI>(subGO);
        subTMP.text      = "Des vagues infinies vous attendent.";
        subTMP.fontSize  = 22; subTMP.color = new Color(0.7f, 0.7f, 0.7f);
        subTMP.alignment = TextAlignmentOptions.Center;

        var hintGO  = UIChild(panel, "TXT_Hint");
        Rect(hintGO, 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 0f, 35f, 600f, 30f);
        var hintTMP = Ensure<TextMeshProUGUI>(hintGO);
        hintTMP.text = "Tip : Echap = Pause  |  Click JOUER pour commencer";
        hintTMP.fontSize = 15; hintTMP.color = new Color(0.45f, 0.45f, 0.45f);
        hintTMP.alignment = TextAlignmentOptions.Center;
    }

    static Transform SetupScrollContent(GameObject panel, string title, out TMP_Text emptyText)
    {
        var titleGO  = UIChild(panel, "TXT_Title");
        Rect(titleGO, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0f, -35f, 700f, 60f);
        var titleTMP = Ensure<TextMeshProUGUI>(titleGO);
        titleTMP.text = title; titleTMP.fontSize = 34;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white; titleTMP.alignment = TextAlignmentOptions.Center;

        var svGO = new GameObject("ScrollView", typeof(RectTransform));
        svGO.transform.SetParent(panel.transform, false);
        var svRT = svGO.GetComponent<RectTransform>();
        svRT.anchorMin = new Vector2(0.02f, 0.03f); svRT.anchorMax = new Vector2(0.98f, 0.88f);
        svRT.offsetMin = Vector2.zero; svRT.offsetMax = Vector2.zero;
        Ensure<Image>(svGO).color = new Color(0, 0, 0, 0);
        var sr = Ensure<ScrollRect>(svGO);
        sr.horizontal = false; sr.vertical = true;
        sr.scrollSensitivity = 30f;

        var vpGO = UIChild(svGO, "Viewport");
        Stretch(vpGO);
        var vpImg = Ensure<Image>(vpGO);
        vpImg.color = new Color(0, 0, 0, 0.01f);
        var mask = Ensure<Mask>(vpGO);
        mask.showMaskGraphic = false;
        sr.viewport = vpGO.GetComponent<RectTransform>();

        var contentGO = UIChild(vpGO, "Content");
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero; contentRT.sizeDelta = Vector2.zero;
        var vlg = Ensure<VerticalLayoutGroup>(contentGO);
        vlg.spacing               = 6f;
        vlg.childAlignment        = TextAnchor.UpperCenter;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight= false;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        var csf = Ensure<ContentSizeFitter>(contentGO);
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = contentRT;

        var emptyGO  = UIChild(panel, "TXT_Empty");
        Rect(emptyGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 0f, 500f, 50f);
        emptyText = Ensure<TextMeshProUGUI>(emptyGO);
        emptyText.text = "Aucune entree pour l'instant.";
        emptyText.fontSize = 20; emptyText.color = new Color(0.5f, 0.5f, 0.5f);
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyGO.SetActive(false);

        return contentGO.transform;
    }

    static void SetupSettingsContent(GameObject panel,
        out Slider musicSlider, out Slider sfxSlider,
        out TMP_Text musicLabel, out TMP_Text sfxLabel)
    {
        var titleGO  = UIChild(panel, "TXT_Title");
        Rect(titleGO, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0f, -35f, 600f, 60f);
        var titleTMP = Ensure<TextMeshProUGUI>(titleGO);
        titleTMP.text = "PARAMETRES"; titleTMP.fontSize = 34;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white; titleTMP.alignment = TextAlignmentOptions.Center;

        // Musique
        var mlbGO = UIChild(panel, "TXT_MusicLabel");
        Rect(mlbGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 80f, 500f, 38f);
        musicLabel = Ensure<TextMeshProUGUI>(mlbGO);
        musicLabel.text = "Musique : 100%"; musicLabel.fontSize = 22;
        musicLabel.color = Color.white; musicLabel.alignment = TextAlignmentOptions.Center;

        var mSliderGO = MakeSlider(panel, "Slider_Music", new Color(0.3f, 0.6f, 1f));
        Rect(mSliderGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, 35f, 500f, 28f);
        musicSlider = mSliderGO.GetComponent<Slider>();

        // Effets
        var slbGO = UIChild(panel, "TXT_SFXLabel");
        Rect(slbGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -35f, 500f, 38f);
        sfxLabel = Ensure<TextMeshProUGUI>(slbGO);
        sfxLabel.text = "Effets : 100%"; sfxLabel.fontSize = 22;
        sfxLabel.color = Color.white; sfxLabel.alignment = TextAlignmentOptions.Center;

        var sSliderGO = MakeSlider(panel, "Slider_SFX", new Color(1f, 0.65f, 0.2f));
        Rect(sSliderGO, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0f, -80f, 500f, 28f);
        sfxSlider = sSliderGO.GetComponent<Slider>();

        // Bouton Effacer données
        var clearGO = MakeButton(panel, "BTN_Clear", "Effacer classement + succes",
                                  new Color(0.4f, 0.1f, 0.1f));
        Rect(clearGO, 0.5f, 0f, 0.5f, 0f, 0.5f, 0f, 0f, 55f, 380f, 50f);
    }

    static void WireMenuBtn(SerializedObject so, string propName, GameObject container, string childName)
    {
        var child = container.transform.Find(childName);
        if (child == null) return;
        var btn = child.GetComponent<Button>();
        if (btn == null) return;
        var prop = so.FindProperty(propName);
        if (prop != null) { prop.objectReferenceValue = btn; so.ApplyModifiedProperties(); }
    }

    static void AddToBuildSettings(string scenePath, int index)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        // Retire si déjà présent
        scenes.RemoveAll(s => s.path == scenePath);

        var newScene = new EditorBuildSettingsScene(scenePath, true);
        if (index >= scenes.Count) scenes.Add(newScene);
        else                       scenes.Insert(index, newScene);

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
