using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerModel : MonoBehaviour
{
    [SerializeField]
    [Range(2, 3)]
    private int m_dimension = 2;
    public int Dimension
    {
        get { return m_dimension; }
        set { m_dimension = value; }
    }

    private int m_id;
    public int ID
    {
        get { return m_id; }
    }

    private Vector3 m_scale; 
    public Vector3 Scale
    {
        get { return m_scale; }
    }

    public void init(int id, Vector3 scale)
    {
        m_id = id;
        m_scale = scale; 
    }
}
