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
/// 쌓기로직
/// </summary>
public class StackManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Transform stackPivot;
    [SerializeField] private float distY = 0.2f;    // 아이템간의 y간격
    [SerializeField] private float flyDuration = 0.5f;

    [Header("UI Ref")]
    [SerializeField] private GameObject maxText;
    private Coroutine maxUICoroutine;

    // 현재 가지고 있는 아이템
    private List<StackData> stackRes = new List<StackData>();

    // ID별 풀
    private Dictionary<int, Stack<GameObject>> itemPools = new Dictionary<int, Stack<GameObject>>();


    // 오브젝트풀
    //private Stack<GameObject> pool = new Stack<GameObject>();
    //private List<GameObject> stackRes = new List<GameObject>();     // 현재 등에 있는 자원

    private PlayerController pc;


    // 최대값 확인
    public bool IsFull => stackRes.Count >= pc.GetMax();


    private void Awake()
    {
        pc = GetComponent<PlayerController>();
    }


    // Stack
    public void StackPrefab(GameObject prefab, int itemID, Vector3 startPos)
    {
        if (IsFull) {
            // 더이상 루트X
            // UI
            if(maxUICoroutine != null) StopCoroutine(maxUICoroutine);
            maxUICoroutine = StartCoroutine(ShowMaxUI());

            return;
        }

        // 풀링
        GameObject obj = GetPooling(prefab, itemID);

        // 리스트에 데이터 추가
        stackRes.Add(new StackData(obj, itemID));

        // 목적지 로컬 위치
        Vector3 targetPos = new Vector3(0, (stackRes.Count -1) * distY, 0);

        // 연출
        CollectAnimate(obj, startPos, targetPos);

    }


    // 스택연출
    private void CollectAnimate(GameObject obj, Vector3 startPos, Vector3 targetPos)
    {
        // 날아가는 동안 플레이어와 분리
        obj.transform.SetParent(null);
        obj.transform.position = startPos;

        // 최종좌표
        Vector3 targetRealPos = stackPivot.TransformPoint(targetPos);

        obj.transform.DOJump(targetRealPos, 2f, 1, flyDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 도착하면 부모설정
                obj.transform.SetParent(stackPivot);
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

        // 리스트에서 제거
        //stackRes.RemoveAll(x => x.itemID == targetItemID);
        //RealignStack();
        // 4. 골라낸 아이템들을 납부존으로 촤르르륵 날려보내기
        //for (int i = 0; i < extracted.Count; i++)
        //{
        //    GameObject obj = extracted[i].obj;
        //    obj.transform.SetParent(null); // 부모 해제

        //    obj.transform.DOJump(targetPivot.position, 1.5f, 1, 0.3f)
        //        .SetDelay(i * 0.05f)
        //        .SetEase(Ease.InQuad)
        //        .OnComplete(() => {
        //            ReturnToPool(obj, targetItemID); // 도착하면 풀로 반환
        //        });
        //}

        //return extracted.Count;     // 보낸 갯수 리턴

        // 반환할 실제 오브젝트들만 따로 리스트
        List<GameObject> itemsToGive = new List<GameObject>();
        foreach (var data in extracted)
        {
            itemsToGive.Add(data.obj);
        }

        // 실제 스택에서 제거하고 정렬
        stackRes.RemoveAll(x => x.itemID == targetItemID);
        RealignStack();

        return itemsToGive;

    }

    // 스택아이템이 빠졌으면 아래로 정렬
    private void RealignStack()
    {
        for (int i = 0; i < stackRes.Count; i++)
        {
            Vector3 targetLocalPos = new Vector3(0, i * distY, 0);

            stackRes[i].obj.transform.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutQuad);
        }
    }


    // 오브젝트 풀링 
    private GameObject GetPooling(GameObject prefab, int itemID)
    {
        if (!itemPools.ContainsKey(itemID))
            itemPools[itemID] = new Stack<GameObject>();

        GameObject obj;
        if (itemPools[itemID].Count > 0)
        {
            obj = itemPools[itemID].Pop();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, stackPivot);
        }
        return obj;
    }

    // 풀 반환
    private void ReturnToPool(GameObject obj, int itemID)
    {
        obj.SetActive(false);
        obj.transform.SetParent(stackPivot); // 풀에 있을 땐 플레이어 자식으로 정리
        itemPools[itemID].Push(obj);
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


}
