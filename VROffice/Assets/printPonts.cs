using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class printPonts : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<String> log = new List<string>();
        foreach (Transform planeObject in transform)
        {
            Vector3[] corners = getRealCorners(planeObject.gameObject);
            foreach (Vector3 corner in corners)
            {
                log.Add(corner[0]+" "+corner[1]+" "+corner[2]);
            }
           
        }
        File.WriteAllLines(@"D:\BachelorThesis\VROffice\OptimizerTesting\Pointfiles\EnvPlanes018090.txt",log);
    }

    // Update is called once per frame
    void Update()
    {
        
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

        return corners;
    }
}
