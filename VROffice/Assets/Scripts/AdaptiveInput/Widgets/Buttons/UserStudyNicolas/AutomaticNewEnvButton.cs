using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This button starts the Autonatic Definition of an Environment
 */
public class AutomaticNewEnvButton : MenuButton
{
    public List<GameObject> otherButtons;
    public ConfirmButton confirmButton;
    public BackButton backButton;

    public override void attach(Controller controller)
    {
        XRManager.Manager.setPassthrough();
        foreach (GameObject button in otherButtons)
        {
            button.SetActive(false);
        }
        gameObject.SetActive(false);
        
        confirmButton.caller = ConfirmButton.Caller.AutomaticNew;
        confirmButton.gameObject.SetActive(true);
        backButton.caller = BackButton.Caller.AutomaticNew;
        backButton.gameObject.SetActive(true);
        
        StudyTimer.Timer.startTimer("AutomaticNew");
    }
}