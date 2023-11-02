using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRManager : MonoBehaviour
{
    [Header("Skybox")]
    [SerializeField]
    private Material m_skybox;

    [Header("Passthrough")]
    [SerializeField]
    private OVRManager m_ovrManager; 
    [SerializeField]
    private OVRPassthroughLayer m_passthrough;
    [SerializeField]
    private bool m_usePassthrough = false;
    private bool m_usePassthroughPrev = false;

    // Controls
    //private bool m_pKey, m_pKeyPrev; 

    public void setImmersive()
    {
        RenderSettings.skybox = m_skybox;
        m_passthrough.enabled = false;
    }

    public void setPassthrough()
    {
        RenderSettings.skybox = null;
        m_passthrough.enabled = true;
    }

    IEnumerator endApplication(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_usePassthrough && !m_usePassthroughPrev)
        {
            Debug.Log("Passthrough");
            setPassthrough();
        }
        else if (!m_usePassthrough && m_usePassthroughPrev)
        {
            Debug.Log("Immersive");
            setImmersive();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_ovrManager.isInsightPassthroughEnabled = false;
            m_passthrough.enabled = false;
            StartCoroutine(endApplication(0.5f));
        }

        m_usePassthroughPrev = m_usePassthrough;
    }
}
