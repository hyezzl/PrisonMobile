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

    [Header("AI Ref")]
    [SerializeField] private GiveZone cacheZone;        // AI의 자원이 이동할 Zone


    private int itemID = 1001;      // 돌의 고유 아이템 ID
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
        CanInteract = false;

        // 플레이어가 캔경우
        if (player != null)
        {
            CollectToPlayer(player);
        }
        // AI가 캔 경우
        else
        {
            CollectToGiveZone();
        }

        StartCoroutine(Respawn());
    }


    // 자원 캐기 완료 (플레이어용)
    private void CollectToPlayer(PlayerInteractHandler player)
    {
        if (player == null || player.stackManager == null) return;

        // 스택 매니저에 프리팹/현재 위치 전달
        player.stackManager.StackPrefab(stackPrefab, itemID, transform.position);


        // 비활성화
        mr.enabled = false;
        col.enabled = false;
    }


    // AI용 자원캐기
    private void CollectToGiveZone()
    {
        if (cacheZone != null)
        {
            // 프리팹 생성
            GameObject item = Instantiate(stackPrefab, transform.position, Quaternion.identity);

            // GiveZone의 리스트에 추가하고 바로 날아가게 함
            cacheZone.AddItemFromAI(item);
        }
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
