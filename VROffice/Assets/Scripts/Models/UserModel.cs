using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserModel 
{
    private Matrix4x4 currPose;
    private Matrix4x4 currPoseInv;
    private Matrix4x4 prevPose;
    private Matrix4x4 prevPoseInv; 

    public Vector3 position
    {
        get { return currPose.GetPosition(); }
    }

    public Vector3 forward
    {
        get { return currPose.MultiplyVector(Vector3.forward).normalized; }
    }

    public Vector3 right
    {
        get { return currPose.MultiplyVector(Vector3.right).normalized;  }
    }

    public Vector3 up
    {
        get { return currPose.MultiplyVector(Vector3.up).normalized; }
    }


    public Vector3 lastPosition
    {
        get { return prevPose.GetPosition(); }
    }

    public Vector3 lastForward
    {
        get { return prevPose.MultiplyVector(Vector3.forward).normalized; }
    }

    public Vector3 lastRight
    {
        get { return prevPose.MultiplyVector(Vector3.right).normalized; }
    }

    public Vector3 lastUp
    {
        get { return prevPose.MultiplyVector(Vector3.up).normalized; }
    }

    private Matrix4x4 getPose(Transform t)
    {
        Vector3 position = t.position;
        Vector3 forward = t.forward;
        forward.y = 0;
        forward.Normalize();
        return Matrix4x4.TRS(position, Quaternion.LookRotation(forward), Vector3.one);
    }

    public void setSourcePose(UserModel pose)
    {
        prevPose = pose.prevPose;
        prevPoseInv = pose.prevPoseInv;
    }

    public void setSourcePose(Transform user)
    {
        prevPose = getPose(user);
        prevPoseInv = prevPose.inverse;
    }

    public void setTargetPose(Transform user)
    {
        currPose = getPose(user);
        currPoseInv = currPose.inverse;
    }

    public void update(Transform user, bool updatePose)
    {
        prevPose = currPose;
        if (updatePose) {
            currPose = getPose(user);
        }

        prevPoseInv = prevPose.inverse;
        currPoseInv = currPose.inverse;
    }

    public UserModel(Matrix4x4 pose)
    {
        currPose = pose;
        prevPose = pose;

        currPoseInv = currPose.inverse;
        prevPoseInv = pose.inverse;
    }

    public UserModel(Transform user)
    {
        currPose = getPose(user);
        prevPose = getPose(user);

        currPoseInv = currPose.inverse;
        prevPoseInv = prevPose.inverse;
        
    }

    public Vector3 currLocalPoint(Vector3 point)
    {
        return currPoseInv.MultiplyPoint3x4(point);
    }

    public Vector3 currGlobalPoint(Vector3 point)
    {
        return currPose.MultiplyPoint3x4(point);
    }

    public Vector3 currGlobalVector(Vector3 point)
    {
        return currPose.MultiplyVector(point);
    }

    public Vector3 prevLocalPoint(Vector3 point)
    {
        return prevPoseInv.MultiplyPoint3x4(point);
    }

    public Vector3 prevGlobalPoint(Vector3 point)
    {
        return prevPose.MultiplyPoint3x4(point);
    }

    public Vector3 prevLocalVector(Vector3 vector)
    {
        return prevPoseInv.MultiplyVector(vector);
    }
}
