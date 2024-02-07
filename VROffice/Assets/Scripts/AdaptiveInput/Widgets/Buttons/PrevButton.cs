using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PrevButton : MenuButton
{
    public ManualOldEnvButton oldButton;
    public override void attach(Controller controller)
    {
        List<Transform> envs = oldButton.environmentsTransforms;
        envs.First().gameObject.SetActive(false);
        Transform last = envs.Last();
        envs.Remove(last);
        envs.Insert(0,last);
        last.gameObject.SetActive(true);
    }
}