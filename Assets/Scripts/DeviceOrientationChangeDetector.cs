using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DeviceOrientationChangeDetector : MonoBehaviour
{
    public float lanscapeZ = -7.75f;
    public float portraitZ = -15f; 

    bool prevIsLandscapeOrientation;
    
    void Start(){
        prevIsLandscapeOrientation = isLandscape;
        UpdateCamera();
    }

    void UpdateCamera() {
        Vector3 pos = Camera.main.transform.position;
        pos.z = isLandscape ? lanscapeZ : portraitZ;
        Camera.main.transform.position = pos;
    }

    void Update(){
        if(isLandscape != prevIsLandscapeOrientation)
            UpdateCamera();
        prevIsLandscapeOrientation = isLandscape;
    }

    bool isLandscape{
        get {
            return Input.deviceOrientation == DeviceOrientation.LandscapeLeft ||
                Input.deviceOrientation == DeviceOrientation.LandscapeRight;
        }
    }
}
