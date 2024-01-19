using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : FlatInput
{
    private const float KEY_SIZE = 1;
    private const float GAP = 0.1f;
    private const float WIN_HEIGHT = 5.5f;
    private const float WIN_WIDTH = 12.1f;
    private const float ENTER_1_CENTER_X = 5.25f;
    private const float ENTER_1_CENTER_Y = 0.05f;
    private const float ENTER_1_WIDTH = 1.6f;
    private const float ENTER_2_CENTER_X = 5.5f;
    private const float ENTER_2_CENTER_Y = -0.5f;
    private const float ENTER_2_WIDTH = 1.1f;
    private const float ENTER_2_HEIGHT = 2.1f;
    private const float SPACE_CENTER_X = -0.05f;
    private const float SPACE_CENTER_Y = -2.15f;
    private const float SPACE_WIDTH = 5.4f;

    public MeshRenderer m_mr;
    public Material m_lowerKeyboardMaterial;
    public Material m_upperKeyboardMaterial; 

    public bool m_lower;

    private float m_countdownResetHighlight;
    private bool m_resetHighlight;

    private List<int> m_enterKeyIdxs = new List<int>();

    private string[] m_capitalizedNums = new string[] { ")", "!", "@", "#", "$", "%", "^", "&", "*", "(" };

    public void setKeyboard(bool lower)
    {
        if (lower)
        {
            m_mr.material = m_lowerKeyboardMaterial;
        }
        else
        {
            m_mr.material = m_upperKeyboardMaterial;
        }
    }

    private void highlightKey(Vector2 pos, Vector2 scale, bool highlight)
    {
        m_mr.material.SetFloat("_HighlightKeyB", 0);

        if (!highlight)
        {
            m_mr.material.SetFloat("_HighlightKeyA", 0);
            return;
        }

        m_mr.material.SetFloat("_HighlightKeyA", 1);
        Vector4 keyParams = new Vector4(pos.x, pos.y, 0.5f * scale.x + 0.005f, 0.5f * scale.y + 0.005f);
        keyParams += new Vector4(0.5f, 0.5f, 0, 0);
        m_mr.material.SetVector("_KeyA", keyParams);
        m_resetHighlight = true;
        m_countdownResetHighlight = 0.5f; 
    }

    private void highlightKey(Vector2 posA, Vector2 scaleA, Vector2 posB, Vector2 scaleB, bool highlight)
    {
        if (!highlight)
        {
            m_mr.material.SetFloat("_HighlightKeyA", 0);
            m_mr.material.SetFloat("_HighlightKeyB", 0);
            return;
        }

        m_mr.material.SetFloat("_HighlightKeyA", 1);
        Vector4 keyAParams = new Vector4(posA.x, posA.y, 0.5f * scaleA.x + 0.005f, 0.5f * scaleA.y + 0.005f);
        keyAParams += new Vector4(0.5f, 0.5f, 0, 0);
        m_mr.material.SetVector("_KeyA", keyAParams);

        m_mr.material.SetFloat("_HighlightKeyB", 1);
        Vector4 keyBParams = new Vector4(posB.x, posB.y, 0.5f * scaleB.x + 0.005f, 0.5f * scaleB.y + 0.005f);
        keyBParams += new Vector4(0.5f, 0.5f, 0, 0);
        m_mr.material.SetVector("_KeyB", keyBParams);

        m_resetHighlight = true;
        m_countdownResetHighlight = 0.5f;
    }

    public override void attach(Controller controller)
    {
        Debug.Log("Attach");

        controller.getSelectedPosSurface(m_inputWindow, out Vector3 pos, out Vector3 forward, out Vector3 up);
        Vector2 localPos = m_inputWindow.InverseTransformPoint(pos);
        InputRegion region = getInputRegion(localPos);
        if (region == null)
            return; 

        string key = region.Name; 

        if (key == "SHIFT") {
            m_lower = !m_lower;
            setKeyboard(m_lower);
            return; 
        }


        bool hasInputMappingKey = Constants.m_inputMappingKeys.ContainsKey(key);
        bool hasInputMappingModifier = Constants.m_inputMappingModifiers.ContainsKey(key);
        if (hasInputMappingKey && hasInputMappingModifier)
        {
            if (m_outputApplication != null)
                m_outputApplication.keyInput(Constants.m_inputMappingKeys[key], Constants.m_inputMappingModifiers[key]);
        }
        else if (hasInputMappingKey)
        {
            if (m_outputApplication != null)
                m_outputApplication.keyInput(Constants.m_inputMappingKeys[key]);
        }
        else
        {
            if (m_lower)
                key = key.ToLower();
            else
            {
                if (int.TryParse(key, out int numInput))
                {
                    key = m_capitalizedNums[numInput];
                }
            }
            if (m_outputApplication != null)
                m_outputApplication.stringInput(key);
            
            highlightKey(region.Center, region.Scale, true);
        }

        if (key == "ENTER")
        {
            InputRegion enterA = m_inputRegions[m_enterKeyIdxs[0]];
            InputRegion enterB = m_inputRegions[m_enterKeyIdxs[1]];
            highlightKey(enterA.Center, enterA.Scale, enterB.Center, enterB.Scale, true);
        }
        else if (key == "BACK" || !hasInputMappingKey)
        {
            highlightKey(region.Center, region.Scale, true);
        }
    }

    private void initRegions()
    {
        m_inputRegions.Clear();

        Vector2 offset = new Vector2(-0.5f, 0.5f);

        Vector2 keySize = new Vector2(KEY_SIZE / WIN_WIDTH, KEY_SIZE / WIN_HEIGHT);
        Vector2 gap = new Vector2(GAP / WIN_WIDTH, GAP / WIN_HEIGHT);

        // Top row 
        offset.x += 0.5f * keySize.x + gap.x;
        offset.y -= 0.5f * keySize.y;
        Vector3 pos = offset;
        string[] letters = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "BACK" };
        foreach (string letter in letters)
        {
            string l = letter;
            addInputRegion(l, pos, keySize);
            pos.x += keySize.x + gap.x;
        }

        // Second row 
        offset.x += 0;
        offset.y -= keySize.y + gap.y;
        pos = offset;
        letters = new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
        foreach (string letter in letters)
        {
            string l = letter;
            addInputRegion(l, pos, keySize);
            pos.x += keySize.x + gap.x; 
        }

        // Third row 
        offset.x += 0.5f * keySize.x;
        offset.y -= keySize.y + gap.y;
        pos = offset;
        letters = new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
        foreach (string letter in letters)
        {
            string l = letter;
            addInputRegion(l, pos, keySize);
            pos.x += keySize.x + gap.x;
        }

        // Fourth row 
        offset.x -= 0.5f * keySize.x + gap.x;
        offset.y -= keySize.y + gap.y;
        pos = offset;
        letters = new string[] { "SHIFT", "Z", "X", "C", "V", "B", "N", "M", ",", "." };
        foreach (string letter in letters)
        {
            string l = letter;
            addInputRegion(l, pos, keySize);
            pos.x += keySize.x + gap.x;
        }

        // Enter special keys 
        m_enterKeyIdxs.Clear();
        addInputRegion("ENTER", 
            new Vector2(ENTER_1_CENTER_X / WIN_WIDTH, ENTER_1_CENTER_Y / WIN_HEIGHT), 
            new Vector2(ENTER_1_WIDTH / WIN_WIDTH, keySize.y));
        m_enterKeyIdxs.Add(m_inputRegions.Count - 1);
        addInputRegion("ENTER",
            new Vector2(ENTER_2_CENTER_X / WIN_WIDTH, ENTER_2_CENTER_Y / WIN_HEIGHT),
            new Vector2(ENTER_2_WIDTH / WIN_WIDTH, ENTER_2_HEIGHT / WIN_HEIGHT));
        m_enterKeyIdxs.Add(m_inputRegions.Count - 1);
        addInputRegion(" ",
            new Vector2(SPACE_CENTER_X / WIN_WIDTH, SPACE_CENTER_Y / WIN_HEIGHT),
            new Vector2(SPACE_WIDTH / WIN_WIDTH, keySize.y));

    }

    public void init()
    {
        initRegions();
        m_lower = true;
        setKeyboard(m_lower);
    }

    // Start is called before the first frame update
    void Start()
    {
        init();   
    }

    // Update is called once per frame
    void Update()
    {
        if (m_resetHighlight)
        {
            m_countdownResetHighlight -= Time.deltaTime;
            if (m_countdownResetHighlight < 0)
            {
                highlightKey(Vector2.zero, Vector2.zero, false);
                m_resetHighlight = false;
            }
        }
    }
}
