using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


[System.Serializable]
public class StackData
{
    public GameObject obj;
    public int itemID;

    public StackData(GameObject obj, int itemID)
    {
        this.obj = obj;
        this.itemID = itemID;
    }
}

/// <summary>
/// 아이템별 개별 설정을 담는 클래스
/// </summary>
[System.Serializable]
public class ItemStackSet
{
    public string itemName;      // 이름
    public int itemID;           // 아이템 고유 ID
    public Transform pivot;      // 아이템이 쌓일 부모 위치
    public int maxCount = 20;    // 이 아이템의 최대 소지량
    public float distY = 0.2f;   // 위로 쌓이는 간격
}


/// <summary>
/// 쌓기로직
/// </summary>
public class StackManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private List<ItemStackSet> itemSet = new List<ItemStackSet>();
    [SerializeField] private float flyDuration = 0.5f;

    [Header("UI Ref")]
    [SerializeField] private GameObject maxText;
    private Coroutine maxUICoroutine;

    // 현재 가지고 있는 아이템
    private List<StackData> stackRes = new List<StackData>();

    // ID별 풀
    private Dictionary<int, Stack<GameObject>> itemPools = new Dictionary<int, Stack<GameObject>>();

    private PlayerController pc;


    // 최대값 확인
    public bool IsFull(int itemID)
    {
        var config = GetConfig(itemID);
        if (config == null) return true;

        // 내 전체 스택에서 해당 ID를 가진 아이템 개수만 필터링해서 카운트
        int currentCount = stackRes.FindAll(x => x.itemID == itemID).Count;
        return currentCount >= config.maxCount;
    }
    /// <summary>
    /// ID로 해당 아이템의 설정 정보 가져오기
    /// </summary>
    private ItemStackSet GetConfig(int itemID)
    {
        return itemSet.Find(x => x.itemID == itemID);
    }

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
    }


    // Stack (자원)
    public void StackPrefab(GameObject prefab, int itemID, Vector3 startPos)
    {
        if (IsFull(itemID)) {
            // 더이상 루트X
            // UI
            if(maxUICoroutine != null) StopCoroutine(maxUICoroutine);
            maxUICoroutine = StartCoroutine(ShowMaxUI());

            return;
        }

        var config = GetConfig(itemID); // 아이템정보

        // 풀링
        GameObject obj = GetPooling(prefab, itemID, config.pivot);

        int currentTypeCount = stackRes.FindAll(x => x.itemID == itemID).Count;
        stackRes.Add(new StackData(obj, itemID));

        Vector3 targetLocalPos = new Vector3(0, currentTypeCount * config.distY, 0);

        // 연출 실행
        CollectAnimate(obj, startPos, targetLocalPos, config.pivot);

    }

    // 수갑(이미 만들어진 오브젝트) 스택
    public void AddStack(GameObject obj, int itemID)
    {
        if (IsFull(itemID))
        {
            ShowMaxUIFeedback();
            return;
        }

        var config = GetConfig(itemID);

        // 현재 이 종류의 아이템이 몇 개째인지 확인 (위치 계산용)
        int currentTypeCount = stackRes.FindAll(x => x.itemID == itemID).Count;
        stackRes.Add(new StackData(obj, itemID));

        // 해당 아이템 전용 간격으로 위치 계산
        Vector3 targetLocalPos = new Vector3(0, currentTypeCount * config.distY, 0);

        // 해당 아이템 전용 피봇(위치)으로 날려보냄
        CollectAnimate(obj, obj.transform.position, targetLocalPos, config.pivot);
    }


    // 스택연출
    private void CollectAnimate(GameObject obj, Vector3 startPos, Vector3 targetPos, Transform targetPivot)
    {
        // 날아가는 동안 플레이어와 분리
        obj.transform.SetParent(null);
        obj.transform.position = startPos;

        // 최종좌표
        Vector3 targetRealPos = targetPivot.TransformPoint(targetPos);

        obj.transform.DOJump(targetRealPos, 2f, 1, flyDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 도착하면 부모설정
                obj.transform.SetParent(targetPivot);
                obj.transform.localPosition = targetPos;
                obj.transform.localRotation = Quaternion.identity;

                // 스케일 연출 더하기
                obj.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            });

    }

    // ==========================================
    // 납부존에서 특정 자원 한번에 가져감
    public List<GameObject> PopAllItems(int targetItemID)
    {
        List<StackData> extracted = stackRes.FindAll(x => x.itemID == targetItemID);

        if (extracted.Count == 0) return null; // 가져갈 게 없으면 0 반환

        List<GameObject> itemsToGive = extracted.ConvertAll(x => x.obj);

        // 실제 스택에서 제거하고 정렬
        stackRes.RemoveAll(x => x.itemID == targetItemID);
        RealignStack(targetItemID);

        return itemsToGive;

    }

    // 스택아이템이 빠졌으면 아래로 정렬
    private void RealignStack(int itemID)
    {
        var config = GetConfig(itemID);
        if (config == null) return;

        var filteredItems = stackRes.FindAll(x => x.itemID == itemID);

        for (int i = 0; i < filteredItems.Count; i++)
        {
            Vector3 targetLocalPos = new Vector3(0, i * config.distY, 0);
            filteredItems[i].obj.transform.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutQuad);
        }
    }


    // 오브젝트 풀링 
    private GameObject GetPooling(GameObject prefab, int itemID, Transform parent)
    {
        if (!itemPools.ContainsKey(itemID))
            itemPools[itemID] = new Stack<GameObject>();

        GameObject obj;
        if (itemPools[itemID].Count > 0)
        {
            obj = itemPools[itemID].Pop();
            obj.SetActive(true);
            obj.transform.SetParent(parent);
        }
        else
        {
            obj = Instantiate(prefab, parent);
        }
        return obj;
    }

    // 풀 반환
    private void ReturnToPool(GameObject obj, int itemID)
    {
        //obj.SetActive(false);
        //obj.transform.SetParent(stackPivot); // 풀에 있을 땐 플레이어 자식으로 정리
        //itemPools[itemID].Push(obj);
    }

    



    // MAX UI
    private IEnumerator ShowMaxUI()
    {
        maxText.SetActive(true);

        // 커지는 연출
        maxText.transform.localScale = Vector3.zero;
        maxText.transform.DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(0.5f);

        // 작아지는 연출과 함께 삭제
        maxText.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
        {
            maxText.SetActive(false);
        });
    }

    private void ShowMaxUIFeedback()
    {
        if (maxUICoroutine != null) StopCoroutine(maxUICoroutine);
        maxUICoroutine = StartCoroutine(ShowMaxUI());
    }


}
