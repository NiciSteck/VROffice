using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Special confirm button used in the Manual User Study
 */

public class ConfirmSelectionButton : MenuButton
{
    
    public enum Step
    {
        SelectEnv,
        SelectRef
    }

    public Step step;
    public GameObject environments;
    public ConfirmButton confirmButton;
    public BackSelectionButton backSelectionButton;
    public BackButton backButton;
    
    [Header("Selection")] 
    public PrevButton prevButton;
    public NextButton nextButton;
    public override void attach(Controller controller)
    {
        switch (step)
        {
            case Step.SelectEnv:
                step = Step.SelectRef;
                backButton.gameObject.SetActive(false);
                backSelectionButton.step = BackSelectionButton.Step.SelectRef;
                backSelectionButton.gameObject.SetActive(true);
                prevButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                nextButton.oldButton.environmentsTransforms.First().gameObject.SetActive(true);
                foreach (ReferenceButton reference in environments.GetComponentsInChildren<ReferenceButton>(true))
                {
                    reference.gameObject.SetActive(true);
                }
                break;
            case Step.SelectRef:
                if (Calibration.Calibrator.virtualReference == null)
                {
                    Debug.Log("No VirtualReference for Calibration selected yet");
                    return;
                }
                else
                {
                    backSelectionButton.step = BackSelectionButton.Step.Calibrate;
                    foreach (ReferenceButton reference in environments.GetComponentsInChildren<ReferenceButton>(true))
                    {
                        reference.gameObject.SetActive(false);
                    }
                    Calibration.Calibrator.calibrating = true;
                    XRManager.Manager.setPassthrough();
                    confirmButton.caller = ConfirmButton.Caller.ManualOld;
                    confirmButton.gameObject.SetActive(true);
                }
                break;
        }
    }
}
