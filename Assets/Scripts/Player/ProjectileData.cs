using UnityEngine;

/// <summary>
/// Données d'un projectile. Créer via :
/// Assets → clic droit → Create → EFRITY/Weapons/ProjectileData
/// Assigné dans le prefab du projectile.
/// </summary>
[CreateAssetMenu(fileName = "NewProjectile", menuName = "EFRITY/Weapons/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [Tooltip("Vitesse de déplacement en unités/seconde")]
    public float Speed    = 12f;

    [Tooltip("Durée de vie max en secondes (avant auto-destruction)")]
    public float Lifetime = 3f;

    [Tooltip("VFX optionnel à spawner à l'impact (peut être null)")]
    public GameObject HitFXPrefab;
}
