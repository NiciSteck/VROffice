using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControllerPointer : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer m_mr;

    [Header("Scale")]
    [SerializeField]
    private Vector3 m_scale;
    [SerializeField]
    private Vector3 m_pinchingScale;
    [SerializeField]
    private Vector3 m_pinchDownScale;

    [Header("Color")]
    [SerializeField]
    private Color m_color;
    [SerializeField]
    private Color m_pinchDownColor;

    public void setState(float val)
    {
        Vector3 scale;
        Color color; 
        if (val >= 1)
        {
            scale = m_pinchDownScale;
            color = m_pinchDownColor;
        } else
        {
            scale = Vector3.Lerp(m_scale, m_pinchingScale, val);
            color = m_color;
        }
        this.transform.localScale = scale;
        Material mat = m_mr.material;
        mat.color = color;
        m_mr.material = mat;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_mr != null)
            m_mr = this.GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
