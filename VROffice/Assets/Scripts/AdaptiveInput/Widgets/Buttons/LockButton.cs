using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockButton : MenuButton
{
    [Header("Window")]
    [SerializeField]
    private Widget m_attachedWidget; 

    [Header("Icon")]
    [SerializeField]
    private MeshRenderer m_icon;
    [SerializeField]
    private Material m_lockedIcon;
    [SerializeField]
    private Material m_unlockedIcon;

    private bool m_locked = false;

    public void setLocked(bool locked)
    {
        if (locked) {
            m_icon.material = m_unlockedIcon;
            m_attachedWidget.updateState(Widget.State.APP);
        } else {
            m_icon.material = m_lockedIcon;
            m_attachedWidget.updateState(Widget.State.TRANSFORM);
        }
    }

    public override void attach(Controller controller)
    {
        m_locked = !m_locked;
        setLocked(m_locked);
    }

    public void setAttachedWidget(Widget widget, Transform window)
    {
        m_attachedWidget = widget;
        m_attachedWindow = window; 
    }

    // Start is called before the first frame update
    void Start()
    {
        m_locked = false; 
    }
}
