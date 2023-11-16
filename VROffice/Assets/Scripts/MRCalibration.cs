using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRCalibration : MonoBehaviour
{
    [SerializeField]
    private GameObject m_calibrationFeedback;
    
    [Header("Calibration Controls")]
    [SerializeField]
    private Transform m_virtualReference;
    public Transform virtualReference
    {
        set { m_virtualReference = value; }
    }
    [SerializeField]
    private Transform m_environment;
    
    private int m_pointsSet;
    private Vector3[] m_points = new Vector3[3];

    public void reset()
    {
        // if (m_controllerObj != null)                 (only for adaptive controller needed)
        //     m_controllerObj.SetActive(true);
        // if (m_secondaryControllerObj != null)
        //     m_secondaryControllerObj.SetActive(true);
        m_pointsSet = 0;
    }

    //creates the the calibrationFeedback based on 3 corners
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
                //if we create environments with MRMapper make sure to always choose the same orientation
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

    // Start is called before the first frame update
    void Start()
    {
        //disable();
    }

    // Update is called once per frame
    public void Calibrate(Vector3[] vertices)
    {
        m_calibrationFeedback.SetActive(true);
        
        updatePoint(vertices[0], Quaternion.identity);
        setPoint(vertices[0]);
        
        updatePoint(vertices[2], Quaternion.identity);
        setPoint(vertices[2]);
        
        updatePoint(vertices[3], Quaternion.identity);
        setPoint(vertices[3]);

        reset();
        
    }
}
