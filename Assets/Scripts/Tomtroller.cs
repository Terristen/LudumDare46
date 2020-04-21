using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using System.Linq;
using UnityEngine.AI;
//MEC Free from Unity Asset Store
using MEC;

public class Tomtroller : MonoBehaviour
{
    public float turnSpeed = 10f;
 
    public float Health = 100;
    public bool isDead = false;

    Animator animator;
    NavMeshAgent agent;

    
    public float ListenDistance = 30f;
    
    private SoundEvent focusTarget;

    public float secondsOfMemory = 5f;
    private List<SoundEvent> Memory = new List<SoundEvent>();

    public bool isWalking = false;
    public float baseSpeed = 1;

    
    public bool aggressive = false;


    private SoundEvent priorityEvent;
    private CoroutineHandle _moveMonitor;
    private Rigidbody rb;

    void OnSoundEventHandler(SoundEvent e)
    {
        if (Vector3.Distance(transform.position, e.source) <= ListenDistance && e.strength >= .5f)
        {
            Memory.Add(e);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        _moveMonitor = Timing.RunCoroutine(MonitorMovement().CancelWith(gameObject));

        

        SoundsListener.OnSoundEvent += OnSoundEventHandler;
    }

    


    private void Update()
    {
        //Checked for Death
        if (Health <= 0)
        {
            //DIE
            animator.SetBool("fall_backward", true);
            isDead = true;
            return;
        }


    }

    //Manage the Memory after cycle processes are done
    private void LateUpdate()
    {
        List<SoundEvent> toDelete = new List<SoundEvent>();
        Memory.ForEach((SoundEvent e) => { if (Time.time - e.eventTime > secondsOfMemory) toDelete.Add(e); });
        foreach (SoundEvent e in toDelete)
        {
            Memory.Remove(e);
        }

        if (Memory.Count == 0)
        {
            Vector2 tmp = (UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(3f, ListenDistance));
            Vector3 newTarget = transform.position + new Vector3(tmp.x, transform.position.y, tmp.y);
            SoundEvent randEvent = new SoundEvent
            {
                eventTime = Time.time,
                source = newTarget,
                strength = 2,
                isInternal = true
            };
            Memory.Add(randEvent);
        }

    }




    private IEnumerator<float> MonitorMovement()
    {
        agent.angularSpeed = turnSpeed;
        while (true)
        {
            Vector3 newDest = transform.position;

            if (Memory.Count > 0 && !isDead)
            {


                Vector3 avgSoundHeading = new Vector3(
                    Memory.Average(x => (x.source - transform.position).x),
                    Memory.Average(x => (x.source - transform.position).y),
                    Memory.Average(x => (x.source - transform.position).z)
                );

             
                SoundEvent mostImportantSound = Memory.OrderBy(e => {
                    float scalar = 1 - ((Time.time - e.eventTime) / secondsOfMemory); //Time-based sound importance reduction
                    return e.strength * scalar;
                }).LastOrDefault();

                focusTarget = mostImportantSound;
                newDest = focusTarget.source;
                agent.ResetPath();
                agent.SetDestination(newDest);

                aggressive = !focusTarget.isInternal;
            }
            else //There are no objectives to process
            {
                aggressive = false;
            }




            ///RPG Controller
            ///
            if (!isDead && agent.velocity.sqrMagnitude > 0.002)
            {
                agent.speed = baseSpeed;
                agent.speed *= Mathf.Min(1, ((180 - Vector3.Angle(transform.rotation * Vector3.forward, agent.steeringTarget - transform.position)) / 180) * 1.25f);
                animator.SetBool("walking", true);
                animator.SetFloat("vmagnitude", baseSpeed);
            }
            else if (!isDead)
            {
                animator.SetBool("walking", false);
                animator.SetFloat("vmagnitude", baseSpeed);

                //arrived at target so remove it from memory
                if (Vector3.Distance(focusTarget.source, transform.position) <= 3)
                {
                    Memory.Remove(focusTarget);
                }

            }
            else
            {
                agent.speed = 0;
                agent.ResetPath();
            }


            if (!isDead)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(newDest - transform.position), Time.deltaTime * turnSpeed);

            yield return Timing.WaitForOneFrame;
        }
    }
}
