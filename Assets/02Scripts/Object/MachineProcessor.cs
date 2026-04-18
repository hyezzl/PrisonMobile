
using UnityEngine;
using System.Collections;
using DG.Tweening;


public class MachineProcessor : MonoBehaviour
{
    [Header("연결된 구역")]
    public GiveZone inputZone;  // 돌이 들어있는 곳
    public TakeZone outputZone; // 수갑이 나갈 곳

    [Header("생산 설정")]
    public GameObject handcuffPrefab; // 수갑 프리팹
    public float processTime = 0.5f;  // 가공 시간

    private bool isProcessing = false;

    private void Update()
    {
        // 입력 구역에 돌이 있고, 가공 중이 아니라면 가공 시작
        if (!isProcessing && inputZone.giveQueue.Count > 0)
        {
            StartCoroutine(ProcessRoutine());
        }
    }
    
    private IEnumerator ProcessRoutine()
    {
        isProcessing = true;

        // 1. 입력 구역(GiveZone)에서 돌 하나 빼기
        GameObject stone = inputZone.giveQueue.Dequeue();

        // 돌이 사라지는 연출 (작아지면서 삭제)
        stone.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
            Destroy(stone);
        });

        // 2. 가공 시간 대기 (0.5초)
        yield return new WaitForSeconds(processTime);

        // 3. 수갑 생성
        GameObject newHandcuff = Instantiate(handcuffPrefab);

        // 가공기 위치에서 시작하도록 세팅 (선택 사항)
        newHandcuff.transform.position = transform.position;

        // 4. 출력 구역(TakeZone)에 수갑 전달
        outputZone.AddItem(newHandcuff);

        isProcessing = false;
    }
}