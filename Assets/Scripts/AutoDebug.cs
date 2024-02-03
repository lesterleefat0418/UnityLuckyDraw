using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AutoDebug : MonoBehaviour
{
    public CanvasGroup debugText, dryrunText;
    public bool autoDebug = false;
    public loopEvent[] steps;
    private IEnumerator currentIEnumerator = null;


    private void Start()
    {
    }


    private void Update()
    {
        if(ConfigPage.Instance != null) { 
            if (ConfigPage.Instance.autoDebug) {  
                if(!autoDebug) {
                    showDebugText(true);
                    startDebug();
                    autoDebug = true;
                }
            }
            else
            {
                autoDebug= false;
                showDebugText(false);
                if (this.currentIEnumerator != null) StopCoroutine(this.currentIEnumerator);
            }


            if (ConfigPage.Instance.dryrunMode)
            {
                showDryrunText(true);
            }
            else
            {
                showDryrunText(false);
            }
        }
    }

    public void startDebug()
    {
        this.currentIEnumerator = loopEvents();
        StartCoroutine(this.currentIEnumerator);
    }

    IEnumerator loopEvents(){
        for (int i = 0; i < steps.Length; i++)
        {
            yield return new WaitForSeconds(steps[i].NextStepDelay);
            this.showDebug("preparing to loop: " + steps[i].Name);
            if (steps[i] != null) steps[i].Invoke();
        }
    }

    void showDebugText(bool status)
    {
        if (debugText != null)
        {
            debugText.alpha = status ? 1f:0f;
        }
    }

    void showDryrunText(bool status)
    {
        if (dryrunText != null)
        {
            dryrunText.alpha = status ? 1f : 0f;
        }
    }

    public void showDebug(string msg)
    {
#if UNITY_EDITOR
        Debug.Log(msg);
#endif
    }
}

[Serializable]
public class loopEvent
{
    public string Name;
    public UnityEvent steps;
    public float NextStepDelay;

    public void Invoke()
    {
        if(steps != null) steps.Invoke();
    }
}
