using UnityEngine;

namespace ThreeInOneGame.Flying
{
public class SpinPropellerX : MonoBehaviour
{
    // Adjust this value in the inspector to make it spin faster or slower
    public float spinSpeed = 1000f; 

    void Update()
    {
        // Rotates the object around the Z axis (Vector3.forward is (0, 0, 1))
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
    }
}
}
