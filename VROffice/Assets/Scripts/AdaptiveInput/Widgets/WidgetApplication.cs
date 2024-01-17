using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidgetApplication : MonoBehaviour
{

    public virtual void rescale(Vector3 scale) { }

    public virtual void attach(Controller controller) { }

    public virtual void detach(Controller controller) { }

    public virtual void disable() { }

    public virtual void stringInput(string s) { }

    public virtual void keyInput(KeyCode k, EventModifiers modifier = EventModifiers.None) { }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}