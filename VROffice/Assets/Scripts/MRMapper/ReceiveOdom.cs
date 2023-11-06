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

    public void Start()
    {
        Setup(port, log_tag, ProcessReceivedBytes);
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
            transform.position = m_OVRRig.transform.position; //align RealSense to OVR by moving MRMapper to OVR and then move by distance between cameraPos and its origin 
            //Quaternion mapperToOVR = Quaternion.FromToRotation(transform.forward, m_OVRRig.transform.forward);
            transform.rotation = m_OVRRig.transform.rotation; //rotate the angle between where the headset is looking and where the ROS initialized the coordinate system base

            realSense.transform.position = m_OVRRig.transform.position + new Vector3(0, 0.05f, 0);
            realSense.transform.rotation = Quaternion.Inverse(m_OVRRig.transform.rotation); //turn the camera back so it should align with OVR again
            
            m_init = false;
            }

        //update RealSense transform
        if (cameraPos != Vector3.zero)
        {
            realSense.transform.position = transform.position + cameraPos;
            realSense.transform.rotation = transform.rotation * cameraRot;
        }
    }
}