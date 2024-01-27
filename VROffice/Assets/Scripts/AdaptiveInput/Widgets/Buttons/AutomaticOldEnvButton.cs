using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticOldEnvButton : MenuButton
{
    public List<GameObject> otherButtons;
    public ConfirmButton confirmButton;
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
            XRManager.Manager.setImmersive();
            foreach (GameObject button in otherButtons)
            {
                button.SetActive(true);
            }
            confirmButton.gameObject.SetActive(false);
            pressed = false;
        }
        else
        {
            //maybe start MRMapper here?
            m_icon.material = m_backIcon;
            XRManager.Manager.setPassthrough();
            foreach (GameObject button in otherButtons)
            {
                button.SetActive(false);
            }
            confirmButton.m_offset.y = m_offset.y + 0.09f;
            confirmButton.caller = ConfirmButton.Caller.AutomaticOld;
            confirmButton.gameObject.SetActive(true);
            
            pressed = true;
        }
        
    }
}
