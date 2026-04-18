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
    public Queue<GameObject> giveQueue = new Queue<GameObject>();


    protected override void PlayLogic(PlayerInteractHandler player)
    {
        Debug.Log("1111");
        if (player.stackManager == null) return;
        Debug.Log("2222");

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

            // 큐에 넣어서 납부존 소유로 만듦
            giveQueue.Enqueue(item);

            // 현재 납부존에 쌓인 총 개수를 기준으로 위치 계산 (0번부터 시작하므로 -1)
            int currentIndex = giveQueue.Count - 1;

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

        Debug.Log($"[납부 완료] {targetItemID} 아이템 {items.Count}개 받음. 현재 대기열: {giveQueue.Count}개");

    }
}
