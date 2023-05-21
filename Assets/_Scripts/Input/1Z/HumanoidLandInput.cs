using UnityEngine;
using UnityEngine.InputSystem;

public class HumanoidLandInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public bool MoveIsPressed = false;
    public Vector2 LookInput { get; private set; } = Vector2.zero;
    public bool InvertMouseY { get; private set; } = true;
    public float ZoomCameraInput { get; private set; } = 0.0f;
    public bool InvertScroll { get; private set; } = true;
    public bool RunIsPressed { get; private set; } = false;
    public bool CrouchIsPressed { get; private set; } = false;
    public bool JumpIsPressed { get; private set; } = false;
    public float ActivateInput { get; private set; } = 0.0f;
    public bool ChaosIsPressed { get; private set; } = false;

    public bool ChangeCameraWasPressedThisFrame { get; private set; } = false;
    public bool OnOffWasPressedThisFrame { get; private set; } = false;
    public bool ModeWasPressedThisFrame { get; private set; } = false;
    public bool SwitchCharacterWasPressedThisFrame { get; private set; } = false;


    InputActions _input = null;


    private void OnEnable()
    {
        _input = new InputActions();
        _input.HumanoidLand.Enable();

        _input.HumanoidLand.Move.performed += SetMove;
        _input.HumanoidLand.Move.canceled += SetMove;

        _input.HumanoidLand.Look.performed += SetLook;
        _input.HumanoidLand.Look.canceled += SetLook;

        _input.HumanoidLand.Run.started += SetRun;
        _input.HumanoidLand.Run.canceled += SetRun;

        _input.HumanoidLand.Crouch.started += SetCrouch;
        _input.HumanoidLand.Crouch.canceled += SetCrouch;

        _input.HumanoidLand.Jump.started += SetJump;
        _input.HumanoidLand.Jump.canceled += SetJump;

        _input.HumanoidLand.ZoomCamera.started += SetZoomCamera;
        _input.HumanoidLand.ZoomCamera.canceled += SetZoomCamera;

        _input.HumanoidLand.Activate.performed += SetActivate;
        _input.HumanoidLand.Activate.canceled += SetActivate;

        _input.HumanoidLand.Chaos.started += SetChaos;
        _input.HumanoidLand.Chaos.canceled += SetChaos;
    }

    private void OnDisable()
    {
        _input.HumanoidLand.Move.performed -= SetMove;
        _input.HumanoidLand.Move.canceled -= SetMove;

        _input.HumanoidLand.Look.performed -= SetLook;
        _input.HumanoidLand.Look.canceled -= SetLook;

        _input.HumanoidLand.Run.started -= SetRun;
        _input.HumanoidLand.Run.canceled -= SetRun;

        _input.HumanoidLand.Crouch.started -= SetCrouch;
        _input.HumanoidLand.Crouch.canceled -= SetCrouch;

        _input.HumanoidLand.Jump.started -= SetJump;
        _input.HumanoidLand.Jump.canceled -= SetJump;

        _input.HumanoidLand.ZoomCamera.started -= SetZoomCamera;
        _input.HumanoidLand.ZoomCamera.canceled -= SetZoomCamera;

        _input.HumanoidLand.Activate.performed -= SetActivate;
        _input.HumanoidLand.Activate.canceled -= SetActivate;

        _input.HumanoidLand.Chaos.started -= SetChaos;
        _input.HumanoidLand.Chaos.canceled -= SetChaos;

        _input.HumanoidLand.Disable();
    }

    private void Update()
    {
        ChangeCameraWasPressedThisFrame = _input.HumanoidLand.ChangeCamera.WasPressedThisFrame();
        OnOffWasPressedThisFrame = _input.HumanoidLand.OnOff.WasPressedThisFrame();
        ModeWasPressedThisFrame = _input.HumanoidLand.Mode.WasPressedThisFrame();
        SwitchCharacterWasPressedThisFrame = _input.HumanoidLand.SwitchCharacter.WasPressedThisFrame();
    }

    private void SetMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
        MoveIsPressed = !(MoveInput == Vector2.zero);
    }

    private void SetLook(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();
    }

    private void SetRun(InputAction.CallbackContext ctx)
    {
        RunIsPressed = ctx.started;
    }

    private void SetCrouch(InputAction.CallbackContext ctx)
    {
        CrouchIsPressed = ctx.started;
    }

    private void SetJump(InputAction.CallbackContext ctx)
    {
        JumpIsPressed = ctx.started;
    }

    private void SetZoomCamera(InputAction.CallbackContext ctx)
    {
        ZoomCameraInput = ctx.ReadValue<float>();
    }

    private void SetActivate(InputAction.CallbackContext ctx)
    {
        ActivateInput = ctx.ReadValue<float>();
    }

    private void SetChaos(InputAction.CallbackContext ctx)
    {
        ChaosIsPressed = ctx.started;
    }
}
