using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputListener : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Text display;
    int count;

    void Start()
    {
        // Adds a callback that quits the app when Escape is pressed
        Desktopia.Inputs.AddOnKeyDown(KeyCode.Escape, Quit);
    }

    void Update()
    {
        // Increment a value when Space is pressed
        if(Desktopia.Inputs.GetKey(KeyCode.Space)) ++count;
        display.text = count.ToString();
    }

    void Quit()
    {
        Application.Quit();
    }
}
