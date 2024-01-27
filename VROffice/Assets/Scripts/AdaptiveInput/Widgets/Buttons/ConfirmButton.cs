using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmButton : MenuButton
{
    public enum Caller
    {
        ManualNew,
        ManualOld,
        AutomaticNew,
        AutomaticOld
    }

    public Caller caller;

    public GameObject environments;
    public Transform menuHandle;
    
    public override void attach(Controller controller)
    {
        switch (caller)
        {
            case Caller.ManualNew:
                PhysicalEnvironmentManager.Environment.m_definingNewElements = false;
                environments.GetComponent<SerializeEnvs>().save = true;
                break;
            case Caller.ManualOld:
                //maybe turn off calibration?
                break;
            case Caller.AutomaticNew:
                environments.GetComponent<BuildEnv>().build = true;
                break;
            case Caller.AutomaticOld:
                environments.GetComponent<NewRecognizeEnv>().align = true;
                break;
        }
        XRManager.Manager.setImmersive();
        menuHandle.gameObject.SetActive(true);
        Transform user = Camera.main.transform;
        menuHandle.position = user.position + user.forward * 0.5f + user.right * 0.2f;
        menuHandle.rotation = Quaternion.LookRotation(menuHandle.position - user.position);
        transform.parent.gameObject.SetActive(false);
    }
}