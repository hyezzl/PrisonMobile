using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private Animator anim;

    private readonly int hashIsMove = Animator.StringToHash("isMove");
    private readonly int hashIsInteract = Animator.StringToHash("isInteract");
    private readonly int hashLevel = Animator.StringToHash("level"); // 레벨 
    private readonly int hashAttackSpeed = Animator.StringToHash("attackSpeed"); // 속도 

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null) Debug.LogWarning("PlayerAnimation - Failed to Load Animator");
    }

    // 애니메이션 조절
    public void UpdateAnimation(bool isMove, bool isInteract)
    {
        // Base Layer
        anim.SetBool(hashIsMove, isMove);

        // Upper Layer
        anim.SetBool(hashIsInteract, isInteract);

    }

    // 레벨에 따른 애니메이션 데이터 갱신
    public void SetLevelAnimation(int level, float speed)
    {
        // 1. 애니메이터에 레벨 전달 (레벨별로 다른 모션을 쓸 때)
        anim.SetInteger(hashLevel, level);

        // 2. 애니메이션 재생 속도 조절 (레벨이 높을수록 팔을 빨리 휘두름)
        anim.SetFloat(hashAttackSpeed, speed);
    }
}
