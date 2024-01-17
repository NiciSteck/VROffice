using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : Controller
{
    [Header("Oculus Input")]
    [SerializeField]
    private OVRSkeleton m_skeleton;
    public string HandSkeleton
    {
        get
        {
            string str = "";
            foreach (OVRBone bone in m_skeleton.Bones)
            {
                Vector3 position = bone.Transform.position;
                Vector3 forward = bone.Transform.forward;
                str += position.x + "," + position.y + "," + position.z + ",";
                str += forward.x + "," + forward.y + "," + forward.z + ",";
            }
            return str;
        }
    }
    [SerializeField]
    private OVRHand m_hand;
    [SerializeField]
    private Transform m_palm;

    [Header("Pointer")]
    [SerializeField]
    private CursorControllerPointer m_pointer;
    [SerializeField]
    private float m_pointerOffset;
    public Vector3 PointerPosition
    {
        get { return m_pointer.transform.position; }
    }
    public Vector3 PointerForward
    {
        get { return m_pointer.transform.forward; }
    }

    [Header("Line Renderer")]
    [SerializeField]
    private CursorControllerLR m_lr;
    [SerializeField]
    private float m_lrLength; 

    [Header("Cursor")]
    [SerializeField]
    private CursorControllerCursor m_cursor;
    public Vector3 CursorPosition
    {
        get { return m_cursor.transform.position; }
    }

    // Inputs 
    private float m_pinchStrength;
    private bool m_isPinching;
    public bool Pinching
    {
        get { return m_isPinching; }
    }
    private bool m_isPinchingPrev; 
    private Vector3 m_handForward;
    private Vector3 m_handUp;
    private Vector3 m_handRight; 

    // Raycasting 
    private bool m_hovering;
    public bool Hovering
    {
        get { return m_hovering; }
    }
    private RaycastHit m_hoveringObj;

    // Selected
    private bool m_selected; 
    public bool Selecting
    {
        get { return m_selected; }
    }
    private RaycastHit m_selectedObj;
    private float m_selectDistance;
    private Widget m_selectedWidget;
    public string SelectedWidget
    {
        get {
            if (m_selectedWidget != null)
                return m_selectedWidget.name;
            else
                return null;
        }
    }
    private Widget m_prevWidget; 
    private bool m_transformingWidget;
    private float m_selectTime;

    private bool m_enabled; 
    public bool Enabled
    {
        get { return m_enabled; }
    }

    private void enable(bool enabled)
    {
        m_enabled = enabled; 

        m_pointer.gameObject.SetActive(enabled);

        // LR disabled unless hovering
        m_lr.gameObject.SetActive(true);

        // Cursor disabled unless hovering 
        m_cursor.gameObject.SetActive(false);

        // Clear hovering and selected elements when disabled (TODO)
        
    }

    private void getHandInput()
    {
        m_pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        m_isPinching = m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        m_handForward = m_palm.up;
        m_handUp = m_palm.right;
        m_handRight = -m_palm.forward;
        if (m_hand.getHandType() == OVRHand.Hand.HandRight)
        {
            m_handForward *= -1;
        }
        if (m_hand.getHandType() == OVRHand.Hand.HandLeft)
        {
            m_handUp *= -1;
        }
    }

    private void updatePose()
    {
        this.transform.position = m_hand.PointerPose.position;
        this.transform.forward = m_hand.PointerPose.forward;
    }

    private void updatePointer()
    {
        m_pointer.transform.position = this.transform.position + (m_pointerOffset * this.transform.forward);
        m_pointer.transform.forward = this.transform.forward;

        if (m_isPinching)
            m_pointer.setState(1);
        else
            m_pointer.setState(m_pinchStrength);
    }

    private bool raycast(out RaycastHit hit)
    {
        return Physics.Raycast(this.transform.position, this.transform.forward, out hit, Mathf.Infinity, Constants.InteractableMask);
    }

    private void updateLR()
    {
        Vector3 start = this.transform.position;
        Vector3 end = this.transform.position + m_lrLength * this.transform.forward;
        if (m_hovering)
        {
            end = m_hoveringObj.point;
        }
        m_lr.setDirection(start, end);
        m_lr.setState(m_isPinching);

        /*
        if (!m_hovering)
            return;

        m_lr.gameObject.SetActive(true);

        Vector3 start = this.transform.position;
        Vector3 end = m_hoveringObj.point;
        m_lr.setDirection(start, end);
        m_lr.setState(m_isPinching);
        */
    }

    private void updateCursor()
    {
        if (!m_hovering)
            return;

        m_cursor.gameObject.SetActive(true);

        Vector3 start = this.transform.position;
        Vector3 end = m_hoveringObj.point;
        Vector3 toEnd = end - start; 
        m_cursor.setState(m_isPinching);
        m_cursor.setScale((end - start).magnitude);
        m_cursor.transform.position = end - 0.5f * m_cursor.scale * toEnd.normalized;
    }

    private void updateRaySelected()
    {
        if (m_selectedWidget == null)
            return;

        Vector3 start = this.transform.position;
        Vector3 end = start + m_selectDistance * this.transform.forward;

        if (m_selectedWidget.GetComponentInChildren<Collider>().Raycast(new Ray(start, this.transform.forward), out RaycastHit hit, Mathf.Infinity))
            end = hit.point;

        Vector3 toEnd = end - start;

        m_lr.gameObject.SetActive(true);
        m_lr.setDirection(start, end);
        m_lr.setState(m_isPinching);

        // TODO: update
        m_cursor.gameObject.SetActive(true);
        m_cursor.setState(m_isPinching);
        m_cursor.setScale(toEnd.magnitude);
        m_cursor.transform.position = end - 0.5f * m_cursor.scale * toEnd.normalized;

        Vector3 direction = toEnd.normalized;
        m_pointer.transform.position = this.transform.position + (m_pointerOffset * direction);
        m_pointer.transform.forward = direction;
        if (m_isPinching)
            m_pointer.setState(1);
        else
            m_pointer.setState(m_pinchStrength);
    }
    
    private void pinch()
    {
        if (!m_hovering)
            return;

        select();
    }

    private void select()
    {
        m_selected = true;
        m_selectTime = 0; 
        m_selectedObj = m_hoveringObj;

        m_selectDistance = Vector3.Distance(this.transform.position, m_selectedObj.point);
        bool attached = false; 
        Widget.State state = Widget.State.APP;
        if (m_selectedObj.transform.parent != null && m_selectedObj.transform.parent.tag == "Widget")
        {
            m_selectedWidget = m_selectedObj.transform.parent.GetComponent<Widget>();
            if (m_selectedWidget != null)
            {
                attached = m_selectedWidget.attach(this, out state);
                m_transformingWidget = attached; 
                if (m_selectedWidget != m_prevWidget && m_prevWidget != null && m_prevWidget.GetComponent<BrowserWidget>() != null)
                {
                    m_prevWidget.GetComponent<BrowserWidget>().detach();
                }
            }
        }
    }

    public override void detach()
    {
        if (!m_selected)
            return;

        m_selected = false;
        if (m_selectedWidget != null)
        {
            m_selectedWidget.detach(this);
            m_prevWidget = m_selectedWidget;
            m_selectedWidget = null;
        }
    }

    public override bool selecting()
    {
        return m_selected;
    }


    
    public override void getSelectedPosMidAir(out Vector3 pos, out Vector3 forward, out Vector3 up)
    {
        pos = this.transform.position + m_selectDistance * this.transform.forward;
        forward = this.transform.forward;
        up = Vector3.Cross(forward, m_handRight);
    }

    public override void getSelectedPosSurface(Transform surface, out Vector3 pos, out Vector3 forward, out Vector3 up)
    {
        Vector3 origin = this.transform.position;
        Vector3 dir = this.transform.forward;

        Ray fromController = new Ray(origin, dir);
        Plane surfacePlane = new Plane(surface.forward, surface.position);
        if (!surfacePlane.Raycast(fromController, out float enter))
        {
            surfacePlane.SetNormalAndPosition(-surface.forward, surface.position);
            surfacePlane.Raycast(fromController, out enter);
        }
        pos = fromController.GetPoint(enter);

        forward = surface.forward;
        up = (m_handUp - Vector3.Dot(m_handUp, forward) * forward).normalized;
    }

    private bool m_clickRelease = false;
    public override void getBrowserInteractions(Transform browser, 
        out bool hovering, out bool clickingLeft, out bool clickingRight, out bool scrolling,
        out bool clickingLeftRaw, out bool clickingRightRaw, 
        out Vector3 origin, out Vector3 dir)
    {
        hovering = false;
        clickingLeft = false;
        clickingRight = false;
        clickingLeftRaw = false;
        clickingRightRaw = false;
        scrolling = false;

        if (m_isPinching && m_selectTime > 0.2f)
        {
            // Dragging
            // Scrolling enabled 
            scrolling = true;

            // Scrolling disabled
            clickingLeftRaw = true;
        }

        if (m_clickRelease)
        {
            Debug.Log("Click");
            m_clickRelease = false;

            // Click
            // Scrolling enabled 
            clickingLeft = true;

            // Scrolling disabled
            clickingLeftRaw = true;
        }

        /*
        if (m_isPinching && m_selectTime > 0.2f)
        {
            // Dragging
            scrolling = true;
            clickingLeftRaw = true;
            hovering = true;
        } 
        if (m_clickRelease)
        {
            // Click 
            m_clickRelease = false;
            clickingLeft = true;
            clickingLeftRaw = true;
            hovering = true;
        }
        */

        origin = this.transform.position;
        dir = this.transform.forward;
        
    }


    // Start is called before the first frame update
    void Start()
    {
        m_type = Type.CURSOR;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_hand.IsTracked || !m_hand.IsDataValid || !m_hand.IsPointerPoseValid)
        {
            enable(false);
            return; 
        }

        enable(true);

        getHandInput();
        updatePose();
        

        if (!m_selected || (m_selected && !m_transformingWidget))
        {
            // No selected object 
            updatePointer();
            m_hovering = raycast(out m_hoveringObj);
            updateLR();
            updateCursor();
        } else if (m_selected && m_selectedWidget != null)
        {
            // Mid-air element selected
            updateRaySelected();
        }

        if (m_isPinching && !m_isPinchingPrev)
        {
            pinch();
        } else if (m_isPinching && m_selected)
        {
            m_selectTime += Time.deltaTime;   
        }
        else if (m_isPinchingPrev && !m_isPinching)
        {
            if (!m_clickRelease && m_selectTime > 0 && m_selectTime < 0.2f)
                m_clickRelease = true; 
            detach();
        }

        m_isPinchingPrev = m_isPinching;
    }

    private void OnDisable()
    {
        detach();
        if (m_pointer != null)
            m_pointer.gameObject.SetActive(false);
        if (m_lr != null)
            m_lr.gameObject.SetActive(false);
        if (m_cursor != null)
            m_cursor.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_pointer.setState(0);
        m_lr.setState(false);
        m_cursor.setState(false);
        m_hovering = false;
        m_selected = false;
        m_transformingWidget = false;
    }
}
