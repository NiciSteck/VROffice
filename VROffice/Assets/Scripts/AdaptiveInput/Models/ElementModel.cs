using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementModel : MonoBehaviour
{
    [Range(2, 3)]
    private int m_dimension = 2;
    public int Dimension
    {
        get { return m_dimension; }
    }

    private int m_id;
    public int ID
    {
        get { return m_id; }
    }

    private float m_priority;
    public float Priority
    {
        get { return m_priority; }
    }

    [SerializeField]
    [Range(0,1)]
    private float m_interactionFrequency;
    public float InteractionFrequency
    {
        get { return m_interactionFrequency; }
    }

    private Vector3 m_scale;
    public Vector3 Scale
    {
        get { return m_scale; }
    }
    private Vector3Int m_voxelNum;
    public Vector3Int VoxelNum
    {
        get { return m_voxelNum; }
        set { m_voxelNum = value; }
    }

    private Vector3 m_lastPosition;
    public Vector3 lastPosition
    {
        get { return m_lastPosition; }
    }

    private Vector3 m_lastRelativePosition;
    public Vector3 lastRelativePosition
    {
        get { return m_lastRelativePosition; }
    }

    private Vector3 m_lastRelativeForward;
    public Vector3 lastRelativeForward
    {
        get { return m_lastRelativeForward; }
    }

    private Vector3 m_lastRelativeUp;
    public Vector3 lastRelativeUp
    {
        get { return m_lastRelativeUp; }
    }

    private bool m_attachedToSurface;
    public bool AttachedToSurface
    {
        get { return m_attachedToSurface; }
    }

    private Controller.Type m_lastController;
    public Controller.Type lastController {
        get { return m_lastController; }
    }

    // Position at optimization
    /*
    private Vector3 m_position;
    public Vector3 Position
    {
        get { return m_position; }
    }
    private Vector3 m_userPosition; 
    public Vector3 UserPosition
    {
        get { return m_userPosition; }
    }

    // Optimized position
    private bool m_optimized;
    public bool isOptimizedSet
    {
        get { return m_optimized; }
    }
    private Vector3 m_positionOptimized;
    public Vector3 PositionOptimized
    {
        get { return m_positionOptimized; }
    }
    private Vector3 m_userPositionOptimized;
    public Vector3 UserPositionOptimized
    {
        get { return m_userPositionOptimized; }
    }

    // User defined placement
    private bool m_userDefined = false; 
    public bool isUserDefinedSet
    {
        get { return m_userDefined; }
    }
    private Vector3 m_positionUserDefined;
    public Vector3 PositionUserDefined
    {
        get { return m_positionUserDefined; }
    }
    private Vector3 m_userPositionUserDefined;
    public Vector3 UserPositionUserDefined
    {
        get { return m_userPositionUserDefined; }
    }

    public void setOptimizedPlacement(Vector3 position, Vector3 userPosition)
    {
        m_optimized = true; 
        m_positionOptimized = position;
        m_userPositionOptimized = userPosition;
    }

    public void setUserDefinedPlacement(Vector3 position, Vector3 userPosition)
    {
        m_userDefined = true;
        m_positionUserDefined = position;
        m_userPositionUserDefined = userPosition;
    }
    */

    public void init(int id, Vector3 position, Vector3 relativePosition, Vector3 relativeForward, Vector3 relativeUp, Vector3 scale, float priority, bool attachedToSurface, Controller.Type type)
    {
        m_id = id;
        m_lastPosition = position;
        m_lastRelativePosition = relativePosition;
        m_lastRelativeForward = relativeForward;
        m_lastRelativeUp = relativeUp;
        m_scale = scale;
        m_priority = priority;
        m_attachedToSurface = attachedToSurface;
        m_lastController = type; 
    }

    public void calcVoxelNum(Vector3 voxelScale)
    {
        m_voxelNum = new Vector3Int(
            Mathf.RoundToInt(m_scale.x / voxelScale.x),
            Mathf.RoundToInt(m_scale.y / voxelScale.y),
            Mathf.RoundToInt(m_scale.z / voxelScale.z));
        if (m_dimension == 2)
            m_voxelNum.z = 1;
    }

    public void calcVoxelNum(Vector3 voxelScale, float buffer)
    {
        m_voxelNum = new Vector3Int(
            Mathf.RoundToInt((m_scale.x + buffer) / voxelScale.x),
            Mathf.RoundToInt((m_scale.y + buffer) / voxelScale.y),
            Mathf.RoundToInt((m_scale.z + buffer) / voxelScale.z));
        if (m_dimension == 2)
            m_voxelNum.z = 1;
    }
}
