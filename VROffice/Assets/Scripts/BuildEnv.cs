using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/*
 * This script turns the captured surfaces from MRMapper into an Environment and saves it
 */

public class BuildEnv : MonoBehaviour
{
    public PhysicalEnvironmentManager pEnvManager;
    public GameObject mrMapper;
    [SerializeField] private Transform envCollection;

    public bool build;
    private bool buildPrev = false;

    // Update is called once per frame
    void Update()
    {
        if (build && !buildPrev)
        {
            GameObject newEnv = new GameObject("Env-NewAutomaticEnv");
            newEnv.AddComponent<EnvModel>();
            newEnv.transform.SetParent(envCollection);
            StartCoroutine(setPoints(newEnv.transform));

            ReceivePlanes planesScript = mrMapper.GetComponent<ReceivePlanes>();
            planesScript.freeze = true;
            foreach (GameObject plane in planesScript.planes)
            {
                plane.GetComponent<MeshRenderer>().enabled = false;
            }
            GameObject.Find("Point Cloud").GetComponent<MeshRenderer>().enabled = false;
        }
        buildPrev = build;
    }

    //submit the corners of the surfaces to the PhysicalEnvironmentManager to build the Environment
    private IEnumerator setPoints(Transform env)
    {
        pEnvManager.Env = env;
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
        centerOnChildren(env);
        pEnvManager.useEnvironment();
        pEnvManager.m_definingNewElements = false;
        build = false;
        mrMapper.GetComponent<AlignSystems>().enabledAlignment = true;
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
