using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PhysicalEnvironmentManager : MonoBehaviour
{
    private int m_pointsSet;
    private Vector3[] m_points = new Vector3[4];

    public static PhysicalEnvironmentManager Environment; 

    public enum EnvElement { Surface, Volume, Obstacle }

    [Header("Controller Settings")]
    [SerializeField]
    private GameObject m_controllerObj;
    [SerializeField]
    private OVRInput.Controller m_controller;
    public OVRInput.Controller Controller
    {
        get { return m_controller; }
    }
    [SerializeField]
    private Vector3 m_controllerOffset;

    [SerializeField]
    private Material m_containerMaterial; 
    [SerializeField]
    private Color m_definingSurfaceColor;
    [SerializeField]
    private Color m_surfaceColor;
    [SerializeField]
    private Color m_definingVolumeColor;
    [SerializeField]
    private Color m_volumeColor;
    [SerializeField]
    private Color m_definingObstacleColor;
    [SerializeField]
    private Color m_obstacleColor;
    [SerializeField]
    private GameObject m_containerInstantiator;

    [Header("Environment")]
    //[SerializeField]
    //private Transform m_surfacesParent;
    [SerializeField]
    private List<ContainerModel> m_containers = new List<ContainerModel>();
    public List<ContainerModel> Containers
    {
        get { return m_containers; }
    }
    
    [SerializeField]
    private List<ObstacleModel> m_obstacles = new List<ObstacleModel>();
    public List<ObstacleModel> Obstacles
    {
        get { return m_obstacles; }
    }
    [NonSerialized] 
    public Vector3 mrPoint;
    [Header("Controls")]
    [SerializeField]
    private Transform m_env;
    public Transform Env
    {
        get { return m_env; }
        set { m_env = value; }
    }
    [SerializeField]
    private bool m_clearEnv;
    private bool m_clearEnvPrev;
    [SerializeField]
    private bool m_useEnv;
    private bool m_useEnvPrev;
    [SerializeField]
    public bool m_definingNewElements;
    private bool m_definingNewElementsPrev; 
    public EnvElement m_definingElement;
    [SerializeField] 
    public bool mrDefinePoint;

    public void disable()
    {
        // if (m_controllerObj != null)             (only important with adaptive hands)
        //     m_controllerObj.SetActive(false);
        m_containerInstantiator.SetActive(false);
    }

    public void reset()
    {
        // if (m_controllerObj != null)             (only important with adaptive hands)
        //     m_controllerObj.SetActive(true);
        m_containerInstantiator.SetActive(true);

        m_pointsSet = 0;
    }

    public void updatePoint(Vector3 position, Quaternion rotation)
    {
        if (m_pointsSet == 0)
        {
            m_containerInstantiator.transform.position = position;
            m_containerInstantiator.transform.localScale = new Vector3(0.01f, 0.01f, 0.001f);
            m_containerInstantiator.transform.rotation = rotation;
        }
        else if (m_pointsSet == 1)
        {
            m_containerInstantiator.transform.position = 0.5f * (m_points[0] + position);
            Vector3 ab = position - m_points[0];
            if (m_definingElement != EnvElement.Surface)
            {
                ab[1] = 0; 
            }
            Vector3 forward = Vector3.Cross(ab, rotation * Vector3.up).normalized;
            Vector3 up = Vector3.Cross(rotation * Vector3.forward, ab).normalized;
            m_containerInstantiator.transform.rotation = Quaternion.LookRotation(forward, up);
            m_containerInstantiator.transform.localScale = new Vector3(ab.magnitude, 0.01f, 0.001f);
        }
        else if (m_pointsSet == 2)
        {
            Vector3 ab = m_points[1] - m_points[0];
            Vector3 ac = position - m_points[0];
            if (m_definingElement != EnvElement.Surface)
            {
                ab[1] = 0;
                ac[1] = 0;
            }
            Vector3 bc = ac - Vector3.Dot(ac, ab.normalized) * ab.normalized;
            Vector3 normal = Vector3.Cross(ab, ac);
            m_containerInstantiator.transform.position = m_points[0] + (0.5f * ab) + (0.5f * bc);
            m_containerInstantiator.transform.localScale = new Vector3(ab.magnitude, bc.magnitude, 0.001f);
            m_containerInstantiator.transform.rotation = Quaternion.LookRotation(normal, bc);
        }
        else if (m_pointsSet == 3)
        {
            Vector3 ab = m_points[1] - m_points[0];
            Vector3 ac = m_points[2] - m_points[0];
            ab[1] = 0;
            ac[1] = 0;
            Vector3 bc = ac - Vector3.Dot(ac, ab.normalized) * ab.normalized;
            Vector3 normal = Vector3.Cross(ab, ac);
            normal.Normalize();
            Vector3 abc = (m_points[0] + m_points[1] + m_points[2]) / 3;
            float height = Vector3.Dot(position - abc, normal);
            m_containerInstantiator.transform.position = m_points[0] + (0.5f * ab) + (0.5f * bc) + (0.5f * height * normal);
            m_containerInstantiator.transform.localScale = new Vector3(ab.magnitude, height, bc.magnitude);
            m_containerInstantiator.transform.rotation = Quaternion.LookRotation(bc, Vector3.up);
        }
    }

    private void setPoint(Vector3 position)
    {
        int dim = 3;
        if (m_definingElement == EnvElement.Surface)
            dim = 2; 

        if (m_pointsSet < dim + 1)
        {
            m_points[m_pointsSet] = position;
            m_pointsSet++;

            if (m_pointsSet >= dim + 1)
            {
                if (m_definingElement == EnvElement.Obstacle) addObstacle();
                else addContainer();
                reset();
            }
        }
    }

    private void addObstacle()
    {
        if (m_env == null)
            return; 

        GameObject obstacle = GameObject.Instantiate(m_containerInstantiator);
        obstacle.name = "Obstacle-" + m_obstacles.Count;
        
        // Scale
        Vector3 scale = obstacle.transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);
        scale.z = Mathf.Abs(scale.z);
        obstacle.transform.localScale = scale;

        // Parent
        obstacle.transform.SetParent(m_env);

        // Material 
        MeshRenderer mr = obstacle.GetComponent<MeshRenderer>();
        mr.material = m_containerMaterial;

        // Add to obstacle list
        ObstacleModel obstacleModel = obstacle.AddComponent<ObstacleModel>();
        m_obstacles.Add(obstacleModel);
    }

    private void addContainer()
    {
        if (m_env == null)
            return; 

        GameObject container = new GameObject(); 
        // Position 
        container.transform.SetPositionAndRotation(m_containerInstantiator.transform.position, m_containerInstantiator.transform.rotation);
        
        // Scale
        GameObject containerGeom =  GameObject.Instantiate(m_containerInstantiator, container.transform);
        containerGeom.transform.localPosition = Vector3.zero;
        containerGeom.transform.localRotation = Quaternion.identity;
        Vector3 scale = containerGeom.transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);
        scale.z = Mathf.Abs(scale.z);
        containerGeom.transform.localScale = scale;

        // Set parent 
        container.transform.SetParent(m_env);

        // Collision parameters 
        containerGeom.GetComponent<BoxCollider>().isTrigger = true;

        // Rendering 
        MeshRenderer mr = containerGeom.GetComponent<MeshRenderer>();
        mr.material = m_containerMaterial;
        ContainerModel containerModel = container.AddComponent<ContainerModel>();

        // Surface or volume 
        switch (m_definingElement)
        {
            case EnvElement.Surface:
                container.name = "Surface-" + m_containers.Count;
                container.tag = "Surface";
                containerGeom.name = "Surface";
                containerGeom.layer = Constants.SurfaceLayer;
                //mr.material.color = m_surfaceColor;
                containerModel.Dimension = 2;
                break;
            case EnvElement.Volume:
                container.name = "Volume-" + m_containers.Count;
                container.tag = "Volume";
                containerGeom.name = "Volume";
                containerGeom.layer = Constants.VolumeLayer;
                //mr.material.color = m_volumeColor;
                mr.enabled = false;
                containerModel.Dimension = 3;
                break;
        }
        m_containers.Add(containerModel);
    }

    public ContainerModel addVolume(Vector3 position, Vector3 forward, Vector3 up, Vector3 scale)
    {
        GameObject container = new GameObject();

        // Position, Rotation, Scale
        container.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, up));
        GameObject containerGeom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        containerGeom.transform.SetParent(container.transform);
        containerGeom.transform.localPosition = Vector3.zero;
        containerGeom.transform.localRotation = Quaternion.identity;
        containerGeom.transform.localScale = scale;

        // Collision parameters 
        containerGeom.GetComponent<BoxCollider>().isTrigger = true;

        // Set parent 
        container.transform.SetParent(m_env);

        // Additional properties 
        ContainerModel containerModel = container.AddComponent<ContainerModel>();
        container.name = "Volume-" + m_containers.Count;
        container.tag = "Volume";
        containerGeom.name = "Volume";
        containerGeom.layer = Constants.VolumeLayer;

        // Rendering 
        MeshRenderer mr = containerGeom.GetComponent<MeshRenderer>();
        mr.material = m_containerMaterial;
        mr.enabled = false;
        //mr.material.color = m_volumeColor;

        containerModel.Dimension = 3;
        return containerModel;
    }

    public void clearEnv()
    {
        //foreach (ContainerModel container in m_containers)
        //    GameObject.Destroy(container.gameObject);
        //foreach (ObstacleModel obstacle in m_obstacles)
        //    GameObject.Destroy(obstacle.gameObject);

        m_containers.Clear();
        m_obstacles.Clear();

        m_clearEnv = false;
    }

    public void setDefiningElement(PhysicalEnvironmentManager.EnvElement element)
    {
        reset();
        m_definingElement = element;
    }

    /*
    public bool checkObstructed(Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float abDist = ab.magnitude;
        Vector3 abDir = ab.normalized;
        Ray abRay = new Ray(a, abDir);
        RaycastHit hit; 
        foreach (ObstacleModel obstacle in m_obstacles)
        {
            Collider col = obstacle.GetComponent<Collider>();
            if (col != null)
                if (col.Raycast(abRay, out hit, Mathf.Infinity))
                    if (hit.distance < abDist)
                        return true;
        }
        foreach (ContainerModel container in m_containers)
        {
            if (container.Dimension != 2)
                continue;
            Collider col = container.GetComponent<Collider>();
            if (col != null)
                if (col.Raycast(abRay, out hit, Mathf.Infinity))
                    if (hit.distance < abDist)
                        return true;
        }

        return false; 
    }
    */

    public void useEnvironment()
    {
        clearEnv();

        m_env.gameObject.SetActive(true);
        foreach (Transform element in m_env)
        {
            // Surface
            ContainerModel surface = element.GetComponent<ContainerModel>();
            if (surface != null)
                m_containers.Add(surface);

            // Obstacles 
            ObstacleModel obstacle = element.GetComponent<ObstacleModel>();
            if (obstacle != null)
                m_obstacles.Add(obstacle);
        }

        m_useEnv = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Environment = this;
        if (m_env != null)
        {
            useEnvironment();
        }
        disable();
    }

    /*
    private void OnEnable()
    {
        reset();
    }

    private void OnDisable()
    {
        disable();
    }
    */

    // Update is called once per frame
    void Update()
    {
        // Controls enabled or disabled 
        if (m_definingNewElements && !m_definingNewElementsPrev)
        {
            reset();
        } 
        if (!m_definingNewElements && m_definingNewElementsPrev)
        {
            disable(); 
        }

        if (m_containerInstantiator.activeInHierarchy)
        {
            Quaternion rotation = OVRInput.GetLocalControllerRotation(m_controller);
            Vector3 position = OVRInput.GetLocalControllerPosition(m_controller) 
                + m_controllerOffset.x * (rotation * Vector3.right)
                + m_controllerOffset.y * (rotation * Vector3.up)
                + m_controllerOffset.z * (rotation * Vector3.forward);
            if (mrDefinePoint)
            {
                updatePoint(mrPoint, rotation);
            }
            else
            {
                updatePoint(position, rotation);
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_controller) && !mrDefinePoint)
            {
                setPoint(position);
            }else if (mrDefinePoint)
            {
                setPoint(mrPoint);
                mrDefinePoint = false;
            }
                
        }

        // Update instantiator color 
        switch (m_definingElement)
        {
            case EnvElement.Surface:
                m_containerInstantiator.GetComponent<MeshRenderer>().material.color = m_definingSurfaceColor;
                break;
            case EnvElement.Volume:
                m_containerInstantiator.GetComponent<MeshRenderer>().material.color = m_definingVolumeColor;
                break;
            case EnvElement.Obstacle:
                m_containerInstantiator.GetComponent<MeshRenderer>().material.color = m_definingObstacleColor;
                break; 
        }

        if (m_clearEnv && !m_clearEnvPrev) {
            clearEnv(); 
        }

        if (m_useEnv && !m_useEnvPrev)
        {
            Debug.Log("Physical Environment Manager: Use Environment");
            useEnvironment(); 
        }

        m_clearEnvPrev = m_clearEnv;
        m_definingNewElementsPrev = m_definingNewElements;
    }
}