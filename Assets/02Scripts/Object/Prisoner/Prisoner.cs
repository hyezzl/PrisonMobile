using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Prisoner : MonoBehaviour
{
    [Header("UI연결")]
    public PrisonerUI ui;

    public int requiredHandcuffs; // 필요한 수갑 개수
    public int currentHandcuffs = 0;


    public bool isSatisfied => currentHandcuffs >= requiredHandcuffs;

    // 줄의 맨 앞에 도착했을 때 호출
    public void ShowRequestUI() => ui.Show();



    public void Init()
    {
        requiredHandcuffs = Random.Range(2, 5); // 2~4개
        currentHandcuffs = 0;
        ui.InitUI(requiredHandcuffs);
    }

    // 수갑을 가져가는 연출
    public Tween TakeHandcuff(GameObject handcuff, float moveDuration)
    {
        ui.Show();

        handcuff.transform.SetParent(transform);
        // 점프 애니메이션 실행
        return handcuff.transform.DOLocalJump(Vector3.up * 1.5f, 2f, 1, moveDuration)
            .OnComplete(() =>
            {
                currentHandcuffs++;
                ui.UpdateUI(currentHandcuffs, requiredHandcuffs, 0.1f); // 숫자 갱신
                handcuff.SetActive(false);
            });
    }

    public void GoToPrison(Transform prisonTarget)
    {
        ui.Hide();      // UI끔
        transform.DOMove(prisonTarget.position, 1.5f).OnComplete(() => {
            Destroy(gameObject); // 감옥 도착 시 처리 (혹은 비활성화)
        });
    }
}
