using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumeZone : BaseZone
{
    [Header("Consume Settings")]
    [SerializeField] private int requiredAmount = 100; // 목표 금액
    [SerializeField] private int currentAmount = 0;    // 현재 납부된 금액

    [Header("Reward Settings")]
    [SerializeField] private RewardType rewardType;    // 보상 종류 (LevelUp, SummonAI 등)

    [Header("Consume UI")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image fillImg;

    private bool isComplete = false;        // 모두 납부 되었는지?

    private void Start()
    {
        if (fillImg != null) fillImg.fillAmount = 0;
        UpdateUI();
    }

    protected override void PlayLogic(PlayerInteractHandler player)
    {
        if (isComplete || player.stackManager == null) return;

        int need = requiredAmount - currentAmount;      // 필요한 재화
        if (need <= 0) return;

        // 플레이어에게서 필요한 만큼(혹은 가진 전부) 돈을 가져옴
        List<GameObject> moneyToConsume = player.stackManager.PopItems(targetItemID, need);

        if (moneyToConsume != null && moneyToConsume.Count > 0)
        {
            ProcessConsumption(moneyToConsume, player);
        }
    }


    private void ProcessConsumption(List<GameObject> items, PlayerInteractHandler player)
    {
        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];
            currentAmount++;

            // 돈 흡수 연출
            item.transform.SetParent(this.transform);

            // DOTween의 Delay를 활용해서 순차적으로 날아오게 함
            float delay = i * 0.05f;

            item.transform.DOMove(this.transform.position, 0.3f)
                .SetDelay(delay)
                .OnStart(() => {
                    // 사운드!
                })
                .OnComplete(() => {
                    UpdateUI();      

                    priceText.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);

                    Destroy(item); // 혹은 풀링 반환

                    if (currentAmount >= requiredAmount && !isComplete)
                    {
                        CompleteZone(player);
                    }
                });
        }
    }

    private void UpdateUI()
    {
        // 텍스트
        if (priceText != null)
        {
            int need = requiredAmount - currentAmount;
            priceText.text = need.ToString();

            priceText.transform.DOKill();
            priceText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }

        // 게이지 채우기
        if (fillImg != null)
        {
            float targetFill = (float)currentAmount / requiredAmount;

            fillImg.DOKill();
            fillImg.DOFillAmount(targetFill, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    private void CompleteZone(PlayerInteractHandler player)
    {
        isComplete = true;

        // 보상을 받을 주체에게 알림
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ReceiveReward(rewardType); // 보상 타입을 넘겨줌
        }

        // 완료 연출
        transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

}
