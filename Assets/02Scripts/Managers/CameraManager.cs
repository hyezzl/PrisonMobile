using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum CameraType
{ 
    PlayerCam,
    FirstLevelUpCam,
    CellCam,
}

public class CameraManager : Singleton<CameraManager>
{
    [Header("카메라 설정")]
    [SerializeField] private List<CinemachineVirtualCamera> cams = new List<CinemachineVirtualCamera>();


    private PlayerController pc;

    private void Start()
    {
        ChangeCamera(CameraType.PlayerCam);

        pc = FindObjectOfType<PlayerController>();
        if (pc == null) Debug.Log("CameraManager - Failed to Load PlayerController");
    }


    /// <summary>
    /// 특정 카메라로 전환했다가 일정 시간 후 플레이어에게 돌아오는 코루틴 호출
    /// </summary>
    public void SwitchCameraWithDuration(CameraType targetType, float duration)
    {
        StartCoroutine(CameraSequence(targetType, duration));
    }

    private IEnumerator CameraSequence(CameraType targetType, float duration)
    {
        // 카메라 연출 시작 전 입력 차단
        if (pc != null)
            pc.ChangeGameMode(GameMode.Stop);

        // 1. 대상 카메라 활성화 (우선순위 높임)
        ChangeCamera(targetType);

        // 2. 지정된 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. 다시 플레이어 카메라로 복귀
        ChangeCamera(CameraType.PlayerCam);

        // 대기
        yield return new WaitForSeconds(1f);

        // 모드 재변경
        if (pc != null)
            pc.ChangeGameMode(GameMode.Play);
    }

    public void ChangeCamera(CameraType targetType)
    {
        int index = (int)targetType;

        if (index < 0 || index >= cams.Count)
        {
            Debug.LogWarning($"[CameraManager] {targetType}에 해당하는 카메라가 리스트에 없습니다!");
            return;
        }

        // 모든 카메라의 우선순위를 낮추고 대상만 높임
        for (int i = 0; i < cams.Count; i++)
        {
            if (cams[i] == null) continue;

            // 대상 인덱스면 20, 아니면 10
            cams[i].Priority = (i == index) ? 20 : 10;
        }
    }

}
