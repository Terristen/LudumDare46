using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using System.Linq;
using UnityEngine.AI;
//MEC Free from Unity Asset Store
using MEC;

public class Bobtroller : MonoBehaviour
{
    public float turnSpeed = 10f;
    KeyCode leftKey = KeyCode.A;
    KeyCode rightKey = KeyCode.D;
    int throwButton = 0;

    public float Health = 100;
    public bool isDead = false;

    Animator animator;
    NavMeshAgent agent;

    public Camera currentCam;
    public Texture2D DropCursor;
    public Texture2D ThumpCursor;
    public int MaxDistractions = 30;
    public int CurrentDistractions = 30;

    public float ListenDistance = 30f;
    public List<GameObject> soundMakers = new List<GameObject>();
    public GameObject soundMaker;
    public GameObject nextSoundMaker;
    private SoundEvent focusTarget;

    public float dropHeight = 3;
    public float secondsOfMemory = 5f;
    private List<SoundEvent> Memory = new List<SoundEvent>();

    public bool isWalking = false;
    public float baseSpeed = 1;

    [Range(.01f, 1)]
    public float aggression = 1;

    [Range(.01f,1)]
    public float satiated = 1;

    public float flickPower = 200;
    public bool aggressive = false;

    
    private SoundEvent priorityEvent;
    private CoroutineHandle _moveMonitor;
    private Rigidbody rb;

    void OnSoundEventHandler(SoundEvent e)
    {
        if (Vector3.Distance(transform.position, e.source) <= ListenDistance && e.strength >= .5f)
        {
            //Debug.Log("Sound Heard D:" + Vector3.Distance(transform.position, e.source) + " Rel. Strength:" + e.strength);
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
            //Set restraints on startup if using Rigidbody.
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        _moveMonitor = Timing.RunCoroutine(MonitorMovement().CancelWith(gameObject));

        soundMaker = soundMakers[UnityEngine.Random.Range(0, soundMakers.Count)];
        nextSoundMaker = soundMakers[UnityEngine.Random.Range(0, soundMakers.Count)];

        SoundsListener.OnSoundEvent += OnSoundEventHandler;
    }

    private GameObject GetNewSoundMakerPrefab()
    {
        var tmp = soundMaker;
        soundMaker = nextSoundMaker;
        nextSoundMaker = soundMakers[UnityEngine.Random.Range(0, soundMakers.Count)];
        return tmp;
    }


    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //Checked for Death
        if(Health <= 0)
        {
            //DIE
            animator.SetBool("fall_backward", true);
            isDead = true;
            return;
        }


        RaycastHit hit;
        Ray ray = currentCam.ScreenPointToRay(Input.mousePosition);
        bool hitSomething = Physics.Raycast(ray, out hit, 100.0f);

        if (Input.GetMouseButtonDown(throwButton))
        {
            if (hitSomething)
            {
                SoundMaker sm = hit.transform.gameObject.GetComponent<SoundMaker>();
                if(sm == null && CurrentDistractions > 0)
                {
                    var prefab = GetNewSoundMakerPrefab();
                    GameObject obj = GameObject.Instantiate(prefab, hit.point + (Vector3.up * dropHeight), UnityEngine.Random.rotation);
                    SoundMaker smc = obj.GetComponent<SoundMaker>();
                    if (smc == null)
                        smc = obj.AddComponent<SoundMaker>();

                    //smc.AddListener(this);
                    

                    CurrentDistractions--;

                } else
                {
                    
                    //Debug.Log("Kick the box:" + (ray.direction.normalized * 100).ToString());
                    //hit.rigidbody.AddForceAtPosition(hit.transform.worldToLocalMatrix * ray.direction.normalized * flickPower, hit.point);
                    hit.rigidbody.AddForce(ray.direction.normalized * flickPower, ForceMode.Impulse);
                    
                    SoundsListener.RegisterSoundEvent(new SoundEvent
                    {
                        eventTime = Time.time,
                        isInternal = false,
                        source = hit.point,
                        strength = flickPower * hit.rigidbody.mass
                    });

                }

            }

            


        }

        if (hitSomething)
        {
            SoundMaker sm = hit.transform.gameObject.GetComponent<SoundMaker>();
            if (sm == null)
            {
                Cursor.SetCursor(DropCursor, new Vector2(63,97), CursorMode.Auto);
            } else
            {
                Cursor.SetCursor(ThumpCursor, new Vector2(20,30), CursorMode.Auto);
            }
        } else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        //animator.SetBool("walking", isWalking);
        //animator.SetBool("aggressive", aggressive);
    }

