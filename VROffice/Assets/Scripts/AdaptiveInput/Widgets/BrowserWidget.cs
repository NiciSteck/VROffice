using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

public class BrowserWidget : WidgetApplication
{
    private Browser m_browser;
    private PointerUIBase m_browserPointer;
    [Header("Controllers")]
    [SerializeField]
    private Controller m_controller;
    private Controller m_prevController;
    [SerializeField]
    private bool m_scrollEnabled = false;

    public PointerUIBase getBrowserPointer()
    {
        return m_browserPointer;
    }

    private bool m_clickingLeftPrev = false;
    private void feedPointers(PointerUIBase target)
    {
        bool hovering = false;
        bool clickingLeft = false;
        bool clickingRight = false;
        bool clickingLeftRaw = false;
        bool clickingRightRaw = false; 
        bool scrolling = false;
        Vector3 origin = Vector3.zero;
        Vector3 dir = Vector3.zero;
        if (!m_controller)
            return;

        m_controller.getBrowserInteractions(this.transform, 
            out hovering, out clickingLeft, out clickingRight, out scrolling, 
            out clickingLeftRaw, out clickingRightRaw, 
            out origin, out dir);

        MouseButton activeButton = 0;
        if (m_scrollEnabled)
        {
            if (clickingLeft)
                activeButton = MouseButton.Left;
            else if (clickingRight)
                activeButton = MouseButton.Right;
        }
        else
        {
            if (clickingLeftRaw)
                activeButton = MouseButton.Left;
            else if (clickingRightRaw)
                activeButton = MouseButton.Right;
        }

        target.FeedPointerState(new PointerUIBase.PointerState
        {
            is2D = false,
            position3D = new Ray(origin, dir),
            activeButtons = activeButton,
            scrollDelta = Vector2.zero,
            scrollEnabled = m_scrollEnabled,
            scrolling = scrolling
        });
    }

    public override void rescale(Vector3 scale)
    {
        m_browser.transform.localScale = scale; 
    }

    public override void attach(Controller controller)
    {
        if (m_controller != controller)
        {
            m_controller = controller;
        }

        // Retarget keyboard input 
        if (KeyboardManager.Keyboard != null)
            KeyboardManager.Keyboard.updateKeyboardOutputWidget(this);
    }

    public void detach()
    {
        m_controller = null;
    }

    public override void detach(Controller controller)
    {
        m_controller = null;
        m_browserPointer.externalInput = feedPointers;
    }


    public override void disable()
    {

        m_controller = null;
        m_prevController = null;
        
        m_browserPointer.externalInput = feedPointers;
    }

    public override void stringInput(string s)
    {
        m_browser.TypeText(s);
    }

    public override void keyInput(KeyCode k, EventModifiers modifier = EventModifiers.None)
    {
        m_browser.PressKey(k, modifier: modifier);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_browser = this.GetComponentInChildren<Browser>();
        m_browserPointer = this.GetComponentInChildren<PointerUIBase>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_controller != m_prevController)
        {
            if (m_prevController != null)
                m_prevController.detach();
            m_browserPointer.externalInput = feedPointers;
        
        }
        m_prevController = m_controller;
        
    }
}
