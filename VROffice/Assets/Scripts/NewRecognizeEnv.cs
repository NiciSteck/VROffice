using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class NewRecognizeEnv : MonoBehaviour
{
    private List<EnvModel> envList = new List<EnvModel>();

    [SerializeField]
    private GameObject mrMapper;

    [SerializeField] private bool align = false;
    [SerializeField] private bool recenter = false;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform environment in transform)
        {
            EnvModel env = environment.GetComponent<EnvModel>();
            if (env != null)
                envList.Add(env);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (align||recenter)
        {
            List<EnvModel> probableEnvs = new List<EnvModel>();
            if (recenter)
            {
                probableEnvs = new List<EnvModel>(transform.GetComponentsInChildren<EnvModel>(false));
            }
            else
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                probableEnvs = recognize();
            }
            
            align = false;
            recenter = false;
            StartCoroutine(ExecuteOptimization(probableEnvs));
        }
    }

    private String jsonifyPlanes(List<GameObject> planes, bool fromMrMapper)
    {
        String jsonString;
        if (fromMrMapper)
        {
            jsonString = "\"mr\":[";
        }
        else
        {
            jsonString = "\"env\":[";
        }
        foreach (GameObject plane in planes)
        {
            Vector3[] corners;
            corners = getRealCorners(fromMrMapper ? plane : plane.transform.GetChild(0).gameObject);
            foreach (Vector3 corner in corners)
            {
                float[] cornerFloat = new float[3];
                cornerFloat[0] = corner.x;
                cornerFloat[1] = corner.y;
                cornerFloat[2] = corner.z;
                jsonString += JsonUtility.ToJson(new JsonUnityPoint
                    {label = plane.name.Split(" ")[0], point = cornerFloat})+",";
            }
        }
        jsonString = jsonString.Substring(0,jsonString.Length-1) + "]";
        return jsonString;
    }
    
    private Vector3[] getRealCorners(GameObject plane)
    {
        Vector3[] vertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;
        
        for (int i = 0; i < vertices.Length; i++)
        { 
            vertices[i] = plane.transform.TransformPoint(vertices[i]);
        }
        
        vertices = vertices.Distinct().ToArray();
        Array.Sort(vertices,new Vector3Comparer());
        
        //since the planes of AdaptiveInput are scaled cubes we have 8 points which we need to merge to 4
        Vector3[] corners = new Vector3[4];
        if (vertices.Length == 8)
        {
            int count = 0;
            foreach (Vector3 vertex in vertices)
            {
                bool alreadySelected = false;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = corners[i];
                    if ((vertex - corner).magnitude < 0.002) //unless the plane is tiny we vertex and corner are the same corner of the planecube, just 0.001 appart
                    {
                        corners[i] = (vertex + corner) / 2;
                        alreadySelected = true;
                        break;
                    }
                }
                if (!alreadySelected)
                {
                    corners[count] = vertex;
                    count++;
                }
            }
            Assert.AreEqual(count,4);
            if (count != 4)
            {
                Debug.Log("corners might be detected wrong");
            }
        } else
        {
            corners = vertices; //if there is an error here the provided Mesh is not supported (only planes with 4 vertices or cubes from AdaptiveInput)
        }

        return corners;
    }
    
    //returns the most similar Environment to the Surfaces recognized by MRMapper
    public List<EnvModel> recognize()
    {
        //List<GameObject> planes = mrMapper.GetComponent<ReceivePlanes>().planes; only works with the real mrmapper
        List<GameObject> planes = new List<GameObject>();
        foreach (Transform mrPlaneTrans in mrMapper.transform)
        {
            GameObject mrChild = mrPlaneTrans.gameObject;
            if (mrChild.name.Contains("plane"))
            {
                planes.Add(mrChild);
            }
        }

        List<EnvModel> probableEnvs = new List<EnvModel>();
        int allowedNoise = 1;
        //assign a similarity score to each environment
        foreach (EnvModel envModel in envList)
        {
            Transform env = envModel.gameObject.transform;
            bool probable = true;
            foreach (String label in Labels.classes)
            {
                int nrMapper = 0;
                foreach (GameObject plane in planes)
                {
                    nrMapper += plane.name.Contains(label) ? 1 : 0;
                }

                int nrEnv = 0;
                foreach (ContainerModel model in envModel.containers)
                {
                    nrEnv += model.gameObject.name.Contains(label) ? 1 : 0;
                }

                if (Math.Abs(nrMapper - nrEnv) > allowedNoise)
                {
                    probable = false;
                }
            }

            if (probable)
            {
                probableEnvs.Add(envModel);
            }
        }
        return probableEnvs;
    }

    //returns input number if both inputs are the same. If they are unequal it deducts the absolute difference between the two with a minimum of 0
    private int symmDiff(int soll, int ist)
    {
        return Math.Max(0, soll - Math.Abs(soll - ist));
    }

    public void centerChildrenOnPoint(Transform parent, Vector3 center)
    {
        List<Transform> children = parent.Cast<Transform>().ToList();
        foreach (Transform child in children)
        {
            child.parent = null;
        }
        parent.position = center;
        foreach (Transform child in children)
        {
            child.parent = parent;
        }
    }

    public IEnumerator ExecuteOptimization(List<EnvModel> probableEnvs)
    {
        //get Planes form MrMapper
        //List<GameObject> mrPlanes = mrMapper.GetComponent<ReceivePlanes>().planes; only works with the real mrmapper
        List<GameObject> mrPlanes = new List<GameObject>();
        foreach (Transform mrPlaneTrans in mrMapper.transform)
        {
            GameObject mrChild = mrPlaneTrans.gameObject;
            if (mrChild.name.Contains("plane"))
            {
                mrPlanes.Add(mrChild);
            }
        }

        String jsonMr = jsonifyPlanes(mrPlanes, true);

        float wait = 0.1f;
        int timeout = 50;
        OptimizationResult bestResult = new OptimizationResult{quat = new float[]{0.0f,0.0f,0.0f,1.0f},error = float.MaxValue,completed = false};
        GameObject bestEnvObject = null;

        foreach (EnvModel env in probableEnvs)
        {
            //get Planes of the Envirnment
            List<GameObject> envPlanes = new List<GameObject>();
            foreach (ContainerModel container in env.containers)
            {
                envPlanes.Add(container.gameObject);
            }
            GameObject envObject = env.gameObject;
            
            //RestClient.Get("http://127.0.0.1:5005/result").Then(response => Debug.Log("Get: " + response.Text)); //debug
            Debug.Log(env.gameObject.name);
            String restMessage = "{" + jsonifyPlanes(envPlanes, false) + "," + jsonMr + "}";
            RestClient.Put("http://127.0.0.1:5005/result",restMessage).Then(response => Debug.Log("Put: " + response.Text));
            
            OptimizationResult result = new OptimizationResult{quat = new float[]{0.0f,0.0f,0.0f,1.0f},error = float.MaxValue,completed = false};
            int count = 0;
            while (!result.completed&&count<timeout)
            {
                RestClient.Get("http://127.0.0.1:5005/result").Then(response =>
                    result = JsonUtility.FromJson<OptimizationResult>(response.Text));
                count++;
                yield return new WaitForSeconds(wait);
            }
            
            if (!result.completed)
            { 
                Debug.Log("Optimization timed out after 10 seconds");
            } else if (result.error < bestResult.error)
            {
                bestResult = result;
                bestEnvObject = envObject;
            }
            RestClient.Put("http://127.0.0.1:5005/reset", "{}");
        }
        
        
        
        
        Quaternion optimizedRot = new Quaternion(bestResult.quat[0], bestResult.quat[1], bestResult.quat[2], bestResult.quat[3]);
        Vector3 envCenterPoint = new Vector3(bestResult.envCenter[0], bestResult.envCenter[1], bestResult.envCenter[2]);
        Vector3 mrCenterPoint = new Vector3(bestResult.mrCenter[0], bestResult.mrCenter[1], bestResult.mrCenter[2]);
        centerChildrenOnPoint(bestEnvObject.transform, envCenterPoint);
        bestEnvObject.transform.localRotation = optimizedRot * bestEnvObject.transform.localRotation;
        
        if (!bestEnvObject.activeSelf)
        {
            bestEnvObject.SetActive(true);
        }
        bestEnvObject.transform.position = mrCenterPoint;
    }
    
    [Serializable]
    private struct OptimizationResult
    {
        public float[] quat;
        public float error;
        public bool completed;
        public float[] envCenter;
        public float[] mrCenter;
    }
    
    [Serializable]
    private struct JsonUnityPoint
    {
        public String label;
        public float[] point;
    }
}
