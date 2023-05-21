using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] HumanoidLandInput _input;
    [SerializeField] CameraController _cameraController;
    [SerializeField] HumanoidLandController _redController;
    [SerializeField] HumanoidLandController _blueController;

    bool _isvSyncEnabled = false;
    int _targetFrameRate = 120;
    float _targetPhysicsRate = 60.0f;

    private void Awake()
    {
        QualitySettings.vSyncCount = _isvSyncEnabled ? 1 : 0;

        Application.targetFrameRate = _targetFrameRate;

        Time.fixedDeltaTime = 1.0f / _targetPhysicsRate;
        Time.maximumDeltaTime = Time.fixedDeltaTime * 1.25f;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (_input.SwitchCharacterWasPressedThisFrame)
        {
            _redController.enabled = !_redController.enabled;
            _blueController.enabled = !_blueController.enabled;
            _cameraController.cinemachine3rdPerson.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = 
                (_redController.enabled ? _redController.CameraFollow : _blueController.CameraFollow);
            _cameraController.cinemachine3rdPerson.GetComponent<Cinemachine.CinemachineVirtualCamera>().LookAt = 
                (_redController.enabled ? _redController.CameraFollow : _blueController.CameraFollow);

            _cameraController.cinemachine1stPerson.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = 
                (_redController.enabled ? _redController.CameraFollow : _blueController.CameraFollow);

            _cameraController.cinemachineOrbit.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = 
                (_redController.enabled ? _redController.CameraFollow : _blueController.CameraFollow);
            _cameraController.cinemachineOrbit.GetComponent<Cinemachine.CinemachineVirtualCamera>().LookAt = 
                (_redController.enabled ? _redController.CameraFollow : _blueController.CameraFollow);

            _redController.GetComponentInChildren<Manipulator>().enabled = !_redController.GetComponentInChildren<Manipulator>().enabled;
            _blueController.GetComponentInChildren<Manipulator>().enabled = !_blueController.GetComponentInChildren<Manipulator>().enabled;
        }
    }
}
