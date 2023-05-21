using UnityEngine;

public class HumanoidAnimationController : MonoBehaviour
{
    Rigidbody _rigidbody = null;
    Animator _animator = null;
    HumanoidLandController _controller = null;

    int _VelXHash = 0;
    int _VelZHash = 0;

    int _IsFallingHash = 0;
    int _IsCrouchingHash = 0;
    int _IsJumpingHash = 0;

    bool _IsFalling = false;
    bool _IsCrouching = false;
    bool _IsJumping = false;

    float _fallCounter = 0.0f;

    void Start()
    {
        _rigidbody = GetComponentInParent<Rigidbody>();
        _controller = GetComponentInParent<HumanoidLandController>();
        _animator = GetComponent<Animator>();

        _VelXHash = Animator.StringToHash("VelX");
        _VelZHash = Animator.StringToHash("VelZ");
        
        _IsFallingHash = Animator.StringToHash("IsFalling");
        _IsCrouchingHash = Animator.StringToHash("IsCrouching");
        _IsJumpingHash = Animator.StringToHash("IsJumping");
    }

    void Update()
    {
        Vector3 localVel = _rigidbody.transform.InverseTransformDirection(_rigidbody.velocity);
        _animator.SetFloat(_VelXHash, localVel.x);
        _animator.SetFloat(_VelZHash, localVel.z);

        bool isFallingThisFrame = Falling();
        if (!(_IsFalling == isFallingThisFrame))
        {
            _IsFalling = isFallingThisFrame;
            _animator.SetBool(_IsFallingHash, isFallingThisFrame);
        }

        bool isCrouchingThisFrame = _controller._playerIsCrouching;
        if (!(_IsCrouching == isCrouchingThisFrame))
        {
            _IsCrouching = isCrouchingThisFrame;
            _animator.SetBool(_IsCrouchingHash, isCrouchingThisFrame);
        }

        bool isJumpingThisFrame = _controller._playerIsJumping;
        if (!(_IsJumping == isJumpingThisFrame))
        {
            _IsJumping = isJumpingThisFrame;
            _animator.SetBool(_IsJumpingHash, isJumpingThisFrame);
        }
    }

    private bool Falling()
    {
        bool falling = false;
        if (!(_controller._playerIsJumping) && !(_controller._playerIsGrounded) && !(_controller._playerIsAscendingStairs) && !(_controller._playerIsDescendingStairs))
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
}
