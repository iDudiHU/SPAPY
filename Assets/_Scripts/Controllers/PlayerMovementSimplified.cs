using TigrisDigitalCreative._Input;
using UnityEngine;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;

namespace TigrisDigitalCreative._Scripts.Controllers
{
    public class PlayerMovementSimplified : MonoBehaviour
    {
        #region Private Variables
        [Header("General Config")]
        [SerializeField] InputManager _input;

        [Header("General Debug")]
        [SerializeField] Vector3 _playerMoveInput = Vector3.zero;
        [SerializeField] Vector3 _playerCalculatedForce = Vector3.zero;

        [Header("General")]
        Rigidbody _rigidbody = null;
        CapsuleCollider _capsuleCollider = null;

        LayerMask _layerMaskEverythingButPlayer = new LayerMask();


        [Header("Camera Config")]
        [SerializeField] PlayerCameraController _cameraController;
        public Transform CameraFollow;
        [SerializeField] float _rotationSpeedMultiplier = 180.0f;
        [SerializeField] float _pitchSpeedMultiplier = 180.0f;
        [SerializeField] float _playerLookInputLerpTime = 0.35f;

        [Header("Camera Debug")]
        [SerializeField] float _cameraPitch = 0.0f;

        [Header("Camera")]
        Vector3 _playerLookInput = Vector3.zero;
        Vector3 _previousPlayerLookInput = Vector3.zero;
        
        [Header("Movement Config")]
        [SerializeField] float _movementMultiplier = 30.0f;
        [SerializeField] float _notGroundedMovementMultiplier = 1.25f;
        [SerializeField] float _runMultiplier = 2.5f;
        [SerializeField] float _minimumVelocityMagnitude = 0.25f;


        [Header("Ground Check Config")]
        [SerializeField][Range(0.0f, 1.8f)] float _groundCheckRadiusMultiplier = 0.9f;
        [SerializeField][Range(-0.95f, 1.05f)] float _groundCheckDistanceTolerance = 0.05f;

        [Header("Ground Check Debug")]
        [SerializeField] bool _playerIsGrounded = true;

        [Header("Ground Check")]
        RaycastHit _groundCheckHit = new RaycastHit();

        [SerializeField] private Vector3 _playerCenterPoint = Vector3.zero;


        [Header("Gravity Config")]
        [SerializeField] private Transform _obbjectOfInfluence;
        [SerializeField] private bool _isGravityOn = true;
        [SerializeField] private bool _isArtificialGravityOn = false;
        [SerializeField] float _gravityFallMin = 0.0f;
        [SerializeField] float _gravityFallIncrementTime = 0.05f;
        [SerializeField] Vector3 _gravityDirection = Vector3.down;

        [Header("Magnetic Boots")] 
        [SerializeField] private bool _isMagneticBootsOn = false;
        [SerializeField] private float _magneticBootsRange = 10;
        [SerializeField] private float _magneticBootslerpTime = 360f;
        [SerializeField] private RaycastHit MagneticSurfaceHit = new RaycastHit();
        [SerializeField] private LayerMask _walkableLayerMask;

        [Header("Gravity Debug")]
        [SerializeField] bool _playerIsFalling = false;
        [SerializeField] float _gravityFallCurrent = 0.0f;
        [SerializeField] float _playerFallTimer = 0.0f;
        
        [Header("Jumping Config")]
        [SerializeField] float _initialJumpForceMultiplier = 750.0f;
        [SerializeField] float _continualJumpForceMultiplier = 0.1f;
        [SerializeField] float _jumpTime = 0.175f;
        [SerializeField] float _coyoteTime = 0.15f;
        [SerializeField] float _jumpBufferTime = 0.2f;

        [Header("Jumping Debug")]
        [SerializeField] float _jumpTimeCounter = 0.0f;
        [SerializeField] float _coyoteTimeCounter = 0.0f;
        [SerializeField] float _jumpBufferTimeCounter = 0.0f;
        [SerializeField] bool _playerIsJumping = false;
        [SerializeField] bool _jumpWasPressedLastFrame = false;
        #endregion
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            int playerLayer = LayerMask.NameToLayer("Player");
            _layerMaskEverythingButPlayer = ~(1 << playerLayer);
        }

        private void Update()
        {

        }

        private void FixedUpdate()
        {
            if (!_cameraController.UsingOrbitalCamera)
            {
                _playerLookInput = GetLookInput();
                PlayerLook();
                PitchCamera();
            }

            _playerMoveInput = GetMoveInput();
            _playerCalculatedForce = _playerMoveInput;

            PlayerVariables();

            _playerIsGrounded = PlayerGroundCheck();

            _playerCalculatedForce = PlayerMove();
            _playerCalculatedForce = PlayerRun();

            if (_isArtificialGravityOn)
            {
                // Adjust the player's rotation to align with the direction of gravity
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -_gravityDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _magneticBootslerpTime * Time.deltaTime);
            }

