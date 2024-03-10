using UnityEngine;

public class CharControllerVelocityVisualizer : MonoBehaviour
{
    private CharacterController cct;
    private void Awake() { 
        cct = GetComponentInParent<CharacterController>();
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, transform.position + cct.velocity);
    }
}
