using System.Collections.Generic;
using UnityEngine;

public class Manipulator : MonoBehaviour
{
    const float GravitationalConstant = 0.667408f;

    [SerializeField] HumanoidLandInput _input;
    [SerializeField] OnScreenCounter _counter;

    MeshRenderer _meshRenderer;

    private List<Rigidbody> Attractees = new List<Rigidbody>();

    [SerializeField] bool _manipulatorIsEnabled = false;
    [SerializeField] bool _manipulatorToggledOn = false;
    [SerializeField] bool _manipulatorModeToggled = false;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (_input.OnOffWasPressedThisFrame) { OnOff(); }
        if (_input.ModeWasPressedThisFrame) { ChangeMode(); }
        _manipulatorToggledOn = _input.ActivateInput > 0.0f;
        SetColor();
    }

    private void OnOff()
    {
        _manipulatorIsEnabled = !_manipulatorIsEnabled;
    }

    private void ChangeMode()
    {
        _manipulatorModeToggled = !_manipulatorModeToggled;
    }

    private void SetColor() // attract = blue || repel = red
    {
        if (_manipulatorIsEnabled || _manipulatorToggledOn)
        {
            if (_manipulatorModeToggled)
            {
                _meshRenderer.material.color = Color.red;
            }
            else
            {
                _meshRenderer.material.color = Color.blue;
            }
        }
        else
        {
            if (_manipulatorModeToggled)
            {
                _meshRenderer.material.color = Color.white + Color.red;
            }
            else
            {
                _meshRenderer.material.color = Color.white + Color.blue;
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody attractee in Attractees)
        {
            if (attractee != this)
            {
                Attract(attractee);
            }
        }
    }

    private void Attract(Rigidbody rbToAttract)
    {
        if (_manipulatorIsEnabled || _manipulatorToggledOn)
        {
            Vector3 direction = transform.position - rbToAttract.position;
            float distance = direction.magnitude;

            if (distance == 0.0f) { return; } // NOTE: If "on top of each other" then don't apply any force (exit). Or a minimum distance could be set.

            float forceMagnitude = 0.0f;
            if (_input.ActivateInput > 0.0f)
            {
                forceMagnitude = GravitationalConstant * (750.0f * rbToAttract.mass) / distance * _input.ActivateInput;
            }
            else
            {
                forceMagnitude = GravitationalConstant * (750.0f * rbToAttract.mass) / distance;
            }
            Vector3 force = direction.normalized * forceMagnitude;

            if (_manipulatorModeToggled)
            {
                rbToAttract.AddForce(-force);
            }
            else
            {
                rbToAttract.AddForce(force);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!(other.attachedRigidbody == null) && !(other.attachedRigidbody.isKinematic))
        {
            if (!(Attractees.Contains(other.attachedRigidbody))) // NOTE: Large objects (such as "planets") were getting added to the list multiple times. This check solves this issue.
            {
                Attractees.Add(other.attachedRigidbody);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Attractees.Contains(other.attachedRigidbody))
        {
            if (_manipulatorIsEnabled || _manipulatorToggledOn)
            {
                other.attachedRigidbody.useGravity = false;
            }
            else
            {
                if (!(other.gameObject.CompareTag("Player")))
                {
                    other.attachedRigidbody.useGravity = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!(other.attachedRigidbody == null))
        {
            if (Attractees.Contains(other.attachedRigidbody))
            {
                Attractees.Remove(other.attachedRigidbody);
                if (!(other.gameObject.CompareTag("Player")))
                {
                    other.attachedRigidbody.useGravity = true;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_manipulatorIsEnabled || _manipulatorToggledOn)
        {
            if (Attractees.Contains(collision.rigidbody))
            {
                if (!(collision.gameObject.CompareTag("Player")))
                {
                    Attractees.Remove(collision.rigidbody);
                    Destroy(collision.gameObject);
                    _counter.Counter++;
                }
            }
        }
    }
}
