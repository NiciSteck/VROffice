using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public enum Type { NONE, CURSOR, PINCH, POINT };
    protected Type m_type; 
    public Type type
    {
        get { return m_type; }
    }

    public virtual bool selecting()
    {
        return false;
    }
    public virtual void getBrowserInteractions(Transform browser, out bool hovering, out bool clickingLeft, out bool clickingRight, out bool scrolling, out bool clickingLeftRaw, out bool clickingRightRaw, out Vector3 origin, out Vector3 dir)
    {
        hovering = false;
        clickingLeft = false;
        clickingRight = false;
        clickingLeftRaw = false;
        clickingRightRaw = false;
        scrolling = false; 
        origin = Vector3.zero;
        dir = Vector3.forward; 
    }

    public virtual void getSelectedPosMidAir(out Vector3 pos, out Vector3 forward, out Vector3 up)
    {
        pos = Vector3.zero;
        forward = Vector3.forward;
        up = Vector3.up;
    }

    public virtual void getSelectedPosSurface(Transform surface, out Vector3 pos, out Vector3 forward, out Vector3 up) { 
        pos = Vector3.zero;
        forward = Vector3.forward;
        up = Vector3.up;
    }

    public virtual void detach() { }

    public virtual void setTouching() { }
    
    protected bool normalFacingUser(Transform surface)
    {
        Vector3 headDirection = Camera.main.transform.position - surface.position;
        headDirection /= headDirection.magnitude;
        return Vector3.Angle(surface.forward, headDirection) <
               Vector3.Angle(surface.forward * -1, headDirection);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}