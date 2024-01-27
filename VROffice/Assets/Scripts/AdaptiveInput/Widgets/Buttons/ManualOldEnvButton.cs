using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ManualOldEnvButton : MenuButton
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
        //TODO figure out a way to selcet from envs
        if (pressed)
        {
            m_icon.material = m_oldIcon;

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

            foreach (GameObject button in otherButtons)
            {
                button.SetActive(false);
            }
            confirmButton.m_offset.y = m_offset.y + 0.09f;
            confirmButton.caller = ConfirmButton.Caller.ManualOld;
            confirmButton.gameObject.SetActive(true);
            
            pressed = true;
        }
        
    }

    private void Start()
    {
        
    }
}
