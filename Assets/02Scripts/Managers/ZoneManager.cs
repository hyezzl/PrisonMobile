using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class ConsumeZoneData
{
    public string entryEventID; // 이 존을 깨울 이벤트 ID
    public ConsumeZone zone;    // 대상 ConsumeZone
}

public class ZoneManager : MonoBehaviour
{
    [Header("관리할 소비 존들")]
    [SerializeField] private List<ConsumeZoneData> waitingZones = new List<ConsumeZoneData>();

    private void Awake()
    {
        // 시작할 때 entryEventID가 설정된 존들은 일단 다 꺼둡니다.
        foreach (var data in waitingZones)
        {
            if (data.zone != null && !string.IsNullOrEmpty(data.entryEventID))
            {
                data.zone.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameEvents.StartEvent>(OnCheckZoneEvent);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameEvents.StartEvent>(OnCheckZoneEvent);
    }

    private void OnCheckZoneEvent(GameEvents.StartEvent evt)
    {
        // 들어온 이벤트 ID와 일치하는 모든 존을 활성화
        foreach (var data in waitingZones)
        {
            if (data.entryEventID == evt.eventID)
            {
                ActivateZone(data.zone);
            }
        }
    }

    private void ActivateZone(ConsumeZone targetZone)
    {
        if (targetZone == null || targetZone.gameObject.activeSelf) return;

        // 오브젝트 활성화
        targetZone.gameObject.SetActive(true);
    }
}
