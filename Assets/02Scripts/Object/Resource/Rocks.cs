using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Rocks : MonoBehaviour, IActionTarget
{
    [Header("Setting")]
    [SerializeField] private float respawnTime = 5f;

    [Header("Prefab")]
    [SerializeField] private GameObject stackPrefab;    // 시각프리팹정보 저장


    private MeshRenderer mr;
    private Collider col;


    public bool CanInteract { get; private set; } = true;


    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }


    public void Interact(PlayerInteractHandler player)
    {
        if (!CanInteract) return;

        var stackManager = player.stackManager;

        CanInteract = false;
        Collect(player);

        // 비활성화
        StartCoroutine(Respawn());
    }


    // 자원 캐기 완료
    private void Collect(PlayerInteractHandler player)
    {
        if (player == null || player.stackManager == null) return;

        // 스택 매니저에 프리팹/현재 위치 전달
        player.stackManager.StackPrefab(stackPrefab, transform.position);


        // 비활성화
        mr.enabled = false;
        col.enabled = false;
    }

    private IEnumerator Respawn()
    { 
        yield return new WaitForSeconds(respawnTime);

        // 다시 생성
        OnRespawn();
    }


    // 생성
    private void OnRespawn()
    {
        mr.enabled = true;
        col.enabled = true;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    CanInteract = true; // 연출이 끝나면 다시 캘 수 있음
                });
    }
}
