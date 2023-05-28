using TigrisDigitalCreative._Input;
using UnityEngine;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;

namespace TigrisDigitalCreative._Scripts.Controllers
{
    public class PlayerMovementController : MonoBehaviour
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
        
        [Header("Skin Width Config")]
        [SerializeField] float _skinWidth = 0.025f;
        [SerializeField] bool _avoidAscendingTooSteepOfSlope = true;

        [Header("Skin Width Debug")]
        [SerializeField] bool _playerSurroundingsThisFrame = true;

        [Header("Skin Width")]
        float _maxCapsuleColliderRadius = 0.0f;


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
        [SerializeField] private float _magneticBootsRange = 10;
        [SerializeField] private float _magneticBootslerpTime = 360f;

        [Header("Gravity Debug")]
        [SerializeField] bool _playerIsFalling = false;
        [SerializeField] float _gravityFallCurrent = 0.0f;
        [SerializeField] float _playerFallTimer = 0.0f;


        [Header("Slope Config")]
        [SerializeField] float _maxSlopeAngle = 47.5f;

        [Header("Slope Debug")]
        [SerializeField] bool _playerIsOnSlope = false;
        [SerializeField] bool _playerIsSliding = false;
        [SerializeField] bool _playerWasSlidingLastFrame = false;
        [SerializeField] float _slideCounter = 0.0f;
        
        [Header("Stairs Config")]
        [SerializeField][Range(0.0f, 1.0f)] float _maxStepHeight = 0.5f;
        [SerializeField][Range(0.0f, 1.0f)] float _stepHeightTolerance = 0.05f;
        [SerializeField][Range(0.0f, 1.0f)] float _maxStepDepth = 0.5f;
        [SerializeField] float _stairRiserAngleTolerance = 2.5f;
        [SerializeField] float _ascendingStairsMovementMultiplier = 0.7f;
        [SerializeField] float _descendingStairsMovementMultiplier = 0.7f;
        [SerializeField] float _maximumAngleOfApproachToAscend = 89.9f;
        [SerializeField] float _maximumAngleOfApproachToDescend = 89.9f;
        [SerializeField] float _stairFloatModifier = 1.1f;

        [Header("Stairs Debug")]
        [SerializeField] bool _playerIsAscendingStairs = false;
        [SerializeField] bool _playerIsDescendingStairs = false;
        [SerializeField] float _playerHalfHeightToGround = 0.0f;

        [Header("Stairs")]
        float _stairNecessaryFloatHeightDifference = 0.0f;
        bool _resetFloat = false;
        int _numberOfStepDetectRays = 0;
        float _rayIncrementAmount = 0.0f;


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
            
            _maxCapsuleColliderRadius = _capsuleCollider.radius;
            
            _numberOfStepDetectRays = Mathf.RoundToInt((_maxStepHeight * 100.0f) * 0.5f);
            _rayIncrementAmount = _maxStepHeight / _numberOfStepDetectRays;

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

            _playerCalculatedForce = PlayerStairs();
            _playerCalculatedForce = PlayerSlope();

            _playerCalculatedForce = PlayerSurroundings();

            _playerCalculatedForce = PlayerMove();
            _playerCalculatedForce = PlayerRun();

