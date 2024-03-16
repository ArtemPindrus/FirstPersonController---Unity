using UnityEngine;

public class BoxCast : MonoBehaviour
{
    [SerializeField] private Vector3 extends;
    [SerializeField] private Vector3 direction;
    [SerializeField] private float distance;

    [SerializeField] private bool debug;

    private RaycastHit _hit;

    public BoxCast Initialize(Vector3 extends, Vector3 direction, float distance, bool debug = false) {
        this.extends = extends;
        this.direction = direction;
        this.distance = distance;

        this.debug = debug;

        return this;
    }

    public RaycastHit Hit => _hit;

    public bool HitLastFrame => Hit.collider != null;

    private void Update() {
        Physics.BoxCast(transform.position, extends, direction, out _hit, Quaternion.identity, distance);

        if (debug) Debug.Log($"Ray hit of {gameObject}: {HitLastFrame}");
    }

    private void OnDrawGizmos() {
        if (debug) {
            Gizmos.DrawCube(transform.position, extends * 2);
        }
    }
}
