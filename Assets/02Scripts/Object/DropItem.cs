using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour, IActionTarget
{
    [SerializeField] private int itemID = 1002; // 수갑 ID
    [SerializeField] private GameObject stackPrefab; // 플레이어 등에 쌓일 모양

    public void Interact(PlayerInteractHandler player)
    {
        // 이미 줍고 있는 중이거나 스택이 가득 찼으면 무시
        //if (player.stackManager == null || player.stackManager.IsFull) return;

        // 플레이어 스택에 추가 (현재 내 위치에서 날아가는 연출)
        player.stackManager.StackPrefab(stackPrefab, itemID, transform.position);

        // 주웠으니까 필드에 있는 이 객체는 제거 (또는 풀로 반환)
        Destroy(gameObject);
    }
}
