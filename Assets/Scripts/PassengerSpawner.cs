using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    public GameObject passengerPrefab;
    public Transform cartAnchor;
    public Vector3[] seatOffsets; // Set in Inspector
    
    void Start()
    {
        Rigidbody cartRb = cartAnchor.GetComponent<Rigidbody>();
        
        foreach (Vector3 offset in seatOffsets)
        {
            // Calculate seat position
            Vector3 seatPosition = cartAnchor.TransformPoint(offset);
            Quaternion seatRotation = cartAnchor.rotation;
            
            // Instantiate passenger
            GameObject passenger = Instantiate(
                passengerPrefab, 
                seatPosition, 
                seatRotation
            );
            
            // Get body reference
            Transform body = passenger.transform.GetChild(0);
            
            // Configure FixedJoint
            FixedJoint joint = body.GetComponent<FixedJoint>();
            joint.connectedBody = cartRb;
            
            // Parent under cart for organization
            // passenger.transform.SetParent(cartAnchor);
        }
    }
}