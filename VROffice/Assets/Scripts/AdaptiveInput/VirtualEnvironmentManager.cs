using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualEnvironmentManager : MonoBehaviour
{
    public static VirtualEnvironmentManager Environment;
    
    [SerializeField]
    private float m_surfaceOffset;
    public float SurfaceOffset
    {
        get { return m_surfaceOffset; }
    }
    
    [SerializeField]
    private Transform m_elementsContainer;
    public Transform ElementsContainer
    {
        get { return m_elementsContainer; }
        set { m_elementsContainer = value; }
    }

    private void Start()
    {
        Environment = this;
    }
}
