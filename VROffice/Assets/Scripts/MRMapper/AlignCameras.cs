using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


/*
 * This script recenters the Oculus Quest when it is too far from the camera position. 
 */
public class AlignCameras : MonoBehaviour
{
    GameObject realSense;
    GameObject oculus;
    GameObject offset; 

    // define the position of the Oculus relative to the RealSense
    public Vector3 dist_fromQuestToRealSense = new Vector3(0, 0.05f, 0);
    public Vector3 rot_fromQuestToRealSense = new Vector3(0, 0, 0);

    private Quaternion realSenseToOculusRot;


    void Start()
    {
        realSense = GameObject.Find("RealSense");
        oculus = GameObject.Find("CenterEyeAnchor");
        offset = GameObject.Find("OVRCameraRig"); 
    }

    public void updateCameras()
    {
        realSenseToOculusRot = Quaternion.Euler(rot_fromQuestToRealSense); 

        // compute where the RealSense should be positioned 
        Vector3 shouldPos = oculus.transform.position + dist_fromQuestToRealSense;
        Quaternion shouldRot = oculus.transform.rotation * realSenseToOculusRot;

        // where the RealSense is currently positioned
        Vector3 isPos = realSense.transform.position;
        Quaternion isRot = realSense.transform.rotation;   


        // compute the correction 
        Vector3 posDiff = shouldPos - isPos;
        Quaternion rotDiff = shouldRot * Quaternion.Inverse(isRot);

        float absPosDiff = posDiff.magnitude;
        float absRotDiff = 2f * Mathf.Rad2Deg * Mathf.Acos(Mathf.Abs(rotDiff.w));


        float tPos = 0.05f;
        float tRot = 10f;

        //move the entire MRMapper object to realign cameras and other children based on that
        if (absPosDiff > tPos || absRotDiff > tRot)
        {
            transform.position = posDiff + transform.position;
            transform.rotation = rotDiff * transform.rotation;
            Debug.Log("Resetting Oculus offset"); 
        }

  
    }
}
