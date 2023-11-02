using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelModel : MonoBehaviour
{
    private int m_cId; 
    public int cId
    {
        get { return m_cId; }
    }
    private Vector3Int m_voxel;
    public Vector3Int voxel
    {
        get { return m_voxel; }
    }

    [Range(2,3)]
    private int m_dimension;
    public int Dimension
    {
        get { return m_dimension; }
        set { m_dimension = value; }
    }

    private Vector3 m_position;
    public Vector3 position
    {
        get { return m_position; }
    }
    private Vector3 m_relativePosition;
    public Vector3 relativePosition
    {
        get { return m_relativePosition; }
    }
    private Vector3 m_forward;
    public Vector3 forward
    {
        get { return m_forward; }
    }
    private Vector3 m_up;
    public Vector3 up
    {
        get { return m_up; }
    }

    private Renderer m_r; 

    public void init(int cId, int xIdx, int yIdx, int zIdx, 
        int dim, 
        Vector3 pos, Vector3 relativePosition, 
        Vector3 forward, Vector3 up,
        Renderer r)
    {
        m_cId = cId;
        m_voxel = new Vector3Int(xIdx, yIdx, zIdx);
        m_dimension = dim; 
        m_position = pos;
        m_relativePosition = relativePosition;
        m_forward = forward;
        m_up = up; 
        m_r = r;
        enableRenderer(false);
    }

    public void enableRenderer(bool enabled)
    {
        m_r.enabled = enabled; 
    }

    public void visualizeValue(float value)
    {
        enableRenderer(true);
        //Color c = Color.Lerp(Color.red, Color.blue, value);
        //c.a = 0.3f; 
        Color c = Color.red; 
        c.a = value; 
        m_r.material.color = c;
    }

    public new string ToString()
    {
        return m_cId + ", " + m_voxel.x + ", " + m_voxel.y + ", " + m_voxel.z;
    }
}
