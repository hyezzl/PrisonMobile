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

    [Header("레이어 피벗 설정")]
    [SerializeField] private Transform pivot01; // 돌전용 + 우선순위 (플레이어에 더 가까움)
    [SerializeField] private Transform pivot02;

    [Header("ID 설정")]
    [SerializeField] private int stoneID = 1001;
    [SerializeField] private int moneyID = 1003;


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

        // 돌을 주웠을때, 돈을 가지고있다면 돈을 밀어냄
        if (itemID == stoneID) UpdateMoneyColumnPivot(true);

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

        // 리스트에 넣기전에 돈 기둥을 체크해서 밀어냄
        if (itemID == stoneID)
        {
            UpdateMoneyColumnPivot(true); // true: 강제로 밀어내기 모드
        }

        stackRes.Add(new StackData(obj, itemID));       // 리스트에 추가

        // 돌이면 무조건 pivot1, 돈이면 현재상황에 맞게
        Transform targetPivot = (itemID == moneyID) ? GetCurrentMoneyPivot() : config.pivot;

        int curCount = stackRes.FindAll(x => x.itemID == itemID).Count - 1;
        Vector3 targetLocalPos = new Vector3(0, curCount * config.distY, 0);

        // 해당 아이템 전용 피봇(위치)으로 날려보냄
        CollectAnimate(obj, obj.transform.position, targetLocalPos, targetPivot);

        // UI 업데이트
        if (itemID == moneyID)
        {
            int totalMoney = stackRes.FindAll(x => x.itemID == moneyID).Count;
            UIManager.Instance.UpdateMoneyUI(totalMoney);
        }
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

        // 만약 돌을 다 썼다면 돈 기둥을 1번 피벗으로 당겨옴
        if (targetItemID == stoneID) UpdateMoneyColumnPivot();

        // UI업데이트
        if (targetItemID == moneyID)
        {
            UIManager.Instance.UpdateMoneyUI(0); // 다 줬으니까 0으로
        }

        return itemsToGive;
    }

    // 필요한 만큼만 POP
    public List<GameObject> PopItems(int targetItemID, int count)
    {
        // 해당 ID의 아이템만 필터링
        List<StackData> allOfThisType = stackRes.FindAll(x => x.itemID == targetItemID);

        // 부족하면 가진 만큼만 주거나, 혹은 아예 안 주거나 선택 (여기선 가진 만큼만)
        int amountToGive = Mathf.Min(count, allOfThisType.Count);
        if (amountToGive <= 0) return null;

        List<GameObject> itemsToGive = new List<GameObject>();

        for (int i = 0; i < amountToGive; i++)
        {
            // 리스트의 맨 뒤(가장 위)에 있는 것부터 꺼냄
            StackData data = allOfThisType[allOfThisType.Count - 1 - i];
            itemsToGive.Add(data.obj);
            stackRes.Remove(data); // 실제 전체 리스트에서 제거
        }

        // 남은 아이템들 아래로 정렬
        RealignStack(targetItemID);

        // 돌을 썼을 때 돈 기둥 위치 체크 (기존 로직 활용)
        if (targetItemID == stoneID) UpdateMoneyColumnPivot();

        // UI 업데이트
        if (targetItemID == moneyID)
        {
            int remainingCount = stackRes.FindAll(x => x.itemID == moneyID).Count;
            UIManager.Instance.UpdateMoneyUI(remainingCount);
        }

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

    //===============================================
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


    //===============================================
    // 돈-돌의 컬럼위치 변경
    private Transform GetCurrentMoneyPivot()
    {
        // 돌이 하나라도 있으면 Pivot02, 없으면 Pivot01 반환
        int stoneCount = stackRes.FindAll(x => x.itemID == stoneID).Count;
        return (stoneCount > 0) ? pivot02 : pivot01;
    }

    private void UpdateMoneyColumnPivot()
    {
        Transform targetPivot = GetCurrentMoneyPivot();
        var moneyItems = stackRes.FindAll(x => x.itemID == moneyID);

        for (int i = 0; i < moneyItems.Count; i++)
        {
            GameObject obj = moneyItems[i].obj;
            var config = GetConfig(moneyID);

            // 부모 변경
            obj.transform.SetParent(targetPivot);

            // 부드럽게 새 피벗의 위치로 이동합니다.
            Vector3 targetPos = new Vector3(0, i * config.distY, 0);
            obj.transform.DOLocalMove(targetPos, 0.25f).SetEase(Ease.OutQuad);
            obj.transform.DOLocalRotate(Vector3.zero, 0.25f);
        }
    }

    private void UpdateMoneyColumnPivot(bool isForcing = false)
    {
        // 돌이 있거나, 지금 막 추가될 예정(isForcing)이면 Pivot02로 타겟 설정
        int stoneCount = stackRes.FindAll(x => x.itemID == stoneID).Count;
        Transform targetPivot = (stoneCount > 0 || isForcing) ? pivot02 : pivot01;

        var moneyItems = stackRes.FindAll(x => x.itemID == moneyID);

        for (int i = 0; i < moneyItems.Count; i++)
        {
            GameObject obj = moneyItems[i].obj;
            var config = GetConfig(moneyID);

            obj.transform.SetParent(targetPivot);

            Vector3 targetPos = new Vector3(0, i * config.distY, 0);

            // 트윈 충돌 방지
            obj.transform.DOKill();
            obj.transform.DOLocalMove(targetPos, 0.25f).SetEase(Ease.OutQuad);
            obj.transform.DOLocalRotate(Vector3.zero, 0.25f);
        }
    }




    //===============================================
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
