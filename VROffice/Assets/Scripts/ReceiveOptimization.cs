using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveOptimization : RosReceiver
{
    int port = 5005;
    string log_tag = "Optimization Receiver";
    
    
    public void Start()
    {
        Setup(port, log_tag, ProcessReceivedBytes);
    }
    
    private void ProcessReceivedBytes(byte[] data)
    {
        float[] q = new float[4];
        q[0] = BitConverter.ToSingle(data, 0);
        q[1] = BitConverter.ToSingle(data, 4);
        q[2] = BitConverter.ToSingle(data, 8);
        q[3] = BitConverter.ToSingle(data, 12);
        Quaternion optimalRot = RtabQuatToUnity(q);
        
        
    }
}
