using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

public class CameraSwitcher : MonoBehaviour
{
    CinemachineClearShot cams;

    
    // Start is called before the first frame update
    void Start()
    {
        cams = GetComponent<CinemachineClearShot>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //previous cam
        if (Input.GetKeyDown(KeyCode.A))
        {
            var sortedCams = cams.ChildCameras.OrderBy(c => c.State.ShotQuality).ToList().TakeWhile(c => c.State.ShotQuality != cams.LiveChild.State.ShotQuality && Mathf.Abs(c.State.ShotQuality - cams.LiveChild.State.ShotQuality) <= 10);
            var prevCam = sortedCams.ToArray()[Random.Range(0, sortedCams.Count())];

            if (prevCam == null)
                return;

            cams.LiveChild = prevCam;
        }

        //next cam
        if (Input.GetKeyDown(KeyCode.D))
        {
            var sortedCams = cams.ChildCameras.OrderBy(c => c.State.ShotQuality).ToList().TakeWhile(c => c.State.ShotQuality != cams.LiveChild.State.ShotQuality && Mathf.Abs(c.State.ShotQuality - cams.LiveChild.State.ShotQuality) <= 10);
            var prevCam = sortedCams.ToArray()[Random.Range(0, sortedCams.Count())];

            if (prevCam == null)
                return;

            cams.LiveChild = prevCam;
        }
    }
}
