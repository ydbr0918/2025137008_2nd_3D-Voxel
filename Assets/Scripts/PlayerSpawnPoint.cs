using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public static Vector3 position;
    public static Quaternion rotation;

    void Awake()
    {
        position = transform.position;
        rotation = transform.rotation;
    }
}
