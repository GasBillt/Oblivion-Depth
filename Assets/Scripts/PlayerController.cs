using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float climbSpeed = 3f;
    [Range(100, 500)] public float mouseSensitivity = 300f;
    public float groundCheckRadius = 0.3f;
    public float wallCheckDistance = 0.5f;

    [Header("Jump Settings")]
    public float baseJumpHeight = 2f;
    public float maxJumpHeight = 4f;
    public float jumpForwardForce = 5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public int maxJumps = 2;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float longFallMultiplier = 4f;
    public float longFallThreshold = 1f;

    [Header("Camera Effects")]
public float maxFallShakeIntensity = 0.2f;
public float fallShakeSpeed = 15f;
public float fallShakeDelay = 0.5f; 
public float landingDropAmount = 0.5f;
public float landingDropDuration = 0.2f;
public float landingReturnDuration = 0.5f;
public float minLandingSpeed = 5f;
private Vector3 landingDropOffset = Vector3.zero;
private Vector3 cameraShakeOffset = Vector3.zero;
private Vector3 cameraTargetPosition;

    [Header("Ladder Settings")]
    public float ladderSnapDistance = 1f;
    public float ladderEnterDuration = 0.5f;
    public float ladderExitDuration = 0.5f;
    public float ladderExitForwardDistance = 1f;
    public float ladderExitUpDistance = 0.5f;
    public Image ladderPrompt;

    [Header("References")]
    public Transform groundCheck;
    public Transform takeCheck;
    public Transform ladderCheck;
    public Transform ladderTopCheck;
    public Camera playerCamera;
    public LayerMask groundLayer;
    public LayerMask ladderLayer;
    public LayerMask wallLayer;
    public Animator climbAnimator;

    private Rigidbody rb;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isAgainstWall;
    private float currentSpeed;
    private float xRotation;
    private int jumpsRemaining;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;
    private float jumpStartTime;
    private Collider currentLadder;
    private bool isNearLadder;
    private float fallTimeCounter;
    private Vector3 originalCameraLocalPosition;
    private float landingTimer;
    private bool isLanding;
    private float landingImpactSpeed;
    private float fallShakeSeed;
    private float fallShakeDelayTimer; // Таймер задержки тряски

    private enum LadderState { None, Entering, OnLadder, ExitingTop, ExitingBottom }
    private LadderState ladderState = LadderState.None;
    private float ladderTransitionTimer;
    private Vector3 ladderStartPosition;
    private Quaternion ladderStartRotation;
    private Quaternion ladderTargetRotation;
    private Vector3 ladderTargetPosition;
    private Vector3 ladderCenter;

    // ========== START FUNCTION (добавлена инициализация) ==========
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fallShakeDelayTimer = 0f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        jumpsRemaining = maxJumps;
        fallTimeCounter = 0f;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
        
        // Критически важная инициализация позиции камеры
        originalCameraLocalPosition = playerCamera.transform.localPosition;
        cameraTargetPosition = originalCameraLocalPosition;

        // Гарантируем, что исходная позиция камеры не ниже минимальной
        if (originalCameraLocalPosition.y < 0.15f)
        {
            originalCameraLocalPosition.y = 0.15f;
            playerCamera.transform.localPosition = originalCameraLocalPosition;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (ladderPrompt != null)
            ladderPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleInput();
        
        switch (ladderState)
        {
            case LadderState.Entering:
                UpdateLadderEnter();
                break;
                
            case LadderState.ExitingTop:
            case LadderState.ExitingBottom:
                UpdateLadderExit();
                break;
                
            case LadderState.None:
                HandleCameraRotation();
                break;
        }

        HandleJumpBuffer();
        
        HandleCameraEffects();
        
        // Плавное перемещение камеры к целевой позиции
        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            cameraTargetPosition,
            Time.deltaTime * 15f
        );
        
        // Дополнительная проверка минимальной высоты камеры
        if (playerCamera.transform.localPosition.y < 0.15f)
        {
            Vector3 clampedPosition = playerCamera.transform.localPosition;
            clampedPosition.y = 0.15f;
            playerCamera.transform.localPosition = clampedPosition;
            cameraTargetPosition = clampedPosition;
        }
    }
    void FixedUpdate()
    {
        wasGrounded = isGrounded;
        CheckGround();
        CheckWalls();
        CheckLadder();
        
        if (ladderState == LadderState.None)
        {
            HandleMovement();
            HandleJump();
            HandleFall();
        }
        else if (ladderState == LadderState.OnLadder)
        {
            HandleLadderMovement();
        }
    }

    void HandleInput()
    {
        if (ladderState == LadderState.None)
        {
            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        }

        if (Input.GetButtonDown("Jump") && ladderState == LadderState.None)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isNearLadder && ladderState == LadderState.None)
            {
                StartLadderEnter();
            }
            else if (ladderState == LadderState.OnLadder)
            {
                ExitLadderManual();
            }
        }
    }

    // ================== FALL HANDLING ==================
    void HandleFall()
    {
        if (rb.linearVelocity.y < 0 && ladderState == LadderState.None)
        {
            fallTimeCounter += Time.fixedDeltaTime;
            float currentFallMultiplier = fallTimeCounter > longFallThreshold 
                ? longFallMultiplier 
                : fallMultiplier;
            
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (currentFallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump") && !isGrounded)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // ========== HANDLE CAMERA EFFECTS FUNCTION ==========
    void HandleCameraEffects()
    {
        // Сброс смещений
        landingDropOffset = Vector3.zero;
        cameraShakeOffset = Vector3.zero;

        // Эффекты не применяются на лестнице
        if (ladderState != LadderState.None)
        {
            cameraTargetPosition = originalCameraLocalPosition;
            fallShakeDelayTimer = 0f;
            return;
        }

        // Тряска камеры при падении (с задержкой)
        if (!isGrounded && rb.linearVelocity.y < 0 && !isLanding)
        {
            fallShakeDelayTimer += Time.deltaTime;

            if (fallShakeDelayTimer >= fallShakeDelay)
            {
                float fallSpeed = Mathf.Abs(rb.linearVelocity.y);
                float shakeIntensity = Mathf.Clamp01(fallSpeed / 60f) * maxFallShakeIntensity;

                float x = Mathf.PerlinNoise(fallShakeSeed, Time.time * fallShakeSpeed) * 2f - 1f;
                float y = Mathf.PerlinNoise(fallShakeSeed + 10f, Time.time * fallShakeSpeed) * 2f - 1f;
                float z = Mathf.PerlinNoise(fallShakeSeed + 20f, Time.time * fallShakeSpeed) * 2f - 1f;

                cameraShakeOffset = new Vector3(x, y, z) * shakeIntensity;
            }
        }
        else
        {
            fallShakeDelayTimer = 0f;
        }

        // Эффект приземления
        if (isLanding)
        {
            landingTimer += Time.deltaTime;
            float progress;

            if (landingTimer <= landingDropDuration)
            {
                progress = landingTimer / landingDropDuration;
                float dropAmount = landingDropAmount * (landingImpactSpeed / minLandingSpeed);
                landingDropOffset = Vector3.down * dropAmount * progress;
            }
            else
            {
                progress = (landingTimer - landingDropDuration) / landingReturnDuration;
                float dropAmount = landingDropAmount * (landingImpactSpeed / minLandingSpeed);
                landingDropOffset = Vector3.down * dropAmount * (1f - progress);
            }

            if (landingTimer >= landingDropDuration + landingReturnDuration)
            {
                isLanding = false;
                landingDropOffset = Vector3.zero;
            }
        }

        // Обновление целевой позиции камеры
        cameraTargetPosition = originalCameraLocalPosition + landingDropOffset + cameraShakeOffset;
        
        // Гарантируем, что камера не опускается ниже минимальной высоты
        if (cameraTargetPosition.y < 0.15f)
        {
            cameraTargetPosition.y = 0.15f;
        }
    }

    // ========== START LANDING EFFECT FUNCTION ==========
    void StartLandingEffect(float impactSpeed)
    {
        isLanding = true;
        landingTimer = 0f;
        landingImpactSpeed = impactSpeed;
    }

// ========== CHECK GROUND FUNCTION (добавлен сброс позиции камеры) ==========
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Обнаружение приземления
        if (!wasGrounded && isGrounded)
        {
            // Рассчитываем скорость падения
            float fallSpeed = Mathf.Abs(rb.linearVelocity.y);

            // Запускаем эффект приземления если скорость достаточно высокая
            if (fallSpeed > minLandingSpeed)
            {
                StartLandingEffect(fallSpeed);
            }

            jumpsRemaining = maxJumps;
            isJumping = false;
            fallTimeCounter = 0f;
            fallShakeDelayTimer = 0f; // Сброс таймера при приземлении

            // Сброс позиции камеры при приземлении
            cameraTargetPosition = originalCameraLocalPosition;
            playerCamera.transform.localPosition = originalCameraLocalPosition;
        }

        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    void StartLadderEnter()
    {
        if (currentLadder == null) return;
        
        ladderState = LadderState.Entering;
        ladderTransitionTimer = 0f;
        ladderStartPosition = transform.position;
        ladderStartRotation = transform.rotation;
        
        Vector3 directionToLadder = currentLadder.transform.position - transform.position;
        directionToLadder.y = 0;
        ladderTargetRotation = Quaternion.LookRotation(directionToLadder.normalized);
        
        ladderCenter = currentLadder.bounds.center;
        ladderCenter.y = transform.position.y;
        
        Vector3 ladderForward = currentLadder.transform.forward;
        Vector3 ladderFrontPosition = currentLadder.bounds.center + 
                                    ladderForward * (currentLadder.bounds.size.z / 2);
        
        ladderTargetPosition = ladderFrontPosition - ladderForward * ladderSnapDistance;
        ladderTargetPosition.y = transform.position.y;
        
        if (climbAnimator != null) climbAnimator.Play("Climb");
        
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
    }

    void UpdateLadderEnter()
    {
        ladderTransitionTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(ladderTransitionTimer / ladderEnterDuration);
        
        transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition, progress);
        transform.rotation = Quaternion.Slerp(ladderStartRotation, ladderTargetRotation, progress);
        
        Vector3 lookDirection = ladderCenter - playerCamera.transform.position;
        lookDirection.y = 0;
        Quaternion targetCameraRotation = Quaternion.LookRotation(lookDirection);
        
        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation, 
            targetCameraRotation, 
            progress
        );
        
        if (progress >= 1f)
        {
            ladderState = LadderState.OnLadder;
            transform.position = ladderTargetPosition;
            transform.rotation = ladderTargetRotation;
            playerCamera.transform.rotation = targetCameraRotation;
        }
    }

    void HandleLadderMovement()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 climbDirection = Vector3.up * verticalInput;
        
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            Vector3 newPosition = transform.position + climbDirection * climbSpeed * Time.fixedDeltaTime;
            
            Vector3 ladderForward = currentLadder.transform.forward;
            Vector3 ladderFrontPosition = currentLadder.bounds.center + 
                                        ladderForward * (currentLadder.bounds.size.z / 2);
            ladderTargetPosition = ladderFrontPosition - ladderForward * ladderSnapDistance;
            ladderTargetPosition.y = newPosition.y;
            
            transform.position = ladderTargetPosition;
            
            if (verticalInput > 0 && CheckTopReached())
            {
                StartLadderExit(true);
                return;
            }
            
            if (verticalInput < 0 && isGrounded)
            {
                StartLadderExit(false);
                return;
            }
        }
        
        Vector3 lookDirection = ladderCenter - playerCamera.transform.position;
        lookDirection.y = 0;
        playerCamera.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    bool CheckTopReached()
    {
        if (currentLadder == null) return false;
        
        Collider[] ladderTops = Physics.OverlapBox(
            ladderTopCheck.position,
            ladderTopCheck.localScale * 0.5f,
            ladderTopCheck.rotation,
            ladderLayer
        );

        return ladderTops.Length == 0;
    }

    void StartLadderExit(bool isTopExit)
    {
        ladderState = isTopExit ? LadderState.ExitingTop : LadderState.ExitingBottom;
        ladderTransitionTimer = 0f;
        ladderStartPosition = transform.position;
        
        Vector3 exitDirection = -currentLadder.transform.forward;
        
        if (isTopExit)
        {
            ladderTargetPosition = transform.position + 
                exitDirection * ladderExitForwardDistance + 
                Vector3.up * ladderExitUpDistance;
            
            if (climbAnimator != null) climbAnimator.Play("TopClimbOver");
        }
        else
        {
            ladderTargetPosition = transform.position - exitDirection * ladderExitForwardDistance;
            if (climbAnimator != null) climbAnimator.Play("BottomClimbOver");
        }
    }

    void UpdateLadderExit()
    {
        ladderTransitionTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(ladderTransitionTimer / ladderExitDuration);
        
        transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition, progress);
        
        Vector3 lookDirection = ladderCenter - playerCamera.transform.position;
        lookDirection.y = 0;
        playerCamera.transform.rotation = Quaternion.LookRotation(lookDirection);
        
        if (progress >= 1f) CompleteLadderExit();
    }

    void CompleteLadderExit()
    {
        ladderState = LadderState.None;
        rb.isKinematic = false;
        currentLadder = null;
    }

    void ExitLadderManual()
    {
        ladderState = LadderState.None;
        rb.isKinematic = false;
        currentLadder = null;
    }

    void HandleCameraRotation()
    {
        if (ladderState != LadderState.None) return;
    
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
    
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0 && CanJump() && ladderState == LadderState.None)
        {
            isJumping = true;
            jumpStartTime = Time.time;
            PerformJump();
            jumpBufferCounter = 0;
        }
    }

    bool CanJump()
    {
        return jumpsRemaining > 0 && (isGrounded || coyoteTimeCounter > 0);
    }

    void PerformJump()
    {
        float jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y)) * baseJumpHeight;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || 
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            Vector3 forwardForce = (transform.forward * Input.GetAxisRaw("Vertical") +
                                  transform.right * Input.GetAxisRaw("Horizontal")).normalized;
            rb.AddForce(forwardForce * jumpForwardForce, ForceMode.VelocityChange);
        }

        jumpsRemaining--;
        coyoteTimeCounter = 0;
        fallTimeCounter = 0f;
        fallShakeDelayTimer = 0f; // Сброс таймера при прыжке
    }

    void CheckLadder()
    {
        if (ladderState != LadderState.None) return;

        Collider[] ladders = Physics.OverlapBox(
            ladderCheck.position,
            ladderCheck.localScale * 0.5f,
            ladderCheck.rotation,
            ladderLayer
        );

        currentLadder = null;
        isNearLadder = false;
        
        foreach (Collider col in ladders)
        {
            if (col.CompareTag("Ladder"))
            {
                isNearLadder = true;
                currentLadder = col;
                break;
            }
        }

        if (ladderPrompt != null)
            ladderPrompt.gameObject.SetActive(isNearLadder);
    }

    void CheckWalls()
    {
        isAgainstWall = Physics.CheckSphere(transform.position, wallCheckDistance, wallLayer);
    }

    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            Vector3 moveDirection = (transform.right * moveHorizontal + 
                                   transform.forward * moveVertical).normalized;
            rb.linearVelocity = new Vector3(
                moveDirection.x * currentSpeed,
                rb.linearVelocity.y,
                moveDirection.z * currentSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void HandleJump()
    {
        if (isJumping && rb.linearVelocity.y > 0)
        {
            if (!Input.GetButton("Jump") || Time.time - jumpStartTime > 0.5f)
            {
                isJumping = false;
            }
            else
            {
                float additionalForce = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * 
                                      (maxJumpHeight - baseJumpHeight));
                rb.AddForce(Vector3.up * additionalForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(ladderCheck.position, ladderCheck.localScale);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(ladderTopCheck.position, ladderTopCheck.localScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(takeCheck.position, takeCheck.localScale);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, wallCheckDistance);
    }
}