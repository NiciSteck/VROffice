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
            GameObject mainPlane = debugPlane;
            //get the assigned main plane of the environment to calibrate with it
            foreach (Transform planeObject in env.transform)
            {
                if (planeObject.gameObject.name.Contains("main"))
                {
                    mainPlane = planeObject.GetChild(0).gameObject;
                }
            }

            Vector3[] corners = mainPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Array.Sort(corners, new Vector3Comparer());
            
            //transform corners to WorldSpace
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = mainPlane.transform.TransformPoint(corners[i]);
            }
          
            
            calibratior.GetComponent<MRCalibration>().Calibrate(corners);


        }
        align = false;
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
