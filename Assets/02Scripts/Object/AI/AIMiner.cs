using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMiner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AIMinerState currentState = AIMinerState.Idle;
    [SerializeField] private float miningSpeed = 2.0f; // 플레이어보다 느림
    [SerializeField] private float searchRadius = 20f;      // 시야범위
    [SerializeField] private LayerMask resourceLayer;

    private NavMeshAgent agent;
    private Animator anim;
    private Rocks target;
    private bool isWorking = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }


    private void Update()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f && agent.remainingDistance > agent.stoppingDistance;
        anim.SetBool("isMove", isMoving);

        if (isWorking) return;

        switch (currentState)
        {
            case AIMinerState.Idle:
                SearchNextTarget();
                break;

            case AIMinerState.MoveTo:
                CheckArrival();
                break;
        }
    }

    // 바위 찾기
    private void SearchNextTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, resourceLayer);

        float closestDist = Mathf.Infinity;
        Rocks closestRock = null;

        foreach (var hit in hits)
        {
            Rocks rock = hit.GetComponent<Rocks>();

            // 캘수있는 상태인지 확인
            if (rock != null && rock.CanInteract)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestRock = rock;
                }
            }
        }

        if (closestRock != null)
        {
            target = closestRock;
            agent.SetDestination(target.transform.position);
            currentState = AIMinerState.MoveTo;
        }
    }


    // 2. 바위에 도착했는지 확인
    private void CheckArrival()
    {
        if (target == null || !target.CanInteract)
        {
            currentState = AIMinerState.Idle;
            return;
        }

        // 목적지에 도착했는지 판단 (남은 거리가 정지 거리보다 작을 때)
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(MiningProcess());
        }
    }

    // 3. 자원 캐기 연출 및 로직
    private IEnumerator MiningProcess()
    {
        isWorking = true;
        currentState = AIMinerState.Mining;

        // 애니메이션
        anim.SetBool("mining", true);

        SoundManager.Instance.PlaySFX(0);

        // 바위 앞에 멈춰서 채집 시간만큼 대기
        agent.isStopped = true;

        yield return new WaitForSeconds(miningSpeed);

        if (target != null && target.CanInteract)
        {
            // 매개변수로 null을 보내서 AI임을 알림
            target.Interact(null);
        }
        anim.SetBool("mining", false);

        agent.isStopped = false;
        isWorking = false;

        currentState = AIMinerState.Idle;
    }
}
