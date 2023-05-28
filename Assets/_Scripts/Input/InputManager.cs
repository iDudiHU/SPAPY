using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace TigrisDigitalCreative._Input
{
    public class InputManager : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; } = Vector2.zero;
        public bool MoveIsPressed = false;
        public Vector2 LookInput { get; private set; } = Vector2.zero;
        public bool InvertMouseY { get; private set; } = true;
        public bool AimIsPressed { get; private set; } = false;
        public float ZoomCameraInput { get; private set; } = 0.0f;
        public bool InvertScroll { get; private set; } = true;
        public bool ShootInput { get; private set; } = false;
        public bool RunIsPressed { get; private set; } = false;
        public bool JumpIsPressed { get; private set; } = false;
        public bool MagneticBootsIsOn{ get; private set; } = false;
        public bool ChangeCameraWasPressedThisFrame { get; private set; } = false;
        public bool InteractWasPressedThisFrame { get; private set; } = false;
        public bool EscapeWasPressedThisFrame { get; private set; } = false;


        IAA _input = null;


        private void OnEnable()
        {
            _input = new IAA();
            _input.HumanoidLand.Enable();

            _input.HumanoidLand.Move.performed += SetMove;
            _input.HumanoidLand.Move.canceled += SetMove;

            _input.HumanoidLand.Look.performed += SetLook;
            _input.HumanoidLand.Look.canceled += SetLook;
            
            _input.HumanoidLand.Aim.started += SetAim;
            _input.HumanoidLand.Aim.canceled += SetAim;

            _input.HumanoidLand.Shoot.performed += SetShoot;
            _input.HumanoidLand.Shoot.canceled += SetShoot;
            
            _input.HumanoidLand.Run.started += SetRun;
            _input.HumanoidLand.Run.canceled += SetRun;

            _input.HumanoidLand.Jump.started += SetJump;
            _input.HumanoidLand.Jump.canceled += SetJump;

            _input.HumanoidLand.ZoomCamera.started += SetZoomCamera;
            _input.HumanoidLand.ZoomCamera.canceled += SetZoomCamera;

            _input.HumanoidLand.MagneticBoots.started += SetMagneticBoots;
            _input.HumanoidLand.MagneticBoots.canceled += SetMagneticBoots;
            _input.HumanoidLand.MagneticBoots.performed += SetMagneticBoots;

        }

        private void OnDisable()
        {
            _input.HumanoidLand.Move.performed -= SetMove;
            _input.HumanoidLand.Move.canceled -= SetMove;

            _input.HumanoidLand.Look.performed -= SetLook;
            _input.HumanoidLand.Look.canceled -= SetLook;

            _input.HumanoidLand.Aim.started -= SetAim;
            _input.HumanoidLand.Aim.canceled -= SetAim;

            _input.HumanoidLand.Shoot.performed -= SetShoot;
            _input.HumanoidLand.Shoot.canceled -= SetShoot;
            
            _input.HumanoidLand.Run.started -= SetRun;
            _input.HumanoidLand.Run.canceled -= SetRun;

            _input.HumanoidLand.Jump.started -= SetJump;
            _input.HumanoidLand.Jump.canceled -= SetJump;

            _input.HumanoidLand.ZoomCamera.started -= SetZoomCamera;
            _input.HumanoidLand.ZoomCamera.canceled -= SetZoomCamera;
            
            _input.HumanoidLand.MagneticBoots.started -= SetMagneticBoots;
            _input.HumanoidLand.MagneticBoots.canceled -= SetMagneticBoots;
            _input.HumanoidLand.MagneticBoots.performed -= SetMagneticBoots;


            _input.HumanoidLand.Disable();
        }

        private void Update()
        {
            ChangeCameraWasPressedThisFrame = _input.HumanoidLand.ChangeCamera.WasPressedThisFrame();
            InteractWasPressedThisFrame = _input.HumanoidLand.Interact.WasPressedThisFrame();
            EscapeWasPressedThisFrame = _input.HumanoidLand.ToggleMenu.WasPressedThisFrame();
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

        private void SetAim(InputAction.CallbackContext ctx)
        {
            AimIsPressed = ctx.started;
        }

        private void SetJump(InputAction.CallbackContext ctx)
        {
            JumpIsPressed = ctx.started;
        }

        private void SetZoomCamera(InputAction.CallbackContext ctx)
        {
            ZoomCameraInput = ctx.ReadValue<float>();
        }

        private void SetShoot(InputAction.CallbackContext ctx)
        {
            ShootInput = ctx.started;
        }
        
        private void SetMagneticBoots(InputAction.CallbackContext ctx)
        {
            switch(ctx.phase)
            {
                case InputActionPhase.Started: // the key has been pressed
                    MagneticBootsIsOn = true;
                    break;
                case InputActionPhase.Performed: // a slow tap has been detected
                    MagneticBootsIsOn = true;
                    break;
                case InputActionPhase.Canceled: // the key has been released
                    MagneticBootsIsOn = false;
                    break;
            }
        }

    }
}
