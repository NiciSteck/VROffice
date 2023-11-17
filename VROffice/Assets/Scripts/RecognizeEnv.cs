using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecognizeEnv : MonoBehaviour
{
    private List<EnvModel> envList = new List<EnvModel>();

    [SerializeField]
    private GameObject mrMapper;

    [SerializeField] private bool align = false;

    [SerializeField] 
    private GameObject calibratior;

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
            //TODO get the detected plane
            
            Vector3 shouldPos = targetPlane.transform.position;
            Quaternion shouldRot = getRealRotation(targetPlane);
            
            Quaternion isRot = getRealRotation(mainPlane);
            Quaternion rotDiff = shouldRot * Quaternion.Inverse(isRot);
            transform.rotation = rotDiff * transform.rotation;
            
            Vector3 isPos = mainPlane.transform.position;
            transform.position += shouldPos - isPos;

        }
        align = false;
    }
    
    //finds the Rotation of a plane if it had the coordinate system up = surface normal and forward = long edge
    private Quaternion getRealRotation(GameObject plane)
    {
        Vector3[] vertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;
        
        for (int i = 0; i < vertices.Length; i++)
        { 
            vertices[i] = plane.transform.TransformPoint(vertices[i]);
        }
        
        vertices = vertices.Distinct().ToArray();
        Array.Sort(vertices,new Vector3Comparer());
        
        //since the planes of AdaptiveInput are scaled cubes we have 8 points which we need to merge to 4 (hopefully they are sorted correctly)
        Vector3[] corners = new Vector3[4];
        if (vertices.Length == 8)
        {
            corners[0] = (vertices[0] + vertices[1])/2;
            corners[1] = (vertices[2] + vertices[3])/2;
            corners[2] = (vertices[4] + vertices[5])/2;
            corners[3] = (vertices[6] + vertices[7])/2;
        } else
        {
            corners = vertices; //if there is an error here the provided Mesh is not supported (only planes with 4 vertices or cubes from AdaptiveInput)
        }
        
        
        Vector3 cornersMean = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
        Vector3 ba = corners[0] - corners[2];
        Vector3 baCenter = (corners[2] + corners[0]) / 2;
        Vector3 bc = corners[3] - corners[2];
        Vector3 bcCenter = (corners[2] + corners[3]) / 2;
        
        if (ba.magnitude > bc.magnitude)
        {
            Vector3 normal = Vector3.Cross(baCenter - cornersMean, bcCenter - cornersMean);
            return Quaternion.LookRotation(ba,normal);
        }
        else
        {
            Vector3 normal = Vector3.Cross(bcCenter - cornersMean,baCenter - cornersMean);
            return Quaternion.LookRotation(bc, normal);
        }
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
}
