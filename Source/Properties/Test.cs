using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Test : MonoBehaviour
{
    private void Start()
    {
        XRBaseInteractable grabbable = GetComponent<XRBaseInteractable>();

        grabbable.onFirstHoverEnter.AddListener(arg0 => Debug.Log("onFirstHoverEnter"));
        grabbable.onLastHoverExit.AddListener(arg0 => Debug.Log("onLastHoverExit"));
        
        grabbable.onSelectEnter.AddListener(arg0 => Debug.LogWarning("onSelectEnter"));
        grabbable.onSelectExit.AddListener(arg0 => Debug.LogWarning("onSelectExit"));
        
        grabbable.onActivate.AddListener(arg0 => Debug.LogWarning("onActivate"));
        grabbable.onDeactivate.AddListener(arg0 => Debug.LogWarning("onDeactivate"));
    }
    
    
}
