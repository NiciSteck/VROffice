using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class NextButton : MenuButton
{
    public ManualOldEnvButton oldButton;
    public override void attach(Controller controller)
    {
        List<Transform> envs = oldButton.environmentsTransforms;
        Transform first = envs.First();
        first.gameObject.SetActive(false);
        envs.Remove(first);
        envs.Add(first);
        envs.First().gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        oldButton.environmentsTransforms.First().gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        oldButton.environmentsTransforms.First().gameObject.SetActive(false);
    }
}