using System.Collections.Generic;

/// <summary>
/// Système publish/subscribe générique.
/// Aucun script n'a besoin de référence directe à un autre pour communiquer.
///
/// Usage — publier un événement :
///   EventBus&lt;PlayerDamagedEvent&gt;.Publish(new PlayerDamagedEvent { Damage = 10f });
///
/// Usage — s'abonner (dans un MonoBehaviour) :
///   1. Implémenter IEventListener&lt;PlayerDamagedEvent&gt;
///   2. OnEnable()  → EventBus&lt;PlayerDamagedEvent&gt;.Subscribe(this);
///   3. OnDisable() → EventBus&lt;PlayerDamagedEvent&gt;.Unsubscribe(this);
///   4. public void OnEvent(PlayerDamagedEvent e) { ... }
/// </summary>
public static class EventBus<T> where T : IGameEvent
{
    private static readonly HashSet<IEventListener<T>> _listeners = new HashSet<IEventListener<T>>();

    /// <summary>Abonne un listener à cet événement.</summary>
    public static void Subscribe(IEventListener<T> listener)
    {
        _listeners.Add(listener);
    }

    /// <summary>Désabonne un listener. Toujours appeler dans OnDisable().</summary>
    public static void Unsubscribe(IEventListener<T> listener)
    {
        _listeners.Remove(listener);
    }

    /// <summary>
    /// Publie l'événement à tous les listeners abonnés.
    /// Copie défensive du HashSet pour éviter les erreurs si un listener
    /// se désinscrit pendant l'itération.
    /// </summary>
    public static void Publish(T gameEvent)
    {
        foreach (var listener in new HashSet<IEventListener<T>>(_listeners))
        {
            listener.OnEvent(gameEvent);
        }
    }
}
