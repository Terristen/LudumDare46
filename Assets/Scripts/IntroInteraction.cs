using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroInteraction : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }
}
