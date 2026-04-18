using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseZone : MonoBehaviour, IActionTarget
{
    [Header("Zone 설정")]
    public int targetItemID; // 이 Zone에서 취급하는 아이템 ID
    public float interactionDelay = 0.1f; // 상호작용 간격 (연속 납부/획득 속도)

    protected float lastInteractionTime;

    
    public virtual void Interact(PlayerInteractHandler player)
    {
        // 쿨타임 체크
        if (Time.time < lastInteractionTime + interactionDelay) return;

        lastInteractionTime = Time.time;
        // 실제 로직은 자식 클래스에서 정의함
        PlayLogic(player);

    }

    protected abstract void PlayLogic(PlayerInteractHandler player);
}
