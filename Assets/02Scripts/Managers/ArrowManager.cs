using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ArrowType
{ 
    Direction,
    Floating,
}

[System.Serializable]
public class ArrowTargetData
{
    public string eventID;
    public ArrowType type;
    public GameObject arrow;
    public Transform target;     // 가리킬 목적지
}

public class ArrowManager : MonoBehaviour
{

    [Header("화살표 설정")]
    [SerializeField] private List<ArrowTargetData> arrowDataList = new List<ArrowTargetData>();

    // 빠른검색을 위한 딕셔너리
    private Dictionary<string, List<ArrowTargetData>> arrows = new Dictionary<string, List<ArrowTargetData>>();

    // 현 시점 켜질 화살표
    private List<ArrowTargetData> curArrows = new List<ArrowTargetData>();



    private void Awake()
    {
        // 리스트데이터를 딕셔너리로 변환
        foreach (var data in arrowDataList)
        {
            if (!arrows.ContainsKey(data.eventID))
                arrows.Add(data.eventID, new List<ArrowTargetData>());

            arrows[data.eventID].Add(data);
            if (data.arrow != null) data.arrow.SetActive(false);
        }
    }

    private void Start()
    {
        SetArrowTarget("E001");
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameEvents.StartEvent>(OnStartEvent);
        EventBus.Instance.Subscribe<GameEvents.ClearArrow>(OnClearArrow);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameEvents.StartEvent>(OnStartEvent);
        EventBus.Instance.Unsubscribe<GameEvents.ClearArrow>(OnClearArrow);
    }

    private void Update()
    {
        foreach (var active in curArrows)
        {
            // 타입이 Direction이고,회전시킬 대상(부모)이 존재하며, 목적지(target)가 설정되어 있을 때만 실행
            if (active.type == ArrowType.Direction && active.arrow != null && active.target != null)
            {
                // 타겟 방향 계산 (부모 위치 기준)
                Vector3 dir = active.target.position - active.arrow.transform.position;

                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    active.arrow.transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }
    }

    // 이벤트가 들어왔을때
    private void OnStartEvent(GameEvents.StartEvent evt)
    {
        SetArrowTarget(evt.eventID);
    }


    // 외부에서 호출할 함수
    public void SetArrowTarget(string eventID)
    {
        // 기존에 켜져있던 화살표가 있다면 끄기
        foreach (var pair in arrows)
        {
            foreach (var data in pair.Value)
            {
                if (data.arrow != null)
                    data.arrow.SetActive(false);
            }
        }

        curArrows.Clear();

        // 데이터 검색
        if (arrows.TryGetValue(eventID, out List<ArrowTargetData> newList))
        {
            foreach (var data in newList)
            {
                if (data.arrow != null)
                {
                    data.arrow.SetActive(true);
                    curArrows.Add(data); // 활성 리스트에 추가
                }
            }
        }
    }


    private void OnClearArrow(GameEvents.ClearArrow evt) {
        ClearArrows();
    }


    // 모든 화살표를 즉시 끄는 함수
    public void ClearArrows()
    {
        foreach (var pair in arrows)
        {
            foreach (var data in pair.Value)
            {
                if (data.arrow != null) data.arrow.SetActive(false);
            }
        }
        curArrows.Clear();
    }

}
