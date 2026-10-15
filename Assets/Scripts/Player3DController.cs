using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Player3DController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] public bool invulnerableDuringDash = true;

    private float minX = -9.5f;
    private float maxX = 9.5f;
    private float minZ = -9.5f;
    private float maxZ = 9.5f;

    private bool isDashing = false;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private Vector3 dashDirection;
    private bool hasDashedOnce = false;

    private TrailRenderer trail;
    private PlayerAnimator anim;
    private float verticalVelocity = 0f;

    [Header("Effects")]
    [SerializeField] private Material trailMaterial;
    private CharacterController controller;
    
#if ENABLE_INPUT_SYSTEM
    private InputAction dashAction;
#endif

    private void Awake()
    {
        anim = GetComponent<PlayerAnimator>();
        
#if ENABLE_INPUT_SYSTEM
        dashAction = new InputAction("Dash");
        dashAction.AddBinding("<Keyboard>/leftShift");
        dashAction.AddBinding("<Keyboard>/rightShift");
        dashAction.AddBinding("<Gamepad>/buttonSouth");
#endif

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.8f;
        trail.endWidth = 0.1f;
        if (trailMaterial != null)
        {
            trail.material = trailMaterial;
        }
        trail.startColor = new Color(1f, 1f, 0.8f, 0.5f);
        trail.endColor = new Color(1f, 1f, 1f, 0f);
        trail.emitting = false;

        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            Destroy(rb);
        }
        
        controller.radius = 0.35f;
        controller.height = 1.6f;
        controller.center = new Vector3(0, 0.8f, 0); // 0.8 is half of 1.6, putting base at 0
        controller.stepOffset = 0.05f; // Reduced so player can't step up rocks
        controller.slopeLimit = 15f; // Reduced so player can't walk up slanted rocks
        controller.skinWidth = 0.03f;
        controller.minMoveDistance = 0f; // Fix gravity ignoring when standing still
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        dashAction?.Enable();
#endif
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameStarted += ResetDash;
        }
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        dashAction?.Disable();
#endif
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameStarted -= ResetDash;
        }
    }

    private void ResetDash()
    {
        isDashing = false;
        dashTimeRemaining = 0f;
        dashCooldownRemaining = 0f;
        trail.emitting = false;
    }

    private void Start()
    {
        CalculateBounds();
        
        // Scale down the character slightly to make it look shorter/smaller
        Transform bodyPivot = transform.Find("BodyPivot");
        if (bodyPivot != null)
        {
            // Original scale was ~0.78, making it 0.62 reduces height by ~20%
            bodyPivot.localScale = new Vector3(0.62f, 0.62f, 0.62f); 
        }
    }

    private void CalculateBounds()
    {
        float fMinX = float.MaxValue, fMaxX = float.MinValue;
        float fMinZ = float.MaxValue, fMaxZ = float.MinValue;
        GameObject env = GameObject.Find("Environment");
        if (env != null)
        {
            foreach (Transform child in env.transform)
            {
                if (child.name.Contains("Fence"))
                {
                    Vector3 pos = child.position;
                    if (pos.x < fMinX) fMinX = pos.x;
                    if (pos.x > fMaxX) fMaxX = pos.x;
                    if (pos.z < fMinZ) fMinZ = pos.z;
                    if (pos.z > fMaxZ) fMaxZ = pos.z;
                }
            }
            minX = fMinX + 0.8f;
            maxX = fMaxX - 0.8f;
            minZ = fMinZ + 0.8f;
            maxZ = fMaxZ - 0.8f;
        }
    }

    public bool IsDashing => isDashing;
    public float DashCooldownProgress => Mathf.Clamp01((dashCooldown - dashCooldownRemaining) / dashCooldown);

    private void Update()
    {
        if (GameManager3D.Instance != null && GameManager3D.Instance.CurrentState != GameManager3D.GameState.Playing) 
            return;

        if (dashCooldownRemaining > 0)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }

        bool dashPressed = false;
#if ENABLE_INPUT_SYSTEM
        dashPressed = dashAction.WasPressedThisFrame();
#else
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
#endif

        if (dashPressed && dashCooldownRemaining <= 0 && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining <= 0)
            {
                EndDash();
            }
        }
    }

    private void LateUpdate()
    {
        if (GameManager3D.Instance != null && GameManager3D.Instance.CurrentState != GameManager3D.GameState.Playing) 
            return;

        HandleMovement();
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;
        trail.emitting = true;
        
        if (!hasDashedOnce)
        {
            hasDashedOnce = true;
            if (DialogManager.Instance != null) DialogManager.Instance.TriggerDialog("FirstDash");
        }
        
        Vector2 input = GetInput();
        if (input.sqrMagnitude > 0)
        {
            dashDirection = new Vector3(input.x, 0, input.y).normalized;
        }
        else
        {
            dashDirection = transform.forward;
        }

        // Camera FOV effect
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.fieldOfView = 62f;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        trail.emitting = false;
    }

    private Vector2 GetInput()
    {
        Vector2 input = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
        }
        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            input += leftStick;
        }
#else
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
#endif
        return input;
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;

        if (isDashing)
        {
            movement = dashDirection * (moveSpeed * dashSpeedMultiplier * Time.deltaTime);
        }
        else
        {
            Vector2 input = GetInput();
            if (input.sqrMagnitude > 0)
            {
                input.Normalize();
                movement = new Vector3(input.x, 0, input.y) * (moveSpeed * Time.deltaTime);
                
                // Rotate player to face movement direction
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * 15f);
            }
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            if (isDashing)
            {
                // Obstacle Collision during dash
                if (Physics.SphereCast(transform.position + Vector3.up * 0.8f, 0.35f, dashDirection, out RaycastHit hit, new Vector3(movement.x, 0, movement.z).magnitude + 0.1f, LayerMask.GetMask("Obstacle", "Default")))
                {
                    movement.x = dashDirection.x * Mathf.Max(0, hit.distance - 0.1f);
                    movement.z = dashDirection.z * Mathf.Max(0, hit.distance - 0.1f);
                    EndDash();
                    ObstacleVisual ov = hit.collider.GetComponentInParent<ObstacleVisual>();
                    if (ov != null) ov.Wobble();
                }
            }

            controller.Move(movement);

            // Clamp inside arena (CharacterController allows Move, but we ensure it stays in bounds)
            Vector3 pos = transform.position;
            bool outOfBounds = false;
            if (pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ)
            {
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
                outOfBounds = true;
            }

            if (outOfBounds)
            {
                controller.enabled = false;
                transform.position = pos;
                controller.enabled = true;
            }
        }

        // Recover FOV smoothly
        if (!isDashing)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.fieldOfView > 55f)
            {
                mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 55f, Time.deltaTime * 30f);
            }
        }
    }
}
