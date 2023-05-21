using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaos : MonoBehaviour
{
    Rigidbody _rigidbody;
    HumanoidLandInput _input;

    float _chaosStartMultiplier = 7.5f;
    float _chaosEndMultiplier = 22.5f;

    bool _chaosWasPressedLastFrame = false;
    float _gameTimeChaosLastPressed = 0.0f;
    float _chaosCooldownTimeAmount = 0.25f; // in seconds

    float _chaosActiveTime = 0.0f;

    private void Awake()
    {
        _input = GameObject.Find("InputHandler").GetComponent<HumanoidLandInput>();
        _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.useGravity = true;
    }

    private void FixedUpdate()
    {
        if (_input.ChaosIsPressed && Time.time - _gameTimeChaosLastPressed > _chaosCooldownTimeAmount)
        {
            _rigidbody.useGravity = false;

            float randomValue = Random.value * _rigidbody.mass;
            _rigidbody.AddForce(new Vector3(0.0f, randomValue * _chaosStartMultiplier, 0.0f));
            _rigidbody.AddRelativeTorque(new Vector3(randomValue, randomValue, randomValue));

            if (!(_chaosWasPressedLastFrame))
            {
                _chaosActiveTime = Time.time;
            }

            _chaosWasPressedLastFrame = true;
        }
        else if (!(_rigidbody.useGravity) && _chaosWasPressedLastFrame)
        {
            _rigidbody.useGravity = true;

            float randomValue = Random.value * _rigidbody.mass * (Time.time - _chaosActiveTime) * _chaosEndMultiplier;
            _rigidbody.AddRelativeForce(new Vector3(0.0f, randomValue, 0.0f), ForceMode.Impulse);

            _gameTimeChaosLastPressed = Time.time;
            _chaosWasPressedLastFrame = false;
        }
    }
}
