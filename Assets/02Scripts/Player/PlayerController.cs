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

public class PlayerController : MonoBehaviour
{

    // 플레이어 상태변화 알림이
    public event Action<PlayerState> OnStateChange;

    // 플레이어 현 상태
    private PlayerState curState;
    private int curLevel = 0;

    // 참조
    private PlayerMove pm;
    private PlayerAnimation anim;
    private IInputHandler inputHandler;


    // 상태변수
    private bool isInteract;

    private void Awake()
    {
        pm = GetComponent<PlayerMove>();
        anim = GetComponent<PlayerAnimation>();
        inputHandler = GetComponent<IInputHandler>();
        if (pm == null || anim == null || inputHandler == null) Debug.LogWarning("PlayerController - Failed to Ref");
    }



    private void Update()
    {
        Vector2 input = inputHandler.GetMovement;
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

    


}
