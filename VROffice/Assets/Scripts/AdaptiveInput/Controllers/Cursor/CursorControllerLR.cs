using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControllerLR : MonoBehaviour
{
    [SerializeField]
    private LineRenderer m_lr; 

    [Header("Offsets")]
    [SerializeField]
    private float m_startOffset;
    [SerializeField]
    private float m_endOffset; 

    [Header("Color Settings")]
    [SerializeField]
    private Color m_color;
    [SerializeField]
    private Color m_pinchingColor;
    // Transparent color 
    private Color m_transparentColor = new Color(1f, 1f, 1f, 0f);
    
    public void setDirection(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        m_lr.SetPosition(0, start + m_startOffset*direction);
        m_lr.SetPosition(1, end);
    }

    public void setState(bool pinching)
    {
        Gradient gradient = new Gradient();
        Color endColor;
        if (pinching)
            endColor = m_pinchingColor;
        else
            endColor = m_color;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(m_transparentColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(endColor.a, 1.0f) });
        m_lr.colorGradient = gradient;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_lr != null)
            m_lr = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
