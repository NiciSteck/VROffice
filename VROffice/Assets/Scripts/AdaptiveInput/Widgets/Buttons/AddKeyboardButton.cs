using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddKeyboardButton : AddWidgetButton
{
    [Header("Window")]
    [SerializeField]
    private Widget m_attachedWidget;

    [Header("Icon")]
    [SerializeField]
    private MeshRenderer m_icon;
    [SerializeField]
    private Material m_openIcon;
    [SerializeField]
    private Material m_closeIcon;

    private bool m_keyboardOpen = false;

    // Keyboard
    private GameObject m_keyboard;

    [Header("Placement")]
    [SerializeField]
    private Vector3 m_keyboardPositionOffset;
    [SerializeField]
    private float m_keyboardRotationOffset; 

    private void newKeyboard()
    {
        GameObject widget = GameObject.Instantiate(m_widget,GameObject.Find("Elements").transform);
        widget.name = m_widget.name;
        
        VirtualEnvironmentManager.Environment.addElement(widget.GetComponent<ElementModel>(), true);
        KeyboardManager.Keyboard.addKeyboard(widget.GetComponent<KeyboardInput>());

        m_keyboard = widget;
        m_keyboard.GetComponent<KeyboardInput>().setOutputApplication(m_attachedWidget.GetComponent<WidgetApplication>());
    }

    private void openKeyboard() {

        if (m_keyboard == null)
            newKeyboard();

        m_keyboard.SetActive(true);
        
        /*
        Camera cam = Camera.main;
        Vector3 pos = cam.transform.position;
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();
        pos += 0.3f * forward;
        m_keyboard.transform.position = pos;
        m_keyboard.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        m_keyboard.GetComponent<Widget>().place(pos, Quaternion.LookRotation(forward, Vector3.up), false);
        */

        Vector3 position = m_attachedWidget.Position +
             m_keyboardPositionOffset.x * m_attachedWidget.Right +
             (m_keyboardPositionOffset.y - 0.5f * m_attachedWidget.Scale.y) * m_attachedWidget.Up +
             m_keyboardPositionOffset.z * m_attachedWidget.Forward;
        m_keyboard.transform.position = position;
        m_keyboard.transform.rotation = m_attachedWidget.Rotation;
        m_keyboard.transform.Rotate(m_keyboardRotationOffset, 0, 0);
        Quaternion rotation = m_keyboard.transform.rotation;
        m_keyboard.GetComponent<Widget>().place(position, rotation, false);
        m_keyboard.GetComponent<KeyboardInput>().setKeyboard(true);



        // Adaptation 
        //if (AdaptiveLayout.Adaptation != null)
        //    AdaptiveLayout.Adaptation.startOptimization();
    }

    private void closeKeyboard() {
        if (m_keyboard == null)
            return;

        m_keyboard.GetComponent<Widget>().detachFromSurface();
        m_keyboard.SetActive(false);
    }

    public override void attach(Controller controller)
    {
        

        if (!m_keyboardOpen)
        {
            //Debug.Log("Opening Keyboard");
            openKeyboard();
            m_icon.material = m_closeIcon;
        }
        else
        {
            //Debug.Log("Closing Keyboard");
            closeKeyboard();
            m_icon.material = m_openIcon;
        }


        m_keyboardOpen = !m_keyboardOpen;

        
    }

    public void setAttachedWidget(Widget widget, Transform window)
    {
        m_attachedWidget = widget;
        m_attachedWindow = window;
    }
}
