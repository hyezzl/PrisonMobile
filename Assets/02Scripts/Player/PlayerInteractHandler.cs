using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자원은 플레이어 근처와 닿으면 Interact,
/// Zone은 플레이어의 몸이 실제구역안에 들어가면 Interact
/// </summary>
/// 
public class PlayerInteractHandler : MonoBehaviour
{
    // 같은 오브젝트에 있는 컴포넌트를 미리 들고있음 (허브)
    public StackManager stackManager { get; private set; }
    private PlayerController pc;

    [Header("레벨별 채집")]
    [SerializeField] private List<GameObject> rangeColliders; // 레벨별 콜라이더 오브젝트
    public List<float> rangeSpeeds;         // 레벨별 채집속도

    [Header("Layer Settings")]
    [SerializeField] private LayerMask targetLayer;    // 자원 레이어 
    [SerializeField] private LayerMask zoneLayer;       // Zone 레이어

    private bool isInteract = false;

    // 현재 콜라이더 안에 들어와 있는 타겟
    private List<Collider> curTargets = new List<Collider>();


    private void Awake()
    {
        stackManager = GetComponent<StackManager>();
        pc = GetComponent<PlayerController>();

        // 시작할 때 현재 레벨에 맞는 콜라이더만 켜기
        UpdateInteractionCollider(pc.GetLevel());
    }


    private void Update()
    {
        // 타겟이 있고 채집 중이 아니면 루프 시작
        if (curTargets.Count > 0 && !isInteract)
        {
            // Null이 된 타겟(이미 파괴된 오브젝트 등) 정리
            curTargets.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);

            if (curTargets.Count > 0)
                StartCoroutine(InteractLoop());
        }
    }



    // 자원과의 Interact
    private IEnumerator InteractLoop()
    {
        isInteract = true;

        while (true) 
        {
            // 루프 시작 시점에 이미 파괴되었거나 비활성화된 타겟들을 먼저 리스트에서 제거
            curTargets.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy || !t.enabled);

            if (curTargets.Count == 0) break;   // 리스트가 비었으면 루프종료


            int curLevel = pc.GetLevel();
            float curSpeed = rangeSpeeds[curLevel];

            pc.SetInteract(true);


            // ///////////////////////////////////////////
            //          레벨별 채집
            //////////////////////////////////////////////
            if (curLevel == 0)
            {
                // 레벨 0: 범위 내에서 가장 정면 하나만
                IActionTarget best = GetBestTargetFromList();
                if (best != null)
                { 
                    best.Interact(this);

                    // 사운드 (단일)
                    SoundManager.Instance.PlaySFX((int)SFXType.Hit);
                }
                else
                    break; // 혹시라도 타겟을 못 찾으면 즉시 루프 탈출
            }
            else
            {
                // 루프 도중 리스트 변형 방지를 위해 복사본 사용
                var targetsToProcess = curTargets.ToArray();
                foreach (var col in targetsToProcess)
                {
                    if (col != null && col.gameObject.activeInHierarchy)
                    {
                        IActionTarget target = col.GetComponent<IActionTarget>();
                        target?.Interact(this);

                        // 사운드 (단일)
                        SoundManager.Instance.PlaySFX((int)SFXType.Hit);

                        yield return new WaitForSeconds(0.05f);
                    }
                }
            }

            // 쿨타임 적용
            yield return new WaitForSeconds(curSpeed * 0.6f);

            // 한 사이클 이후 리스트 재정리
            curTargets.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);    // 리스트 재정리
        }

        pc.SetInteract(false);
        isInteract = false;
    }

    //=================================================
    // Zone과의 Interact
    public void OnTriggerStay(Collider other)
    {
        // 닿은 물체가 Zone 레이어인지 확인
        if (((1 << other.gameObject.layer) & zoneLayer) != 0)
        {
            // 부딪힌 존(Zone)의 상호작용 인터페이스 가져오기
            IActionTarget zone = other.GetComponent<IActionTarget>();

            if (zone != null)
            {
                zone.Interact(this);
            }
        }
    }



    //=================================================
    // 자원과의 Interact
    public void OnTriggerEnter(Collider other)
    {
        // 자원 레이어라면 리스트에 추가
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            // 플레이어와 충돌한것은 제거
            if (other.transform.root != transform.root)
            {
                if (!curTargets.Contains(other))
                {
                    curTargets.Add(other);
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        // 범위를 벗어나면 리스트에서 제거
        if (curTargets.Contains(other))
        {
            curTargets.Remove(other);
        }
    }


    // 시야콜라이더 레벨따라서 제어
    // 레벨업 시 호출
    public void UpdateInteractionCollider(int level)
    {
        for (int i = 0; i < rangeColliders.Count; i++)
        {
            if (rangeColliders[i] != null)
                rangeColliders[i].SetActive(i == level);
        }
        curTargets.Clear();
    }


    // 가장 정면에 가까운 대상 찾기
    private IActionTarget GetBestTargetFromList()
    {
        IActionTarget closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in curTargets)
        {
            if (col == null) continue;
            IActionTarget target = col.GetComponent<IActionTarget>();

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = target;
            }
        }
        return closest;
    }

}
