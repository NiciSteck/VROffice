using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuildEnv : MonoBehaviour
{
    public PhysicalEnvironmentManager pEnvManager;
    private GameObject mrMapper;
    [SerializeField] private Transform envCollection;

    public bool build;
    private bool buildPrev = false;
    
    // Start is called before the first frame update
    void Start()
    {
        mrMapper = transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (build && !buildPrev)
        {
            GameObject newEnv = new GameObject("Env-NewAutomaticEnv");
            newEnv.AddComponent<EnvModel>();
            newEnv.transform.SetParent(envCollection);
            StartCoroutine(setPoints(newEnv.transform));
        }
        buildPrev = build;
    }

    private IEnumerator setPoints(Transform env)
    {
        pEnvManager.Env = env.transform;
        pEnvManager.m_definingNewElements = true;
        
        EnvModel model = env.GetComponent<EnvModel>();
        List<GameObject> planes = mrMapper.GetComponent<ReceivePlanes>().planes;
        
        foreach (GameObject plane in planes)
        {
            Mesh mesh = plane.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for (int i = 0; i < 3; i++)
            {
                int index = triangles[i];
                Vector3 corner = vertices[index];
                pEnvManager.mrPoint = plane.transform.TransformPoint(corner);
                pEnvManager.mrDefinePoint = true;
                yield return new WaitWhile(() => pEnvManager.mrDefinePoint);
            }
            renameSurface(env,plane.name);
        }
        updateEnvModel(model);
        centerOnChildren(env);
        
        pEnvManager.Env = null;
        pEnvManager.m_definingNewElements = false;
        build = false;
    }

    private void renameSurface(Transform env, string name)
    {
        foreach (Transform child in env)
        {
            if (child.name.Contains("Surface"))
            {
                child.name = name;
                break;
            }
        }
    }

    private void updateEnvModel(EnvModel model)
    {
        List<ContainerModel> containers = model.containers;
        foreach (Transform child in model.gameObject.transform)
        {
            containers.Add(child.GetComponent<ContainerModel>());
        }
    }
    
    public void centerOnChildren(Transform parent)
    {
        List<Transform> children = parent.Cast<Transform>().ToList();
        Vector3 center = Vector3.zero;
        foreach (Transform child in children)
        {
            center += child.position;
            child.parent = null;
        }
        parent.position = center/children.Count;
        foreach (Transform child in children)
        {
            child.parent = parent;
        }
    }
}
