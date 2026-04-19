using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 납부존
/// </summary>
public class GiveZone : BaseZone
{
    [Header("납부 설정")]
    public Transform depositPivot; // 자원이 옮겨질 위치
    public float spacingX = 0.5f;  // 가로 2줄 간격 (0.5면 왼쪽 -0.25, 오른쪽 +0.25)
    public float spacingY = 0.3f;  // 위로 쌓이는 간격

    // 납부존이 가진 아이템
    public List<GameObject> giveList = new List<GameObject>();


    protected override void PlayLogic(PlayerInteractHandler player)
    {
        if (player.stackManager == null) return;

        // 플레이어의 스택에서 targetItemID와 일치하는 아이템을 전부
        List<GameObject> items = player.stackManager.PopAllItems(targetItemID);

        if (items != null && items.Count > 0)
        {
            lastInteractionTime = Time.time;
            ProcessGiveItems(items);
        }
    }

    private void ProcessGiveItems(List<GameObject> items)
    {
        // 2. 플레이어에게서 뺏어온 아이템들을 하나씩 내 구역에 2줄로 세팅
        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];
            giveList.Add(item); // 리스트 끝에 추가

            // 현재 납부존에 쌓인 총 개수를 기준으로 위치 계산 (0번부터 시작하므로 -1)
            int currentIndex = giveList.Count - 1;

            // --- 2줄 쌓기 공식 ---
            float posX = 0;
            float posY = (currentIndex / 2) * spacingY;
            float posZ = (currentIndex % 2 == 0) ? -spacingX / 2f : spacingX / 2f;

            Vector3 targetLocalPos = new Vector3(posX, posY, posZ);

            // 부모를 납부존 피봇으로 변경
            item.transform.SetParent(depositPivot);

            // 날아오는 연출 
            item.transform.DOLocalJump(targetLocalPos, 2f, 1, 0.4f)
                .SetDelay(i * 0.05f) // 차례대로 출발
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    item.transform.localPosition = targetLocalPos;
                    item.transform.localRotation = Quaternion.identity;
                });
            // 회전 연출
            item.transform.DOLocalRotate(Vector3.zero, 0.4f)
                .SetDelay(i * 0.05f)
                .SetEase(Ease.OutQuad);
        }
    }

    // 가공기가 하나씩 뺄때마다 호출될 함수
    public GameObject OnGetItem()
    {
        if (giveList.Count == 0) return null;

        //가장 오래된(아래에 있는) 아이템 추출
        int lastIndex = giveList.Count - 1;
        GameObject item = giveList[lastIndex];

        //남은 아이템들 재정렬 (아래로 한 칸씩 이동)
        giveList.RemoveAt(lastIndex);

        return item;
    }
}
