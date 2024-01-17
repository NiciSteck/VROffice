using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveController : MonoBehaviour
{

    [Header("Oculus Input")]
    [SerializeField]
    private Transform m_head;
    [SerializeField]
    private OVRSkeleton m_skeleton;
    [SerializeField]
    private OVRHand m_hand;
    public string HandSkeleton
    {
        get
        {
            string str = "";
            foreach (OVRBone bone in m_skeleton.Bones)
            {
                Vector3 position = bone.Transform.position;
                Quaternion rotation = bone.Transform.rotation;
                str += position.x + "," + position.y + "," + position.z + ",";
                str += rotation.x + "," + rotation.y + "," + rotation.z + "," + rotation.w + ",";
            }
            return str;
        }
    }
    public bool IsTracked
    {
        get { return m_hand.IsTracked; }
    }
    /*
    [SerializeField]
    private float m_handCloseRadius;
    */

    [Header("Controllers")]
    [SerializeField]
    private bool m_adapt; 
    public bool adapt
    {
        set { m_adapt = value; }
    }
    [SerializeField]
    private List<Controller> m_controllers = new List<Controller>(); 
    public enum ActiveController { CURSOR, PINCH, POINT, CURSOR_POINT, NONE };
    [SerializeField]
    private ActiveController m_activeController;
    public ActiveController CurrentController
    {
        get { return m_activeController; }
    }
    public CursorController CursorController
    {
        get { return (CursorController)m_controllers[(int)ActiveController.CURSOR]; }
    }
    public PointController PointController
    {
        get { return (PointController)m_controllers[(int)ActiveController.POINT]; }
    }
    public PinchController PinchController
    {
        get { return (PinchController)m_controllers[(int)ActiveController.PINCH]; }
    }
    private ActiveController m_activeControllerPrev; 
    private enum ControllerState { INVALID, DIRECT, IN_RANGE, DISTANT };
    private ControllerState m_adaptiveState; 

    [Header("Debug")]
    [SerializeField]
    private bool m_showValues;
    private bool m_showValuesPrev;

    [Header("Adaptation Parameters")]
    [SerializeField]
    private float m_reachDistanceThreshold;
    [SerializeField]
    private float m_directInteractionDistanceThreshold; 
    [SerializeField]
    private float m_pointerAngleThreshold;
    [SerializeField]
    private float m_pinchStrengthThreshold; 
    [SerializeField]
    private float m_wristSpeedThreshold;
    [SerializeField]
    private float m_wristSpeedEWMAParameter;
    //private float m_controllerSelectionWristSpeedExponent;
    private float m_wristSpeedRelative;
    private float m_wristSpeedEWMA;
    
    /*
    // Adaptation based on wrist speed
    [SerializeField]
    private float m_wristSpeedEWMAParameter;
    [SerializeField]
    private float m_controllerSelectionWristSpeedWeight;
    
    
    [SerializeField]
    private float m_controllerSelectionClosestObjDistanceWeight;
    [SerializeField]
    private float m_controllerSelectionClosestObjDistanceThreshold;
    [SerializeField]
    private float m_controllerSelectionClosestObjDistanceExponent;
    [SerializeField]
    private float m_controllerSelectionPointerAngleWeight;
    [SerializeField]
    private float m_controllerSelectionPointerAngleThreshold;
    */

    // Hand 
    private bool m_handTracked, m_handTrackedPrev;
    private bool m_handValid, m_handValidPrev;
    private Vector3 m_indexPosition, m_indexPositionPrev, m_indexVelocity;
    private Vector3 m_thumbPosition, m_thumbPositionPrev, m_thumbVelocity;
    private Vector3 m_wristPosition, m_wristPositionPrev, m_wristVelocity;
    private Vector3 m_middleKnucklePosition, m_middleKnucklePositionPrev, m_middleKnuckleVelocity;
    private Vector3 m_palmPosition, m_palmPositionPrev, m_palmVelocity;
    private Vector3 m_handForward;
    private Vector3 m_handUp;
    private Quaternion m_handRotation, m_handRotationPrev;
    private float m_handRotationSpeed; 
    private bool m_handPointerValid, m_handPointerValidPrev;
    private Vector3 m_handPointerPosition;
    private Vector3 m_handPointerForward;
    private Ray m_handPointerRay; 
    private Quaternion m_handPointerRotation, m_handPointerRotationPrev;
    private float m_handPointerRotationSpeed; 
    private bool m_isPinching, m_isPinchingPrev;
    private float m_pinchStrength, m_pinchStrengthPrev;
    private float m_pinchAngle, m_pinchAnglePrev; 

    // Head 
    private Vector3 m_headPosition, m_headPositionPrev, m_headVelocity;
    private Vector3 m_headForward;
    private Vector3 m_headRaycastVec; 
    private Quaternion m_headRotation, m_headRotationPrev;
    private float m_headRotationSpeed;

   

    public int getNumControllers()
    {
        return m_controllers.Count;
    }

    private void deactivateControllers()
    {
        foreach (Controller controller in m_controllers)
        {
            if (controller.gameObject != null)
                controller.gameObject.SetActive(false);
        }
    }

    public void activateController(ActiveController active)
    {
        if (m_activeController != ActiveController.NONE)
        {
            if (m_activeController == ActiveController.CURSOR_POINT)
            {
                if (m_controllers[(int)ActiveController.CURSOR].selecting() || m_controllers[(int)ActiveController.POINT].selecting())
                {
                    return;
                }
            } else
            {
                if (m_controllers[(int)m_activeController].selecting())
                {
                    return;
                }
            }
        }
        setController(active);

        //if (m_activeController != ActiveController.NONE && m_controller != null && m_controller.selecting())
        //    return;

        //setController(active);
    }

    public void setController(ActiveController active)
    {
        deactivateControllers();
        if (active != ActiveController.NONE)
        {
            m_activeController = active;
            m_activeControllerPrev = m_activeController;  

            if (m_activeController == ActiveController.CURSOR_POINT)
            {
                m_controllers[(int)ActiveController.CURSOR].gameObject.SetActive(true);
                m_controllers[(int)ActiveController.POINT].gameObject.SetActive(true);
            } else
            {
                m_controllers[(int)m_activeController].gameObject.SetActive(true);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        activateController(m_activeController);

    }

    private void getHandPose()
    {
        m_indexPosition = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
        m_thumbPosition = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform.position;
        m_wristPosition = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.position;
        m_middleKnucklePosition = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle1].Transform.position;
        m_palmPosition = Vector3.Lerp(m_wristPosition, m_middleKnucklePosition, 0.5f);
        m_handForward = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.up;
        m_handUp = -m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.right;
        Vector3 pinchBase = Vector3.Lerp(m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index1].Transform.position, m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb2].Transform.position, 0.5f);
        m_pinchAngle = Vector3.Angle(m_indexPosition - pinchBase, m_thumbPosition - pinchBase);
        m_isPinching = m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        m_pinchStrength = m_hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
    }
    
    private void getPointerPose()
    {
        // Pointer pose
        m_handPointerValid = m_hand.IsPointerPoseValid;
        m_handRotation = m_hand.transform.rotation;
        if (m_handPointerValid)
        {
            m_handPointerForward = m_hand.PointerPose.forward; // (m_indexPosition - m_headPosition).normalized; 
            m_handPointerPosition = m_hand.PointerPose.position;
            m_handPointerRotation = m_hand.PointerPose.rotation;
        }
    }

    private void getHeadPose()
    {
        // Head pose 
        m_headPosition = m_head.position;
        m_headForward = m_head.forward;
        m_headRotation = m_head.rotation;
    }

    private void getHandMotion()
    {
        if (!m_handValidPrev)
            return;

        m_indexVelocity = (m_indexPosition - m_indexPositionPrev) / Time.deltaTime;
        m_thumbVelocity = (m_thumbPosition - m_thumbPositionPrev) / Time.deltaTime;
        m_wristVelocity = (m_wristPosition - m_wristPositionPrev) / Time.deltaTime;
        //m_wristSpeedRelative = ((m_wristPosition - m_headPosition).magnitude - (m_wristPositionPrev - m_headPositionPrev).magnitude) / Time.deltaTime;
        m_middleKnuckleVelocity = (m_middleKnucklePosition - m_middleKnucklePositionPrev) / Time.deltaTime;
        m_palmVelocity = (m_palmPosition - m_palmPositionPrev) / Time.deltaTime;

        m_handRotationSpeed = Quaternion.Angle(m_handRotationPrev, m_handRotation) / Time.deltaTime;

        m_wristSpeedEWMA = Mathf.Lerp(m_wristVelocity.magnitude, m_wristSpeedEWMA, m_wristSpeedEWMAParameter);
    }

    private void getPointerMotion()
    {
        if (!(m_handPointerValid && m_handPointerValidPrev))
            return; 

        m_handPointerRotationSpeed = Quaternion.Angle(m_handPointerRotationPrev, m_handPointerRotation) / Time.deltaTime;
    }

    private void getHeadMotion()
    {
        m_headVelocity = (m_headPosition - m_headPositionPrev) / Time.deltaTime;
        m_headRotationSpeed = Quaternion.Angle(m_headRotationPrev, m_headRotation) / Time.deltaTime;
    }

    private float getPointerAngleFromElement(Transform interactable)
    {
        Collider collider = interactable.Find("Window").GetComponent<Collider>();
        if (collider == null)
            return Vector3.Angle(m_handPointerForward, interactable.position - m_palmPosition);

        if (collider.Raycast(m_handPointerRay, out RaycastHit hit, Mathf.Infinity))
            return 0;

        Plane interactablePlane = new Plane(m_headForward, interactable.position);
        if (interactablePlane.Raycast(m_handPointerRay, out float planeHit))
        {
            Vector3 planeHitPoint = m_handPointerRay.GetPoint(planeHit);
            Vector3 planeHitInteractableClosest = collider.ClosestPoint(planeHitPoint);
            return Vector3.Angle(m_handPointerForward, planeHitInteractableClosest - m_palmPosition);
        }

        return Vector3.Angle(m_handPointerForward, interactable.position - m_palmPosition);
    }

    private void setPreviousValues()
    {
        m_handTrackedPrev = m_handTracked;
        m_handValidPrev = m_handValid;

        m_indexPositionPrev = m_indexPosition;
        m_thumbPositionPrev = m_thumbPosition;
        m_wristPositionPrev = m_wristPosition;
        m_middleKnucklePositionPrev = m_middleKnucklePosition;
        m_palmPositionPrev = m_palmPosition;
        m_handRotationPrev = m_handRotation;

        m_handPointerValidPrev = m_handPointerValid;
        m_handPointerRotationPrev = m_handPointerRotation;

        m_isPinchingPrev = m_isPinching;
        m_pinchStrengthPrev = m_pinchStrength;
        m_pinchAnglePrev = m_pinchAngle;

        m_headPositionPrev = m_headPosition;
        m_headRotationPrev = m_headRotation;

        m_showValuesPrev = m_showValues;
    }

    private void adaptController()
    {
        // No adaptation when hand data is invalid 
        m_handTracked = m_hand.IsTracked;
        m_handValid = m_hand.IsDataValid;
        if (!m_handTracked || !m_handValid)
        {
            m_adaptiveState = ControllerState.INVALID;
            return;
        }

        getHandPose();
        getPointerPose();
        getHeadPose();
        m_headRaycastVec = (m_indexPosition - m_headPosition).normalized;

        getHandMotion();
        getPointerMotion();
        getHeadMotion();

        // 7 September 2022 Implementation
        // Case: no valid hand pointer
        if (!m_handPointerValid)
        {
            deactivateControllers();
            m_activeController = ActiveController.NONE;
            setPreviousValues();
            m_adaptiveState = ControllerState.INVALID;
            return;
        }

        RaycastHit focusHit;
        GameObject focusObj = null;

        // Case: within direct interaction range
        // Finger protruding through
        Collider[] directInteractables = Physics.OverlapSphere(m_indexPosition, m_directInteractionDistanceThreshold, Constants.WidgetMask);
        if (directInteractables.Length >= 0)
        {
            foreach (Collider interactable in directInteractables)
            {
                if (interactable.Raycast(new Ray(m_headPosition, m_headRaycastVec), out RaycastHit hit, (m_indexPosition - m_headPosition).magnitude))
                {
                    focusObj = interactable.gameObject;
                    break;
                }
            }
        }
        // Intersecting
        float minDist;
        directInteractables = Physics.OverlapSphere(m_indexPosition, 0.03f, Constants.WidgetMask);
        if (directInteractables.Length >= 0)
        {
            minDist = Mathf.Infinity;
            foreach (Collider interactable in directInteractables)
            {
                if (interactable.Raycast(new Ray(m_headPosition, m_headRaycastVec), out RaycastHit hit, (m_indexPosition - m_headPosition).magnitude))
                {
                    focusObj = interactable.gameObject;
                    break;
                }

                Vector3 toInteractable = interactable.ClosestPoint(m_indexPosition) - m_indexPosition;
                Vector3 toInteractableDir = toInteractable.normalized;
                float dot = Mathf.Max(Vector3.Dot(toInteractableDir, m_handPointerPosition), Vector3.Dot(toInteractableDir, m_headRaycastVec));
                if (dot > Mathf.Cos(30))
                {
                    float toInteractableDist = toInteractable.magnitude;
                    if (Physics.Raycast(new Ray(m_indexPosition, toInteractableDir), toInteractableDist - 0.01f, Constants.ObstacleMask | Constants.SurfaceMask))
                        continue;

                    if (toInteractableDist <= minDist)
                    {
                        minDist = toInteractableDist;
                        focusObj = interactable.gameObject;
                    }
                }
            }
        }
        // Close enough 
        RaycastHit[] allDirectInteractables = Physics.RaycastAll(m_handPointerPosition, m_handPointerForward, m_directInteractionDistanceThreshold, Constants.ObstacleMask | Constants.SurfaceMask | Constants.WidgetMask);
        minDist = Mathf.Infinity;
        GameObject minDistObj = null;
        foreach (RaycastHit hit in allDirectInteractables)
        {
            float toHit = (hit.point - m_handPointerPosition).magnitude;
            if (hit.transform.gameObject.layer == Constants.WidgetLayer && toHit - 0.02f < minDist)
            {
                minDist = toHit - 0.02f;
                minDistObj = hit.transform.gameObject;
            }
            else if (toHit < minDist)
            {
                minDist = toHit;
                minDistObj = hit.transform.gameObject;
            }
        }
        if (minDistObj != null && minDistObj.layer == Constants.WidgetLayer)
            focusObj = minDistObj;
        if (focusObj != null)
        {
            Widget focusWidget = focusObj.transform.parent.GetComponent<Widget>();
            if (focusWidget.state == Widget.State.TRANSFORM && focusWidget.transform.parent == VirtualEnvironmentManager.Environment.ElementsContainer)
            {
                if (m_activeController != ActiveController.PINCH)
                    activateController(ActiveController.PINCH);
            }
            else
            {
                if (m_activeController != ActiveController.POINT)
                    activateController(ActiveController.POINT);
            }
            setPreviousValues();
            m_adaptiveState = ControllerState.DIRECT;
            return;
        }

        // Case: beyond direct interaction range, within reach 
        if (Physics.Raycast(new Ray(m_indexPosition, m_handPointerForward), out focusHit, Mathf.Infinity, Constants.ObstacleMask | Constants.SurfaceMask | Constants.WidgetMask))
        {
            focusObj = focusHit.transform.gameObject;
            if (focusObj.layer == Constants.WidgetLayer && focusHit.distance < m_reachDistanceThreshold)
            {
                if (m_wristSpeedEWMA > m_wristSpeedThreshold) // Within a direct interaction range or in movement 
                {
                    Widget focusWidget = focusObj.transform.parent.GetComponent<Widget>();
                    if (focusWidget.state == Widget.State.TRANSFORM && focusWidget.transform.parent == VirtualEnvironmentManager.Environment.ElementsContainer)
                    {
                        if (m_activeController != ActiveController.PINCH)
                            activateController(ActiveController.PINCH);
                    }
                    else
                    {
                        if (m_activeController != ActiveController.POINT)
                            activateController(ActiveController.POINT);
                    }
                    setPreviousValues();
                    return;
                }
            }
        }

        // Case: out of reach
        if (m_activeController != ActiveController.CURSOR)
            activateController(ActiveController.CURSOR);
        setPreviousValues();
        return;

        // 6 September 2022 Implementation 
        /*
        // Case: no valid hand pointer
        if (!m_handPointerValid)
        {
            deactivateControllers();
            m_activeController = ActiveController.NONE;
            setPreviousValues();
            m_adaptiveState = ControllerState.INVALID;
            return;
        }

        GameObject focusObj = null;

        // Case: within direct interaction range
        Collider[] directInteractables = Physics.OverlapSphere(m_indexPosition, m_directInteractionDistanceThreshold, Constants.WidgetMask);
        if (directInteractables.Length >= 0)
        {
            float minDist = Mathf.Infinity;
            foreach (Collider interactable in directInteractables)
            {
                if (interactable.Raycast(new Ray(m_headPosition, m_headRaycastVec), out RaycastHit hit, (m_indexPosition - m_headPosition).magnitude))
                {
                    focusObj = interactable.gameObject;
                    break;
                }

                Vector3 toInteractable = interactable.ClosestPoint(m_indexPosition) - m_indexPosition;
                Vector3 toInteractableDir = toInteractable.normalized;
                float dot = Mathf.Max(Vector3.Dot(toInteractableDir, m_handPointerPosition), Vector3.Dot(toInteractableDir, m_headRaycastVec));
                if (dot > 0.5f)
                {
                    float toInteractableDist = toInteractable.magnitude;
                    if (Physics.Raycast(new Ray(m_indexPosition, toInteractableDir), toInteractableDist - 0.01f, Constants.ObstacleMask | Constants.SurfaceMask))
                        continue;

                    if (toInteractableDist <= minDist)
                    {
                        minDist = toInteractableDist;
                        focusObj = interactable.gameObject;
                    }
                }
            }
        }
        if (focusObj != null)
        {
            Widget focusWidget = focusObj.transform.parent.GetComponent<Widget>();
            if (focusWidget.state == Widget.State.TRANSFORM && focusWidget.transform.parent == VirtualEnvironmentManager.Environment.ElementsContainer)
            {
                if (m_activeController != ActiveController.PINCH)
                    activateController(ActiveController.PINCH);
            }
            else
            {
                if (m_activeController != ActiveController.POINT)
                    activateController(ActiveController.POINT);
            }
            setPreviousValues();
            m_adaptiveState = ControllerState.DIRECT;
            return;
        }

        // Case: beyond direct interaction range, within reach 
        if (Physics.Raycast(new Ray(m_indexPosition, m_handPointerForward), out RaycastHit focusHit, Mathf.Infinity, Constants.ObstacleMask | Constants.SurfaceMask | Constants.WidgetMask))
        {
            focusObj = focusHit.transform.gameObject;
            if (focusObj.layer == Constants.WidgetLayer && focusHit.distance < m_reachDistanceThreshold)
            {
                if (m_wristSpeedEWMA > m_wristSpeedThreshold) // Within a direct interaction range or in movement 
                {
                    Widget focusWidget = focusObj.transform.parent.GetComponent<Widget>();
                    if (focusWidget.state == Widget.State.TRANSFORM && focusWidget.transform.parent == VirtualEnvironmentManager.Environment.ElementsContainer)
                    {
                        if (m_activeController != ActiveController.PINCH)
                            activateController(ActiveController.PINCH);
                    }
                    else
                    {
                        if (m_activeController != ActiveController.POINT)
                            activateController(ActiveController.POINT);
                    }
                    setPreviousValues();
                    return;
                }
            }
        }

        // Case: out of reach
        if (m_activeController != ActiveController.CURSOR)
            activateController(ActiveController.CURSOR);
        setPreviousValues();
        return;
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (m_adapt)
            adaptController();
        if (m_activeController != m_activeControllerPrev)
            activateController(m_activeController);

        m_activeControllerPrev = m_activeController;
    }
}
