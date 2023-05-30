using System.Collections;
using System.Collections.Generic;
using TigrisDigitalCreative._Input;
using UnityEngine;
using UnityEngine.UI;

public class GravityBootsUI : MonoBehaviour
{
    public Image BootsOnImage;

    public InputManager _input;
    // Update is called once per frame
    void Update()
    {
        if (_input.MagneticBootsIsOn) {
            BootsOnImage.color = Color.green;
        }
        else {
            BootsOnImage.color = Color.red;
        }
    }
}
