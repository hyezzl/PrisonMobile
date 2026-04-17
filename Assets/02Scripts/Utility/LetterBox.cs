using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterBox : MonoBehaviour
{
    private float targetAspect = 9.0f / 16.0f;  // 고정 비율
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) Debug.LogWarning("LetterBox - Failed to Load Camera");

        if (cam != null)
        {
            float screenAspect = (float)Screen.width / Screen.height;   // 실제 게임화면 가로/세로 비율
            float ratio = screenAspect / targetAspect;                  // 실제/목표 비율의 비율 값

            Rect rect = cam.rect;

            if (ratio < 1) // 가로 > 세로
            {
                rect.width = 1f;
                rect.height = ratio;
                rect.x = 0f;
                rect.y = (1f - ratio) / 2f;
            }
            else
            {
                float revRatio = 1f / ratio;
                rect.width = revRatio;
                rect.height = 1f;
                rect.x = (1f - revRatio) / 2f;
                rect.y = 0f;
            }
            cam.rect = rect;
        }
        else { Debug.Log("LetterBox - Failed to Load Target Camera"); }
    }
}
