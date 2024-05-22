using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferencePool : MonoBehaviour
{
    public int MaxReferenceCount;
    
    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < MaxReferenceCount; i++)
        {
            EventManager.IRegisterations registerations;
        }
    }
    
}
