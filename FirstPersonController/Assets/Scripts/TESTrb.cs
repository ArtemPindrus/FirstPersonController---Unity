using System.Linq;
using UnityEngine;

public class TESTrb : MonoBehaviour
{
    [SerializeField] private Vector3 direction;
    [SerializeField] private float speed;

    [SerializeField] private bool sweep;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        rb.velocity = direction * speed;


        if (sweep) {
            RaycastHit[] hits = rb.SweepTestAll(direction, 0.1f);

            if (hits.Any(x => x.collider.TryGetComponent(out CharacterController _))) {
                Debug.Log("controller found");
                rb.velocity = Vector3.zero;
            } 
        }
    }
}
