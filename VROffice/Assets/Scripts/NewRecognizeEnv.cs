using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using UnityEngine;
using UnityEngine.Assertions;

public class NewRecognizeEnv : MonoBehaviour
{
    private List<EnvModel> envList = new List<EnvModel>();

    [SerializeField]
    private GameObject mrMapper;

    [SerializeField] private bool align = false;
    
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
        if (align)
        {
            EnvModel env = recognize();
            
            //get Planes of the Envirnment
            List<GameObject> envPlanes = new List<GameObject>();
            foreach (ContainerModel container in env.containers)
            {
                envPlanes.Add(container.gameObject);
            }

            //get Planes form MrMapper
            //List<GameObject> mrPlanes = mrMapper.GetComponent<ReceivePlanes>().planes;
            
            List<GameObject> mrPlanes = new List<GameObject>();
            foreach (Transform mrPlaneTrans in mrMapper.transform)
            {
                GameObject mrChild = mrPlaneTrans.gameObject;
                if (mrChild.name.Contains("plane"))
                {
                    mrPlanes.Add(mrChild);
                }
            }

            RestClient.Get("http://127.0.0.1:5005/result").Then(response => Debug.Log("Get: " + response.Text));
            
            String restMessage = "{" + jsonifyPlanes(envPlanes, false) + "," + jsonifyPlanes(mrPlanes, true) + "}";
            RestClient.Put("http://127.0.0.1:5005/result",restMessage).Then(response => Debug.Log("Put: " + response.Text));

            Vector3 centroidMr = Vector3.zero;
            foreach (GameObject plane in mrPlanes)
            {
                centroidMr += plane.transform.position;
            }

            centroidMr /= mrPlanes.Count;
            Debug.Log(env.gameObject.transform.position);
            Debug.Log(centroidMr);
            env.gameObject.transform.position = centroidMr;
            
            Debug.Log(restMessage);
            align = false;
            StartCoroutine(WaitForOptimization(env));
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
    public EnvModel recognize()
    {
        //List<GameObject> planes = mrMapper.GetComponent<ReceivePlanes>().planes;
        List<GameObject> planes = new List<GameObject>();
        foreach (Transform mrPlaneTrans in mrMapper.transform)
        {
            GameObject mrChild = mrPlaneTrans.gameObject;
            if (mrChild.name.Contains("plane"))
            {
                planes.Add(mrChild);
            }
        }
        

        //assign a similarity score to each environment
        foreach (EnvModel envModel in envList)
        {
            Transform env = envModel.gameObject.transform;
            envModel.similarity += symmDiff(planes.Count,env.childCount); //increases similarity by the symmDiff of the number of planes

            //increases similarity by symmDiff of the number of planes that have the same label
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

                envModel.similarity += symmDiff(nrMapper, nrEnv);
            }
            
            //surface area
            
            //angles/normals
            
            //distance between planes
        }
        
        envList.Sort();
        return envList[0];
    }

    //returns input number if both inputs are the same. If they are unequal it deducts the absolute difference between the two with a minimum of 0
    private int symmDiff(int soll, int ist)
    {
        return Math.Max(0, soll - Math.Abs(soll - ist));
    }

    IEnumerator WaitForOptimization(EnvModel env)
    {
        float wait = 0.1f;
        OptimizationResult result = new OptimizationResult{quat = new float[]{0.0f,0.0f,0.0f,1.0f},error = 0.0f,completed = false};
        yield return new WaitForSeconds(wait);
        int count = 0;
        while (!result.completed&&count<10)
        {
            RestClient.Get("http://127.0.0.1:5005/result").Then(response =>
                result = JsonUtility.FromJson<OptimizationResult>(response.Text));
            count++;
            yield return new WaitForSeconds(wait);
        }
        GameObject envObject = env.gameObject;
        
        Quaternion optimizedRot = new Quaternion(result.quat[0], result.quat[1], result.quat[2], result.quat[3]);
        envObject.transform.localRotation = optimizedRot * envObject.transform.localRotation;
        
        if (!envObject.activeSelf)
        {
            envObject.SetActive(true);
        }

        if (count == 10 && !result.completed)
        {
            Debug.Log("Optimization timed out after 10 seconds");
        }
        RestClient.Put("http://127.0.0.1:5005/reset", "{}");
    }

    [Serializable]
    private struct OptimizationResult
    {
        public float[] quat;
        public float error;
        public bool completed;
    }
    
    [Serializable]
    private struct JsonUnityPoint
    {
        public String label;
        public float[] point;
    }
}
