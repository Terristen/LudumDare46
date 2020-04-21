using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SoundEvent
{
    public Vector3 source;
    public float strength;
    public float eventTime;
    public bool isInternal;
}

public class SoundMaker : MonoBehaviour
{
    
    List<Bobtroller> listeners = new List<Bobtroller>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void AddListener(Bobtroller listener)
    //{
    //    listeners.Add(listener);
    //}

    public void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        //listeners.ForEach((x) => x.HearSound(new SoundEvent { source = collision.GetContact(0).point, strength = collision.relativeVelocity.magnitude * rb.mass, eventTime = Time.time, isInternal=false }));
        SoundEvent se = new SoundEvent { source = collision.GetContact(0).point, strength = collision.relativeVelocity.magnitude * rb.mass, eventTime = Time.time, isInternal = false };

        SoundsListener.RegisterSoundEvent(se);
    }
}
