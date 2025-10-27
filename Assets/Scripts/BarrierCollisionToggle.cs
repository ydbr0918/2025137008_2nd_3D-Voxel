using UnityEngine;



public class BarrierCollisionToggle : MonoBehaviour
{
    [Header("Refs")]
    public CameraSwitch camSwitch;          // �װ� ���� ī�޶� ����ġ

    [Header("Layer Names")]
    public string playerLayerName = "Player";
    public string barrierLayerName = "Barrier";   // ���� ���̾�

    int playerLayer = -1;
    int barrierLayer = -1;
    bool lastIsFirstPerson;

    void Awake()
    {
        if (!camSwitch && Camera.main) camSwitch = Camera.main.GetComponent<CameraSwitch>();

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        barrierLayer = LayerMask.NameToLayer(barrierLayerName);

        if (playerLayer < 0 || barrierLayer < 0)
        {
            Debug.LogError("[BarrierCollisionToggle] Layer �̸��� Ȯ���ϼ���. (Player, Barrier)");
            enabled = false;
            return;
        }

        // ���� ���� �ݿ�
        lastIsFirstPerson = camSwitch ? camSwitch.IsFirstPerson : false;
        ApplyState(lastIsFirstPerson);
    }

    void Update()
    {
        if (!camSwitch) return;
        if (camSwitch.IsFirstPerson != lastIsFirstPerson)
        {
            lastIsFirstPerson = camSwitch.IsFirstPerson;
            ApplyState(lastIsFirstPerson);
        }
    }

    void ApplyState(bool isFirstPerson)
    {
        // 3��Ī(= isFirstPerson == false)�� �� Barrier �浹 ����
        bool ignoreBarrier = !isFirstPerson;

        Physics.IgnoreLayerCollision(playerLayer, barrierLayer, ignoreBarrier);

        // ����: ����� �����̹Ƿ� �� �� ȣ��� ���
        // �Ϲ� ��(Wall)�� �մ��� ���� �� �׻� �浹�� (���̾� ��Ʈ���� �⺻��)
#if UNITY_EDITOR
        Debug.Log($"[BarrierCollisionToggle] IsFP={isFirstPerson} �� Ignore(Player,Barrier)={ignoreBarrier}");
#endif
    }
}
