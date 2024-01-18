using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Sounds;

    private AudioSource m_source;

    [SerializeField]
    private AudioClip m_attach;
    [SerializeField]
    private AudioClip m_click;
    [SerializeField]
    private AudioClip m_release;
    [SerializeField]
    private AudioClip m_snap;
    [SerializeField]
    private AudioClip m_detach;
    [SerializeField]
    private AudioClip m_success;
    [SerializeField]
    private AudioClip m_failure; 

    public void attach()
    {
        m_source.PlayOneShot(m_attach);
    }

    public void click()
    {
        m_source.PlayOneShot(m_click);
    }

    public void release()
    {
        m_source.PlayOneShot(m_release);
    }

    public void snap()
    {
        m_source.PlayOneShot(m_snap);
    }

    public void detach()
    {
        m_source.PlayOneShot(m_detach);
    }

    public void success()
    {
        m_source.PlayOneShot(m_success);
    }

    public void failure()
    {
        m_source.PlayOneShot(m_failure);
    }

    // Start is called before the first frame update
    void Start()
    {
        Sounds = this;
        m_source = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
