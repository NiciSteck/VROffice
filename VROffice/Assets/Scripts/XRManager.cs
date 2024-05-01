using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script controlls the Passthrough of the HMD
 */

public class XRManager : MonoBehaviour
{
    public static XRManager Manager;
    
    [Header("Skybox")]
    [SerializeField]
    private Material m_skybox;

    [Header("Passthrough")]
    [SerializeField]
    private OVRPassthroughLayer m_passthrough;
    [SerializeField]
    private bool m_usePassthrough = false;
    private bool m_usePassthroughPrev = false;

    private void Start()
    {
        Manager = this;
    }

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
            Debug.Log(m_usePassthrough);
            Debug.Log(m_usePassthroughPrev);
            m_usePassthrough = !m_usePassthrough;
            return;
        }

        m_usePassthroughPrev = m_usePassthrough;
    }
}
