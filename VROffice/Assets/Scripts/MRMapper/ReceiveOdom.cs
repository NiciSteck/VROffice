using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
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

    [SerializeField] private int msgsReset = 1;
    private int msgsReceived = 0;
    private AlignSystems _alignSystems;
    
    
    public void Start()
    {
        Setup(port, log_tag, ProcessReceivedBytes);
        _alignSystems = GetComponent<AlignSystems>();
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
        

        //update RealSense transform
        if (cameraPos != Vector3.zero)
        {
            realSense.transform.localPosition = cameraPos;
            realSense.transform.localRotation = cameraRot;
        }
        
        //call to AlignSystems to reset position
        msgsReceived = (msgsReceived + 1) % msgsReset;
        if (msgsReceived == 0)
        {
            if (_alignSystems != null)
            {
                _alignSystems.updateCameras();
            }
        }
    }
}