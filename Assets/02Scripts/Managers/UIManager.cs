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
        moneyText.text = amount.ToString("N0"); // 1,000 단위 쉼표 추가

        moneyText.transform.DOKill();
        moneyText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }
}
