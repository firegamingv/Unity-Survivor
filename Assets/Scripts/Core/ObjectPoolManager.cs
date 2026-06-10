using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestionnaire de pools d'objets générique.
/// Interdit tout Instantiate/Destroy dans le code de gameplay.
/// Tous les objets fréquents (ennemis, projectiles, XPOrbs, VFX) passent par ici.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static ObjectPoolManager Instance { get; private set; }

    // ─── Pools ────────────────────────────────────────────────────────────────
    // Clé : prefab source → Valeur : file d'objets désactivés disponibles
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools
        = new Dictionary<GameObject, Queue<GameObject>>();

    // Reverse lookup : instance → prefab source (pour Release)
    private readonly Dictionary<GameObject, GameObject> _prefabLookup
        = new Dictionary<GameObject, GameObject>();

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Pas de DontDestroyOnLoad : jeu mono-scène.
    }

    // ─── API publique ─────────────────────────────────────────────────────────

    /// <summary>
    /// Récupère un objet du pool (ou l'instancie si le pool est vide).
    /// Positionne l'objet AVANT de l'activer pour éviter les triggers au mauvais endroit.
    /// Appelle OnSpawn() automatiquement.
    /// </summary>
    public T Get<T>(GameObject prefab, Vector3 spawnPosition) where T : MonoBehaviour
    {
        EnsurePool(prefab);

        GameObject obj = _pools[prefab].Count > 0
            ? _pools[prefab].Dequeue()
            : CreateNew(prefab);

        // Position AVANT SetActive pour qu'aucun trigger ne se déclenche à (0,0,0)
        obj.transform.position = spawnPosition;
        obj.SetActive(true);

        if (obj.TryGetComponent<IPoolable>(out var poolable))
            poolable.OnSpawn();

        return obj.GetComponent<T>();
    }

    /// <summary>Surcharge sans position (spawn à (0,0,0) — à éviter pour les projectiles).</summary>
    public T Get<T>(GameObject prefab) where T : MonoBehaviour
        => Get<T>(prefab, Vector3.zero);

    /// <summary>
    /// Retourne un objet dans son pool.
    /// Appelle OnDespawn() automatiquement.
    /// </summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        if (obj.TryGetComponent<IPoolable>(out var poolable))
            poolable.OnDespawn();

        obj.SetActive(false);

        if (_prefabLookup.TryGetValue(obj, out var prefab))
        {
            _pools[prefab].Enqueue(obj);
        }
        else
        {
            // Sécurité : si l'objet n'a pas de prefab connu, on détruit
            Debug.LogWarning($"[ObjectPoolManager] Prefab inconnu pour {obj.name}. Destruction.");
            Destroy(obj);
        }
    }

    /// <summary>Surcharge pratique pour passer directement un Component.</summary>
    public void Release(Component component)
    {
        if (component != null) Release(component.gameObject);
    }

    /// <summary>
    /// Pré-remplit le pool pour éviter les instanciations en cours de jeu.
    /// Appeler depuis Bootstrap ou GameManager.Start().
    /// </summary>
    public void PreWarm(GameObject prefab, int count)
    {
        EnsurePool(prefab);
        for (int i = 0; i < count; i++)
        {
            GameObject obj = CreateNew(prefab);
            if (obj.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnDespawn();
            obj.SetActive(false);
            _pools[prefab].Enqueue(obj);
        }
    }

    // ─── Privé ────────────────────────────────────────────────────────────────
    private void EnsurePool(GameObject prefab)
    {
        if (!_pools.ContainsKey(prefab))
            _pools[prefab] = new Queue<GameObject>();
    }

    private GameObject CreateNew(GameObject prefab)
    {
        // Spawn hors-écran pour éviter tout trigger pendant la création
        GameObject obj = Instantiate(prefab, new Vector3(0f, -9999f, 0f), Quaternion.identity);
        obj.SetActive(false);
        obj.name = prefab.name;
        _prefabLookup[obj] = prefab;
        return obj;
    }
}
