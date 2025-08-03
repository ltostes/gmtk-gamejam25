using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    public GameObject passengerPrefab;
    public GameObject cartPrefab;
    public Transform cartAnchor;
    public Vector3[] seatOffsets; // Set in Inspector

    private GameObject cartContainer;

    public float seatRandRange = 0.02f;

    void Start()
    {
        passengerReset();
    }

    [ContextMenu("Reset Passengers")]
    public void passengerReset()
    {
        if (cartContainer != null)
        {
            Destroy(cartContainer);
        }

        // Cart and passenger container
        cartContainer = new GameObject();

        cartContainer.name = "CartContainer";

        // Cart Anchor rb
        Rigidbody cartAnchorRb = cartAnchor.GetComponent<Rigidbody>();

        // Calculate cart position
        Vector3 cartPosition = cartAnchor.TransformPoint(new Vector3(0, 0, 0));
        Quaternion cartRotation = cartAnchor.rotation;

        // Instantiate cart
        GameObject cart = Instantiate(
            cartPrefab,
            cartPosition,
            cartRotation
        );

        cart.transform.SetParent(cartContainer.transform);

        HingeJoint cartHJoint = cart.GetComponent<HingeJoint>();
        cartHJoint.connectedBody = cartAnchorRb;

        Rigidbody cartRb = cart.GetComponent<Rigidbody>();

        foreach (Vector3 offset in seatOffsets)
        {
            // Calculate seat position
            Vector3 seatPosition = cartAnchor.TransformPoint(offset);
            Quaternion seatRotation = cartAnchor.rotation;

            Vector3 seatPosition_WithRandom = seatPosition + new Vector3(Random.value * 2 * seatRandRange - seatRandRange, 0, 0);

            // Instantiate passenger
            GameObject passenger = Instantiate(
                passengerPrefab,
                seatPosition_WithRandom,
                seatRotation
            );

            // Get body reference
            Transform body = passenger.transform.GetChild(0);
            Transform head = passenger.transform.GetChild(1);

            // Configure FixedJoint
            FixedJoint joint = body.GetComponent<FixedJoint>();
            joint.connectedBody = cartRb;

            // Configure parent
            passenger.transform.SetParent(cartContainer.transform);

        }
    }

    public int getLivePassengers()
    {
        return cartContainer.transform.GetComponentsInChildren<CharacterJoint>().Length;
    }

    [ContextMenu("Kill a passenger")]
    public void killAPassenger()
    {
        CharacterJoint[] joints = cartContainer.transform.GetComponentsInChildren<CharacterJoint>();
        CharacterJoint firstJoint = joints[0];
        firstJoint.breakForce = 1;
    }
}