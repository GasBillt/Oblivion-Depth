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

    [Header("Ladder Settings")]
    public float ladderSnapDistance = 1f; // Расстояние от передней грани лестницы
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

    // Состояния подъема по лестнице
    private enum LadderState { None, Entering, OnLadder, ExitingTop, ExitingBottom }
    private LadderState ladderState = LadderState.None;
    private float ladderTransitionTimer;
    private Vector3 ladderStartPosition;
    private Quaternion ladderStartRotation;
    private Quaternion ladderTargetRotation;
    private Vector3 ladderTargetPosition;
    private Vector3 ladderCenter; // Центр лестницы для ориентации камеры

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        jumpsRemaining = maxJumps;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (ladderPrompt != null)
            ladderPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleInput();
        
        // Обработка состояний лестницы
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
    }

    void FixedUpdate()
    {
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

        // Взаимодействие с лестницей
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

    void StartLadderEnter()
    {
        if (currentLadder == null) return;
        
        ladderState = LadderState.Entering;
        ladderTransitionTimer = 0f;
        
        // Запоминаем начальные позиции
        ladderStartPosition = transform.position;
        ladderStartRotation = transform.rotation;
        
        // Рассчитываем целевой поворот (лицом к лестнице)
        Vector3 directionToLadder = currentLadder.transform.position - transform.position;
        directionToLadder.y = 0;
        ladderTargetRotation = Quaternion.LookRotation(directionToLadder.normalized);
        
        // Рассчитываем центр лестницы для ориентации камеры
        ladderCenter = currentLadder.bounds.center;
        ladderCenter.y = transform.position.y; // Сохраняем текущую высоту игрока
        
        // Рассчитываем позицию передней грани лестницы с учетом snap distance
        Vector3 ladderForward = currentLadder.transform.forward;
        Vector3 ladderFrontPosition = currentLadder.bounds.center + 
                                    ladderForward * (currentLadder.bounds.size.z / 2);
        
        // Целевая позиция: передняя грань + отступ (назад от лестницы)
        ladderTargetPosition = ladderFrontPosition - ladderForward * ladderSnapDistance;
        ladderTargetPosition.y = transform.position.y;
        
        // Запускаем анимацию входа
        if (climbAnimator != null)
        {
            climbAnimator.Play("Climb");
        }
        
        // Отключаем физику
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
    }

    void UpdateLadderEnter()
    {
        ladderTransitionTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(ladderTransitionTimer / ladderEnterDuration);
        
        // Плавное перемещение и поворот
        transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition, progress);
        transform.rotation = Quaternion.Slerp(ladderStartRotation, ladderTargetRotation, progress);
        
        // Ориентация камеры к центру лестницы
        Vector3 lookDirection = ladderCenter - playerCamera.transform.position;
        lookDirection.y = 0; // Только горизонтальное направление
        Quaternion targetCameraRotation = Quaternion.LookRotation(lookDirection);
        
        // Применяем поворот камеры
        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation, 
            targetCameraRotation, 
            progress
        );
        
        // Завершение входа
        if (progress >= 1f)
        {
            ladderState = LadderState.OnLadder;
            
            // Фиксируем окончательную позицию и поворот
            transform.position = ladderTargetPosition;
            transform.rotation = ladderTargetRotation;
            playerCamera.transform.rotation = targetCameraRotation;
        }
    }

    void HandleLadderMovement()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 climbDirection = Vector3.up * verticalInput;
        
        // Движение вверх/вниз
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            Vector3 newPosition = transform.position + climbDirection * climbSpeed * Time.fixedDeltaTime;
            
            // Обновляем позицию относительно лестницы
            Vector3 ladderForward = currentLadder.transform.forward;
            Vector3 ladderFrontPosition = currentLadder.bounds.center + 
                                        ladderForward * (currentLadder.bounds.size.z / 2);
            ladderTargetPosition = ladderFrontPosition - ladderForward * ladderSnapDistance;
            ladderTargetPosition.y = newPosition.y;
            
            transform.position = ladderTargetPosition;
            
            // Проверка достижения верха
            if (verticalInput > 0 && CheckTopReached())
            {
                StartLadderExit(true);
                return;
            }
            
            // Проверка достижения низа
            if (verticalInput < 0 && isGrounded)
            {
                StartLadderExit(false);
                return;
            }
        }
        
        // Обновление ориентации камеры к центру лестницы
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
        
        // Рассчитываем целевую позицию для выхода
        Vector3 exitDirection = -currentLadder.transform.forward;
        
        if (isTopExit)
        {
            ladderTargetPosition = transform.position + 
                exitDirection * ladderExitForwardDistance + 
                Vector3.up * ladderExitUpDistance;
            
            // Запускаем анимацию выхода сверху
            if (climbAnimator != null)
            {
                climbAnimator.Play("TopClimbOver");
            }
        }
        else
        {
            ladderTargetPosition = transform.position - 
                exitDirection * ladderExitForwardDistance;
            
            // Запускаем анимацию выхода снизу
            if (climbAnimator != null)
            {
                climbAnimator.Play("BottomClimbOver");
            }
        }
    }

    void UpdateLadderExit()
    {
        ladderTransitionTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(ladderTransitionTimer / ladderExitDuration);
        
        // Плавное перемещение
        transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition, progress);
        
        // Фиксация камеры на лестнице при выходе
        Vector3 lookDirection = ladderCenter - playerCamera.transform.position;
        lookDirection.y = 0;
        playerCamera.transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // Завершение выхода
        if (progress >= 1f)
        {
            CompleteLadderExit();
        }
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
        // Убрать проверку isAgainstWall из условия
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

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            Vector3 forwardForce = (transform.forward * Input.GetAxisRaw("Vertical") +
                                  transform.right * Input.GetAxisRaw("Horizontal")).normalized;
            rb.AddForce(forwardForce * jumpForwardForce, ForceMode.VelocityChange);
        }

        jumpsRemaining--;
        coyoteTimeCounter = 0;
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            jumpsRemaining = maxJumps;
            isJumping = false;
        }

        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
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

        // Всегда разрешать движение, даже при столкновении со стеной
        if (moveHorizontal != 0 || moveVertical != 0)
        {
            Vector3 moveDirection = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
            rb.linearVelocity = new Vector3(
                moveDirection.x * currentSpeed,
                rb.linearVelocity.y,
                moveDirection.z * currentSpeed
            );
        }
        else
        {
            // Останавливать только горизонтальное движение
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
                float additionalForce = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (maxJumpHeight - baseJumpHeight));
                rb.AddForce(Vector3.up * additionalForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    void HandleFall()
    {
        if (rb.linearVelocity.y < 0 && ladderState == LadderState.None)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump") && !isGrounded)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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