using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PowerManager : MonoBehaviour
{
    [Header("Lever / Power")]
    public int requiredLevers = 3;      // �ʿ��� ���� ��
    public UnityEvent onPowered;        // ���� Ȱ��ȭ �� 1ȸ ȣ��

    HashSet<string> activated = new HashSet<string>();
    bool powered = false;

    public bool IsPowered => powered;

    // �������� ȣ��
    public void ActivateLever(string leverId)
    {
        if (powered) return;
        if (string.IsNullOrEmpty(leverId)) leverId = System.Guid.NewGuid().ToString();

        if (activated.Add(leverId))
        {
            if (activated.Count >= requiredLevers)
            {
                powered = true;
                onPowered?.Invoke();
#if UNITY_EDITOR
                Debug.Log("[PowerManager] POWERED ON");
#endif
            }
        }
    }
}
