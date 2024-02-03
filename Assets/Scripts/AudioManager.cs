using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource spinningEffect;
    public AudioSource congratulationEffect;
    public AudioSource startButtonEffect;
    public AudioSource machinePushEffect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySpinning()
    {
        if(this.spinningEffect != null)
        {
            this.spinningEffect.Stop();
            this.spinningEffect.Play();
        }
    }

    public void PlayCongratulation()
    {
        if (this.congratulationEffect != null)
        {
            this.congratulationEffect.Stop();
            this.congratulationEffect.Play();
        }
    }

    public void PlayStartButton()
    {
        if (this.startButtonEffect != null)
        {
            this.startButtonEffect.Stop();
            this.startButtonEffect.Play();
        }
    }

    public void PlayMachinePush()
    {
        if (this.machinePushEffect != null)
        {
            this.machinePushEffect.Stop();
            this.machinePushEffect.Play();
        }
    }

}
