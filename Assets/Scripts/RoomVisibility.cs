using UnityEngine;
using System.Collections.Generic;

public class RoomVisibility : MonoBehaviour
{
    public static RoomVisibility CurrentRoom;
    public List<GameObject> roomObjects = new List<GameObject>(); // 이 방의 오브젝트들

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CurrentRoom != this)
        {
            // 이전 방 끄기
            if (CurrentRoom) CurrentRoom.SetActive(false);
            // 새 방 켜기
            CurrentRoom = this;
            SetActive(true);
        }
    }

    public void SetActive(bool active)
    {
        foreach (var o in roomObjects)
            if (o) o.SetActive(active);
    }
}
