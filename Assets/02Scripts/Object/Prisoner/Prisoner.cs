using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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


    public bool isSatisfied => currentHandcuffs >= requiredHandcuffs;

    // 줄의 맨 앞에 도착했을 때 호출
    public void ShowRequestUI() => ui.Show();



    private void Start()
    {
        moneyspawnPivot = gameObject.GetComponent<Transform>();
    }

    public void Init()
    {
        requiredHandcuffs = Random.Range(2, 5); // 2~4개
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
            // 수갑이 몸에 닿기 직전에 말풍선을 치워버려서 "완료됨"을 즉시 알림
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

    public void GoToPrison(Transform prisonTarget)
    {
        ui.Hide();      // UI끔

        // 6개의 돈 냄
        if (targetZone != null)
        {
            int baseIndex = targetZone.GetNextIndex();

            for (int i = 0; i < 6; i++)
            {
                int currentIndex = baseIndex + i;

                // 창고(TakeZone) 기준의 목표 로직 좌표 계산
                Vector3 targetLocalPos = CalculateStackPos(currentIndex);

                // 현재위치에서 돈생성
                GameObject money = Instantiate(moneyPrefab, moneyspawnPivot.position, Quaternion.identity);

                targetZone.AddMoneyToStack(money, targetLocalPos, i * 0.05f);
            }
        }

        transform.DOMove(prisonTarget.position, 1.5f).OnComplete(() => {
            Destroy(gameObject); // 감옥 도착 시 처리 (혹은 비활성화)
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
}
