using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsListener : MonoBehaviour
{
    public static SoundsListener instance;

    public delegate void SoundEventHandler(SoundEvent e);
    public static SoundEventHandler OnSoundEvent;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null) 
            instance = this;


    }

    public static void RegisterSoundEvent(SoundEvent e)
    {
        OnSoundEvent?.Invoke(e);
    }
    
}
