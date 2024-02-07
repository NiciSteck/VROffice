using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MenuButton
{
    public enum Caller
    {
        ManualNew,
        ManualOld,
        AutomaticNew,
        AutomaticOld
    }

    public Caller caller;

    public List<GameObject> otherButtons;
    public ConfirmButton confirmButton;
    public ConfirmSelectionButton confirmSelectionButton;
    public GameObject environments;

    [Header("Selection")] 
    public PrevButton prevButton;
    public NextButton nextButton;
    
    public override void attach(Controller controller)
    {
        switch (caller)
        {
            case Caller.ManualNew:
                PhysicalEnvironmentManager.Environment.m_definingNewElements = false;
                PhysicalEnvironmentManager.Environment.clearEnv();
                Destroy(PhysicalEnvironmentManager.Environment.Env.gameObject);
                break;
            case Caller.ManualOld:
                prevButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                confirmSelectionButton.gameObject.SetActive(false);
                break;
            case Caller.AutomaticNew:
                break;
            case Caller.AutomaticOld:
                break;
        }
        XRManager.Manager.setImmersive();
        foreach (GameObject button in otherButtons)
        {
            button.SetActive(true);
        }
        confirmButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
