using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TakeZone : BaseZone
{
    [Header("쌓기 설정")]
    public Transform stackPivot;
    public float spacingY = 0.3f; // 수직 쌓기 간격

    public Stack<GameObject> takeStack = new Stack<GameObject>(); // 가져갈 아이템들

    // 이벤트 발행 변수
    protected bool isFirst = true;

    protected override void PlayLogic(PlayerInteractHandler player)
    {
        // 플레이어에게 아이템을 하나씩 줌
        if (takeStack.Count > 0)
        {
            GameObject item = takeStack.Pop();

            // 플레이어 스택 매니저에 추가
            player.stackManager.AddStack(item, targetItemID);

            lastInteractionTime = Time.time;

            // 처음 체크
            CheckFirst();
        }
    }

    // 가공기에서 아이템을 넣어줄 때 호출하는 함수
    public void AddItem(GameObject item)
    {
        takeStack.Push(item);

        // Y축 수직 쌓기 위치 계산
        float newY = (takeStack.Count - 1) * spacingY;
        Vector3 targetPos = new Vector3(0, newY, 0);

        item.transform.SetParent(stackPivot);

        // 연출
        item.transform.DOLocalJump(targetPos, 0.7f, 1, 0.2f)
            .SetEase(Ease.OutQuad);
        item.transform.DOLocalRotate(Vector3.zero, 0.3f);
    }


    // 처음 납부되었을때 한번만 실행(이벤트)
    protected void CheckFirst()
    {
        if (!isFirst || string.IsNullOrEmpty(eventID)) return;

        // 이벤트 발행
        EventBus.Instance.Publish(new GameEvents.StartEvent(eventID));

        isFirst = false;
    }
}
