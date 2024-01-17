using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchController : Controller
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

    [Header("Input Settings")]
    [SerializeField]
    private float m_pinchRadius;
    [SerializeField]
    private float m_hoverThreshold; 

    // Inputs 
    private float m_pinchStrength;
    private bool m_isPinching;
    public bool Pinching
    {
        get { return m_isPinching; }
    }
    private bool m_isPinchingPrev;
    private Vector3 m_indexTipPos;
    private Vector3 m_thumbTipPos;
    private Vector3 m_colliderPos;
    public Vector3 ColliderPosition
    {
        get { return m_colliderPos; }
    }
    private Vector3 m_handForward;
    private Vector3 m_handUp;

    // Touching 
    private Collider[] m_touching;

    // Selected
    private bool m_selected;
    public bool Selecting
    {
        get { return m_selected; }
    }
    private Widget m_selectedWidget;
    public string SelectedWidget
    {
        get
        {
            if (m_selectedWidget != null)
                return m_selectedWidget.name;
            else
                return null;
        }
    }
    private Widget m_prevWidget; 
    private float m_selectTime;

    private bool m_enabled;
    public bool Enabled
    {
        get { return m_enabled; }
    }

    private void getHandInput()
    {
        m_pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        m_isPinching = m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        m_indexTipPos = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
        m_thumbTipPos = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform.position;
        m_handForward = m_palm.up;
        m_handUp = m_palm.right;
        if (m_hand.getHandType() == OVRHand.Hand.HandRight)
        {
            m_handForward *= -1;
        }
        if (m_hand.getHandType() == OVRHand.Hand.HandLeft)
        {
            m_handUp *= -1;
        }
        m_colliderPos = Vector3.Lerp(m_indexTipPos, m_thumbTipPos, 0.5f);
    }

    private void updatePose()
    {
        this.transform.position = m_colliderPos;
    }

    private void checkTouching()
    {
        m_touching = Physics.OverlapSphere(this.transform.position, m_pinchRadius, Constants.InteractableMask);
    }

    private void pinch()
    {
        checkTouching();
        float closestDist = Mathf.Infinity;
        Widget closest = null; 
        foreach (Collider touching in m_touching)
        {
            if (touching.transform.parent != null && touching.transform.parent.tag == "Widget")
            {
                float dist = Vector3.Distance(this.transform.position, touching.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist; 
                    closest = touching.transform.parent.GetComponent<Widget>();
                }

            }
        }
        bool attached = false;
        Widget.State state = Widget.State.APP;
        if (closest != null)
        {
            m_selectedWidget = closest;
            m_selected = true;
            attached = m_selectedWidget.attach(this, out state);
            if (m_selectedWidget != m_prevWidget && m_prevWidget != null && m_prevWidget.GetComponent<BrowserWidget>() != null)
            {
                m_prevWidget.GetComponent<BrowserWidget>().detach();
            }
        }
        m_selectTime = 0; 
    }

    public override bool selecting()
    {
        return m_selected;
    }

    public override void getSelectedPosSurface(Transform surface, out Vector3 pos, out Vector3 forward, out Vector3 up)
    {
        pos = this.transform.position;
        pos = surface.InverseTransformPoint(pos);
        pos.z = 0;
        pos = surface.TransformPoint(pos);
        forward = surface.forward;
        up = (m_handUp - Vector3.Dot(m_handUp, forward) * forward).normalized;
    }

    public override void getSelectedPosMidAir(out Vector3 pos, out Vector3 forward, out Vector3 up)
    {
        pos = this.transform.position;
        forward = m_handForward;
        up = m_handUp;
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

        origin = browser.InverseTransformPoint(this.transform.position);
        origin.z = -m_pinchRadius;
        origin = browser.TransformPoint(origin);
        dir = browser.forward;
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
    

    // Start is called before the first frame update
    void Start()
    {
        m_type = Type.PINCH;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_skeleton.IsDataValid || !m_hand.IsTracked || !m_hand.IsDataValid)
        {
            m_enabled = false;
            return;
        }
        m_enabled = true;

        getHandInput();
        updatePose();

        if (m_isPinching && !m_isPinchingPrev)
        {
            pinch();
        }
        else if (m_isPinching && m_selected)
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

    private void OnEnable()
    {
        m_touching = new Collider[0];
        m_selected = false; 
    }

    private void OnDisable()
    {
        detach();
    }

}
