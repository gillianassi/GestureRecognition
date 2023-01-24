using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem Particle;
    public float ActivationDuration = 2.0f;

    private float currentTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (Particle == null)
        {
            return;
        }
        Particle.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Particle == null)
        {
            return;
        }

        if (!Particle.isPlaying)
        {
            return;
        }
        
        currentTime -= Time.deltaTime;
        if(currentTime <  0) 
        { 
            Particle.Stop();
        }
    }

    public void PlayPaticle()
    {
        if (Particle == null) 
        { 
            return; 
        }

        if(Particle.isPlaying)
        {
            return;
        }

        Particle.Play();
        currentTime= ActivationDuration;
    }
    public void StopPaticle()
    {
        if (Particle == null)
        {
            return;
        }

        if (!Particle.isPlaying)
        {
            return;
        }

        Particle.Stop();
    }
}
