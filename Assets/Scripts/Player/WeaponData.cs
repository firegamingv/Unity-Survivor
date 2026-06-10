using UnityEngine;

/// <summary>
/// Données d'une arme. Créer via :
/// Assets → clic droit → Create → EFRITY/Weapons/WeaponData
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "EFRITY/Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Stats")]
    [Tooltip("Dégâts de base par projectile")]
    public float BaseDamage      = 20f;
    [Tooltip("Attaques par seconde")]
    public float BaseAttackSpeed = 1f;
    [Tooltip("Rayon de détection des ennemis")]
    public float BaseRange       = 8f;

    [Header("Projectile")]
    [Tooltip("Prefab du projectile à instancier (via ObjectPool)")]
    public GameObject ProjectilePrefab;
}
