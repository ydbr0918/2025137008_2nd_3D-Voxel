using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PowerManager : MonoBehaviour
{
    [Header("Lever / Power")]
    public int requiredLevers = 3;      // 필요한 레버 수
    public UnityEvent onPowered;        // 전력 활성화 시 1회 호출

    HashSet<string> activated = new HashSet<string>();
    bool powered = false;

    public bool IsPowered => powered;

    // 레버에서 호출
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
