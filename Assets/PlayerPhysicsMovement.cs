using System;
using System.Collections;
using System.Collections.Generic;
using TigrisDigitalCreative._Input;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEditor.Experimental.GraphView.GraphView;

//[RequireComponent(typeof(Rigidbody))] 
[DefaultExecutionOrder(-1)]
public class PlayerPhysicsMovement : MonoBehaviour
{
    #region Private Variables

    [Header("=== References ===")] [SerializeField]
    InputManager m_Input;

    [SerializeField] PlayerAnimationController m_AnimController;
    CapsuleCollider m_Collider;
    Rigidbody m_RigidBody, m_ConnectedRigidbody, m_PreviousConnectedRigidbody;

    [Header("=== Movement Settings ===")] [SerializeField]
    Transform m_PlayerInputSpace = default;

    [SerializeField, Range(0f, 100f)] float m_MaxAcceleration = 10f, m_MaxInAirAcceleration = 1f;
    [SerializeField, Range(0f, 100f)] float m_WalkMaxSpeed = 9f;
    [SerializeField, Range(0f, 100f)] float m_RunMaxSpeed = 15f;
    Vector3 m_Velocity, m_RelativeVelocity, m_DesiredVelocity, m_ConnectionVelocity;
    public Vector3 RelativeVelocity => m_RelativeVelocity;
    //Needed for animated objects
    Vector3 m_ConnectionWorldPosition, m_ConnectionLocalPosition;

    [Header("=== Ground Settings ===")]
    //Grounded Stuff
    Vector3 m_UpAxis, m_RightAxis, m_ForwardAxis;
    bool m_UnwalkableSurfaceHit = false;

    public bool m_AlignToSurface = true;
    [SerializeField] float m_AlignmentSpeed = 0.5f;
    Quaternion m_gravityAlignment = Quaternion.identity;
    public bool IsGrounded => m_GroundContactCount > 0;
    private bool OnSteep => m_SteepContactCount > 0;
    int m_GroundContactCount, m_SteepContactCount;
    RaycastHit m_GroundCheckHit;
    [SerializeField, Range(0f, 90f)] float m_MaxSlopeAngle = 25f, m_MaxStairsAngle = 50f;
    float m_MinGroundDotProduct, m_MinStairsDotProduct;
    Vector3 m_ContactNormal, m_SteepNormal;
    int m_stepsSinceLastGrounded, m_StepsSinceLastJump;
    [SerializeField, Range(0f, 100f)] float m_MaxSnapSpeed = 100f;
    [SerializeField, Min(0f)] float m_MaxSnapDistance = 1.5f;

    [SerializeField] LayerMask m_GroundLayerMask = -1, m_StairsMask = -1;

    //Jump
    [SerializeField, Range(0f, 10f)] private float m_JumpHeight = 2f;
    [SerializeField, Range(0, 5)] int m_MaxAirJumpAmmount = 1;
    [SerializeField] float m_ContinualJumpForceMultiplier = 0.1f;
    [SerializeField] float m_JumpTime = 0.175f;
    [SerializeField] float m_JumpTimeCounter = 0.0f;
    [SerializeField] float m_CoyoteTime = 0.15f;
    [SerializeField] float m_CoyoteTimeCounter = 0.0f;
    [SerializeField] float m_JumpBufferTime = 0.2f;
    [SerializeField] float m_JumpBufferTimeCounter = 0.0f;
    [SerializeField] bool m_JumpWasPressedLastFrame = false;
    int m_CurrentJumpAmmount = 0;
    public bool m_IsJumping { get; private set; } = false;

    //GravityStuffWhileJumping
    private float m_GravityScale = 1f;

    #endregion

    #region Public Variables

    #endregion

    #region Unity Functions

