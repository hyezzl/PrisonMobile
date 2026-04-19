using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrisonerUI : MonoBehaviour
{
    [Header("UI Ref")]
    public GameObject bubbleRoot;   // 말풍선 부모 오브젝트
    public Image fillImage;         // 초록색 게이지 (Image Type: Filled 필수)
    public TMP_Text countText;      // 남은 수갑 개수 텍스트

    // 초기 설정
    public void InitUI(int requiredCount)
    {
        fillImage.fillAmount = 0;
        countText.text = requiredCount.ToString();
        Hide(); // 생성 직후에는 숨김
    }

    public void Show() => bubbleRoot.SetActive(true);
    public void Hide() => bubbleRoot.SetActive(false);

    // 남은 개수 갱신
    public void UpdateUI(int current, int total, float duration)
    {
        // 수직으로 게이지 채우기
        fillImage.fillAmount = 0;
        fillImage.DOFillAmount(1f, duration).OnComplete(() => {
            fillImage.fillAmount = 0;
            countText.text = (total - current).ToString();
            countText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        });
    }
}
