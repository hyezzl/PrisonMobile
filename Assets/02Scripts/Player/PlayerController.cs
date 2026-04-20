using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerState
{ 
    Idle,
    Move,
    Interact,
    Achieve,    // 무언가 달성하거나, 얻었을 때
}


public enum RewardType
{ 
    LevelUp,            // 플레이어 무기 업그레이드
    AddAIminer,         // 채굴꾼 AI
    AddAIdeliver,       // 수갑배달 AI
    ExpensionCell,      // 감옥 늘리기
}

public enum GameMode
{ 
    Play,
    Stop,       // 플레이어의 입력 막음
}



public class PlayerController : MonoBehaviour
{

    // 플레이어 상태변화 알림이
    public event Action<PlayerState> OnStateChange;

    // 플레이어 현 상태
    private PlayerState curState;
    private int curLevel = 0;
    private GameMode curMode = GameMode.Play;

    [Header("Level Settings")]
    [SerializeField] private List<GameObject> weaponModels; // 레벨별 무기 모델

    // 채집 관련 능력치 (LevelUp 시 자동 업데이트됨)
    public float collectRange { get; private set; } = 2.0f;
    public float collectCooldown { get; private set; } = 1.0f;


    // 제어 변수
    private bool isControlLock = false;


    // 참조
    private PlayerMove pm;
    private PlayerAnimation anim;
    private IInputHandler inputHandler;
    private PlayerInteractHandler interactHandler;


    // 상태변수
    private bool isInteract;




    private void Awake()
    {
        pm = GetComponent<PlayerMove>();
        anim = GetComponent<PlayerAnimation>();
        inputHandler = GetComponent<IInputHandler>();
        interactHandler = GetComponent<PlayerInteractHandler>();
        if (pm == null || anim == null || inputHandler == null) Debug.LogWarning("PlayerController - Failed to Ref");
    }


    private void Start()
    {
        // 시작 레벨(0)에 대한 애니메이션 초기화
        if (anim != null && interactHandler != null)
        {
            anim.SetLevelAnimation(curLevel, interactHandler.rangeSpeeds[0]);
        }
    }



    private void Update()
    {
        Vector2 input = (curMode == GameMode.Play) ? inputHandler.GetMovement : Vector2.zero;
        bool existInput = input.sqrMagnitude > 0.01f;

        // 상태 갱신
        UpdateState(input);

        // 이동은 입력이 있을때만, 중력은 항상
        if (existInput)
            pm.Movement(input);

        pm.ApplyGravity();

        // 애니메이션 갱신
        anim.UpdateAnimation(existInput, isInteract);


        // 상태알림이
        if (Input.GetKeyDown(KeyCode.F1)) { AlarmState(); }
    }


    // 스스로 모니터링
    private PlayerState InspectState(Vector2 input)
    {
        // 상호작용 중인지?
        if (isInteract) return PlayerState.Interact;    // 상호작용 중이면 무조건 상호작용

        if (inputHandler.GetMovement.sqrMagnitude > 0.01f) return PlayerState.Move;
        return PlayerState.Idle;
    }

    // 상태 변화 주시
    private void UpdateState(Vector2 input)
    {
        PlayerState state = InspectState(input);

        // 상태가 변하면 알림
        if (state != curState)
        {
            curState = state;
            OnStateChange?.Invoke(curState);        // 이벤트 발행
        }
    }

    private void AlarmState()
    {
        Debug.Log($"현재 레벨 : {curLevel}");
        Debug.Log($"현재 상태 : {curState}");
        Debug.Log($"현재 모드 : {curMode}");
    }


    // 외부에서 상호작용 상태 변경
    public void SetInteract(bool value) => isInteract = value;


    // 레벨에 따른 최대 수용량
    public int GetMax()
    {
        return curLevel * 10 + 10;
    }

    public int GetLevel()
    { return curLevel; }


    // 소비존 달성 보상
    public void ReceiveReward(RewardType type)
    {
        switch (type)
        {
            case RewardType.LevelUp:
                LevelUp();
                break;

            case RewardType.AddAIminer:
                // TODO: AI 광부 소환 로직 연결
                Debug.Log("AI 광부 추가됨!");
                break;

            case RewardType.AddAIdeliver:
                // TODO: AI 배달꾼 소환 로직 연결
                Debug.Log("AI 배달꾼 추가됨!");
                break;
        }
    }

    private void LevelUp()
    {
        curLevel++;

        // 콜라이더/리스트를 갱신
        if (interactHandler != null)
        {
            interactHandler.UpdateInteractionCollider(curLevel);
        }

        // 레벨업 시 애니메이션도 변경
        if (anim != null && interactHandler != null)
        {
            int speedIndex = Mathf.Clamp(curLevel, 0, interactHandler.rangeSpeeds.Count - 1);
            float currentSpeed = interactHandler.rangeSpeeds[speedIndex];

            anim.SetLevelAnimation(curLevel, currentSpeed);
        }

        Debug.Log($"레벨업!! 현 레벨 : {curLevel}");
    }


    // 외부에서 게임 모드를 바꿀 수 있는 함수
    public void ChangeGameMode(GameMode mode)
    {
        curMode = mode;
        Debug.Log($"게임 모드 변경: {mode}");
    }

}
