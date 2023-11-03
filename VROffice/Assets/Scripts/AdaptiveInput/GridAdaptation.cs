using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAdaptation : MonoBehaviour
{
    [Header("User Pose")]
    [SerializeField]
    private Camera m_cam;
    [SerializeField]
    private Transform m_leftHand, m_rightHand;
    private List<GameObject> m_surfaces = new List<GameObject>();
    private List<GameObject> m_obstacles = new List<GameObject>();

    [Header("Grid Settings")]
    [SerializeField]
    private float m_highlightNear;
    [SerializeField]
    private float m_highlightFar;
    [SerializeField]
    private float m_viewNear;
    [SerializeField]
    private float m_viewFar;
    [SerializeField]
    [Range(1,10)]
    private float m_viewExponent;
    [SerializeField]
    private float m_outlineStart;
    [SerializeField]
    private float m_contactHighlightNear;
    [SerializeField]
    private float m_contactHighlightFar;
    [SerializeField]
    [Range(1,3)]
    private float m_contactMultiplier;

    private bool m_init = false; 

    private void updateObjects()
    {
        m_surfaces.Clear();
        // Surfaces
        foreach (ContainerModel container in PhysicalEnvironmentManager.Environment.Containers)
        {
            if (container.Dimension == 2) {
                m_surfaces.Add(container.transform.Find("Surface").gameObject);
            }       
        }

        // Obstacles
        m_obstacles.Clear();
        foreach (ObstacleModel obstacle in PhysicalEnvironmentManager.Environment.Obstacles)
        {
            m_obstacles.Add(obstacle.gameObject);
        }
    }

    private void setGraduationScale(GameObject obj)
    {
        Vector3 scale = obj.transform.localScale;
        float scaleFactor = 2f * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        obj.GetComponent<MeshRenderer>().material.SetFloat("_GraduationScale", scaleFactor);
    }

    private float getUserDistance(GameObject obj) {
        Vector3 viewPos = m_cam.transform.position;
        Vector3 leftHandPos = m_leftHand.position;
        Vector3 rightHandPos = m_rightHand.position;
        Collider objCol = obj.GetComponent<Collider>();
        float viewDist = Vector3.Distance(viewPos, objCol.ClosestPoint(viewPos));
        float closestDist = viewDist;
        closestDist = Mathf.Min(closestDist, Vector3.Distance(leftHandPos, objCol.ClosestPoint(leftHandPos)));
        closestDist = Mathf.Min(closestDist, Vector3.Distance(rightHandPos, objCol.ClosestPoint(rightHandPos)));
        return closestDist;
    }

    private float getUserViewDist(GameObject obj)
    {
        Vector3 viewPos = m_cam.transform.position;
        Collider objCol = obj.GetComponent<Collider>();
        return Vector3.Distance(viewPos, objCol.ClosestPoint(viewPos));
    }

    private float getScale(GameObject obj)
    {
        Vector3 scale = obj.transform.localScale;
        return Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
    }

    private float getGridScale(GameObject obj)
    {
        float viewDist = getUserViewDist(obj);
        float scale = 1;

        if (viewDist < m_viewFar) {
            scale = 1 - Mathf.Clamp01((viewDist - m_viewNear) / (m_viewFar - m_viewNear));
            scale = Mathf.Lerp(1, 16, Mathf.Pow(scale, m_viewExponent / getScale(obj)));
        } else
        {
            // scale = 1 - Mathf.Clamp01((viewDist - m_viewFar) / (m_outlineStart - m_viewFar));
            scale = 1;
        }
        return scale; 

        
    }

    private void init()
    {
        if (m_init)
            return;

        if (PhysicalEnvironmentManager.Environment == null)
            return;

        updateObjects();
        foreach (GameObject obj in m_surfaces)
            setGraduationScale(obj);
        foreach (GameObject obj in m_obstacles)
            setGraduationScale(obj);

        m_init = true; 
    }

    // Start is called before the first frame update
    void Start()
    {
        //init();
    }

    // Update is called once per frame
    void Update()
    {
        //init();

        updateObjects();

        // Adaptive gridlines
        foreach (GameObject obj in m_obstacles)
        {
            float closestDist = getUserDistance(obj);
            float contact = 1 - Mathf.Clamp01((closestDist - m_highlightNear) / (m_highlightFar - m_highlightNear));
            obj.GetComponent<MeshRenderer>().material.SetFloat("_ColorInterp", contact);
            float contactGridEffect = 1 - Mathf.Clamp01((closestDist - m_contactHighlightNear) / (m_contactHighlightFar - m_contactHighlightNear));
            float gridContactMultiplier = Mathf.Lerp(1, m_contactMultiplier, contactGridEffect);
            float scale = Mathf.Clamp(gridContactMultiplier * getGridScale(obj), 1, 16);
            if (scale <= 0)
            {
                obj.GetComponent<MeshRenderer>().enabled = false;
            } else
            {
                obj.GetComponent<MeshRenderer>().enabled = true;
            }
            obj.GetComponent<MeshRenderer>().material.SetFloat("_Scale", scale);
        }
        foreach (GameObject obj in m_surfaces)
        {
            float scale = 1.5f * getGridScale(obj);
            if (scale <= 0)
            {
                obj.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                obj.GetComponent<MeshRenderer>().enabled = true;
            }
            obj.GetComponent<MeshRenderer>().material.SetFloat("_Scale", scale);
        }

    }
}
