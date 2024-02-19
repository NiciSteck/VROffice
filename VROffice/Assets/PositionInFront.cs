using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInFront : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForSec());
    }

    IEnumerator WaitForSec()
    {
        yield return new WaitForSeconds(1.0f);
        Transform user = Camera.main.transform;
        Vector3 targetPosition = user.position + user.forward * 0.5f + user.right * -0.2f - user.up * 0.3f;
        transform.GetComponent<Widget>().place(targetPosition,Camera.main.transform.rotation, false);
        yield return null;
    }
}
