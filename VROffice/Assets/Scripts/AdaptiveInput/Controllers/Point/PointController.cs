using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointController : Controller
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

    [Header("Collider")]
    [SerializeField]
    private PointControllerCollider m_collider;
    public Vector3 ColliderPosition
    {
        get { return m_collider.transform.position;  }
    }

    [Header("Interactions")]
    [SerializeField]
    private float m_clickThreshold; 
    [SerializeField]
    private float m_releaseThreshold;
    [SerializeField]
    private float m_hoverThreshold; 

    // Inputs
    private Vector3 m_handForward;
    private Vector3 m_handUp;

    // Interactions 
    private List<Transform> m_touching = new List<Transform>();

    // States
    private bool m_isTouching;
    public bool Touching
    {
        get { return m_isTouching; }
    }

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

    private void enter(Transform obj)
    {
        if (!m_touching.Contains(obj))
        {
            m_touching.Add(obj);
        }
    }

    private void exit(Transform obj)
    {
        if (m_touching.Contains(obj))
        {
            m_touching.Remove(obj);
        }
    }

    private void enable(bool enabled)
    {
        m_enabled = enabled;
        m_collider.gameObject.SetActive(enabled);
    }

    private void getHandInput()
    {
        this.transform.position = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
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
    }

    private void updateCollider()
    {
        m_collider.transform.position = this.transform.position;
    }

    private void touch()
    {
        if (m_touching.Count <= 0)
            return;

        bool attached = false;
        Widget.State state = Widget.State.APP;
        foreach (Transform touching in m_touching)
        {
            if (touching.parent != null && touching.parent.tag == "Widget")
            {
                m_selectedWidget = touching.parent.GetComponent<Widget>();
                if (m_selectedWidget != null)
                {
                    m_selected = true;
                    m_selectTime = 0; 
                    if ((m_selectedWidget.state == Widget.State.APP) ||
                        (m_selectedWidget.state == Widget.State.TRANSFORM && m_selectedWidget.placement == Widget.Placement.ON_SURFACE))
                    {
                        attached = m_selectedWidget.attach(this, out state);
                        if (m_selectedWidget != m_prevWidget && m_prevWidget != null && m_prevWidget.GetComponent<BrowserWidget>() != null)
                        {
                            m_prevWidget.GetComponent<BrowserWidget>().detach();
                        }
                    }
                    break;
                }
            }
        }

        m_isTouching = true;
        m_collider.setState(true);
    }

    public override void setTouching()
    {
        m_isTouching = true;
        m_collider.setState(true);
    }

    private bool canReleaseSelected()
    {
        if (m_selectedWidget == null)
            return true;

        if (m_selectedWidget.placement == Widget.Placement.MIDAIR && m_selectedWidget.state == Widget.State.TRANSFORM)
            return true;

        Vector3 toSelected = m_selectedWidget.transform.position - this.transform.position;
        if (m_selectedWidget.placement == Widget.Placement.ON_SURFACE)
        {
            toSelected = m_selectedWidget.transform.parent.InverseTransformDirection(toSelected);
            bool facingUser = normalFacingUser(m_selectedWidget.transform.parent.transform);
            toSelected = facingUser ? toSelected * -1 : toSelected;
        }
        else
            toSelected = m_selectedWidget.transform.InverseTransformDirection(toSelected);
        if (toSelected.z > m_releaseThreshold)
            return true;

        return false; 
    }
    
    private bool normalFacingUser(Transform surface)
    {
        Vector3 headDirection = Camera.main.transform.position - surface.position;
        headDirection /= headDirection.magnitude;
        return Vector3.Angle(surface.forward, headDirection) <
               Vector3.Angle(surface.forward * -1, headDirection);
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

        bool facingUser = normalFacingUser(surface);
        forward = facingUser ? surface.forward*-1 : surface.forward;
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

        if (m_selected && m_selectTime > 0.2f)
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
        origin.z = -m_clickThreshold;
        origin = browser.TransformPoint(origin);
        dir = browser.forward;
    }


    // Start is called before the first frame update
    void Start()
    {
        m_collider.enter = enter;
        m_collider.exit = exit;
        m_type = Type.POINT;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_skeleton.IsDataValid || !m_hand.IsTracked || !m_hand.IsDataValid)
        {
            enable(false);
            return;
        }

        enable(true);

        getHandInput();
        updateCollider();

        // Update state 
        if (!m_selected)
        {
            if (m_isTouching && m_touching.Count <= 0)
            {
                m_isTouching = false;
                m_collider.setState(false);
            }

            if (!m_isTouching)
                touch();
        } else
        {
            m_collider.setState(true);
            m_selectTime += Time.deltaTime;
            if (canReleaseSelected())
            {
                detach();
                if (!m_clickRelease && m_selectTime > 0 && m_selectTime < 0.2f)
                    m_clickRelease = true;
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

    private void OnDisable()
    {
        if (m_collider.gameObject != null)
            m_collider.gameObject.SetActive(false);
        if (canReleaseSelected())
        {
            if (m_selectedWidget != null)
                m_selectedWidget.detach(this);
            m_selected = false;
            m_selectedWidget = null;
            m_clickRelease = true;
        }
    }

    private void OnEnable()
    {
        m_touching.Clear();
        m_collider.setState(false);
        m_isTouching = false;
        m_selected = false;
    }

}
