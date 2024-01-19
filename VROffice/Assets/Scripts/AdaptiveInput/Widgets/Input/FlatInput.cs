using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputRegion
{
    private string name;
    public string Name
    {
        get { return name; }
    }
    private Vector2 center;
    public Vector2 Center
    {
        get { return center; }
    }
    private Vector2 dimensions;
    public Vector2 Scale
    {
        get { return dimensions; }
    }

    public InputRegion(string name, Vector2 center, Vector2 dimensions)
    {
        this.name = name;
        this.center = center;
        this.dimensions = dimensions;
    }

    public override string ToString()
    {
        return this.name + " " + this.center.ToString() + " " + this.dimensions.ToString();
    }

    public bool contains(Vector2 pos)
    {
        return Mathf.Abs(pos.x - center.x) < dimensions.x * 0.5f &&
            Mathf.Abs(pos.y - center.y) < dimensions.y * 0.5f; 
    }
}

public class FlatInput : WidgetApplication
{

    [Header("Input")]
    [SerializeField]
    protected Transform m_inputWindow;

    [Header("Output")]
    [SerializeField]
    protected WidgetApplication m_outputApplication;
    public WidgetApplication outputApplication
    {
        get { return m_outputApplication; }
    }

    protected List<InputRegion> m_inputRegions = new List<InputRegion>(); 

    protected void addInputRegion(string name, Vector2 center, Vector2 dimensions)
    {
        //Debug.Log(name);
        InputRegion region = new InputRegion(name, center, dimensions);
        //Debug.Log(region.ToString());
        m_inputRegions.Add(region);
    }

    protected string checkInputRegion(Vector2 pos)
    {
        foreach (InputRegion region in m_inputRegions)
        {
            if (region.contains(pos)) {
                //Debug.Log("Input: " + region.Name);
                return region.Name; 
            }
                
        }
        return "";
    }

    protected InputRegion getInputRegion(Vector2 pos)
    {
        foreach (InputRegion region in m_inputRegions)
        {
            if (region.contains(pos))
            {
                return region;
            }

        }
        return null; 
    }

    public override void rescale(Vector3 scale)
    {
        m_inputWindow.localScale = scale;
    }

    public override void attach(Controller controller)
    {
        controller.getSelectedPosSurface(m_inputWindow, out Vector3 pos, out Vector3 forward, out Vector3 up);
        Vector2 localPos = m_inputWindow.InverseTransformPoint(pos);
        string region = checkInputRegion(localPos);

        // Parse input
        if (m_outputApplication == null)
            return;

        bool hasInputMappingKey = Constants.m_inputMappingKeys.ContainsKey(region);
        bool hasInputMappingModifier = Constants.m_inputMappingModifiers.ContainsKey(region);
        if (hasInputMappingKey && hasInputMappingModifier)
        {
            m_outputApplication.keyInput(Constants.m_inputMappingKeys[region], Constants.m_inputMappingModifiers[region]);
        } else if (hasInputMappingKey)
        {
            m_outputApplication.keyInput(Constants.m_inputMappingKeys[region]);
        } else
        {
            m_outputApplication.stringInput(region);
        }
    }

    public void setOutputApplication(WidgetApplication application)
    {
        Debug.Log("Setting output application");
        m_outputApplication = application;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