    //Manage the Memory after cycle processes are done
    private void LateUpdate()
    {
        List<SoundEvent> toDelete = new List<SoundEvent>();
        Memory.ForEach((SoundEvent e) => { if (Time.time - e.eventTime > secondsOfMemory) toDelete.Add(e); });
        foreach(SoundEvent e in toDelete)
        {
            Memory.Remove(e);
        }

        if(Memory.Count == 0)
        {
            Vector2 tmp = (UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(3f, ListenDistance));
            Vector3 newTarget = transform.position + new Vector3(tmp.x,transform.position.y,tmp.y);
            SoundEvent randEvent = new SoundEvent {
                eventTime = Time.time,
                source = newTarget,
                strength = 2,
                isInternal = true
            };
            Memory.Add(randEvent);
        }

    }

   

    // Update is called once per frame
    void FixedUpdate()
    {
        //Secret Manual Control --> remove before launch ;)
        //if (Input.GetKey(leftKey))
        //{
        //    transform.Rotate(Vector3.up, Time.fixedDeltaTime * turnSpeed * -1);
        //}
        //if (Input.GetKey(rightKey))
        //{
        //    transform.Rotate(Vector3.up, Time.fixedDeltaTime * turnSpeed * 1);
        //}

        

        
        
    }

    //public void HearSound(SoundEvent bump)
    //{
    //    Memory.Add(bump);
    //}



    private IEnumerator<float> MonitorMovement()
    {
        agent.angularSpeed = turnSpeed;
        while (true)
        {
            Vector3 newDest = transform.position;

            if (Memory.Count > 0 && !isDead)
            {
                //aggressive = true;

                
                Vector3 avgSoundHeading = new Vector3(
                    Memory.Average(x => (x.source - transform.position).x),
                    Memory.Average(x => (x.source - transform.position).y),
                    Memory.Average(x => (x.source - transform.position).z)
                );

                //Vector3 avgEventPosition = new Vector3(
                //    Memory.Average(x => x.source.x),
                //    Memory.Average(x => x.source.y),
                //    Memory.Average(x => x.source.z)
                //);

                //newDest = Vector3.Lerp(avgEventPosition, Memory.Last().source, 0.5f);
                SoundEvent mostImportantSound = Memory.OrderBy(e => {
                    float scalar = 1 - ((Time.time - e.eventTime) / secondsOfMemory); //Time-based sound importance reduction
                    return e.strength * scalar;
                }).LastOrDefault();

                //SoundEvent lastSound = Memory.OrderBy(e => {
                //    return e.eventTime;
                //}).LastOrDefault();
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
                agent.speed = baseSpeed;// (aggressive) ? baseSpeed * 1.2f: baseSpeed;
                agent.speed *= Mathf.Min(1, ((180 - Vector3.Angle(transform.rotation * Vector3.forward, agent.steeringTarget - transform.position)) / 180) * 1.25f);
                //agent.speed *= (!aggressive) ? 1 : 1 + (1 - aggression);
                animator.SetBool("walking", true);
                animator.SetFloat("vmagnitude", baseSpeed);
            }
            else if(!isDead)
            {
                animator.SetBool("walking", false);
                animator.SetFloat("vmagnitude", baseSpeed);

                //arrived at target so remove it from memory
                if (Vector3.Distance(focusTarget.source, transform.position) <= 3)
                {
                    Memory.Remove(focusTarget);
                }

            } else
            {
                agent.speed = 0;
                agent.ResetPath();
            }


            if(!isDead)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(newDest - transform.position), Time.deltaTime * turnSpeed);

            yield return Timing.WaitForOneFrame;
        }
    }
}
