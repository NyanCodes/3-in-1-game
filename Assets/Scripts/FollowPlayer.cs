using UnityEngine;

namespace ThreeInOneGame.Driving
{
public class FollowPlayer : MonoBehaviour
{
  public GameObject player;
private Vector3 offset = new Vector3(0, 5, -10);
    void Start()
  {
    
  }
    void Update()
    {
        transform.position = player.transform.position + offset;
    }
}
}
