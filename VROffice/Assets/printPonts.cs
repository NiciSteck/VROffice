using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

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
        File.WriteAllLines(@"D:\BachelorThesis\VROffice\OptimizerTesting\Pointfiles\undefined.txt",log);
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
}
