using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject[] rooms; // Room ∑Á∆ÆµÈ
    public void OnEnterRoom(string roomName)
    {
        foreach (var r in rooms) r.SetActive(r.name == roomName);
    }
}
