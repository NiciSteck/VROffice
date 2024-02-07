using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepDimension : MonoBehaviour
{
    private bool enabledBefore = false;

    private void OnEnable()
    {
        if (!enabledBefore)
        {
            Vector3 scaleTmp = transform.localScale;
            scaleTmp.x /= transform.parent.localScale.x;
            scaleTmp.y /= transform.parent.localScale.y;
            scaleTmp.z /= transform.parent.localScale.z;
            transform.localScale = scaleTmp;
        }
        enabledBefore = true;
    }
}