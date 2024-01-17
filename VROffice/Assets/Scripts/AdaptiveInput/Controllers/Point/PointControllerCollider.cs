using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointControllerCollider : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField]
    private Color m_color;
    [SerializeField]
    private Color m_touchColor; 

    public delegate void Enter(Transform obj);
    public Enter enter;

    public delegate void Stay(Transform obj);
    public Stay stay;

    public delegate void Exit(Transform obj);
    public Exit exit;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & Constants.InteractableMask) > 0) 
        {
            if (enter != null)
            {
                enter(other.transform);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & Constants.InteractableMask) > 0)
        {
            if (stay != null)
            {
                stay(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & Constants.InteractableMask) > 0)
        {
            if (exit != null)
            {
                exit(other.transform);
            }
        }
    }

    public void setState(bool touching)
    {
        if (touching)
            this.GetComponent<MeshRenderer>().material.color = m_touchColor;
        else
            this.GetComponent<MeshRenderer>().material.color = m_color;
    }
}
