using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PrisonerGiveZone : GiveZone
{
    [Header("죄수 관리")]
    public GameObject prisonerPrefab;   // 죄수 프리팹
    public int maxQueueCount = 5;       // 줄 서 있을 최대 죄수 수
    public float queueSpacing = 3f;   // 죄수 간 간격

    [Header("지점 설정")]
    public Transform queueStartPivot; // 첫죄수 서있을 위치
    public Transform givePivot;     // 수갑이 일렬로 깔릴 바닥 위치
    public Transform prisonLocation;  // 수갑 다 받은 죄수가 갈 곳

    [Header("재화 설정")]
    public MoneyTakeZone moneyTakeZone;


    [HideInInspector]
    public List<Prisoner> waitingPrisoners = new List<Prisoner>();      // 현재 생성된 죄수 리스트
    private bool isDistributing = false;        // 현재 수갑 납부 중인지

    private void Start()
    {
        // 처음에 줄을 꽉 채워둡니다.
        for (int i = 0; i < maxQueueCount; i++)
        {
            SpawnNewPrisoner();
        }

        // 첫번째 죄수의 UI는 켜둠
        waitingPrisoners[0].ShowRequestUI();
    }

    private void Update()
    {
        // 바닥에 수갑이 있고, 대기 중인 죄수가 있다면 배분 시작
        if (!isDistributing && giveList.Count > 0 && waitingPrisoners.Count > 0)
        {
            StartCoroutine(DistributeRoutine());
        }
    }

    // 플레이어가 수갑을 냈을 때
    protected override void PlayLogic(PlayerInteractHandler player)
    {
        List<GameObject> items = player.stackManager.PopAllItems(targetItemID);
        if (items != null && items.Count > 0)
        {
            // 중복 로직 방지를 위해 DeployHandcuffs 하나로 통일하는 게 좋아요!
            DeployHandcuffs(items);
        }
    }

    // 수갑을 일렬로 쌓음
    private void DeployHandcuffs(List<GameObject> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];
            giveList.Add(item);

            int index = giveList.Count - 1;

            Vector3 targetPos = new Vector3(0, 0, index * 0.2f);

            item.transform.SetParent(givePivot);
            item.transform.DOLocalJump(targetPos, 2f, 1, 0.4f)
                .SetDelay(i * 0.05f)
                .OnComplete(() => {
                    item.transform.localPosition = targetPos;
                    item.transform.localRotation = Quaternion.identity;
                });
        }
    }

    // 죄수 생성
    private void SpawnNewPrisoner()
    {
        GameObject go = Instantiate(prisonerPrefab);
        Prisoner prisoner = go.GetComponent<Prisoner>();
        prisoner.Init(); // 필요한 수갑 개수 랜덤 설정 (2~4개)

        // 죄수인스턴스에 씬에 있는 MoneyTakeZone 주소 넣어줌
        prisoner.targetZone = moneyTakeZone;

        // 일단 대기열 리스트에 추가
        waitingPrisoners.Add(prisoner);

        // 생성 직후 위치는 줄 맨 끝보다 조금 더 뒤에서 나타나게 (연출)
        int index = waitingPrisoners.Count - 1;
        Vector3 spawnPos = queueStartPivot.position + (queueStartPivot.forward * -index * queueSpacing * 1.5f);
        go.transform.position = spawnPos;

        // 바로 정렬 실행
        SortPrisoners();
    }

    // 납부 로직
    private IEnumerator DistributeRoutine()
    {
        isDistributing = true;
        float transferTime = 0.35f; // 던지는 속도

        while (giveList.Count > 0 && waitingPrisoners.Count > 0)
        {
            Prisoner currentPrisoner = waitingPrisoners[0];

            currentPrisoner.ui.Show();


            while (giveList.Count > 0 && !currentPrisoner.isSatisfied)
            {
                GameObject handcuff = GetItemFromTop();
                if (handcuff != null)
                {
                    // 하나의 수갑이 도착할 때 까지 멈춤
                    yield return currentPrisoner.TakeHandcuff(handcuff, transferTime).WaitForCompletion();

                    // 하나 던지고 다음 거 던지기 전 아주 짧은 간격
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // 3. 다 채웠으면 다음 죄수로 교체
            if (currentPrisoner.isSatisfied)
            {
                yield return new WaitForSeconds(0.3f);      // 잠깐 여유둠

                waitingPrisoners.RemoveAt(0);
                currentPrisoner.GoToPrison(prisonLocation);
                SpawnNewPrisoner();
                SortPrisoners();

                yield return new WaitForSeconds(0.6f); // 줄 당겨지는 시간 대기

                if (waitingPrisoners.Count > 0)
                    waitingPrisoners[0].ui.Show();
            }
            yield return null;
        }
        isDistributing = false;
    }

    // 수갑 줌
    private GameObject GetItemFromTop()
    {
        if (giveList.Count == 0) return null;

        int lastIndex = giveList.Count - 1;
        GameObject item = giveList[lastIndex];
        giveList.RemoveAt(lastIndex);

        return item;
    }

    

    // 죄수들 줄 세우기
    private void SortPrisoners()
    {
        for (int i = 0; i < waitingPrisoners.Count; i++)
        {
            // 각자의 index에 맞는 대기 위치 계산
            Vector3 targetPos = queueStartPivot.position + (queueStartPivot.right * -i * queueSpacing);

            // 부드럽게 한 칸씩 앞으로 땡겨지는 연출
            waitingPrisoners[i].transform.DOMove(targetPos, 0.8f).SetEase(Ease.OutQuad);
        }
    }
}
