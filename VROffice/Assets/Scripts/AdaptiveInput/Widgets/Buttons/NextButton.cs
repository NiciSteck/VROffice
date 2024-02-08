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
    public PrevButton prevButton;
    public Coroutine activeSpinRoutine;
    public override void attach(Controller controller)
    {
        List<Transform> envs = oldButton.environmentsTransforms;
        Transform oldFirst = envs.First();
        oldFirst.gameObject.SetActive(false);
        StopCoroutine(activeSpinRoutine);
        oldFirst.localScale = Vector3.one;
        envs.Remove(oldFirst);
        envs.Add(oldFirst);
        
        Transform newFirst = envs.First();
        newFirst.position = transform.position + Vector3.ProjectOnPlane(transform.forward, Vector3.up) * 0.5f +
                            Vector3.ProjectOnPlane(transform.right, Vector3.up) * -0.08f;
        newFirst.localScale = Vector3.one * 0.6f;
        newFirst.gameObject.SetActive(true);
        activeSpinRoutine = StartCoroutine(Spin(newFirst));
        prevButton.activeSpinRoutine = activeSpinRoutine;
    }

    private void OnEnable()
    {
        Transform env = oldButton.environmentsTransforms.First();
        env.position = transform.position + Vector3.ProjectOnPlane(transform.forward, Vector3.up) * 0.5f +
                            Vector3.ProjectOnPlane(transform.right, Vector3.up) * -0.08f;
        env.localScale = Vector3.one * 0.6f;
        env.gameObject.SetActive(true);
        activeSpinRoutine = StartCoroutine(Spin(env));
        prevButton.activeSpinRoutine = activeSpinRoutine;
    }

    private void OnDisable()
    {
        Transform env = oldButton.environmentsTransforms.First();
        env.gameObject.SetActive(false);
        env.localScale = Vector3.one;
        Transform user = Camera.main.transform;
        env.position = user.position + user.forward * 0.5f + user.right * 0.2f;
        StopCoroutine(activeSpinRoutine);
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