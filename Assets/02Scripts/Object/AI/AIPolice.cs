using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class AIPolice : MonoBehaviour
{
    [Header("지점 설정")]
    [SerializeField] private TakeZone takeZone;             // 수갑이 나오는 곳
    [SerializeField] private PrisonerGiveZone giveZone;     // 수갑을 갖다줄 곳
    [SerializeField] private Transform handPivot;           // 손위치

    [Header("설정")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int maxCapacity = 10;      // 최대 소지량

    private NavMeshAgent agent;
    private Animator anim;
    private List<GameObject> aiStack = new List<GameObject>(); // 들고있는 수갑
    private bool isWorking = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (isWorking) return;

        // 1. 등에 수갑이 없고, TakeZone에 수갑이 있을 때 루틴 시작
        if (aiStack.Count == 0 && takeZone.takeStack.Count > 0)
        {
            StartCoroutine(DeliveryRoutine());
        }
    }

    private IEnumerator DeliveryRoutine()
    {
        isWorking = true;

        // TakeZone으로 이동
        yield return MoveToTarget(takeZone.transform.position);

        // 최대 10개까지 한꺼번에 집기
        while (takeZone.takeStack.Count > 0 && aiStack.Count < maxCapacity)
        {
            GameObject item = takeZone.takeStack.Pop();

            item.transform.DOKill();    // 이전트윈삭제
            aiStack.Add(item);

            // 쌓기
            float newY = (aiStack.Count - 1) * 0.2f;
            Vector3 targetLocalPos = new Vector3(0, newY, 0);

            item.transform.SetParent(handPivot);
            item.transform.localRotation = Quaternion.identity;
            item.transform.DOLocalJump(targetLocalPos, 1f, 1, 0.3f);

            yield return new WaitForSeconds(0.2f); // 하나씩 착착 쌓이는 시간
        }

        // PrisonerGiveZone으로 이동
        yield return MoveToTarget(giveZone.transform.position);

        if (aiStack.Count > 0)
        {
            giveZone.DeployHandcuffs(aiStack);

            aiStack.Clear(); // 리스트 비우기
            yield return new WaitForSeconds(0.5f);
        }

        isWorking = false;
    }

    private IEnumerator MoveToTarget(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);
        anim.SetBool("isMove", true);

        while (true)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                anim.SetBool("isMove", false);
                break;
            }
            yield return null;
        }
    }
}
