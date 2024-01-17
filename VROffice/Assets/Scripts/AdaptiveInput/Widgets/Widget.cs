using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Widget : MonoBehaviour
{
    public enum State { APP, TRANSFORM };
    [Header("Application Settings")]
    [SerializeField]
    private State m_state;
    private State m_prevState;
    public State state
    {
        get { return m_state; }
    }
    [SerializeField]
    private WidgetApplication m_app;
    
    public enum Placement { MIDAIR, ON_SURFACE };
    [Header("Transform Settings")]
    [SerializeField]
    [Range(2, 3)]
    private int m_dimension = 2; 
    [SerializeField]
    private Placement m_placement;
    public Placement placement
    {
        get { return m_placement; }
    }
    [SerializeField]
    private float m_translationUpdateRate = 10;
    [SerializeField]
    private float m_scaleUpdateRate = 5;
    [SerializeField]
    private float m_rotationUpdateRate = 5;
    [SerializeField]
    private float m_attachRadius = 0.1f;
    [SerializeField]
    private float m_doubleTapMaxThreshold = 0.5f;
    [SerializeField]
    private float m_doubleTapMinThreshold = 0.1f;
    [SerializeField]
    private float m_detachDistance = 0.1f;
    [SerializeField]
    private float m_snapRestrictInteractionThreshold = 0.5f;
    private float m_tSnapRestricted; 

    private List<Controller> m_controllers = new List<Controller>();
    private Vector3 m_offsetMidair;
    private Vector3 m_offsetSurface;

    private Vector3 m_initScale;
    private Quaternion m_initRotation;
    private Vector3 m_initBetweenControllers;

    [Header("Control")]
    [SerializeField]
    private bool m_inputEnabled = true; 
    public bool InputEnabled
    {
        set { m_inputEnabled = value; }
    }

    public Vector3 Position
    {
        get { return this.transform.position; }
    }

    public Quaternion Rotation
    {
        get { return this.transform.rotation; }
    }

    public Vector3 Scale
    {
        get { return this.transform.Find("Window").localScale; }
    }

    public Vector3 Up
    {
        get { return this.transform.up; }
    }

    public Vector3 Forward
    {
        get { return this.transform.forward; }
    }

    public Vector3 Right
    {
        get { return this.transform.right; }
    }

    public Vector3 targetPosition
    {
        set { m_target = value; }
        get { return m_target; }
    }
    private Vector3 m_target;
    public Vector3 targetScale
    {
        get { return m_targetScale; }
    }
    private Vector3 m_targetScale;
    public Quaternion targetRotation
    {
        set { m_targetRotation = value; }
        get { return m_targetRotation; }
    }
    private Quaternion m_targetRotation;

    private Dictionary<Controller, float> m_controllerHistory = new Dictionary<Controller, float>();

    private Vector3 m_posAttach;
    private Quaternion m_rotAttach;
    private bool m_attached; 
    private float m_timeAttached;

    private Controller.Type m_lastController;
    public Controller.Type lastController
    {
        get { return m_lastController; }
    }

    private GameObject userHead;
    //private float m_tDetachOptimizationThreshold = 0.5f;

    //[Header("Adaptation Settings")]
    //[SerializeField]
    //private bool m_triggerAdaptation = true; 

    /*
    public Vector3 SelectedPosition
    {
        get { return this.transform.TransformPoint(m_offset); }
    }
    */

    public void updateState(Widget.State state)
    {
        m_state = state; 
    }

    private void doubleTap()
    {
        // Detach from surface 
        if (m_placement == Placement.ON_SURFACE)
        {
            detachFromSurface();
        }
    }

    private void updateControllerHistory()
    {
        List<Controller> controllers = new List<Controller>();
        foreach (Controller controller in m_controllerHistory.Keys) {
            controllers.Add(controller);
        }
        List<Controller> remove = new List<Controller>();
        foreach (Controller controller in controllers)
        {
            m_controllerHistory[controller] += Time.deltaTime;
            if (m_controllerHistory[controller] > m_doubleTapMaxThreshold)
                remove.Add(controller);
        }
        foreach(Controller controller in remove)
        {
            m_controllerHistory.Remove(controller);
        }
    }

    public bool attach(Controller controller, out State state)
    {
        state = m_state;
        if (!m_inputEnabled)
            return false; 

        m_posAttach = this.transform.position;
        m_rotAttach = this.transform.rotation;
        m_lastController = controller.type;
        
        switch (m_state)
        {
            case State.TRANSFORM:
                attachTransform(controller);
                m_attached = true;
                m_timeAttached = 0; 
                return true; 
            case State.APP:
                if (m_app != null)
                {
                    m_app.attach(controller);
                    //SoundManager.Sounds.click();
                }
                break; 
        }
        return false; 
    }

    private bool snapToSurface()
    {
        Collider[] planes = Physics.OverlapSphere(this.transform.position, m_attachRadius, Constants.SurfaceMask);
        if (planes.Length == 0)
            return false;

        float closestDist = Mathf.Infinity;
        Transform closestSurface = null;
        Collider closestSurfaceCollder = null; 
        foreach (Collider plane in planes)
        {
            float toPlane = (plane.ClosestPoint(this.transform.position) - this.transform.position).magnitude;
            Transform surface = plane.transform.parent;
            if (toPlane < closestDist && surface != null && surface.tag == "Surface")
            {
                closestDist = toPlane;
                closestSurface = surface;
                closestSurfaceCollder = plane;
            }
        }
        if (closestSurface != null)
        {
            Vector3 headDirection = userHead.transform.position - closestSurface.position;
            headDirection /= headDirection.magnitude;
            bool facingUser = Vector3.Angle(closestSurface.forward, headDirection) <
                              Vector3.Angle(closestSurface.forward * -1, headDirection);
            
            this.transform.SetParent(closestSurface);
            m_placement = Placement.ON_SURFACE;
            Vector3 target = closestSurfaceCollder.ClosestPoint(this.transform.position);
            target = closestSurface.InverseTransformPoint(target);
            target.z = facingUser ? VirtualEnvironmentManager.Environment.SurfaceOffset * -1 : VirtualEnvironmentManager.Environment.SurfaceOffset;
            target = closestSurface.TransformPoint(target);
            m_target = target;
            
            
            List<Vector3> possibleUp = new List<Vector3>(){closestSurface.up,closestSurface.up*-1,closestSurface.right,closestSurface.right*-1};
            Vector3 facingUp = closestSurface.up;
            foreach (Vector3 vec in possibleUp)
            {
                float newAngle = Vector3.Angle(Vector3.up, vec);
                float minAngle = Vector3.Angle(Vector3.up, facingUp);
                if (newAngle < minAngle)
                {
                    facingUp = vec;
                }
            }
            Debug.Log(facingUser + " " + facingUp);
            m_targetRotation = Quaternion.LookRotation(facingUser?closestSurface.forward*-1:closestSurface.forward, facingUp);
            /*
            float dot = Vector3.Dot(closestSurface.up.normalized, this.transform.up.normalized);
            float rotAngle = Mathf.Abs(Mathf.Rad2Deg * Mathf.Acos(dot));
            this.transform.Rotate(rotAngle, 0, 0);
            m_targetRotation = Quaternion.LookRotation(closestSurface.forward, this.transform.up);
            this.transform.Rotate(-rotAngle, 0, 0);
            */
            
            //SoundManager.Sounds.snap();
            m_tSnapRestricted = m_snapRestrictInteractionThreshold;
            m_controllerHistory.Clear();
            return true;
        }


        return false;
    }

    public void detachFromSurface()
    {
        Transform surface = this.transform.parent;
        if (surface == null || surface.tag != "Surface")
            return; 

        this.transform.SetParent(VirtualEnvironmentManager.Environment.ElementsContainer);
        m_target = this.transform.position - m_detachDistance * surface.forward;
        m_targetRotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        m_placement = Placement.MIDAIR;
        //SoundManager.Sounds.detach();
    }

    public void trySnapToSurface()
    {
        m_controllers.Clear();
        // On release: Attach to nearby plane 
        if (m_placement == Placement.MIDAIR && m_dimension == 2)
        {
            snapToSurface();
        }
    }

    private void detachTransform(Controller controller)
    {
        bool snapped = false;
        if (m_controllers.Contains(controller))
        {
            m_controllers.Remove(controller);

            // Multiple attached controllers
            if (m_controllers.Count > 0)
            {
                Controller other = m_controllers[0];
                m_controllers.Clear();
                attachTransform(other);
                
                /*
                // Reset offsets 
                m_posAttach = this.transform.position;
                m_rotAttach = this.transform.rotation;
                m_lastController = other.type;
                getControllerAttachPose(other, out Vector3 pos, out Quaternion rot);
                m_offsetMidair = -this.transform.InverseTransformPoint(pos);
                m_offsetSurface = this.transform.position - pos;
                */
            }

            // On release: Attach to nearby plane 
            if (m_controllers.Count <= 0 && m_placement == Placement.MIDAIR && m_dimension == 2)
            {
                snapped = snapToSurface();
            }
           
        }

        if (!snapped && m_timeAttached > 0.3f)
        {
            //SoundManager.Sounds.release();
        }
    }

    public void detach(Controller controller)
    {
        switch (m_state)
        {
            case State.TRANSFORM:
                detachTransform(controller);
                // Update Element Model 
                // TODO: 0.3f defines minimum adjustment time
                /*
                if (m_timeAttached > 0.3f)
                {
                    if (VirtualEnvironmentManager.Environment != null)
                        VirtualEnvironmentManager.Environment.updatePriority(this.GetComponent<ElementModel>());
                    AdaptiveLayout.Adaptation.startOptimization();
                }
                */
                m_timeAttached = 0;
                m_attached = false;
                //if (VirtualEnvironmentManager.Environment != null) {
                //    VirtualEnvironmentManager.Environment.updatePriority(this.GetComponent<ElementModel>());
                //}
                //if (m_triggerAdaptation && AdaptiveLayout.Adaptation != null && m_tSinceAttach > m_tDetachOptimizationThreshold)
                //    AdaptiveLayout.Adaptation.startOptimization(); 
                break;
            case State.APP:
                if (m_app != null) 
                    m_app.detach(controller);
                break; 
        }
    }

    private bool checkInSurface()
    {
        Transform surface = this.transform.parent;
        if (surface == null || surface.tag != "Surface")
        {
            Debug.Log("Cant detach");
            return false;
        }
        Transform surfaceGeometry = surface.Find("Surface");
        if (surfaceGeometry == null)
            return false;
        // TODO: 0.05f tolerenace for element within surface 
        Vector3 surfaceScale = 0.5f * surfaceGeometry.localScale; // + 0.05f * Vector3.one; 
        surfaceScale.z = 0;

        // Detach if the center of the window exceeds the surface
        Vector3 point = this.transform.localPosition; // surfaceGeometry.InverseTransformPoint(this.transform.position);
        if (Mathf.Abs(point.x) > Mathf.Abs(surfaceScale.x) || Mathf.Abs(point.y) > Mathf.Abs(surfaceScale.y))
        {
            Debug.Log("should detach");
            return false;
        }
        return true;

        // Detach if a corner of the window exceeds the surface 
        /*
        Vector3 windowScale = window.localScale;

        // Bottom left 
        Vector3 point = 0.5f * new Vector3(-windowScale.x, -windowScale.y, 0);
        point = window.TransformPoint(point);
        point = surfaceGeometry.InverseTransformPoint(point);
        if (Mathf.Abs(point.x) > Mathf.Abs(surfaceScale.x) || Mathf.Abs(point.y) > Mathf.Abs(surfaceScale.y))
            return false;

        // Top left 
        point = 0.5f * new Vector3(-windowScale.x, windowScale.y, 0);
        point = window.TransformPoint(point);
        point = surfaceGeometry.InverseTransformPoint(point);
        if (Mathf.Abs(point.x) > Mathf.Abs(surfaceScale.x) || Mathf.Abs(point.y) > Mathf.Abs(surfaceScale.y))
            return false;

        // Bottom right
        point = 0.5f * new Vector3(windowScale.x, -windowScale.y, 0);
        point = window.TransformPoint(point);
        point = surfaceGeometry.InverseTransformPoint(point);
        if (Mathf.Abs(point.x) > Mathf.Abs(surfaceScale.x) || Mathf.Abs(point.y) > Mathf.Abs(surfaceScale.y))
            return false;

        // Top right
        point = 0.5f * new Vector3(windowScale.x, windowScale.y, 0);
        point = window.TransformPoint(point);
        point = surfaceGeometry.InverseTransformPoint(point);
        if (Mathf.Abs(point.x) > Mathf.Abs(surfaceScale.x) || Mathf.Abs(point.y) > Mathf.Abs(surfaceScale.y))
            return false;

        return true; 
        */
    }

    private void detachControllers()
    {
        int cNum = m_controllers.Count;
        Controller[] controllers = new Controller[cNum];
        for (int cIdx = 0; cIdx < cNum; cIdx++)
            controllers[cIdx] = m_controllers[cIdx];
        for (int cIdx = 0; cIdx < cNum; cIdx++)
            controllers[cIdx].detach();
        m_controllers.Clear();
    }

    private void getControllerAttachPose(Controller controller, out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        rot = Quaternion.identity;
        switch (m_placement)
        {
            case Placement.MIDAIR:
                controller.getSelectedPosMidAir(out pos, out forward, out up);
                break;
            case Placement.ON_SURFACE:
                controller.getSelectedPosSurface(this.transform.parent, out pos, out forward, out up);
                break;
        }
        rot = Quaternion.LookRotation(forward, up);
    }

    //Vector3 m_controllerAttachPos;
    //Quaternion m_controllerAttachRot;
    //Quaternion m_elementAttachRot;
    Matrix4x4 m_elementAttachMat;
    //bool m_updatePitch;
    //bool m_updateYaw; 
    public void attachTransform(Controller controller)
    {
        if (m_controllers.Contains(controller))
            return; 

        // Block interaction for set duration on snap
        if (this.m_placement == Placement.ON_SURFACE && m_tSnapRestricted > 0)
            return; 

        if (m_controllerHistory.ContainsKey(controller) && m_controllerHistory[controller] > m_doubleTapMinThreshold)
        {
            doubleTap();
            m_controllerHistory.Remove(controller);
            return;
        }
        else if (!m_controllerHistory.ContainsKey(controller))
        {
            m_controllerHistory.Add(controller, 0);
            //SoundManager.Sounds.attach();
        }

        m_controllers.Add(controller);
        if (m_controllers.Count == 1)
        {
            getControllerAttachPose(controller, out Vector3 pos, out Quaternion rot);
            Matrix4x4 attachMat = Matrix4x4.TRS(pos, rot, Vector3.one);
            m_elementAttachMat = attachMat.inverse * Matrix4x4.TRS(m_target, m_targetRotation, Vector3.one);
        }
        else if (m_controllers.Count == 2)
        {
            // Bimanual control
            m_initRotation = this.transform.rotation;
            getControllerAttachPose(m_controllers[0], out Vector3 c0pos, out Quaternion c0rot);
            getControllerAttachPose(m_controllers[1], out Vector3 c1pos, out Quaternion c1rot);
            m_initBetweenControllers = c1pos - c0pos;
            Vector3 initDir = m_initBetweenControllers.normalized;
            /*
            m_updatePitch = false;
            m_updateYaw = false; 
            float pitch = Mathf.Abs(Vector3.Dot(initDir, this.transform.up));
            float yaw = Mathf.Abs(Vector3.Dot(initDir, this.transform.right));
            if (pitch > yaw)
            {
                m_updatePitch = true;
            }
            else
            {
                m_updateYaw = true;
            }
            */
        }
    }


    private void getControllerPose(Controller controller, out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        rot = Quaternion.identity;
        switch (m_placement)
        {
            case Placement.MIDAIR:
                controller.getSelectedPosMidAir(out pos, out forward, out up);
                break;
            case Placement.ON_SURFACE:
                controller.getSelectedPosSurface(this.transform.parent, out pos, out forward, out up);
                break;
        }
        Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.LookRotation(forward, up), Vector3.one) * m_elementAttachMat;

        pos = mat.GetPosition();

        
        switch (m_placement)
        {
            case Placement.MIDAIR:
                Vector3 euler = mat.rotation.eulerAngles;
                euler.z = 0;
                rot = Quaternion.Euler(euler);
                break;
            case Placement.ON_SURFACE:
                rot = mat.rotation;
                break;
        }
    }

    private void updateTransform()
    {
        updateControllerHistory();
        
        if (m_controllers.Count == 1)
        {
            Controller controller = m_controllers[0];
            getControllerPose(controller, out Vector3 pos, out Quaternion rot);
            if (m_placement == Placement.ON_SURFACE)
            {
                pos = this.transform.parent.InverseTransformPoint(pos);
                pos.z = VirtualEnvironmentManager.Environment.SurfaceOffset;
                pos = this.transform.parent.TransformPoint(pos);
            }
            m_target = pos;
            m_targetRotation = rot; 
        }
        else if (m_controllers.Count == 2)
        {
            // Bimanual control
            getControllerAttachPose(m_controllers[0], out Vector3 c0pos, out Quaternion c0rot);
            getControllerAttachPose(m_controllers[1], out Vector3 c1pos, out Quaternion c1rot);
            Vector3 pos = Vector3.Lerp(c0pos, c1pos, 0.5f);

            Vector3 betweenControllers = c1pos - c0pos;
            Quaternion rotation = Quaternion.identity;
            switch (m_placement)
            {
                case Placement.MIDAIR:
                    Vector3 euler = Quaternion.FromToRotation(m_initBetweenControllers.normalized, betweenControllers.normalized).eulerAngles;
                    euler.z = 0;
                    //if (!m_updatePitch)
                    //    euler.x = 0;
                    //if (!m_updateYaw)
                    //    euler.y = 0;
                    rotation = Quaternion.Euler(euler);
                    break;
                case Placement.ON_SURFACE:
                    pos = this.transform.parent.InverseTransformPoint(pos);
                    pos.z = VirtualEnvironmentManager.Environment.SurfaceOffset;
                    pos = this.transform.parent.TransformPoint(pos);
                    float angle = Vector3.SignedAngle(m_initBetweenControllers.normalized, betweenControllers.normalized, this.transform.parent.forward);
                    rotation = Quaternion.Euler(0, 0, angle);
                    break;
            }
            m_targetRotation = m_initRotation * rotation;
            m_target = pos; 
        }
        /*
        else if (m_controllers.Count == 2)
        {
            getControllerAttachPose(m_controllers[0], out Vector3 c0, out Quaternion c0rot);
            Vector3 c0dir = c0rot * Vector3.forward;
            getControllerAttachPose(m_controllers[1], out Vector3 c1, out Quaternion c1rot);
            Vector3 c1dir = c1rot * Vector3.forward;
            m_target = Vector3.Lerp(c0, c1, 0.5f);

            Vector3 betweenControllers = c1 - c0;
            float scaleFactor = betweenControllers.magnitude / m_initBetweenControllers.magnitude;
            if (m_dimension == 2)
            {
                m_targetScale.x = scaleFactor * m_initScale.x;
                m_targetScale.y = scaleFactor * m_initScale.y;
            } else if (m_dimension == 3)
            {
                m_targetScale = scaleFactor * m_initScale;
            }

            Quaternion rotation = Quaternion.identity;
            switch (m_placement)
            {
                case Placement.MIDAIR:
                    rotation = Quaternion.FromToRotation(m_initBetweenControllers.normalized, betweenControllers.normalized);
                    break;
                case Placement.ON_SURFACE:
                    float angle = Vector3.SignedAngle(m_initBetweenControllers.normalized, betweenControllers.normalized, this.transform.parent.forward);
                    rotation = Quaternion.Euler(0, 0, angle);
                    break;
            }
            m_targetRotation = m_initRotation * rotation;
        }
        */

        this.transform.position = Vector3.Lerp(this.transform.position, m_target, m_translationUpdateRate * Time.deltaTime);
        this.transform.Find("Window").localScale = Vector3.Lerp(this.transform.Find("Window").localScale, m_targetScale, m_scaleUpdateRate * Time.deltaTime);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, m_targetRotation, m_rotationUpdateRate * Time.deltaTime);

        // Update application transform 
        if (m_app != null)
            m_app.rescale(this.transform.Find("Window").localScale);
        
        

        // Detach from surface accordingly
        if (m_placement == Placement.ON_SURFACE && !checkInSurface())
        {
            detachControllers();
            this.detachFromSurface();
        }
    }

    IEnumerator updatePositionRotation(Vector3 position, Quaternion rotation, bool snap)
    {
        Widget.State state = this.state;
        this.updateState(Widget.State.TRANSFORM);
        this.detachFromSurface();
        this.targetPosition = position;
        this.targetRotation = rotation;
        while (Vector3.Distance(this.Position, position) > 0.01f && Quaternion.Angle(this.Rotation, rotation) > 5f)
            yield return null;
        this.transform.position = this.targetPosition;
        this.transform.rotation = this.targetRotation;
        if (snap)
            this.trySnapToSurface();
        this.updateState(state);
    }

    public void place(Vector3 position, Quaternion rotation, bool snap)
    {
        // Case: Keyboard 
        /*
        KeyboardInput keyboard = this.GetComponent<KeyboardInput>(); 
        if (keyboard != null)
        {
            WidgetApplication outputApplication = keyboard.outputApplication;
            if (outputApplication != null)
            {
                Widget outputApplicationWidget = outputApplication.GetComponent<Widget>();
                Vector3 toOutputApplication = outputApplication.GetComponent<Widget>().targetPosition - Camera.main.transform.position;
                toOutputApplication.y = 0;
                toOutputApplication.Normalize(); 
                rotation = Quaternion.LookRotation(toOutputApplication) * Quaternion.Euler(60, 0, 0);
            }
        }
        */

        /*
        Widget.State state = this.state;
        this.updateState(Widget.State.TRANSFORM);
        this.detachFromSurface();
        this.targetPosition = position;
        this.targetRotation = rotation;
        this.transform.position = position;
        this.transform.rotation = rotation;
        if (snap)
            this.trySnapToSurface();
        this.updateState(state);
        */

        // Default: 
        StartCoroutine(updatePositionRotation(position, rotation, snap));   
    }


    private void Start()
    {
        userHead = GameObject.Find("CenterEyeAnchor");
        m_target = this.transform.position;
        m_targetScale = this.transform.Find("Window").localScale;
        m_targetRotation = this.transform.rotation;
        if (m_dimension == 3)
            detachFromSurface();
    }

    public void revertLastAttachedTransform()
    {
        m_target = m_posAttach;
        m_targetRotation = m_rotAttach;
        //if (m_triggerAdaptation && AdaptiveLayout.Adaptation != null && m_tSinceAttach > m_tDetachOptimizationThreshold)
        //{
        //    AdaptiveLayout.Adaptation.startOptimization();
        //}
    }

    private void Update()
    {
        if (m_attached)
            m_timeAttached += Time.deltaTime;

        // Restrict interactions after snap 
        m_tSnapRestricted = Mathf.Max(0, m_tSnapRestricted - Time.deltaTime);

        switch (m_state)
        {
            case State.TRANSFORM:
                updateTransform(); 
                break;
            case State.APP:
                break; 
        }
        if (m_state != m_prevState)
        {
            if (m_state == State.TRANSFORM && m_app != null)
                m_app.disable();
        }
        m_prevState = m_state;
    }

}
