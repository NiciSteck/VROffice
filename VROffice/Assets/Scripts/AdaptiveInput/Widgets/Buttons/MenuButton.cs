using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : WidgetApplication
{
    [Header("Window")]
    [SerializeField]
    protected Transform m_attachedWindow;

    [Header("Placement")]
    public Vector2 m_offset;
    [SerializeField]
    protected Vector2 m_offsetScaled; 

    private void updatePlacement()
    {
        Vector3 windowPos = m_attachedWindow.position;
        Vector3 windowScale = m_attachedWindow.localScale;
        Vector3 windowForward = m_attachedWindow.forward;
        Vector3 windowUp = m_attachedWindow.up;
        Vector3 windowRight = m_attachedWindow.right;

        this.transform.position = windowPos +
            -((m_offsetScaled.y * windowScale.y) + m_offset.y) * windowUp +
            ((m_offsetScaled.x * windowScale.x) + m_offset.x) * windowRight;

        this.transform.rotation = Quaternion.LookRotation(windowForward, windowUp);
    }
    

    // Update is called once per frame
    protected void Update()
    {
        updatePlacement();
    }
}
