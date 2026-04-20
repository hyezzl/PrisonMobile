using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class Prisoner : MonoBehaviour
{
    [Header("UI연결")]
    public PrisonerUI ui;

    [Header("Pay Money")]
    public GameObject moneyPrefab;
    [HideInInspector]
    public MoneyTakeZone targetZone;


    public int requiredHandcuffs; // 필요한 수갑 개수
    public int currentHandcuffs = 0;
    private Transform moneyspawnPivot;
    private Rigidbody rb;
    private Sequence moveSeq;


    public bool isSatisfied => currentHandcuffs >= requiredHandcuffs;

    // 줄의 맨 앞에 도착했을 때 호출
    public void ShowRequestUI() => ui.Show();


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }


    private void Start()
    {
        moneyspawnPivot = gameObject.GetComponent<Transform>();
    }

    public void Init()
    {
        requiredHandcuffs = Random.Range(3, 6); // 2~4개
        currentHandcuffs = 0;
        ui.InitUI(requiredHandcuffs);
    }

    // 수갑을 가져가는 연출
    public Tween TakeHandcuff(GameObject handcuff, float moveDuration)
    {
        ui.Show();

        int nextCount = currentHandcuffs + 1;
        ui.UpdateUI(nextCount, requiredHandcuffs, moveDuration);

        // 수갑을 다 채웠다면 UI닫아버림
        if (nextCount >= requiredHandcuffs)
        {
            DOVirtual.DelayedCall(moveDuration * 0.8f, () => ui.Hide());
        }

        handcuff.transform.SetParent(transform);

        // 점프 애니메이션 실행
        return handcuff.transform.DOLocalJump(Vector3.up * 1.5f, 2f, 1, moveDuration)
            .OnComplete(() =>
            {
                currentHandcuffs++;
                handcuff.SetActive(false);
            });
    }


    public void GoToPrison(Transform cornerPoint, Transform prisonInside)
    {
        ui.Hide();      // UI끔

        if (targetZone != null)
        {
            int prefabCount = requiredHandcuffs * 2;
            int baseIndex = targetZone.GetNextIndex();

            for (int i = 0; i < prefabCount; i++)
            {
                int currentIndex = baseIndex + i;
                Vector3 targetLocalPos = CalculateStackPos(currentIndex);
                GameObject money = Instantiate(moneyPrefab, moneyspawnPivot.position, Quaternion.identity);

                if (money.TryGetComponent<Money>(out var mItem)) mItem.value = 5;

                targetZone.AddMoneyToStack(money, targetLocalPos, i * 0.02f);
            }
        }

        // ㄴ자 이동 시퀀스
        moveSeq = DOTween.Sequence();

        // 감옥이동 (중간에 한번 코너)
        moveSeq.Append(transform.DOMove(cornerPoint.position, 6f).SetEase(Ease.Linear));
        moveSeq.Append(transform.DOMove(prisonInside.position, 3f).SetEase(Ease.OutQuad));

        // 도착
        moveSeq.OnComplete(() =>
        {
            if (rb != null)
            {
                rb.isKinematic = false;     // 물리 킴
                rb.AddForce(transform.forward * 2f, ForceMode.Impulse);

                // 이벤트 발행
                EventBus.Instance.Publish(new GameEvents.AddPrisoner());
            }
        });
    }

    // 2*3 피벗계산기
    private Vector3 CalculateStackPos(int index)
    {
        int floor = index / 6;
        int indexInFloor = index % 6;
        int row = indexInFloor / 2;
        int col = indexInFloor % 2;

        return new Vector3(col * 0.5f, floor * 0.2f, row * 0.2f);  // <->  /  /  위아래
    }

    //// 그리고 대기
    public void StopAtCorner(Transform cornerPoint)
    {
        if (moveSeq != null) moveSeq.Kill();
        transform.DOKill();

        if (rb != null)
        {
            rb.isKinematic = true;      // 물리 연산 중단
            rb.velocity = Vector3.zero; // 남은 관력 제거
        }

        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        ////
        var manager = FindFirstObjectByType<PrisonManager>();
        int myOrder = (manager != null) ? manager.GetWaitingCnt() : 0;

        float offset = (myOrder + 1) * 1.2f;
        Vector3 waitPos = cornerPoint.position + (cornerPoint.right * offset);
        //////

        transform.DOMove(waitPos, 5f).SetEase(Ease.OutQuad);
    }
}
