using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move Setting")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    private float gravity = 9.81f;

    private CharacterController cc;
    private Camera cam;
    private Vector3 velocity;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null) Debug.LogWarning("PlayerMove - Failed to Load CharacterController");

        cam = Camera.main;
    }


    // 플레이어 움직임
    public void Movement(Vector2 input)
    {
        Vector3 moveDir = GetCameraInput(input);

        if (moveDir.sqrMagnitude > 0.01f)
        { 
            cc.Move(moveDir * moveSpeed * Time.deltaTime);
            
            // 회전
            Quaternion rot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateSpeed * Time.deltaTime);
        }
    }


    // 카메라 보정
    private Vector3 GetCameraInput(Vector2 input)
    {
        Transform camTrans = cam.transform;
        Vector3 forward = camTrans.forward;
        Vector3 right = camTrans.right;

        // y값 무시 후 정규화
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;

    }


    // 중력
    public void ApplyGravity()
    {
        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 바닥으로 누름
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        cc.Move(velocity * Time.deltaTime);
    }

}
