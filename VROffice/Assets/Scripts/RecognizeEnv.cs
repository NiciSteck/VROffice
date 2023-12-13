using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using UnityEngine;
using UnityEngine.Assertions;

public class RecognizeEnv : MonoBehaviour
{
    private List<EnvModel> envList = new List<EnvModel>();

    [SerializeField]
    private GameObject mrMapper;

    [SerializeField] private bool align = false;

    [SerializeField] 
    private GameObject calibrator;

    [SerializeField]
    private GameObject debugPlane;
    
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
            GameObject env = recognize().gameObject;
            if (!env.activeSelf)
            {
                env.SetActive(true);
            }
            GameObject mainPlane = null;
            
            //get the assigned main plane of the environment to calibrate with it
            foreach (Transform planeObject in env.transform)
            {
                if (planeObject.gameObject.name.Contains("main"))
                {
                    mainPlane = planeObject.GetChild(0).gameObject;
                }
            }
            
            GameObject targetPlane = debugPlane;
            //get the target plane from MRMapper
            foreach (Transform mrPlaneTrans in mrMapper.transform)
            {
                GameObject mrPlane = mrPlaneTrans.gameObject;
                String mainName = mainPlane.transform.parent.gameObject.name.Split()[0];
                if (mrPlane.name.Contains(mainName))
                {
                    targetPlane = mrPlane;
                    break;
                }
            }
            
            Vector3 shouldPos = targetPlane.transform.position;
            Quaternion shouldRot = getRealRotation(targetPlane);
            
            Quaternion isRot = getRealRotation(mainPlane);
            Quaternion rotDiff = shouldRot * Quaternion.Inverse(isRot);
            env.transform.rotation = rotDiff * env.transform.rotation;
            
            Vector3 isPos = mainPlane.transform.position;
            env.transform.position += shouldPos - isPos;

            //maybeTODO keep env in upright position

            //RestClient.Put("127.0.0.1:5005/result",);

            //calibrator.GetComponent<MRCalibration>().Calibrate(getRealCorners(targetPlane)); //only works correctly if the vertices are passed in the same order as when virtual reference was created
        }
        align = false;
    }
    
    //finds the Rotation of a plane if it had the coordinate system up = surface normal and forward = long edge.
    //(Only gives the right result if the same corner is closest to the camera)
    private Quaternion getRealRotation(GameObject plane)
    {
        Vector3[] vertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;

        Vector3[] corners = getRealCorners(plane);
        
        Vector3 cornersMean = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
        
        Vector3 shortSide = getShortSide(corners);
        Vector3 longSide = getLongSide(corners);

        Vector3 normal = Vector3.Cross(longSide,shortSide);
        return Quaternion.LookRotation(longSide,normal);
        
    }
    
    /// <param name="corners"> the corners of a plane sorted by the Vector3Comparer </param>
    private Vector3 getShortSide(Vector3[] corners)
    {
        Vector3 zero = corners[0];
        Vector3[] fromZero = { corners[0] - corners[1],corners[0] - corners[2],corners[0] - corners[3]};
        Vector3 shortest = fromZero[0];
        foreach (Vector3 curr in fromZero)
        {
            if (curr.magnitude < shortest.magnitude)
            {
                shortest = curr;
            }
        }
        return shortest;
    }
    
    /// <param name="corners"> the corners of a plane sorted by the Vector3Comparer </param>
    private Vector3 getLongSide(Vector3[] corners)
    {
        Vector3 zero = corners[0];
        Vector3[] fromZero = { corners[0] - corners[1],corners[0] - corners[2],corners[0] - corners[3]};
        Vector3 middle = fromZero[0];
        for (int i = 0; i < 3; i++)
        {
            if (fromZero[i].magnitude > fromZero[(i + 1) % 3].magnitude &&
                fromZero[i].magnitude < fromZero[(i + 2) % 3].magnitude
                ||
                fromZero[i].magnitude < fromZero[(i + 1) % 3].magnitude &&
                fromZero[i].magnitude > fromZero[(i + 2) % 3].magnitude)
            {
                middle = fromZero[i];
            }
        }
        return middle;
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
        List<GameObject> planes = mrMapper.GetComponent<ReceivePlanes>().planes;
        List<GameObject> objects = mrMapper.GetComponent<ReceiveObjects>().icpObjects;
        

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
    
    [Serializable]
    private struct JsonUnityPoint
    {
        public String label;
        public float[] point;
    }
}
