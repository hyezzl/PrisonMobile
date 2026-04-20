using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static GameEvents;

public class EndingManager : MonoBehaviour
{
    [Header("UI Ref")]
    [SerializeField] private GameObject screen;
    [SerializeField] private Image title;
    [SerializeField] private Image logo;
    [SerializeField] private Image btn;


    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameEvents.StartEvent>(StartEnding);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameEvents.StartEvent>(StartEnding);
    }


    private void Start()
    {
        // ÃÊ±âÈ­: UIµé ½ºÄÉÀÏÀ» 0À¸·Î ¸¸µé¾îµÒ
        screen.gameObject.SetActive(false);
        title.transform.localScale = Vector3.zero;
        logo.transform.localScale = Vector3.zero;
        btn.transform.localScale = Vector3.zero;
    }

    private void StartEnding(GameEvents.StartEvent evt)
    {
        if (evt.eventID == "E013")
        {
            StartCoroutine(PlayEndingScene());
        }
    }


    private IEnumerator PlayEndingScene()
    {
        // ¹è°æÄÑÁü
        screen?.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        // Å¸ÀÌÆ² ¶ì¿ä¿Ë
        title.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.5f);

        // ·Î°í
        logo.transform.DOScale(1f, 0.8f).SetEase(Ease.OutExpo);

        yield return new WaitForSeconds(0.3f);

        // ¹öÆ° ¶ì¿ä¿Ë
        btn.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
            btn.transform.DOScale(1.1f, 0.6f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        });


    }
}
