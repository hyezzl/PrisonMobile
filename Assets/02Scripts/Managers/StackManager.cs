using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


/// <summary>
/// 쌓기로직
/// </summary>
public class StackManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Transform stackPivot;
    [SerializeField] private float distY = 0.2f;    // y간의 간격
    [SerializeField] private float flyDuration = 0.5f;

    [Header("UI Ref")]
    [SerializeField] private GameObject maxText;
    private Coroutine maxUICoroutine;


    // 오브젝트풀
    private Stack<GameObject> pool = new Stack<GameObject>();
    private List<GameObject> stackRes = new List<GameObject>();     // 현재 등에 있는 자원

    private PlayerController pc;


    // 최대값 확인
    public bool IsFull => stackRes.Count >= pc.GetMax();


    private void Awake()
    {
        pc = GetComponent<PlayerController>();
    }


    // Stack
    public void StackPrefab(GameObject prefab, Vector3 startPos)
    {
        if (IsFull) {
            // 더이상 루트X
            // UI
            if(maxUICoroutine != null) StopCoroutine(maxUICoroutine);
            maxUICoroutine = StartCoroutine(ShowMaxUI());

            return;
        }

        // 풀링
        GameObject obj = GetPooling(prefab);

        // 목적지 로컬 위치
        Vector3 targetPos = new Vector3(0, stackRes.Count * distY, 0);
        stackRes.Add(obj);

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



    // 오브젝트 풀링 메서드
    private GameObject GetPooling(GameObject prefab)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Pop();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, stackPivot);
        }
        return obj;
    }    



    // 반환
    public void PopStack()
    {
        if (stackRes.Count == 0) return;

        int last = stackRes.Count - 1;

        GameObject obj = stackRes[last];
        stackRes.RemoveAt(last);

        obj.SetActive(false);
        pool.Push(obj);     // 풀에반환
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
