using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class ManualOldEnvButton : MenuButton
{
    public Transform environmentsReference;
    public List<GameObject> otherButtons;
    public ConfirmSelectionButton confirmSelectionButton;
    public BackButton backButton;
    public NextButton nextButton;
    public PrevButton prevButton;
    public List<Transform> environmentsTransforms;
    public override void attach(Controller controller)
    {
        //TODO figure out a way to selcet from envs
        foreach (GameObject button in otherButtons)
        {
            button.SetActive(false);
        }
        gameObject.SetActive(false);

        confirmSelectionButton.step = ConfirmSelectionButton.Step.SelectEnv;
        confirmSelectionButton.gameObject.SetActive(true);
        backButton.caller = BackButton.Caller.ManualOld;
        backButton.gameObject.SetActive(true);
        
        nextButton.gameObject.SetActive(true);
        prevButton.gameObject.SetActive(true);
        
        StudyTimer.Timer.startTimer("ManualOld");
    }

    IEnumerator Start()
    { 
        yield return new WaitForSeconds(0.5f);
        foreach (Transform env in environmentsReference)
        {
            environmentsTransforms.Add(env);
        }
    }
}