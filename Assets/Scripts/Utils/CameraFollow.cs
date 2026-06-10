using UnityEngine;

/// <summary>
/// Suit le joueur en douceur avec un lerp.
/// Attacher sur la Main Camera.
/// Assigner le Transform du Player dans l'Inspector.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private Transform _target;

    [Header("Smoothing")]
    [SerializeField] private float _smoothSpeed = 8f;

    [Header("Offset (garder Z = -10 pour 2D)")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desiredPos = _target.position + _offset;

        // Garde le Z fixe (la caméra ne doit pas se rapprocher des sprites)
        desiredPos.z = _offset.z;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            _smoothSpeed * Time.deltaTime);
    }

    /// <summary>Permet d'assigner la cible depuis le code (ex: GameManager).</summary>
    public void SetTarget(Transform target) => _target = target;
}
