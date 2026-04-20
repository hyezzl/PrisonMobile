using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBillboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        Vector3 camRotation = camTransform.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(0f, camRotation.y, 0f);
    }
}