            if (_isGravityOn || _isArtificialGravityOn) 
            {
                _playerCalculatedForce += PlayerFallGravity();
            }

            _playerCalculatedForce.y = PlayerJump();
            _playerCalculatedForce *= _rigidbody.mass;

            _rigidbody.AddRelativeForce(_playerCalculatedForce, ForceMode.Force);
            EnforceMinimumVelocity();
        }

        private Vector3 GetLookInput()
        {
            _previousPlayerLookInput = _playerLookInput;
            _playerLookInput = new Vector3(_input.LookInput.x, (_input.InvertMouseY ? -_input.LookInput.y : _input.LookInput.y), 0.0f);
            return Vector3.Lerp(_previousPlayerLookInput, _playerLookInput * Time.deltaTime, _playerLookInputLerpTime);
        }

        private void PlayerLook()
        {
            _rigidbody.rotation = Quaternion.Euler(0.0f, _rigidbody.rotation.eulerAngles.y + (_playerLookInput.x * _rotationSpeedMultiplier), 0.0f);
        }

        private void PitchCamera()
        {
            _cameraPitch += _playerLookInput.y * _pitchSpeedMultiplier;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -89.9f, 89.9f);

            CameraFollow.rotation = Quaternion.Euler(_cameraPitch, CameraFollow.rotation.eulerAngles.y, CameraFollow.rotation.eulerAngles.z);
        }

        private Vector3 GetMoveInput()
        {
            return new Vector3(_input.MoveInput.x, 0.0f, _input.MoveInput.y);
        }

        private void PlayerVariables()
        {
            _isMagneticBootsOn = _input.MagneticBootsIsOn;
            if (!_isMagneticBootsOn) {
                return;
            }
            if (MagneticBootsRangeCheck()) {
                Debug.Log(MagneticSurfaceHit.collider.name);
                // Check if the hit object is a magnetic surface
                IMagneticSurface magneticSurfaceInBootsRange = MagneticSurfaceHit.collider.GetComponent<IMagneticSurface>();
                if (magneticSurfaceInBootsRange != null)
                {
                    _obbjectOfInfluence = MagneticSurfaceHit.collider.transform;
                    ChangeGravityDirection(-MagneticSurfaceHit.normal);
                    _isArtificialGravityOn = true;
                } else {
                    _isArtificialGravityOn = false;
                }
            }
        }

        private bool MagneticBootsRangeCheck()
        {
            return Physics.SphereCast(_playerCenterPoint, _capsuleCollider.radius, -_rigidbody.transform.up, out MagneticSurfaceHit, _magneticBootsRange);
        }

        private bool PlayerGroundCheck()
        {
            _playerCenterPoint = _rigidbody.position;

            float sphereCastRadius = _capsuleCollider.radius * _groundCheckRadiusMultiplier;
            float sphereCastTravelDistance =
                _capsuleCollider.bounds.extents.y - sphereCastRadius + _groundCheckDistanceTolerance;

            bool isGrounded = Physics.SphereCast(_playerCenterPoint, sphereCastRadius, -_rigidbody.transform.up,
                out _groundCheckHit, sphereCastTravelDistance);
            Debug.DrawRay(_playerCenterPoint, -_rigidbody.transform.up * sphereCastTravelDistance, Color.red);

            return isGrounded;
        }

        private void MagneticBoots()
        {
            IMagneticSurface magneticSurface = null;
            if (true)
            {
                // Check if the player is grounded on a magnetic surface
                magneticSurface = _groundCheckHit.collider?.GetComponent<IMagneticSurface>();
                if (magneticSurface != null)
                {
                    _obbjectOfInfluence = _groundCheckHit.collider.transform;
                    ChangeGravityDirection(-_groundCheckHit.normal);
                }
            }

            // If the Magnetic Boots are on, check for magnetic surfaces in a wider radius
            if (_input.MagneticBootsIsOn)
            {
                bool isMagneticSurfaceHit = Physics.SphereCast(_playerCenterPoint, _capsuleCollider.radius, -_rigidbody.transform.up, out RaycastHit MagneticSurfaceHit, _magneticBootsRange);
                Debug.DrawRay(_playerCenterPoint, -_rigidbody.transform.up * _magneticBootsRange, Color.red);

                // If the SphereCast hit an object
                if (isMagneticSurfaceHit) 
                {
                    // Check if the hit object is a magnetic surface
                    IMagneticSurface magneticSurfaceInBootsRange = MagneticSurfaceHit.collider.GetComponent<IMagneticSurface>();
                    if (magneticSurfaceInBootsRange != null)
                    {
                        _obbjectOfInfluence = MagneticSurfaceHit.collider.transform;
                        ChangeGravityDirection(-MagneticSurfaceHit.normal);
                    }
                }
            }

            // If there's no object of influence, reset _obbjectOfInfluence and gravity direction
            if (magneticSurface == null && _obbjectOfInfluence != null)
            {
                _obbjectOfInfluence = null;
                if (_isGravityOn) {
                    ChangeGravityDirection(Vector3.down);
                }
                else {
                    ChangeGravityDirection(Vector3.zero);
                }
            }
        }



        private Vector3 PlayerMove()
        {
            return ((_playerIsGrounded) ? (_playerCalculatedForce * _movementMultiplier) : (_playerCalculatedForce * _movementMultiplier * _notGroundedMovementMultiplier));
        }

        private Vector3 PlayerRun()
        {
            Vector3 calculatedPlayerRunSpeed = _playerCalculatedForce;
            if (_input.MoveIsPressed && _input.RunIsPressed)
            {
                calculatedPlayerRunSpeed *= _runMultiplier;
            }
            return calculatedPlayerRunSpeed;
        }
        private Vector3 PlayerFallGravity()
        {
            Vector3 gravity = _gravityDirection * _playerCalculatedForce.y;
            if (_playerIsGrounded)
            {
                _playerIsFalling = false;
                _gravityFallCurrent = _gravityFallMin; // Reset
            }
            else
            {
                _playerIsFalling = true;
                _playerFallTimer -= Time.fixedDeltaTime;
                if (_playerFallTimer < 0.0f)
                {
                    float gravityFallMax = _movementMultiplier * _runMultiplier * 5.0f;
                    float gravityFallIncrementAmount = (gravityFallMax - _gravityFallMin) * 0.1f;
                    if (_gravityFallCurrent < gravityFallMax)
                    {
                        _gravityFallCurrent += gravityFallIncrementAmount;
                    }
                    _playerFallTimer = _gravityFallIncrementTime;
                }
                gravity = _gravityFallCurrent * _gravityDirection.normalized; // gravity magnitude is directed towards _gravityDirection
            }
            return gravity;
        }

        private float PlayerJump()
        {
            float calculatedJumpInput = _playerCalculatedForce.y;

            // TODO: This is crude, may be buggy, and should be improved. Or at least named better.
            bool localPlayerIsGrounded = _playerIsGrounded;
            if (_playerIsJumping && _jumpTimeCounter > 0.0f)
            {
                localPlayerIsGrounded = false;
            }

            SetJumpTimeCounter(localPlayerIsGrounded);
            SetCoyoteTimeCounter(localPlayerIsGrounded);
            SetJumpBufferTimeCounter();

            if (_jumpBufferTimeCounter > 0.0f && !_playerIsJumping && _coyoteTimeCounter > 0.0f)
            {
                calculatedJumpInput = _initialJumpForceMultiplier;
                _playerIsJumping = true;
                _jumpBufferTimeCounter = 0.0f;
                _coyoteTimeCounter = 0.0f;
            }
            else if (_input.JumpIsPressed && _playerIsJumping && !localPlayerIsGrounded && _jumpTimeCounter > 0.0f)
            {
                calculatedJumpInput = _initialJumpForceMultiplier * _continualJumpForceMultiplier;
            }
            else if (_playerIsJumping && localPlayerIsGrounded)
            {
                _playerIsJumping = false;
            }
            return calculatedJumpInput;
        }

        private void SetJumpTimeCounter(bool localPlayerIsGrounded)
        {
            if (_playerIsJumping && !localPlayerIsGrounded)
            {
                _jumpTimeCounter -= Time.fixedDeltaTime;
            }
            else
            {
                _jumpTimeCounter = _jumpTime;
            }
        }

        private void SetCoyoteTimeCounter(bool localPlayerIsGrounded)
        {
            if (localPlayerIsGrounded)
            {
                _coyoteTimeCounter = _coyoteTime;
            }
            else
            {
                _coyoteTimeCounter -= Time.fixedDeltaTime;
            }
        }

        private void SetJumpBufferTimeCounter()
        {
            if (!_jumpWasPressedLastFrame && _input.JumpIsPressed)
            {
                _jumpBufferTimeCounter = _jumpBufferTime;
            }
            else if (_jumpBufferTimeCounter > 0.0f)
            {
                _jumpBufferTimeCounter -= Time.fixedDeltaTime;
            }
            _jumpWasPressedLastFrame = _input.JumpIsPressed;
        }

        private void EnforceMinimumVelocity()
        {
            if (_rigidbody.velocity.magnitude < _minimumVelocityMagnitude)
            {
                _rigidbody.velocity = Vector3.zero;
            }
        }

        // ----------------------
        // -- HELPER FUNCTIONS --
        // ----------------------

        private bool IsWithinRange(float value, float min, float max) => (value >= min && value <= max) || Mathf.Approximately(value, min) || Mathf.Approximately(value, max);
        
        public void TurnGravityOnOrOff(bool OnOrOff)
        {
            _isGravityOn = OnOrOff;
        }

        public void ChangeGravityDirection(Vector3 newDirection)
        {
            _gravityDirection = newDirection.normalized;
        }
    }
}