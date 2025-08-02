using UnityEngine;
using System.Collections;

public class JointStabilizer : MonoBehaviour
{
    public bool stabilizeOnStart = true;
    public float stabilizationTime = 0.1f;

    void Start() {
        if (stabilizeOnStart) StartCoroutine(Stabilize());
    }
    IEnumerator Stabilize() {
        Rigidbody rb = GetComponent<Rigidbody>();
        CharacterJoint joint = GetComponent<CharacterJoint>();
        Vector3 targetPosition = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);

        // Force position
        rb.transform.position = targetPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Freeze temporarily
        rb.isKinematic = true;
        yield return new WaitForSeconds(stabilizationTime);
        rb.isKinematic = false;
    }

}
