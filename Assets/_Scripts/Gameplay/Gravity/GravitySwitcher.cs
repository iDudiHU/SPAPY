using UnityEngine;
using UnityEngine.InputSystem;

public class GravitySwitcher : MonoBehaviour
{
    public float gravityStrength = 9.81f;

    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {

        if (keyboard.upArrowKey.isPressed)
            ChangeGravityDirection(Vector3.up);
        else if (keyboard.downArrowKey.isPressed)
            ChangeGravityDirection(Vector3.down);
        else if (keyboard.leftArrowKey.isPressed)
            ChangeGravityDirection(Vector3.left);
        else if (keyboard.rightArrowKey.isPressed)
            ChangeGravityDirection(Vector3.right);
        
    }

    private void ChangeGravityDirection(Vector3 direction)
    {
        Physics.gravity = direction * gravityStrength;
    }
}
