using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * This button shows the user the previous saved Environment
 */

public class PrevButton : MenuButton
{
    public ManualOldEnvButton oldButton;
    public NextButton nextButton;
    public Coroutine activeSpinRoutine;
    public override void attach(Controller controller)
    {
        List<Transform> envs = oldButton.environmentsTransforms;
        Transform oldFirst = envs.First();
        oldFirst.gameObject.SetActive(false);
        StopCoroutine(activeSpinRoutine);
        oldFirst.localScale = Vector3.one;
        
        Transform newFirst = envs.Last();
        envs.Remove(newFirst);
        envs.Insert(0,newFirst);
        newFirst.position = transform.position + Vector3.ProjectOnPlane(transform.forward, Vector3.up) * 0.5f +
                            Vector3.ProjectOnPlane(transform.right, Vector3.up) * 0.08f;
        newFirst.localScale = Vector3.one * 0.6f;
        newFirst.gameObject.SetActive(true);
        activeSpinRoutine = StartCoroutine(Spin(newFirst));
        nextButton.activeSpinRoutine = activeSpinRoutine;
    }

    IEnumerator Spin(Transform currentEnv)
    {
        while (true)
        {
            currentEnv.Rotate(Vector3.up,0.2f);
            yield return null;
        }
    }
}