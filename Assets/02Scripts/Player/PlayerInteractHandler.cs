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

    [Header("Interaction Settings")]
    [SerializeField] private float range = 1.8f;       // 감지 범위
    [SerializeField] private float interactCooldown = 1.0f;    // 채집 간격 (1초)
    [SerializeField] private LayerMask targetLayer;    // 자원 레이어 
    [SerializeField] private LayerMask zoneLayer;       // Zone 레이어

    private bool isInteract = false;


    private void Awake()
    {
        stackManager = GetComponent<StackManager>();
        pc = GetComponent<PlayerController>();
    }


    private void Update()
    {
        // 자원탐색
        Collider[] cols = Physics.OverlapSphere(transform.position, range, targetLayer);

        // 자원 있고, 채집중 아니라면
        if (cols.Length > 0 && !isInteract)
        {
            StartCoroutine(InteractLoop());
        }
    }



    // 자원과의 Interact
    private IEnumerator InteractLoop()
    {
        isInteract = true;

        while (true) 
        {
            // 주변 타켓 확인
            Collider[] targets = Physics.OverlapSphere(transform.position, range, targetLayer);
            
            if (targets.Length == 0) break;     // 자원이 없으면 멈춤


            // 가장 가까운 타겟
            IActionTarget forwardTarget = GetBestTarget(targets);
            if(forwardTarget == null) break;

            // 상호작용
            pc.SetInteract(true);

            yield return new WaitForSeconds(0.3f);

            // 레벨에 따른 처리
            if (pc.GetLevel() == 0)
            {
                // 가까운 하나만
                var target = targets[0].GetComponent<IActionTarget>();
                target?.Interact(this);
            }
            else
            { 
                // 추후 ~
            }

            // 쿨타임 적용
            yield return new WaitForSeconds(interactCooldown - 0.3f);
        }

        pc.SetInteract(false);
        isInteract = false;
    }

    // Zone과의 Interact
    private void OnTriggerStay(Collider other)
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


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }


    // 가장 정면에 가까운 대상 찾기
    private IActionTarget GetBestTarget(Collider[] targets)
    {
        IActionTarget closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in targets)
        {
            IActionTarget target = col.GetComponent<IActionTarget>();
            if (target == null) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);

            // 정면 각도 체크
            Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToTarget);

            // dot > 0 이면 내 앞쪽, 0.5f 정도면 대략 전방 60도 이내
            if (dot > 0.5f)
            {
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = target;
                }
            }
        }
        return closest;
    }
}
