using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;


public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI moneyText;

    public void UpdateMoneyUI(int amount)
    {
        moneyText.text = amount.ToString("N0");

        // 1. 기존 트윈 즉시 종료
        moneyText.transform.DOKill();

        moneyText.transform.localScale = Vector3.one;
        moneyText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }
}
