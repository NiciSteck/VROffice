using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibration : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField]
    private GameObject m_controllerObj;
    [SerializeField]
    private GameObject m_secondaryControllerObj; 
    [SerializeField]
    private OVRInput.Controller m_controller;
    [SerializeField]
    private Vector3 m_controllerOffset;
    [SerializeField]
    private Transform m_controllerCalibrationSurface;
    [SerializeField]
    private Transform m_secondaryControllerCalibrationSurface;
    [SerializeField]
    private GameObject m_calibrationFeedback;

    [Header("Calibration Settings")]
    [SerializeField]
    private float m_surfaceNormalOffset;
    [SerializeField]
    private bool m_updateClosest;
    [SerializeField]
    private bool m_twoControllerCalibration; 

    [Header("Calibration Controls")]
    [SerializeField]
    private Transform m_virtualReference;
    public Transform virtualReference
    {
        set { m_virtualReference = value; }
    }
    [SerializeField]
    private Transform m_environment;
    public Transform environment
    {
        set { m_environment = value; }
    }
    [SerializeField]
    private bool m_calibrating;
    private bool m_calibratingPrev; 
    public bool calibrating
    {
        set { m_calibrating = value; }
    }
    
    private int m_pointsSet;
    private Vector3[] m_points = new Vector3[3];

    public void disable()
    {
        // if (m_controllerObj != null)
        //     m_controllerObj.SetActive(false);
        // if (m_secondaryControllerObj != null)
        //     m_secondaryControllerObj.SetActive(false);
        m_calibrationFeedback.SetActive(false);
        m_calibrating = false; 
    }

    public void reset()
    {
        // if (m_controllerObj != null)
        //     m_controllerObj.SetActive(true);
        // if (m_secondaryControllerObj != null)
        //     m_secondaryControllerObj.SetActive(true);
        m_calibrationFeedback.SetActive(true);

        m_pointsSet = 0;
    }

    public void updatePoint(Vector3 position, Quaternion rotation)
    {
        if (m_pointsSet == 0)
        {
            m_calibrationFeedback.transform.position = position;
            m_calibrationFeedback.transform.localScale = new Vector3(0.01f, 0.01f, 0.001f);
            m_calibrationFeedback.transform.rotation = rotation;
        }
        else if (m_pointsSet == 1)
        {
            m_calibrationFeedback.transform.position = 0.5f * (m_points[0] + position);
            Vector3 ab = position - m_points[0];
            Vector3 forward = Vector3.Cross(ab, rotation * Vector3.up).normalized;
            Vector3 up = Vector3.Cross(rotation * Vector3.forward, ab).normalized;
            m_calibrationFeedback.transform.rotation = Quaternion.LookRotation(forward, up);
            m_calibrationFeedback.transform.localScale = new Vector3(ab.magnitude, 0.01f, 0.001f);
        } else
        {
            Vector3 ab = m_points[1] - m_points[0];
            Vector3 ac = position - m_points[0];
            Vector3 bc = ac - Vector3.Dot(ac, ab.normalized) * ab.normalized;
            Vector3 normal = Vector3.Cross(ab, ac);
            m_calibrationFeedback.transform.position = m_points[0] + (0.5f * ab) + (0.5f * bc);
            m_calibrationFeedback.transform.localScale = new Vector3(ab.magnitude, bc.magnitude, 0.001f);
            m_calibrationFeedback.transform.rotation = Quaternion.LookRotation(normal, bc);

            
        }
    }

    private void setPoint(Vector3 position)
    {

        if (m_pointsSet < 3)
        {
            m_points[m_pointsSet] = position;
            m_pointsSet++;

            if (m_pointsSet >= 3)
            {
                m_environment.position = Vector3.zero;
                m_environment.rotation = Quaternion.identity;
                m_environment.position = m_calibrationFeedback.transform.position - m_virtualReference.position;
                m_environment.RotateAround(m_virtualReference.position, Vector3.up, Vector3.SignedAngle(m_virtualReference.up, m_calibrationFeedback.transform.up, Vector3.up));

                // Update element transforms 
                //VirtualEnvironmentManager.Environment.updateTransforms();
                //m_environment.position = m_points[0];
                //m_environment.rotation = Quaternion.LookRotation((m_points[1] - m_points[0]).normalized, Vector3.up);
                //AdaptiveLayout.Adaptation.resetUser();
                //AdaptiveLayout.Adaptation.resetEnv();
                reset();
            }
        }
    }

    private void calibrateSurface()
    {
        List<ContainerModel> containers = PhysicalEnvironmentManager.Environment.Containers;
        Transform updateTransform = null;

        Vector3 calibrationPosition = m_controllerCalibrationSurface.position;
        Vector3 calibrationUp = m_controllerCalibrationSurface.up;
        if (m_twoControllerCalibration)
        {
            calibrationPosition = Vector3.Lerp(m_controllerCalibrationSurface.position, m_secondaryControllerCalibrationSurface.position, 0.5f);
            calibrationUp = Vector3.Slerp(m_controllerCalibrationSurface.up, m_secondaryControllerCalibrationSurface.up, 0.5f);
        }

        if (m_updateClosest)
        {
            float closestContainerDist = Mathf.Infinity;
            ContainerModel closestContainer = null;
            foreach (ContainerModel container in containers)
            {
                Collider collider = container.GetComponentInChildren<Collider>();
                if (collider != null)
                {
                    float dist = Vector3.Distance(calibrationPosition, collider.ClosestPoint(calibrationPosition));
                    if (dist < closestContainerDist)
                    {
                        closestContainerDist = dist;
                        closestContainer = container;
                    }
                }
            }

            if (closestContainer != null && closestContainerDist < 0.05f) 
            {
                updateTransform = closestContainer.transform;
            }
        } else
        {
            updateTransform = m_virtualReference;
        }

        if (updateTransform != null)
        {
            updateTransform.position += (Vector3.Dot(calibrationUp, calibrationPosition - updateTransform.position) + m_surfaceNormalOffset) * m_controllerCalibrationSurface.up;
            /*
            if (Vector3.Dot(updateTransform.forward, forward) < 0)
                forward *= -1;
            updateTransform.rotation = Quaternion.LookRotation(forward, updateTransform.up);
            */
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_calibrating && !m_calibratingPrev)
        {
            reset();
        }
        
        if (!m_calibrating && m_calibratingPrev)
        {
            disable(); 
        }

        if (m_calibrationFeedback.activeInHierarchy)
        {
            Quaternion rotation = OVRInput.GetLocalControllerRotation(m_controller);
            Vector3 position = OVRInput.GetLocalControllerPosition(m_controller)
                + m_controllerOffset.x * (rotation * Vector3.right)
                + m_controllerOffset.y * (rotation * Vector3.up)
                + m_controllerOffset.z * (rotation * Vector3.forward);
            updatePoint(position, rotation);
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_controller))
                setPoint(position);
            if (OVRInput.GetDown(OVRInput.Button.One, m_controller))
                calibrateSurface(); 
        }

        m_calibratingPrev = m_calibrating;
    }
}
