using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoneyTakeZone : TakeZone
{
    public void AddMoneyToStack(GameObject money, Vector3 targetLocalPos, float delay)
    {
        takeStack.Push(money);

        money.transform.SetParent(stackPivot);
        money.transform.localRotation = Quaternion.identity;

        money.transform.DOLocalJump(targetLocalPos, 2f, 1, 0.5f)
            .SetDelay(delay)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                 money.transform.localPosition = targetLocalPos;
             });
    }

    // 현재 창고에 몇 개 쌓여있는지
    public int GetNextIndex() => takeStack.Count;
}
