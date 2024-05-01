using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script aligns Elements to the Environment to demonstrating the role of InteractionAdapt
 */

public class AllocateElements : MonoBehaviour
{
    public bool allocate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (allocate)
        {
            cheapAllocation();
            allocate = false;
        }
    }

    private void cheapAllocation()
    {
        List<ContainerModel> containersCopy =
            new List<ContainerModel>(PhysicalEnvironmentManager.Environment.Containers);
        foreach (ElementModel elementModel in VirtualEnvironmentManager.Environment.Elements)
        {
            Transform currElement = elementModel.transform;
            foreach (ContainerModel container in containersCopy)
            {
                Transform surface = container.transform;
                if (elementModel.GetComponent<KeyboardInput>() != null)
                {
                    if (surface.name.Contains("keyboard"))
                    {
                        elementModel.GetComponent<Widget>().place(surface.position,currElement.rotation, true);
                        containersCopy.Remove(container);
                        break;
                    }
                    
                }
                else
                {
                    Vector3 surfaceScale = surface.GetChild(0).lossyScale;
                    Vector3 browserScale = currElement.GetChild(0).lossyScale;
                    Debug.Log(surface.name + " " + currElement.name);
                    if (isApproximatelyEqual(surfaceScale.x*surfaceScale.y, browserScale.x*browserScale.y, 0.5f))
                    {
                        elementModel.GetComponent<Widget>().place(surface.position,currElement.rotation, true);
                        containersCopy.Remove(container);
                        break;
                    }
                }
                

            }
        }
    }

    private bool isApproximatelyEqual(float a, float b, float scale)
    {
        return Math.Abs(a - b) < scale * Math.Max(a, b);
    }
}
