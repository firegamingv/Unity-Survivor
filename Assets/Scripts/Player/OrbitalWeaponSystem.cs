using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère les lames orbitales autour du joueur.
/// Ajouté dynamiquement par WeaponSystem quand l'upgrade OrbitalWeapon est appliqué.
/// </summary>
public class OrbitalWeaponSystem : MonoBehaviour
{
    private readonly List<OrbitalBlade> _blades = new List<OrbitalBlade>();
    private WeaponSystem _weaponSystem;

    // ─── API ──────────────────────────────────────────────────────────────────
    public void AddBlades(int count, WeaponSystem weaponSystem)
    {
        _weaponSystem = weaponSystem;

        int totalAfter = _blades.Count + count;
        for (int i = 0; i < count; i++)
        {
            // Angle initial espacé uniformément parmi toutes les lames futures
            float startAngle = _blades.Count * (360f / totalAfter);
            CreateBlade(startAngle);
        }
    }

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (_weaponSystem == null) return;

        // Les lames suivent les dégâts actuels du joueur (buffs compris)
        float damage = _weaponSystem.AttackDamage * 0.8f;
        foreach (var blade in _blades)
            blade.SetDamage(damage);
    }

    // ─── Privé ────────────────────────────────────────────────────────────────
    private void CreateBlade(float startAngle)
    {
        GameObject go = new GameObject("OrbitalBlade");
        go.layer = gameObject.layer;

        // Visuel : rectangle blanc coloré en or (à remplacer par un vrai sprite)
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = BuildBladeSprite();
        sr.color        = new Color(1f, 0.82f, 0.1f);   // or
        sr.sortingOrder = 5;

        // Collision
        var col    = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.35f;

        var blade = go.AddComponent<OrbitalBlade>();
        float damage = _weaponSystem != null ? _weaponSystem.AttackDamage * 0.8f : 20f;
        blade.Init(transform, startAngle, damage);

        _blades.Add(blade);
    }

    /// <summary>Crée un sprite rectangulaire 8×3 pixels utilisé comme visuel par défaut.</summary>
    private static Sprite BuildBladeSprite()
    {
        const int W = 8, H = 3;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var pixels = new Color[W * H];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        // Pointes effilées aux extrémités
        pixels[0] = pixels[2] = pixels[W * (H - 1)] = pixels[W * H - 1] = new Color(1, 1, 1, 0.4f);
        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), W);
    }
}
