using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualEnvironmentManager : MonoBehaviour
{
    public static VirtualEnvironmentManager Environment;
    
    [SerializeField]
    private List<ElementModel> m_elements = new List<ElementModel>(); 
    public List<ElementModel> Elements
    {
        get { return m_elements; }
    }
    
    [SerializeField]
    private List<Transform> m_interactables = new List<Transform>(); 
    public List<Transform> Interactables
    {
        get { return m_interactables; }
    }
    
    [Header("Surface Settings")]
    [SerializeField]
    private float m_surfaceOffset;
    public float SurfaceOffset
    {
        get { return m_surfaceOffset; }
    }
    
    [Header("Element Set Controls")]
    [SerializeField]
    private Transform m_elementsContainer;
    public Transform ElementsContainer
    {
        get { return m_elementsContainer; }
        set { m_elementsContainer = value; }
    }
    [SerializeField]
    private bool m_clearElements;
    private bool m_clearElementsPrev;
    [SerializeField]
    private bool m_useElements;
    private bool m_useElementsPrev;
    [SerializeField]
    private bool m_setTransformMode;
    public bool setTransformMode
    {
        set { m_setTransformMode = true; }
    }
    private bool m_setTransformModePrev;
    [SerializeField]
    private bool m_setApplicationMode;
    public bool setApplicationMode
    {
        set { m_setApplicationMode = true; }
    }
    private bool m_setApplicationModePrev;
    [SerializeField]
    private bool m_inputEnabled;
    private bool m_inputEnabledPrev;
    
    private void Start()
    {
        Environment = this;
    }
    
    void Update()
    {
        // Clear elements
        if (m_clearElements && !m_clearElementsPrev)
        {
            clearElementSet(); 
            m_clearElements = false;
        }

        // Use elements
        if (m_useElements && !m_useElementsPrev)
        {
            useElementSet();
        }

        // Set transform mode
        if (m_setTransformMode && !m_setTransformModePrev)
        {
            Debug.Log("Virtual Environment Manager: Setting Transform Mode");
            setMode(false);
            m_setTransformMode = false;
        }

        // Set application mode
        if (m_setApplicationMode && !m_setApplicationModePrev)
        {
            Debug.Log("Virtual Environment Manager: Setting Application Mode");
            setMode(true);
            m_setApplicationMode = false;
        }

        // Enable / Disable input 
        if (m_inputEnabled && !m_inputEnabledPrev)
        {
            setInputEnabled(true);
        }
        if (!m_inputEnabled && m_inputEnabledPrev)
        {
            setInputEnabled(false);
        }

        m_clearElementsPrev = m_clearElements;
        m_useElementsPrev = m_useElements;
        m_setTransformModePrev = m_setTransformMode;
        m_setApplicationMode = m_setApplicationModePrev;
        m_inputEnabledPrev = m_inputEnabled;
    }
    
    public void addInteractable(Transform interactable)
    {
        m_interactables.Add(interactable);
    }
    
    public void setMode(bool app)
    {
        foreach (Transform interactable in m_interactables)
        {
            if (!interactable.gameObject.activeInHierarchy)
                continue; 

            Widget widget = interactable.GetComponent<Widget>(); 
            if (widget != null)
            {
                if (app)
                {
                    widget.updateState(Widget.State.APP);
                }
                else
                {
                    widget.updateState(Widget.State.TRANSFORM);
                }
            }
        }
    }

    private void setInputEnabled(bool enabled)
    {
        foreach (Transform interactable in m_interactables)
        {
            if (!interactable.gameObject.activeInHierarchy)
                continue;

            Widget widget = interactable.GetComponent<Widget>();
            if (widget != null)
            {
                widget.InputEnabled = enabled;
            }
        }
    }

    public void addElement(ElementModel element, bool isInteractable)
    {
        m_elements.Add(element);
        if (isInteractable)
            addInteractable(element.transform);
    }
    
    public void clearElementSet()
    {
        foreach (ElementModel element in m_elements)
        {
            Widget widget = element.GetComponent<Widget>();
            widget.detachFromSurface();
        }
        m_elements.Clear();
        m_interactables.Clear(); 
    }
    public void useElementSet()
    {
        clearElementSet();
        foreach (Transform element in m_elementsContainer)
        {
            m_elements.Add(element.GetComponent<ElementModel>());
            m_interactables.Add(element);
        }
        m_useElements = false;
    }
}
