using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddWidgetButton : MenuButton 
{
    [Header("Widget")]
    [SerializeField]
    protected GameObject m_widget;




    public override void attach(Controller controller)
    {
        GameObject widget = GameObject.Instantiate(m_widget,GameObject.Find("Elements").transform);
        widget.name = m_widget.name; 
        Camera cam = Camera.main;
        Vector3 pos = cam.transform.position;
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();
        pos += 0.5f * forward;
        widget.transform.position = pos;
        widget.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        widget.GetComponent<Widget>().place(pos, Quaternion.LookRotation(forward, Vector3.up), false);

        VirtualEnvironmentManager.Environment.addElement(widget.GetComponent<ElementModel>(), true);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
