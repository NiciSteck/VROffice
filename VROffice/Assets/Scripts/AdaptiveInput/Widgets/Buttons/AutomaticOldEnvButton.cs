using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticOldEnvButton : MenuButton
{
    public List<GameObject> otherButtons;
    public ConfirmButton confirmButton;
    public BackButton backButton;

    public override void attach(Controller controller)
    {
        //maybe start MRMapper here?
        XRManager.Manager.setPassthrough();
        foreach (GameObject button in otherButtons)
        {
            button.SetActive(false);
        }
        gameObject.SetActive(false);
        
        confirmButton.caller = ConfirmButton.Caller.AutomaticOld;
        confirmButton.gameObject.SetActive(true);
        backButton.caller = BackButton.Caller.AutomaticOld;
        backButton.gameObject.SetActive(true);
    }
}