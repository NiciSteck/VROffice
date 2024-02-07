using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceButton : MenuButton
{
    private bool pressed;

    [Header("Icon")] 
    public MeshRenderer m_icon;
    public Material m_selectedIcon;
    public Material m_unselectedIcon;

    public override void attach(Controller controller)
    {
        Calibration.Calibrator.virtualReference = transform.parent.parent;
        Calibration.Calibrator.environment = transform.parent.parent.parent;
        foreach (GameObject reference in GameObject.FindGameObjectsWithTag("Reference"))
        {
            reference.transform.parent.GetComponent<ReferenceButton>().m_icon.material = m_unselectedIcon;
        }
        m_icon.material = m_selectedIcon;
    }
}