using UnityEngine;

/*
 *  This class is used to access the controller start button, which resets the head pose.
 */
public class HandPinchMonitor : MonoBehaviour
{
    private void Start()
    {
        OVRManager.display.RecenterPose();
    }

    void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.Start))
            OVRManager.display.RecenterPose();
    }
}
