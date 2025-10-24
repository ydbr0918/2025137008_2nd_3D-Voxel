using UnityEngine;
using System.Collections.Generic;

public class RoomVisibility : MonoBehaviour
{
    public static RoomVisibility CurrentRoom;
    public List<GameObject> roomObjects = new List<GameObject>(); // �� ���� ������Ʈ��

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CurrentRoom != this)
        {
            // ���� �� ����
            if (CurrentRoom) CurrentRoom.SetActive(false);
            // �� �� �ѱ�
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
