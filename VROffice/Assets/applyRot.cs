using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class applyRot : MonoBehaviour
{
    [SerializeField] private GameObject reference;
    // Start is called before the first frame update
    void Start()
    {
        int nrPoints = transform.childCount*4;
        Vector3 center = Vector3.zero;
        foreach (Transform planeObject in transform)
        {
            Vector3[] corners = getRealCorners(planeObject.gameObject);
            foreach (Vector3 corner in corners)
            {
                center += corner;
            }
           
        }
        Debug.Log(center);
        center /= nrPoints;
        Debug.Log(center);
        Vector3 centerRef = Vector3.zero;
        foreach (Transform planeObject in reference.transform)
        {
            Vector3[] corners = getRealCorners(planeObject.gameObject);
            foreach (Vector3 corner in corners)
            {
                centerRef += corner;
            }
           
        }
        centerRef /= nrPoints;

        transform.position += centerRef - center;
        transform.rotation = new Quaternion(-0.007934051030311376f, -0.06139511392100314f, 0.037950793445485825f, 0.9790127450174141f);
        //transform.rotation = new Quaternion(0.25323245f ,0.76204822f, 1.39150149f, 1.12042755f);
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
