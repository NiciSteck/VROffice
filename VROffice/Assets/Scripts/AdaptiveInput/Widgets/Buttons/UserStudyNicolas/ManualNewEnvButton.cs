using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * This button starts the Manual Definition of an Environment
 */

public class ManualNewEnvButton : MenuButton
{
    public List<GameObject> otherButtons;
    public ConfirmButton confirmButton;
    public BackButton backButton;

    public override void attach(Controller controller)
    {
        XRManager.Manager.setPassthrough();
        PhysicalEnvironmentManager.Environment.m_definingNewElements = true;

        foreach (GameObject button in otherButtons)
        {
            button.SetActive(false);
        }
        gameObject.SetActive(false);

        confirmButton.caller = ConfirmButton.Caller.ManualNew;
        confirmButton.gameObject.SetActive(true);
        backButton.caller = BackButton.Caller.ManualNew;
        backButton.gameObject.SetActive(true);
        
        StudyTimer.Timer.startTimer("ManualNew");
    }
}