            if (_isArtificialGravityOn)
            {
                // Calculate the direction to the object of influence
                _gravityDirection = (_obbjectOfInfluence.position - transform.position).normalized;

                // Adjust the player's rotation to align with the direction of gravity
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -_gravityDirection) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _magneticBootslerpTime * Time.deltaTime);
            }

            if (_isGravityOn || _input.MagneticBootsIsOn || _isArtificialGravityOn) 
            {
                _playerCalculatedForce += PlayerFallGravity();
            }

            RecenterPlayerCollider();
            RigidbodyPlayerReaction(); 

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
            SetSkinWidth();
            _isArtificialGravityOn = (_input.MagneticBootsIsOn && _obbjectOfInfluence != null);
        }

        private void SetSkinWidth()
        {
            if (_skinWidth < 0.0f)
            {
                _skinWidth = 0.0f;
            }
            else if (_skinWidth > _maxCapsuleColliderRadius * 0.5f)
            {
                _skinWidth = _maxCapsuleColliderRadius * 0.5f;
            }
            _capsuleCollider.radius = _maxCapsuleColliderRadius - _skinWidth;
        }
        /*
        private bool PlayerGroundCheck()
        {
            // Find the center point of the player
            _playerCenterPoint = _rigidbody.position;

            // Perform a SphereCast downwards from the center point
            float sphereCastRadius = _capsuleCollider.radius * _groundCheckRadiusMultiplier;
            float sphereCastTravelDistance =
                _capsuleCollider.bounds.extents.y - sphereCastRadius + _groundCheckDistanceTolerance;
            bool ground = Physics.SphereCast(_playerCenterPoint, sphereCastRadius, -_rigidbody.transform.up, out _groundCheckHit, sphereCastTravelDistance);
            if (ground) {
                bool newGround = Physics.SphereCast(_playerCenterPoint, _capsuleCollider.radius, -_rigidbody.transform.up, out _groundCheckHit, sphereCastTravelDistance);
                if ((newGround && _input.MagneticBootsIsOn) || !_isGravityOn) {
                    ChangeGravityDirection(-_groundCheckHit.normal);
                    TurnGravityOnOrOff(true);
                }
                
            }
            return ground;
        }*/
        private bool PlayerGroundCheck()
        {
            _playerCenterPoint = _rigidbody.position;

            float sphereCastRadius = _capsuleCollider.radius * _groundCheckRadiusMultiplier;
            float sphereCastTravelDistance = _capsuleCollider.bounds.extents.y - sphereCastRadius + _groundCheckDistanceTolerance;

            bool isGrounded = Physics.SphereCast(_playerCenterPoint, sphereCastRadius, -_rigidbody.transform.up, out _groundCheckHit, sphereCastTravelDistance);
            Debug.DrawRay(_playerCenterPoint, -_rigidbody.transform.up * sphereCastTravelDistance, Color.red);

            IMagneticSurface magneticSurface = null;
            if (isGrounded)
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
                        return true;
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

            return isGrounded;
        }
        
        private Vector3 PlayerStairs()
        {
            Vector3 calculatedStepInput = _playerCalculatedForce;

            bool playerWasAscendingStairsLastFrame = _playerIsAscendingStairs; // cache
            bool playerWasDescendingStairsLastFrame = _playerIsDescendingStairs; // cache

            _playerIsAscendingStairs = false; // reset
            _playerIsDescendingStairs = false; // reset

            if (_input.MoveIsPressed)
            {
                calculatedStepInput = StairHandling(true, calculatedStepInput);
                if (!_playerIsAscendingStairs)
                {
                    calculatedStepInput = StairHandling(false, calculatedStepInput);
                }
            }

            return calculatedStepInput;
        }

        private Vector3 StairHandling(bool Ascending, Vector3 calculatedStepInput)
        {
            float ray = _rayIncrementAmount;
            Vector3 localMoveInput = _playerMoveInput;
            float maximumAngleOfApproach = _maximumAngleOfApproachToAscend;
            if (!(Ascending))
            {
                ray = -ray;
                localMoveInput = -localMoveInput;
                maximumAngleOfApproach = _maximumAngleOfApproachToDescend;
            }
            float centerToSkinDistance = _capsuleCollider.radius + _skinWidth;
            float baseCastDistance = centerToSkinDistance + _maxStepDepth;

            float lowerMaxCastDistance = baseCastDistance / Mathf.Cos(maximumAngleOfApproach * Mathf.Deg2Rad);

            bool performChecks = false;

            RaycastHit closestRay = new RaycastHit();
            for (int x = 1;
                 x <= _numberOfStepDetectRays;
                 x++, ray += _rayIncrementAmount)
            {
                Vector3 rayLower = new Vector3(_playerCenterPoint.x, ((_playerCenterPoint.y - _playerHalfHeightToGround) + ray), _playerCenterPoint.z);
                Debug.DrawRay(rayLower, _rigidbody.transform.TransformDirection(localMoveInput) * 5.0f, Color.cyan, 1.0f);

                RaycastHit hitLower = new RaycastHit();
                Physics.Raycast(rayLower, _rigidbody.transform.TransformDirection(localMoveInput), out hitLower, lowerMaxCastDistance);
                if (!(hitLower.collider == null) && !(hitLower.collider.attachedRigidbody))
                {
                    float angleOfApproach = Vector3.Angle(hitLower.normal, _rigidbody.transform.TransformDirection(-localMoveInput));
                    if (angleOfApproach < maximumAngleOfApproach)
                    {
                        float maxAllowedDistance = baseCastDistance / Mathf.Cos(angleOfApproach * Mathf.Deg2Rad);
                        if (hitLower.distance <= maxAllowedDistance)
                        {
                            float verticalAngle = Vector3.Angle(hitLower.normal, _rigidbody.transform.up);
                            if (IsWithinRange(verticalAngle, 90.0f - _stairRiserAngleTolerance, 90.0f + _stairRiserAngleTolerance))
                            {
                                if (closestRay.collider == null)
                                {
                                    performChecks = true;
                                    closestRay = hitLower;
                                }
                                else if (hitLower.distance <= closestRay.distance)
                                {
                                    closestRay = hitLower;
                                }
                            }
                        }
                    }
                }
            }
            if (performChecks)
            {
                Vector3 lowerRayCastPoint = closestRay.point + (_rayIncrementAmount * (new Vector3(_playerCenterPoint.x, closestRay.point.y, _playerCenterPoint.z) - closestRay.point).normalized);
                // Gets a point that is 1 _rayIncrementAmount closer to the player
                RaycastHit lowerStepHeightDetectRay = new RaycastHit();
                if (Physics.Raycast(lowerRayCastPoint, -_rigidbody.transform.up, out lowerStepHeightDetectRay, _maxStepHeight * 2.0f, _layerMaskEverythingButPlayer))
                {
                    Vector3 UpperRayCastPoint = (closestRay.point - (_rayIncrementAmount * (new Vector3(_playerCenterPoint.x, closestRay.point.y, _playerCenterPoint.z) - closestRay.point).normalized)) + (_rigidbody.transform.up * _maxStepDepth);
                    // Gets a point that is 1 _rayIncrementAmount further from the player and _maxStepDepth further 'up' (relative to the player)
                    Debug.DrawRay(UpperRayCastPoint, Vector3.up, Color.yellow, 5.0f);
                    RaycastHit upperStepHeightDetectRay = new RaycastHit();
                    if (Physics.Raycast(UpperRayCastPoint, -_rigidbody.transform.up, out upperStepHeightDetectRay, _maxStepHeight * 3.0f, _layerMaskEverythingButPlayer))
                    {
                        float stepHeight = upperStepHeightDetectRay.point.y - lowerStepHeightDetectRay.point.y;
                        if (IsWithinRange(stepHeight, 0.01f, _maxStepHeight))
                        {
                            //if (stepHeight > _minStepHeight)
                            //{

                            //}
                            float angleOfApproach = Vector3.Angle(closestRay.normal, _rigidbody.transform.TransformDirection(-localMoveInput));
                            float angleRatio = 0.0f;
                            float anglePenalty = 0.0f;
                            float movementMultiplier = 0.0f;
                            if (Ascending)
                            {
                                _playerIsAscendingStairs = true;
                                //_playerIsGrounded = true;

                                movementMultiplier = _ascendingStairsMovementMultiplier;

                                angleRatio = Mathf.Clamp(angleOfApproach, 0.0f, _maximumAngleOfApproachToAscend) / _maximumAngleOfApproachToAscend;
                                anglePenalty = Mathf.Pow(angleRatio, 1.0f) * _ascendingStairsMovementMultiplier;
                            }
                            else
                            {
                                _playerIsDescendingStairs = true;

                                movementMultiplier = _descendingStairsMovementMultiplier;

                                angleRatio = Mathf.Clamp(angleOfApproach, 0.0f, _maximumAngleOfApproachToDescend) / _maximumAngleOfApproachToDescend;
                                anglePenalty = Mathf.Pow(angleRatio, 1.0f) * _descendingStairsMovementMultiplier;
                            }

                            float stepHeightRatio = stepHeight / _maxStepHeight;
                            float stepHeightPenalty = Mathf.Pow(stepHeightRatio, 1.0f) * movementMultiplier;

                            float penalty = Mathf.Clamp01(stepHeightPenalty - anglePenalty);

                            calculatedStepInput *= 1.0f - penalty;
                        }
                    }
                }
            }
            return calculatedStepInput;
        }

        private Vector3 PlayerSlope()
        {
            Vector3 calculatedPlayerMovement = _playerCalculatedForce;
            _playerIsOnSlope = false; // reset
            _playerIsSliding = false; // reset

            if (_playerIsGrounded && !_playerIsAscendingStairs && !_playerIsDescendingStairs)
            {
                Vector3 localGroundCheckHitNormal = _rigidbody.transform.InverseTransformDirection(_groundCheckHit.normal);

                float groundSlopeAngle = Vector3.Angle(localGroundCheckHitNormal, _rigidbody.transform.up);
                _playerIsOnSlope = groundSlopeAngle != 0.0f;

                if (_playerIsOnSlope)
                {
                    bool slopeIsTooSteep = groundSlopeAngle > _maxSlopeAngle;
                    if (slopeIsTooSteep)
                    {
                        if (_slideCounter < 1.0f)
                        {
                            _slideCounter += Time.fixedDeltaTime;
                        }
                        else if (_slideCounter > 1.0f)
                        {
                            _slideCounter = 1.0f;

                        }

                        if (_input.MoveIsPressed)
                        {
                            Quaternion slopeAngleRotation = Quaternion.FromToRotation(_rigidbody.transform.up, localGroundCheckHitNormal);
                            calculatedPlayerMovement = slopeAngleRotation * calculatedPlayerMovement;
                            float relativeSlopeAngle = Vector3.Angle(calculatedPlayerMovement, _rigidbody.transform.up) - 90.0f;
                            calculatedPlayerMovement += (calculatedPlayerMovement * (relativeSlopeAngle / _maxSlopeAngle) * _slideCounter);

                            // Prevent going up too steep of a slope at an angle
                            calculatedPlayerMovement += Vector3.ProjectOnPlane(-_rigidbody.transform.up, localGroundCheckHitNormal);
                        }
                        else
                        {
                            calculatedPlayerMovement = Vector3.ProjectOnPlane(-_rigidbody.transform.up, localGroundCheckHitNormal);
                            float relativeSlopeAngle = Vector3.Angle(calculatedPlayerMovement, _rigidbody.transform.up) - 90.0f;
                            calculatedPlayerMovement += calculatedPlayerMovement * (relativeSlopeAngle / _maxSlopeAngle);

                            _playerIsGrounded = false;
                        }
                        _playerIsSliding = true;
                    }
                    else
                    {
                        if (_input.MoveIsPressed)
                        {
                            Quaternion slopeAngleRotation = Quaternion.FromToRotation(_rigidbody.transform.up, localGroundCheckHitNormal);
                            calculatedPlayerMovement = slopeAngleRotation * calculatedPlayerMovement;
                            float relativeSlopeAngle = Vector3.Angle(calculatedPlayerMovement, _rigidbody.transform.up) - 90.0f;
                            calculatedPlayerMovement += calculatedPlayerMovement * (relativeSlopeAngle / 90.0f);
                        }
                    }
                }
            }

            if (!(_playerIsSliding) && _playerWasSlidingLastFrame)
            {
                _slideCounter = 0.0f;
            }
            _playerWasSlidingLastFrame = _playerIsSliding;

            return calculatedPlayerMovement;
        }

        private Vector3 PlayerSurroundings()
        {
            Vector3 calculatedInput = _playerCalculatedForce;

            if (_input.MoveIsPressed && _playerSurroundingsThisFrame && !(_playerIsSliding) && !(_playerIsAscendingStairs) && !(_playerIsDescendingStairs) && _skinWidth > 0.0f)
            {
                float castRadius = _capsuleCollider.radius;

                Vector3 normPlayerMoveInput = _rigidbody.transform.TransformDirection(_playerMoveInput).normalized;
                Vector3 modPosition = _playerCenterPoint - (normPlayerMoveInput * _skinWidth);

                Vector3 capsuleTopSphere = new Vector3(modPosition.x, modPosition.y + _capsuleCollider.height/2 - castRadius, modPosition.z);
                Vector3 capsuleBottomSphere = new Vector3(modPosition.x, modPosition.y - _playerHalfHeightToGround + castRadius, modPosition.z);

                RaycastHit capsuleHit = new RaycastHit();
                Physics.CapsuleCast(capsuleTopSphere, capsuleBottomSphere, castRadius, _rigidbody.transform.TransformDirection(_playerMoveInput),
                    out capsuleHit, _skinWidth * 2.0f, _layerMaskEverythingButPlayer);
                if (!(capsuleHit.collider == null) && capsuleHit.collider.attachedRigidbody == null)
                {
                    float verticalAngle = Vector3.Angle(capsuleHit.normal, _rigidbody.transform.up);
                    if (verticalAngle <= 90.0f && verticalAngle > _maxSlopeAngle)
                    {
                        // TODO: Make this better
                        // Crude implementation to limit jittering when at the base of a slope that is too steep to ascend
                        // This has undesirable implications...
                        Vector3 normal = capsuleHit.normal;
                        if (_avoidAscendingTooSteepOfSlope)
                        {
                            normal = Vector3.Scale(Vector3.Normalize(capsuleHit.normal), new Vector3(1, 0, 1));
                        }

                        _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, normal);
                        calculatedInput = Vector3.ProjectOnPlane(calculatedInput, _rigidbody.transform.InverseTransformDirection(normal));
                    }
                }
            }
            _playerSurroundingsThisFrame = true; // reset
            return calculatedInput;
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

        /*private float PlayerFallGravity()
        {
            float gravity = _playerCalculatedForce.y;
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
                gravity = -_gravityFallCurrent;
            }
            return gravity;
        }*/
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


        private void RecenterPlayerCollider()
        {
            _capsuleCollider.center = Vector3.zero;
            //_capsuleCollider.center = _rigidbody.transform.up * (_capsuleCollider.height);
        }

        private void RigidbodyPlayerReaction()
        {
            /*
            if (!_playerIsFalling && _groundCheckHit.collider.attachedRigidbody != null)
            {
                Vector3 foreignForce = new Vector3(_playerCalculatedForce.x, _playerCalculatedForce.y, _playerCalculatedForce.z);
                _groundCheckHit.collider.attachedRigidbody.AddForceAtPosition(-foreignForce, _groundCheckHit.point, ForceMode.Force);
            }*/
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