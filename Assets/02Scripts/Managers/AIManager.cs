using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIMinerState
{
    Idle,
    MoveTo,     // 자원을 향해 움직임
    Mining,     // 자원캐기
}

public enum AIpoliceState
{ 
    Idle,
    MoveTo,
    Deliver,    // 배달
}


public class AIManager : MonoBehaviour
{
    [Header("AI Ref")]
    public GameObject aiMiner;
    public GameObject aiPolice;


    private void Start()
    {
        // 시작 시, AI 꺼둠
        aiMiner?.SetActive(false);
        aiPolice?.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<GameEvents.StartEvent>(SpawnAI);
    }
    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<GameEvents.StartEvent>(SpawnAI);
    }

    private void SpawnAI(GameEvents.StartEvent evt)
    {
        // 광부 소환
        if (evt.eventID == "E011")
        {
            if (aiMiner != null)
                aiMiner.SetActive(true);    // 광부 3명 소환
        }
        // 경찰 소환
        else if (evt.eventID == "E012")
        { 
            if(aiPolice != null)
                aiPolice.SetActive(true);
        }
    }
}
