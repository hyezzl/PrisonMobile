using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoneyTakeZone : TakeZone
{
    protected override void PlayLogic(PlayerInteractHandler player)
    {
        // 1. 돈이 없으면 리턴
        if (takeStack.Count <= 0) return;

        if (player.stackManager.IsFull(targetItemID))
        {
            return;
        }

        // 3. 돈 하나 꺼내기
        GameObject money = takeStack.Pop();

        //////////////////////////////////////////////////////////
        if (money.TryGetComponent<Money>(out var moneyItem))
        {
            // DataManager.Instance.Gold += moneyItem.scoreValue; // 데이터 반영
            // UIManager.Instance.UpdateMoneyUI(DataManager.Instance.Gold); // UI 반영
        }

        player.stackManager.AddStack(money, targetItemID);

        lastInteractionTime = Time.time;

        CheckFirst();
    }

    public void AddMoneyToStack(GameObject money, Vector3 targetLocalPos, float delay)
    {
        takeStack.Push(money);

        money.transform.SetParent(stackPivot);
        money.transform.localRotation = Quaternion.identity;

        money.transform.DOLocalJump(targetLocalPos, 0.7f, 1, 0.2f)
            .SetDelay(delay)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                 money.transform.localPosition = targetLocalPos;
             });
    }

    // 현재 창고에 몇 개 쌓여있는지
    public int GetNextIndex() => takeStack.Count;


    protected override void CheckFirst()
    {
        if (isFirst && !string.IsNullOrEmpty(eventID))
        {
            CameraManager.Instance.SwitchCameraWithDuration(CameraType.FirstLevelUpCam, 3f);
        }

        base.CheckFirst();
    }
}
