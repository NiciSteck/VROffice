using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Keyboard; 

    //[SerializeField]
    //private GameObject m_keyboard;
    [SerializeField]
    private List<KeyboardInput> m_keyboards;

    /*
    [SerializeField]
    private GameObject m_keyboardLockButton;
    [SerializeField]
    private Widget m_openKeyboardButtonMenu;
    [SerializeField]
    private OpenKeyboardButton m_openKeyboardButton;
    [SerializeField]
    private Vector3 m_keyboardSpawnOffset;
    [SerializeField]
    private Vector3 m_keyboardMenuSpawnOffset; 
    [SerializeField]
    private float m_keyboardSpawnRotationOffset;
    

    private BrowserWidget m_lastBrowser;
    public BrowserWidget lastBrowser
    {
        set
        {
            m_lastBrowser = value;
            Debug.Log(value);
        }
    }
    */

    public void updateKeyboardOutputWidget(BrowserWidget app)
    {
        Debug.Log("Retarget keyboard input");

        foreach (KeyboardInput keyboard in m_keyboards)
        {
            if (keyboard.gameObject.activeInHierarchy)
                keyboard.setOutputApplication(app);
        }
        /*
        m_lastBrowser = app;
        if (m_keyboardInput != null)
            m_keyboardInput.setOutputApplication(m_lastBrowser);
        */
    }

    public void addKeyboard(KeyboardInput keyb)
    {
        m_keyboards.Add(keyb);
    }

    public void setKeyboard(bool on)
    {
        /*
        if (m_keyboard == null)
            return; 
        if (!on)
        {
            
            m_keyboard.GetComponent<Widget>().detachFromSurface();
            m_keyboard.SetActive(false);
            m_keyboardLockButton.SetActive(false);
            return;
        }
        m_keyboard.SetActive(true);
        m_keyboardLockButton.SetActive(true);
        Transform user = Camera.main.transform;
        Vector3 position;
        Quaternion rotation;
        if (m_lastBrowser != null)
        {
            Vector3 toLast = m_lastBrowser.transform.position - user.position;
            toLast.y = 0;
            toLast.Normalize();
            position = user.position + m_keyboardSpawnOffset.y * Vector3.up + m_keyboardSpawnOffset.z * toLast;
            rotation = Quaternion.LookRotation(toLast);
        }
        else
        {
            Vector3 forward = user.forward;
            forward.y = 0;
            forward.Normalize();
            position = user.position + m_keyboardSpawnOffset.y * Vector3.up + m_keyboardSpawnOffset.z * forward;
            rotation = Quaternion.LookRotation(forward);
        }
        m_keyboard.transform.SetPositionAndRotation(position, rotation);
        m_keyboard.transform.Rotate(m_keyboardSpawnRotationOffset, 0, 0);
        rotation = m_keyboard.transform.rotation;
        m_keyboard.GetComponent<Widget>().place(position, rotation, false);
        m_keyboardLockButton.GetComponent<LockButton>().setLocked(true);
        if (m_keyboardInput != null)
        {
            m_keyboardInput.setOutputApplication(m_lastBrowser);
            m_keyboardInput.init();
        }
        */
    }

    public void closeKeyboard()
    {
        /*
        if (m_openKeyboardButton != null)
            m_openKeyboardButton.closeKeyboard();
        */
    }

    public void resetMenuPosition()
    {
        /*
        Transform user = Camera.main.transform;
        Vector3 forward = user.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 position = user.position + m_keyboardMenuSpawnOffset.y * Vector3.up + m_keyboardMenuSpawnOffset.z * forward;
        Quaternion rotation = Quaternion.LookRotation((position - user.position).normalized);
        if (m_openKeyboardButtonMenu != null)
        {
            m_openKeyboardButtonMenu.transform.position = position;
            m_openKeyboardButtonMenu.transform.rotation = rotation;
            m_openKeyboardButtonMenu.GetComponent<Widget>().place(position, rotation, false);
        }
        */
    }

        // Start is called before the first frame update
    void Start()
    {
        Keyboard = this; 
        /*
        if (m_keyboard != null)
            m_keyboard.SetActive(false);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
