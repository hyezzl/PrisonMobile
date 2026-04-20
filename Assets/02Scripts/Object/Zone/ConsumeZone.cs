using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ★ 다른 존과는 다르게 ConsumeZone의 eventID는 "실행"할 이벤트!
/// </summary>

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
    private bool isFirst = true;         // 처음 납부?

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

        int needPrefabCount = Mathf.CeilToInt((float)need / 5f);

        // 플레이어에게서 필요한 만큼(혹은 가진 전부) 돈을 가져옴
        List<GameObject> moneyToConsume = player.stackManager.PopItems(targetItemID, needPrefabCount);

        if (moneyToConsume != null && moneyToConsume.Count > 0)
        {
            ProcessConsumption(moneyToConsume, player);
        }
    }


    private void ProcessConsumption(List<GameObject> items, PlayerInteractHandler player)
    {
        // 첫이벤트용
        if (isFirst && items.Count > 0)
        {
            isFirst = false;
            // arrow삭제
            EventBus.Instance.Publish(new GameEvents.ClearArrow());
        }


        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];

            int itemValue = 5;
            if (item.TryGetComponent<Money>(out var mItem))
            {
                itemValue = mItem.value;
            }
            currentAmount += itemValue;

            // 소모될 때 목표 금액을 초과하지 않도록 보정
            if (currentAmount > requiredAmount) currentAmount = requiredAmount;

            // 돈 흡수 연출
            item.transform.SetParent(this.transform);

            // DOTween의 Delay를 활용해서 순차적으로 날아오게 함
            float delay = i * 0.05f;

            item.transform.DOMove(this.transform.position, 0.2f)
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
            priceText.transform.localScale = Vector3.one;

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

        // 이벤트 발행
        EventBus.Instance.Publish(new GameEvents.StartEvent(eventID));

        // 완료 연출
        transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

}
