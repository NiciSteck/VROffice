using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorAdd : MonoBehaviour
{
    public Vector3 dist_fromQuestToRealSense = new Vector3(0, 0.05f, 0);

    public bool go = true;

    public GameObject realSense;

    public GameObject oculus;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (go)
        {

            // compute where the RealSense should be positioned 
            Vector3 shouldPos = oculus.transform.position;
            Quaternion shouldRot = oculus.transform.rotation;

            // where the RealSense is currently positioned
            
            


            // compute the correction 
            Quaternion isRot = realSense.transform.rotation;
            Quaternion rotDiff = shouldRot * Quaternion.Inverse(isRot);
            transform.rotation = rotDiff * transform.rotation;
            
            Vector3 isPos = realSense.transform.position;
            Vector3 posDiff = shouldPos - isPos;
            transform.position = posDiff + transform.position;
            
            Debug.Log("Resetting Oculus offset"); 
            

            go = false;
        }
    }
}
