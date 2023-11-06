using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.PlayerSettings;

/*
 * This script moves the RealSense camera object.
 */
public class ReceiveOdom : RosReceiver
{
    int port = 5002;
    string log_tag = "Odom Receiver";
    [SerializeField] private GameObject realSense;
    [SerializeField] private GameObject m_OVRRig;
    [SerializeField] private bool m_init = true;

    [SerializeField] private int msgsReset = 100;
    private int msgsReceived = 0; //should increase at 10hz
    private AlignCameras alignCameras;
    
    public void Start()
    {
        Setup(port, log_tag, ProcessReceivedBytes);
        alignCameras = GetComponent<AlignCameras>();
    }

    private void ProcessReceivedBytes(byte[] data)
    {
        float[] v = new float[3];
        v[0] = BitConverter.ToSingle(data, 0);
        v[1] = BitConverter.ToSingle(data, 4);
        v[2] = BitConverter.ToSingle(data, 8);
        Vector3 cameraPos = RtabVecToUnity(v);

        float[] q = new float[4];
        q[0] = BitConverter.ToSingle(data, 12);
        q[1] = BitConverter.ToSingle(data, 16);
        q[2] = BitConverter.ToSingle(data, 20);
        q[3] = BitConverter.ToSingle(data, 24);
        Quaternion cameraRot = RtabQuatToUnity(q);
        
        if (m_init)
        {
            transform.position = m_OVRRig.transform.position + alignCameras.dist_fromQuestToRealSense; //idk why offset is too big
            transform.rotation = m_OVRRig.transform.rotation * Quaternion.Euler(alignCameras.rot_fromQuestToRealSense);
            
            realSense.transform.rotation = Quaternion.Inverse(m_OVRRig.transform.rotation); //turn the camera back so it should align with OVR again
            
            m_init = false;
        }

        //update RealSense transform
        if (cameraPos != Vector3.zero)
        {
            realSense.transform.position = transform.position + cameraPos;
            realSense.transform.rotation = transform.rotation * cameraRot;
        }
        
        //call to AlignCameras to reset position
        msgsReceived = (msgsReceived + 1) % msgsReset;
        if (msgsReceived == 0)
        {
            if (alignCameras != null)
            {
                alignCameras.updateCameras();
            }
        }
    }
}