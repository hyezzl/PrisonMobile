using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            if (Instance == null)
            {
                Debug.Log("½Ì±ÛÅæ Çüº¯È¯ ½ÇÆÐ");
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        else { 
            Destroy(gameObject);
            return;
        }

        DoAwake();
    }

    protected virtual void DoAwake() { }
}
