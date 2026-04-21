using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("대기화면 설정")]
    public RectTransform cursor;
    public GameObject idleScreen;
    public float idleTime = 6f;
    private float lastInputTime;

    public float duration = 4f;     // 바퀴시간
    public float width = 300f;
    public float height = 150f;

    private bool isIdle;    // 대기상태인지


    private void Start()
    {
        SetIdleMode(false);
    }

    private void Update()
    {
        // 사용자의 입력을 감지
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            lastInputTime = Time.time;
            if(isIdle) SetIdleMode(false);
        }
        else if (!isIdle && Time.time - lastInputTime > idleTime)
        { 
            SetIdleMode(true);
        }
    }


    public void UpdateMoneyUI(int amount)
    {
        moneyText.text = amount.ToString("N0");

        // 1. 기존 트윈 즉시 종료
        moneyText.transform.DOKill();

        moneyText.transform.localScale = Vector3.one;
        moneyText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }


    // 대기화면
    private void SetIdleMode(bool isIdle)
    {
        this.isIdle = isIdle;
        idleScreen.gameObject.SetActive(isIdle);
        InfinityLoop();
    }


    // 대기화면 연출
    private void InfinityLoop()
    {
        cursor.DOKill();
        cursor.anchoredPosition = Vector3.zero;

        // 좌표포인트
        Vector3[] path = new Vector3[] {
            new Vector3(width/2, height/2, 0),              // 오른쪽 위
            new Vector3(width, 0, 0),                       // 오른쪽 끝
            new Vector3(width/2, -height/2, 0),             // 오른쪽 아래
            new Vector3(0, 0, 0),                           // 중앙 교차점
            new Vector3(-width/2, height/2, 0),             // 왼쪽 위
            new Vector3(-width, 0, 0),                      // 왼쪽 끝
            new Vector3(-width/2, -height/2, 0),            // 왼쪽 아래
        };

        cursor.DOLocalPath(path, duration, PathType.CatmullRom) // CatmullRom을 써야 곡선이 부드러워요
            .SetOptions(true)
            .SetEase(Ease.Linear)                                // 일정한 속도로
            .SetLoops(-1);                              // 무한 반복
    }
}
