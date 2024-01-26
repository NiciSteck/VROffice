using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class OpenTrashcanButton : MenuButton
{
    public GameObject trashcan;
    
    [Header("Icon")]
    [SerializeField]
    private MeshRenderer m_icon;
    [SerializeField]
    private Material m_openIcon;
    [SerializeField]
    private Material m_closeIcon;
    

    public override void attach(Controller controller)
    {
        if (trashcan.activeSelf)
        {
            m_icon.material = m_openIcon;
            trashcan.SetActive(false);
        }
        else
        {
            m_icon.material = m_closeIcon;
            Vector3 userPos = Camera.main.transform.position;
            userPos.y = 0;
            userPos.x -= 0.4f;
            userPos.z += 0.1f;
            trashcan.transform.position = userPos;
            trashcan.transform.rotation = Quaternion.identity;
            trashcan.transform.parent = null;
            trashcan.SetActive(true);
        }
    }

    private void Start()
    {
        
    }
}
