using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSetting : MonoBehaviour
{
    [Header("런타임 해상도")]
    [SerializeField] private Vector2 resolution = new Vector2(720, 1280);    // 게임 실행 해상도 (SetResolution)

    [Header("Basic Refs")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera targetCam;          // 렌더링 카메라
    [SerializeField] private RenderMode renderMode;     // 렌더링 방식
    [SerializeField] private float distance = 8f;      // 카메라-캔버스 거리

    private CanvasScaler scaler;

    private void Awake()
    {
        // 렌더링 설정
        if (canvas.renderMode != renderMode)
            canvas.renderMode = renderMode;

        canvas.worldCamera = targetCam;
        if (renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.planeDistance = distance;
        }

        // 해상도 설정
        if (!TryGetComponent<CanvasScaler>(out scaler))
        {
            Debug.Log("CanvasSetting - Failed to Load CanvasScaler");
        }
        else
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 해상도에 따라 크기 조정
            scaler.referenceResolution = resolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void Start()
    {
        // 게임 시작 시, 설정 해상도 고정
        Screen.SetResolution((int)resolution.x, (int)resolution.y, true); // 전체화면 고정
    }
}
