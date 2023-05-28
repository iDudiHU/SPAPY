using System;
using System.Collections;
using System.Collections.Generic;
using TigrisDigitalCreative._Input;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] InputManager m_Input = null;

    [SerializeField] Rigidbody m_Rigidbody = null;
    [SerializeField] PlayerPhysicsMovement m_PlayerController = null;
    [SerializeField] Animator _animator;
    [Header("Animation Variables")]
    [SerializeField] private float animBlendSpeed = 8.9f;
    private bool _hasAnimator;
    private Rigidbody _rb;
    
    private int _xVelHash;
    private int _zVelHash;
    
    int _IsFallingHash = 0;
    int _IsJumpingHash = 0;
    int _IsWallRunningHash = 0;
    int _IsGroundedHash = 0;
    
    bool _IsFalling = false;
    float _fallCounter = 0.0f;
    bool _IsJumping = false; 
    bool _IsWallRunning = false;
    bool _IsGrounded = false;

    private void Start()
    {
        
        ReferenceSetup();
        AnimatioStringsSetup();
        
    }

    private void Update()
    {
        SetLocomotionBlendTreeAnimation();
        bool isFallingThisFrame = Falling();
        if (_IsFalling != isFallingThisFrame)
        {
            _IsFalling = isFallingThisFrame;
            _IsGrounded = !isFallingThisFrame;
            _animator.SetBool(_IsFallingHash, isFallingThisFrame);
            _animator.SetBool(_IsGroundedHash, _IsGrounded);
        }
        bool isJumpingThisFrame = m_PlayerController.m_IsJumping;
        if (_IsJumping != isJumpingThisFrame)
        {
            _IsJumping = isJumpingThisFrame;
            _animator.SetBool(_IsJumpingHash, isJumpingThisFrame);
        }
    }

    private void AnimatioStringsSetup()
    {
        _xVelHash = Animator.StringToHash("X_Velocity");
        _zVelHash = Animator.StringToHash("Z_Velocity");
        _IsFallingHash = Animator.StringToHash("IsFalling");
        _IsWallRunningHash = Animator.StringToHash("IsWallRunning");
        _IsJumpingHash = Animator.StringToHash("IsJumping");
        _IsGroundedHash = Animator.StringToHash("IsGrounded");
    }

    private void ReferenceSetup()
    {
        if (_animator == null) {
            _hasAnimator = TryGetComponent<Animator>(out _animator);
            if (_animator == null) {
                Debug.Log($"{gameObject.name}: Can't find Animator.");
            }
        }

        if (m_Input == null) {
            m_Input = GetComponentInParent<InputManager>();
            if (m_Input == null) {
                Debug.Log($"{gameObject.name}: Can't find InputManager.");
            }
        }
        
        if (m_PlayerController == null) {
            m_PlayerController = GetComponentInParent<PlayerPhysicsMovement>();
            if (m_PlayerController == null) {
                Debug.Log($"{gameObject.name}: Can't find PlayerPhysicsMovement.");
            }
        }
        
        if (m_Rigidbody == null) {
            m_Rigidbody = GetComponentInParent<Rigidbody>();
            if (m_Rigidbody == null) {
                Debug.Log($"{gameObject.name}: Can't find Rigidbody.");
            }
        }
    }

    private void SetLocomotionBlendTreeAnimation()
    {
        Vector3 localVel = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.velocity);
        _animator.SetFloat(_xVelHash, localVel.x);
        _animator.SetFloat(_zVelHash, localVel.z);
    }
    private bool Falling()
    {
        bool falling = false;
        if (!(m_PlayerController.m_IsJumping) && !(m_PlayerController.IsGrounded))
        {
            if (_fallCounter >= 0.2f)
            {
                falling = true;
            }
            else
            {
                _fallCounter += Time.deltaTime;
            }
        }
        else
        {
            _fallCounter = 0.0f;
        }
        return falling;
    }

    /*
    public void SetAnimationDirecction(float xVelocity, float zVelocity)
    {
        if (_hasAnimator) {
            _animator.SetFloat(_xVelHash, xVelocity);
            Debug.Log($"Xvelocity: {xVelocity}");
            _animator.SetFloat(_zVelHash, zVelocity);
            Debug.Log($"Zvelocity: {zVelocity}");
        }
    }
    */
    
    
}
