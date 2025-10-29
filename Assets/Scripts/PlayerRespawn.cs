using UnityEngine;

public static class PlayerRespawn
{
    public static void Respawn(GameObject player)
    {
        if (player == null) return;
        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        player.transform.SetPositionAndRotation(PlayerSpawnPoint.position, PlayerSpawnPoint.rotation);
    }
}
