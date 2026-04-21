using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PrisonManager : MonoBehaviour
{
    [Header("설정")]
    public int targetCount = 20; // 목표 숫자
    private int curPrisonerCnt = 0; // 현재 감옥에 들어온 죄수 수
    private bool isEventFired = false; // 이벤트 중복 발생 방지

    [Header("UI")]
    public GameObject uiObj;
    public TextMeshProUGUI text;
    public GameObject extendObj;        // 늘어날 감옥plane

    public int waitingCnt = 0;      // 감옥 꽉찬 후 도착한 죄수

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameEvents.AddPrisoner>(AddPrisoner);
        EventBus.Instance.Subscribe<GameEvents.StartEvent>(OnEvent);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameEvents.AddPrisoner>(AddPrisoner);
        EventBus.Instance.Unsubscribe<GameEvents.StartEvent>(OnEvent);
    }

    private void Start()
    {
        UpdateUI();
        extendObj?.SetActive(false);
    }

    public void CountPrisoner()
    {
        if (isEventFired) return;

        curPrisonerCnt++;
        curPrisonerCnt = Mathf.Min(curPrisonerCnt, targetCount);

        // 20명이 되었을 때 이벤트 발생
        if (curPrisonerCnt >= targetCount && !isEventFired)
        {
            isEventFired = true;
            UpdateUI();
            PrisonIsFull();
        }
    }

    // 감옥이 가득 참
    private void PrisonIsFull()
    {
        EventBus.Instance.Publish(new GameEvents.StartEvent("E008"));       // 이벤트 발행

        text?.DOColor(Color.red, 0.3f); // 색상 변경

        // 카메라 이벤트
        CameraManager.Instance.SwitchCameraWithDuration(CameraType.CellCam, 3f);
    }

    private void AddPrisoner(GameEvents.AddPrisoner evt) { 
        CountPrisoner();
        UpdateUI();
    }



    private void UpdateUI()
    {
        if (text != null) {
            text.text = $"{curPrisonerCnt} / 20";
        }
    }

    private void OnEvent(GameEvents.StartEvent evt)
    {
        if (evt.eventID == "E009")
        {
            // 감옥 납부까지 끝나면
            CameraManager.Instance.SwitchCameraWithDuration(CameraType.CellCam, 3f);

            //UI삭제
            uiObj?.SetActive(false);

            // 감옥 확장!
            if (extendObj != null)
            {
                extendObj.transform.localScale = Vector3.zero;
                extendObj.SetActive(true);

                extendObj.transform.DOScale(Vector3.one, 0.6f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => {
                        DOVirtual.DelayedCall(2.0f, () => {
                            // 플레이어 시점일 때 엔딩 실행
                            Debug.Log("엔딩 시작!");
                            EventBus.Instance.Publish(new GameEvents.StartEvent("E013"));
                        });
                    });
            }
        }
    }

    public int GetWaitingCnt() => waitingCnt++;
}
