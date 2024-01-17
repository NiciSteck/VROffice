using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControllerCursor : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer m_mr;

    [Header("Scale Settings")]
    [SerializeField]
    private float m_minScale;
    [SerializeField]
    private float m_scaleFactor;
    [SerializeField]
    private float m_startScaleDistance;
    private float m_distanceScaleFactor;

    [Header("Color Settings")]
    [SerializeField]
    private Color m_color;
    [SerializeField]
    private Color m_pinchingColor;

    public float scale
    {
        get { return m_minScale + m_scaleFactor * m_distanceScaleFactor; }
    }


    public void setState(bool pinching)
    {
        Color color;
        if (pinching)
            color = m_pinchingColor;
        else
            color = m_color;
        
        Material mat = m_mr.material;
        mat.color = color;
        m_mr.material = mat;
    }

    public void setScale(float distance)
    {
        m_distanceScaleFactor = Mathf.Max(0, distance - m_startScaleDistance);
        this.transform.localScale = (m_minScale + m_scaleFactor * m_distanceScaleFactor) * Vector3.one;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}