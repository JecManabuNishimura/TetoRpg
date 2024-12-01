using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraStackManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera[] overlayCameras;

    void Start()
    {
        var mainCamData = mainCamera.GetUniversalAdditionalCameraData();
        mainCamData.cameraStack.Clear();
        mainCamData.cameraStack.Add(Camera.allCameras.First(_ => _.name == "Main Camera"));
        
    }


}
