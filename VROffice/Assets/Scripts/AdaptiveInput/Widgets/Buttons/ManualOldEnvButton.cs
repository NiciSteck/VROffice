using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ManualOldEnvButton : MenuButton
{
    public XRManager xrManager;
    public List<GameObject> otherButtons;
    private bool pressed;
    
    [Header("Icon")]
    [SerializeField]
    private MeshRenderer m_icon;
    [SerializeField]
    private Material m_oldIcon;
    [SerializeField]
    private Material m_backIcon;
    public override void attach(Controller controller)
    {
        if (pressed)
        {
            m_icon.material = m_oldIcon;

            foreach (GameObject button in otherButtons)
            {
                button.SetActive(true);
            }
            pressed = false;

        }
        else
        {
            m_icon.material = m_backIcon;

            foreach (GameObject button in otherButtons)
            {
                button.SetActive(false);
            }
            pressed = true;
        }
        
    }

    private void Start()
    {
        
    }
}
