using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FixCamera : CinemachineExtension
{
    [Header("고정할 각도 설정")]
    public Vector3 fixedRotation = new Vector3(29.017f, -33.69f, 0f);

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // Finalize 단계(모든 계산이 끝난 시점)에서 회전값을 강제로 덮어씁니다.
        if (stage == CinemachineCore.Stage.Finalize)
        {
            state.RawOrientation = Quaternion.Euler(fixedRotation);
        }
    }
}
