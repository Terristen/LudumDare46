using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    AudioSource aud;
    public ParticleSystem particles;
    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
    }

   

    private void OnTriggerEnter(Collider other)
    {
        
        Bobtroller bob = other.gameObject.GetComponent<Bobtroller>();
        if(bob != null)
        {
            Debug.Log("Caught in a Trap! -- Boom!");
            bob.Health -= 25f;
            bob.Health = Mathf.Max(0, bob.Health);

            if (aud != null)
            {
                aud.Play();
            }

            SoundsListener.RegisterSoundEvent(new SoundEvent {
                eventTime = Time.time,
                isInternal = false,
                source = transform.position,
                strength = 200
            });

            if (particles != null)
            {
                particles.Play();
            }
        }
            
        
    }
}