    void Awake()
    {
        m_Input = GetComponent<InputManager>();
        m_RigidBody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<CapsuleCollider>();
        m_RigidBody.useGravity = false;
        OnValidate();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (m_PlayerInputSpace) {
            m_RightAxis = ProjectDirectionOnPlane(m_PlayerInputSpace.right, m_UpAxis);
            m_ForwardAxis = ProjectDirectionOnPlane(m_PlayerInputSpace.forward, m_UpAxis);
        }
        else {
            m_RightAxis = ProjectDirectionOnPlane(Vector3.right, m_UpAxis);
            m_ForwardAxis = ProjectDirectionOnPlane(Vector3.forward, m_UpAxis);
        }

        float currentMaxSpeed = m_Input.RunIsPressed ? m_RunMaxSpeed : m_WalkMaxSpeed;
        m_DesiredVelocity = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y) * currentMaxSpeed;
    }

    private void LateUpdate()
    {
    }

    private void GravityAlignment()
    {
        m_gravityAlignment = Quaternion.FromToRotation(transform.up, m_UpAxis);
        float t = Time.deltaTime * m_AlignmentSpeed;
        t = Helpers.EaseOutCirc(t);
        transform.rotation = Quaternion.Slerp(transform.rotation, m_gravityAlignment * transform.rotation, t);
    }
	private void ContactAlignment() {
		m_gravityAlignment = Quaternion.FromToRotation(transform.up, m_ContactNormal);
		float t = Time.deltaTime * m_AlignmentSpeed;
		t = Helpers.EaseOutCirc(t);
		transform.rotation = Quaternion.Slerp(transform.rotation, m_gravityAlignment * transform.rotation, t);
        m_UpAxis = m_ContactNormal;
	}

	private void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(m_RigidBody.position, out m_UpAxis) * m_GravityScale;
        m_Velocity = m_RigidBody.velocity;
        if (m_Input.MagneticBootsIsOn) {
            ContactAlignment();
		} else {
            GravityAlignment();
			m_Velocity += gravity * Time.unscaledDeltaTime;
		}
        UpdateState();
        AdjustVelocity();
        Jump();
		GravityAlignment();
		m_Velocity += gravity * Time.unscaledDeltaTime;
		//if (m_Input.MagneticBootsIsOn) {
		//    GravityAlignment();
		//    m_Velocity += gravity * Time.unscaledDeltaTime;
		//}
		m_RigidBody.velocity = m_Velocity;
        ClearState();
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnValidate()
    {
        m_MinGroundDotProduct = Mathf.Cos(m_MaxSlopeAngle * Mathf.Deg2Rad);
        m_MinStairsDotProduct = Mathf.Cos(m_MaxStairsAngle * Mathf.Deg2Rad);
    }

    #endregion

    #region Private  Functions

    void UpdateState()
    {
        m_stepsSinceLastGrounded += 1;
        m_StepsSinceLastJump += 1;
        m_Velocity = m_RigidBody.velocity;
        if (IsGrounded || SnapToGround() || CheckSteepContacts()) {
            m_stepsSinceLastGrounded = 0;
			m_GravityScale = 1f;
            if (m_StepsSinceLastJump > 1 && !m_UnwalkableSurfaceHit) {
                m_CurrentJumpAmmount = 0;
            }

            if (m_GroundContactCount > 1) {
                m_ContactNormal.Normalize();
            }
        }
        else {
            m_ContactNormal = m_UpAxis;
            m_GravityScale = Mathf.Clamp(m_GravityScale + 0.3f, 0,2f);
        }

        if (m_ConnectedRigidbody) {
            if (m_ConnectedRigidbody.isKinematic || m_ConnectedRigidbody.mass >= m_RigidBody.mass) {
                UpdateConnectionState();
            }
        }
    }
    void UpdateConnectionState ()
    {
        if (m_ConnectedRigidbody == m_PreviousConnectedRigidbody) {
            Vector3 displacementFromPreviousFrame = m_ConnectedRigidbody.transform.TransformPoint(m_ConnectionLocalPosition) - m_ConnectionWorldPosition;
            m_ConnectionVelocity = displacementFromPreviousFrame / Time.unscaledDeltaTime;
        }
        m_ConnectionWorldPosition = m_RigidBody.position;
        m_ConnectionLocalPosition = m_ConnectedRigidbody.transform.InverseTransformPoint(m_ConnectionWorldPosition);
    }

    void ClearState()
    {
        m_GroundContactCount = 0;
        m_SteepContactCount = 0;
        m_ContactNormal = m_SteepNormal = m_ConnectionVelocity = Vector3.zero;
        m_PreviousConnectedRigidbody = m_ConnectedRigidbody;
        m_ConnectedRigidbody = null;
        m_UnwalkableSurfaceHit = false;
	}
	void Jump() {
		Vector3 jumpDirection;
		float jumpSpeed = 0;
		//Jump Feel Setup
		SetJumpTimeCounter();
		SetCoyoteTimeCounter();
		SetJumpBufferTimeCounter();

		if (IsGrounded) {
			jumpDirection = m_ContactNormal;
		} else if (OnSteep && !m_UnwalkableSurfaceHit) {
			jumpDirection = m_SteepNormal;
			m_CurrentJumpAmmount = 0;
		} else if (m_MaxAirJumpAmmount > 0 && m_CurrentJumpAmmount <= m_MaxAirJumpAmmount) {
			//Prevent jumping an extra time if you fall of a surface
			if (m_CurrentJumpAmmount == 0) {
				m_CurrentJumpAmmount = 1;
			}
			jumpDirection = m_ContactNormal;
		} else {
			return;
		}

		if (m_JumpBufferTimeCounter > 0.0f && !m_IsJumping && m_CoyoteTimeCounter > 0.0f) {
			m_StepsSinceLastJump = 0;
			m_CurrentJumpAmmount++;
			jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * m_JumpHeight);
			jumpDirection = (jumpDirection + m_UpAxis).normalized;
			float alignedSpeed = Vector3.Dot(m_Velocity, jumpDirection);
			if (alignedSpeed > 0f) {
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			m_IsJumping = true;
			m_JumpBufferTimeCounter = 0.0f;
			m_CoyoteTimeCounter = 0.0f;
		} else if (m_Input.JumpIsPressed && m_IsJumping && m_JumpTimeCounter > 0.0f) {
			jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * m_JumpHeight);
			float alignedSpeed = Vector3.Dot(m_Velocity, jumpDirection);
			if (alignedSpeed > 0f) {
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			jumpSpeed *= m_ContinualJumpForceMultiplier;
		} else if ((!m_Input.JumpIsPressed && m_IsJumping && IsGrounded) || m_JumpTimeCounter < 0) {
			m_IsJumping = false;
		}
		m_Velocity += jumpDirection * jumpSpeed;
	}

	//void Jump(Vector3 gravity)
	//{
	//    SetCounters();

	//    Vector3 jumpDirection = GetJumpDirection();
	//    if (jumpDirection == Vector3.zero) return; // No jump direction found

	//    float jumpSpeed = CalculateJumpSpeed(jumpDirection, gravity);
	//    if (jumpSpeed > 0) {
	//        m_Velocity += jumpDirection * jumpSpeed;
	//    }
	//}

	void SetCounters()
    {
        SetJumpTimeCounter();
        SetCoyoteTimeCounter();
        SetJumpBufferTimeCounter();
    }

    Vector3 GetJumpDirection()
    {
        Vector3 jumpDirection = Vector3.zero;
        if (IsGrounded || CanAirJump()) {
            jumpDirection = m_ContactNormal;
            if (!IsGrounded) m_CurrentJumpAmmount = 1;
        }
        else if (OnSteep) {
            jumpDirection = m_SteepNormal;
            m_CurrentJumpAmmount = 0;
        }

        return jumpDirection;
    }

    bool CanAirJump()
    {
        return m_MaxAirJumpAmmount > 0 && m_CurrentJumpAmmount <= m_MaxAirJumpAmmount;
    }

    float CalculateJumpSpeed(Vector3 jumpDirection, Vector3 gravity)
    {
        float jumpSpeed = 0;
        if (IsJumpStarting()) {
            jumpSpeed = GetInitialJumpSpeed(jumpDirection, gravity);
            m_GravityScale = 0f;
            m_IsJumping = true;
        }
        else if (IsContinuingJump()) {
            jumpSpeed = GetContinuedJumpSpeed(jumpDirection, gravity);
        }
        else if (ShouldStopJumping()) {
            m_GravityScale = 0f;
            m_IsJumping = false;
        }

        return jumpSpeed;
    }

    bool IsJumpStarting()
    {
        return m_JumpBufferTimeCounter > 0.0f && !m_IsJumping && m_CoyoteTimeCounter > 0.0f;
    }

    float GetInitialJumpSpeed(Vector3 jumpDirection, Vector3 gravity)
    {
        m_StepsSinceLastJump = 0;
        m_CurrentJumpAmmount++;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * m_JumpHeight);
        jumpDirection = (jumpDirection + m_UpAxis).normalized;
        float alignedSpeed = Vector3.Dot(m_Velocity, jumpDirection);
        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        m_JumpBufferTimeCounter = 0.0f;
        m_CoyoteTimeCounter = 0.0f;
        return jumpSpeed;
    }

    bool IsContinuingJump()
    {
        return m_Input.JumpIsPressed && m_IsJumping && m_JumpTimeCounter > 0.0f;
    }

    float GetContinuedJumpSpeed(Vector3 jumpDirection, Vector3 gravity)
    {
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * m_JumpHeight);
        float alignedSpeed = Vector3.Dot(m_Velocity, jumpDirection);
        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        return jumpSpeed * m_ContinualJumpForceMultiplier;
    }

    bool ShouldStopJumping()
    {
        return (!m_Input.JumpIsPressed && m_IsJumping && IsGrounded) || m_JumpTimeCounter < 0;
    }

    void SetJumpTimeCounter()
    {
        if (m_IsJumping && !IsGrounded) {
            m_JumpTimeCounter -= Time.unscaledDeltaTime;
        }
        else {
            m_JumpTimeCounter = m_JumpTime;
        }
    }

    void SetCoyoteTimeCounter()
    {
        if (IsGrounded || m_CurrentJumpAmmount < m_MaxAirJumpAmmount) {
            m_CoyoteTimeCounter = m_CoyoteTime;
        }
        else {
            m_CoyoteTimeCounter -= Time.unscaledDeltaTime;
        }
    }

    void SetJumpBufferTimeCounter()
    {
        if (!m_JumpWasPressedLastFrame && m_Input.JumpIsPressed) {
            m_JumpBufferTimeCounter = m_JumpBufferTime;
        }
        else if (m_JumpBufferTimeCounter > 0.0f) {
            m_JumpBufferTimeCounter -= Time.unscaledDeltaTime;
        }

        m_JumpWasPressedLastFrame = m_Input.JumpIsPressed;
    }

    bool SnapToGround()
    {
        if (m_stepsSinceLastGrounded > 1 || m_StepsSinceLastJump <= 2) {
            return false;
        }

        float speed = m_Velocity.magnitude;
        if (speed > m_MaxSnapSpeed) {
            return false;
        }

        if (!Physics.Raycast(m_RigidBody.position, -m_UpAxis, out RaycastHit hit, m_MaxSnapDistance,
                m_GroundLayerMask)) {
            return false;
        }

        float upDot = Vector3.Dot(m_UpAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer)) {
            return false;
        }

        m_GroundContactCount = 1;
        m_ContactNormal = hit.normal;
        float dot = Vector3.Dot(m_Velocity, hit.normal);
        if (dot > 0f) {
            m_Velocity = (m_Velocity - hit.normal * dot).normalized * speed;
        }

        m_ConnectedRigidbody = hit.rigidbody;
        return true;
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    void AdjustVelocity()
    {
		m_RelativeVelocity = m_Velocity - m_ConnectionVelocity;
        Vector3 xAxis = ProjectDirectionOnPlane(m_RightAxis, m_ContactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(m_ForwardAxis, m_ContactNormal);

        float currentX = Vector3.Dot(m_RelativeVelocity, xAxis);
        float currentZ = Vector3.Dot(m_RelativeVelocity, zAxis);

        float acceleration = IsGrounded ? m_MaxAcceleration : m_MaxInAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, m_DesiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, m_DesiredVelocity.z, maxSpeedChange);
		m_Velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
		//m_AnimController.SetVelocity(m_Velocity);
	}

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(m_UpAxis, normal);
            if (upDot >= minDot) {
                m_GroundContactCount++;
                m_ContactNormal += normal;
                m_ConnectedRigidbody = collision.rigidbody;
            }
            else if (upDot > -0.01f) {
                if((collision.gameObject.layer & (1 << m_GroundLayerMask)) != 0) {
                    m_UnwalkableSurfaceHit = true;
                }
                m_SteepContactCount++;
                m_SteepNormal += normal;
                if (m_GroundContactCount == 0) {
                    m_ConnectedRigidbody = collision.rigidbody;
                }
            }
        }
    }

    float GetMinDot(int layer)
    {
        return (m_StairsMask & (1 << layer)) == 0 ? m_MinGroundDotProduct : m_MinStairsDotProduct;
    }

    bool CheckSteepContacts()
    {
        if (m_SteepContactCount > 1) {
            m_SteepNormal.Normalize();
            float upDot = Vector3.Dot(m_UpAxis, m_SteepNormal);
            if (upDot >= m_MinGroundDotProduct) {
                m_GroundContactCount = 1;
                m_ContactNormal = m_SteepNormal;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Public  Functions

    #endregion
}