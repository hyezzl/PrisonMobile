using UnityEngine;

public class FloatingArrow : MonoBehaviour
{
    [Header("둥둥 연출 설정")]
    [SerializeField] private float amplitude = 0.1f; // 움직임 범위
    [SerializeField] private float frequency = 6f;   // 움직임 속도

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 둥둥거리는 연출
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}