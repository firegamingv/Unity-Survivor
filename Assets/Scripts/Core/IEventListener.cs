/// <summary>
/// Contrat générique pour tout abonné à l'EventBus.
/// Implémenter cette interface sur un MonoBehaviour pour écouter un événement.
/// </summary>
public interface IEventListener<T> where T : IGameEvent
{
    void OnEvent(T gameEvent);
}
