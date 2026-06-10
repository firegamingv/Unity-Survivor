/// <summary>
/// Types de dégâts utilisés par les projectiles et capacités.
/// Permet d'implémenter des résistances spécifiques par type.
/// </summary>
public enum DamageType
{
    Physical,   // Dégâts physiques — réduits par l'Armor
    Magic,      // Dégâts magiques — ignorent partiellement l'Armor
    True        // Dégâts vrais — ignorent toutes les résistances
}
