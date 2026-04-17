using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private Animator anim;

    private readonly int hashIsMove = Animator.StringToHash("isMove");
    private readonly int hashIsInteract = Animator.StringToHash("isInteract");

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
}
