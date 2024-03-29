using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteWidget : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Transform otherObject = other.transform;
        Debug.Log("entered" + otherObject.name);
        if (otherObject.CompareTag("Deletable"))
        {
            VirtualEnvironmentManager.Environment.Elements.Remove(otherObject.parent.GetComponent<ElementModel>());
            VirtualEnvironmentManager.Environment.Interactables.Remove(otherObject.parent);
            Destroy(otherObject.parent.gameObject,0.1f);
            SoundManager.Sounds.failure();
        }
    }
}
