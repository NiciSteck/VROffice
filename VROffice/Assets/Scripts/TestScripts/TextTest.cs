using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTest : MonoBehaviour
{
    public TextMeshPro text;
    public bool write;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (write)
        {
            text.text = "fuck";
            write = false;
        }
    }
}
