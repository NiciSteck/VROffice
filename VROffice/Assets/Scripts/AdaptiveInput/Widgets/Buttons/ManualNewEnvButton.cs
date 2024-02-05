using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ManualNewEnvButton : MenuButton
{
    public List<GameObject> otherButtons;
    public ConfirmButton confirmButton;
    private bool pressed;
    
    [Header("Icon")]
    [SerializeField]
    private MeshRenderer m_icon;
    [SerializeField]
    private Material m_newIcon;
    [SerializeField]
    private Material m_backIcon;
    public override void attach(Controller controller)
    {
        if (pressed)
        {
            m_icon.material = m_newIcon;
            XRManager.Manager.setImmersive();
            PhysicalEnvironmentManager.Environment.m_definingNewElements = false;
            PhysicalEnvironmentManager.Environment.clearEnv();
            Destroy(PhysicalEnvironmentManager.Environment.Env.gameObject);
            foreach (GameObject button in otherButtons)
            {
                button.SetActive(true);
            }
            confirmButton.gameObject.SetActive(false);
            pressed = false;

        }
        else
        {
            m_icon.material = m_backIcon;
            XRManager.Manager.setPassthrough();
            PhysicalEnvironmentManager.Environment.m_definingNewElements = true;
            foreach (GameObject button in otherButtons)
            {
                button.SetActive(false);
            }
            confirmButton.m_offset.y = m_offset.y + 0.09f;
            confirmButton.caller = ConfirmButton.Caller.ManualNew;
            confirmButton.gameObject.SetActive(true);
            
            pressed = true;
        }
        
    }

    private void Start()
    {
        
    }
}
