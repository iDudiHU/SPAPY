using System;
using UnityEngine;
using Cinemachine;
using TigrisDigitalCreative._Input;

public class PlayerCameraController : MonoBehaviour
{
    public bool UsingOrbitalCamera { get; private set; } = false;

    [SerializeField] InputManager _input;
    [SerializeField]  Rigidbody _rigidbody;

    [SerializeField] float _cameraZoomModifier = 32.0f;

    float _minCameraZoomDistance = 0.0f;
    float _minOrbitCameraZoomDistance = 1.0f;
    float _maxCameraZoomDistance = 12.0f;
    float _maxOrbitCameraZoomDistance = 36.0f;

    CinemachineVirtualCamera _activeCamera;
    int _activeCameraPriorityModifer = 31337;

    public Camera MainCamera;
    public CinemachineVirtualCamera cinemachine1stPerson;
    public CinemachineVirtualCamera cinemachine3rdPerson;
    CinemachineFramingTransposer _cinemachineFramingTransposer3rdPerson;
    public CinemachineVirtualCamera cinemachineOrbit;
    CinemachineFramingTransposer _cinemachineFramingTransposerOrbit;
    
    [Header("Camera Config")]
    public Transform CameraFollow;
    [SerializeField] float _rotationSpeedMultiplier = 180.0f;
    [SerializeField] float _pitchSpeedMultiplier = 180.0f;
    [SerializeField] float _playerLookInputLerpTime = 0.35f;
    Quaternion _gravityAlignment = Quaternion.identity;

    [Header("Camera Debug")]
    [SerializeField] float _cameraPitch = 0.0f;

    [Header("Camera")]
    Vector3 _playerLookInput = Vector3.zero;
    Vector3 _previousPlayerLookInput = Vector3.zero;

    private void Awake()
    {
        _cinemachineFramingTransposer3rdPerson = cinemachine3rdPerson.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cinemachineFramingTransposerOrbit = cinemachineOrbit.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void Start()
    {
        ChangeCamera(); // First time through, lets set the default camera.
    }

    private void Update()
    {
        if (!(_input.ZoomCameraInput == 0.0f)) { ZoomCamera();  }
        if (_input.ChangeCameraWasPressedThisFrame) { ChangeCamera(); }
    }

    private void FixedUpdate()
    {
        if (!UsingOrbitalCamera)
        {
            _playerLookInput = GetLookInput();
            PlayerLook();
            PitchCamera();
        }
    }

    private void LateUpdate()
    {
        _gravityAlignment = Quaternion.FromToRotation(_gravityAlignment * Vector3.up, CustomGravity.GetGravity(_rigidbody.position)) * _gravityAlignment;
    }

    private void ZoomCamera()
    { 
        if (_activeCamera == cinemachine3rdPerson)
        {
            _cinemachineFramingTransposer3rdPerson.m_CameraDistance = Mathf.Clamp(_cinemachineFramingTransposer3rdPerson.m_CameraDistance +
                                (_input.InvertScroll ? _input.ZoomCameraInput : -_input.ZoomCameraInput) / _cameraZoomModifier,
                                _minCameraZoomDistance,
                                _maxCameraZoomDistance);
        }
        else if (_activeCamera == cinemachineOrbit)
        {
            _cinemachineFramingTransposerOrbit.m_CameraDistance = Mathf.Clamp(_cinemachineFramingTransposerOrbit.m_CameraDistance +
                                (_input.InvertScroll ? _input.ZoomCameraInput : -_input.ZoomCameraInput) / _cameraZoomModifier,
                                _minOrbitCameraZoomDistance,
                                _maxOrbitCameraZoomDistance);
        }
    }

    private void ChangeCamera()
    {
        if (cinemachine3rdPerson == _activeCamera)
        {
            SetCameraPriorities(cinemachine3rdPerson, cinemachine1stPerson);
            UsingOrbitalCamera = false;
            MainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
        }
        else if (cinemachine1stPerson == _activeCamera)
        {
            SetCameraPriorities(cinemachine1stPerson, cinemachineOrbit);
            UsingOrbitalCamera = true;
            MainCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
        }
        else if (cinemachineOrbit == _activeCamera)
        {
            SetCameraPriorities(cinemachineOrbit, cinemachine3rdPerson);
            _activeCamera = cinemachine3rdPerson;
            UsingOrbitalCamera = false;
        }
        else // for first time through or if there's an error
        {
            cinemachine3rdPerson.Priority += _activeCameraPriorityModifer;
            _activeCamera = cinemachine3rdPerson;
        }
    }

    private void SetCameraPriorities(CinemachineVirtualCamera CurrentCameraMode, CinemachineVirtualCamera NewCameraMode)
    {
        CurrentCameraMode.Priority -= _activeCameraPriorityModifer;
        NewCameraMode.Priority += _activeCameraPriorityModifer;
        _activeCamera = NewCameraMode;
    }
    
    private Vector3 GetLookInput()
    {
        _previousPlayerLookInput = _playerLookInput;
        _playerLookInput = new Vector3(_input.LookInput.x, (_input.InvertMouseY ? -_input.LookInput.y : _input.LookInput.y), 0.0f);
        return Vector3.Lerp(_previousPlayerLookInput, _playerLookInput * Time.deltaTime, _playerLookInputLerpTime);
    }
   // private void PlayerLook()
   // {
   //     float rotationDelta = _playerLookInput.x * _rotationSpeedMultiplier;
   //     Quaternion rotation = Quaternion.AngleAxis(rotationDelta, CustomGravity.GetUpAxis(_rigidbody.position));
   //     Vector3 dir = Vector3.ProjectOnPlane(rotation * _rigidbody.transform.forward, CustomGravity.GetUpAxis(_rigidbody.position));

   //     // Ensure the direction vector is not zero
   //     if (dir != Vector3.zero) {
   //         Quaternion targetRotation = Quaternion.LookRotation(dir, CustomGravity.GetUpAxis(_rigidbody.position));
   //         //This rotates the player/ I can't rotate the player towards a rotation I have to add the offset to the current rotation in local space
			//_rigidbody.transform.rotation = Quaternion.RotateTowards(_rigidbody.transform.rotation, targetRotation, _rotationSpeedMultiplier * Time.deltaTime);
   //     }
   // }
	private void PlayerLook()
	{
		// Calculate the desired rotation on the Y-axis
		float rotationDelta = _playerLookInput.x * _rotationSpeedMultiplier;

		// Create a Quaternion representing the desired rotation
		Quaternion desiredRotation = Quaternion.Euler(0f, rotationDelta, 0f);

		// Apply the desired rotation to the player's current rotation
		// Only affects the Y-axis
		_rigidbody.transform.localRotation *= desiredRotation;
	}

	private void PitchCamera()
    {
        _cameraPitch += _playerLookInput.y * _pitchSpeedMultiplier;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -60f, 70f);

        Quaternion desiredRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

        CameraFollow.localRotation = desiredRotation;
    }




    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
}
