using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackSelectionButton : MenuButton
{
    public enum Step
    {
        SelectRef,
        Calibrate
    }

    public Step step;

    public BackButton backButton;
    public ConfirmButton confirmButton;
    public ConfirmSelectionButton confirmSelectionButton;
    public GameObject environments;

    [Header("Selection")] 
    public PrevButton prevButton;
    public NextButton nextButton;
    
    public override void attach(Controller controller)
    {
        switch (step)
        {
            case Step.SelectRef:
                backButton.caller = BackButton.Caller.ManualOld;
                backButton.gameObject.SetActive(true);
                prevButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                foreach (ReferenceButton reference in environments.GetComponentsInChildren<ReferenceButton>(true))
                {
                    reference.gameObject.SetActive(false);
                }
                confirmSelectionButton.step = ConfirmSelectionButton.Step.SelectEnv;
                Calibration.Calibrator.virtualReference = null;
                gameObject.SetActive(false);
                break;
            case Step.Calibrate:
                Calibration.Calibrator.calibrating = false;
                confirmSelectionButton.step = ConfirmSelectionButton.Step.SelectRef;
                confirmButton.gameObject.SetActive(false);
                confirmSelectionButton.gameObject.SetActive(true);
                foreach (ReferenceButton reference in environments.GetComponentsInChildren<ReferenceButton>(true))
                {
                    reference.gameObject.SetActive(true);
                }
                XRManager.Manager.setImmersive();
                break;
        }
    }
}
