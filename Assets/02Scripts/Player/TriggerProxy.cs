using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerProxy : MonoBehaviour
{
    public PlayerInteractHandler pHandler;

    private void OnTriggerEnter(Collider other)
    {
        pHandler.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        pHandler.OnTriggerExit(other);
    }
}
