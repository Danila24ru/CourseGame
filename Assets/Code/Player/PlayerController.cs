using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.Cameras;

public class PlayerController : NetworkBehaviour {

    private Animator animator;
    private Rigidbody thisRigidbody;

    [SyncVar]
    private float currentSpeed;
    public float walkSpeed = 2;
    public float sprintSpeed = 5;
    public float playerRotateSpeed = 1f;

    public float jumpHeight = 4;

    [SyncVar]
    protected float verticalAxis;   // принимает значения от -1 до 1 при нажатии W,S
    [SyncVar]
    protected float horizontalAxis; // принимает значения от -1 до 1 при нажатии A,D
    private float angle;          // угол поворот при перемещении
    private float cameraPitch;

    [SerializeField]
    private Vector3 spineOffsetRotation;
    private Transform spine;

    [SyncVar]
    public bool isGrounded;  //стоит ли персонаж на поверхности
    [SyncVar]
    protected bool isAiming;
    [SyncVar]
    private bool inAction; //находится ли персонаж в боевом состоянии
    
    private Text debugText;

    private FreeLookCam freeLookCam;
    private Transform cameraPivot;
    private bool cameraRL;

    Vector3 cameraForward;
    Vector3 cameraRight;
    Vector3 moveDirection;

    Quaternion rotateTo; // rotate player to movement direction


    private void Start ()
    {
        animator      = GetComponent<Animator>();
        thisRigidbody = GetComponent<Rigidbody>();
        freeLookCam = GameObject.Find("FreeLookCameraRig").GetComponent<FreeLookCam>();
        cameraPivot = GameObject.Find("PivotCamera").GetComponent<Transform>();
        debugText   = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>();
        spine = transform.Find("root/pelvis/spine_01");

        transform.name = "Player " + this.netId;
        freeLookCam.m_LockCursor = true;
    }
	
	private void Update ()
    {
        UpdateAnimationStates();

        if (!isLocalPlayer)
            return;

        CheckIsGrounded();
        SetAimFOV(isAiming);

        CmdSetIsAiming(Input.GetKey(KeyCode.Mouse1)); // правая кнопка мыши

        if (Input.GetKeyDown(KeyCode.Alpha4)) 
            ChangeCameraSide();

        if(Input.GetKey(KeyCode.Mouse0)) //левая кнопка мыши
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit lookPointHit;
            Vector3 HitPosition = new Vector3();
            if (Physics.Raycast(ray, out lookPointHit, Mathf.Infinity))
            {
                HitPosition = lookPointHit.point;
            }
            CmdDoFire(true, HitPosition);
        }

        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            CmdDoFire(false, Vector3.forward);
        }
        
        if(Input.GetKeyDown(KeyCode.R))
        {
            Weapon weapon = GetComponentInChildren<Weapon>();
            weapon.CmdReload();
            animator.SetBool("IsReloading", true);
        }
        else
        {
            animator.SetBool("IsReloading", false);
        }
        
        if (isAiming || inAction)
        {
            CmdSetMovementAxis(verticalAxis, horizontalAxis);
            rotateTo = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotateTo, playerRotateSpeed * Time.deltaTime);
        }

        

        if(!isServer)
        {
            CmdSetCurrentSpeed(currentSpeed);
            CmdSetIsGrounded(isGrounded);
        }

    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        MovePlayer();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            //thisRigidbody.velocity = new Vector3(0, jumpHeight, 0);
            thisRigidbody.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse);
            //thisRigidbody.MovePosition(transform.position + new Vector3(0, jumpHeight, 0));
            isGrounded = false;
            
        }
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
            return;
        if (isAiming || inAction)
            SpineRotation();
    }

    [Command(channel = 0)]
    private void CmdDoFire(bool isFiring, Vector3 HitPosition)
    {
        if(isFiring)
        {
            Weapon weapon = GetComponentInChildren<Weapon>();
            weapon.CmdFire(HitPosition);
            inAction = true;
        }

        CmdSetAnimatorValueBool("Fire", isFiring);
    }
    [Command(channel = 1)]
    protected void CmdSetAnimatorValueBool(string id, bool value)
    {
        animator.SetBool(id, value);
        RpcSetAnimatorValueBool(id, value);
    } 
    [ClientRpc(channel = 1)]
    protected void RpcSetAnimatorValueBool(string id, bool value)
    {
        animator.SetBool(id, value);
    }

    /// <summary>
    /// Синхронизация Axis значений
    /// </summary>
    /// <param name="verticalAxis">W,S клавиши</param>
    /// <param name="horizontalAxis">A,D клавиши</param>
    [Command(channel = 1)]
    private void CmdSetMovementAxis(float verticalAxis, float horizontalAxis)
    {
        this.verticalAxis = verticalAxis;
        this.horizontalAxis = horizontalAxis;
    }
    [Command(channel = 1)]
    private void CmdSetInAction(bool inAction)
    {
        this.inAction = inAction;
    }
    [Command(channel = 1)]
    private void CmdSetCurrentSpeed(float speed)
    {
        currentSpeed = speed;
    }
    [Command(channel = 0)]
    private void CmdSetIsGrounded(bool isGrounded)
    {
        this.isGrounded = isGrounded;
    }
    [Command(channel = 0)]
    private void CmdSetIsAiming(bool isAiming)
    {
        this.isAiming = isAiming;
    }

    //изменение положения камеры относительно персонажа (справа/слево)
    private void ChangeCameraSide()
    {
        if (cameraRL)
            cameraPivot.transform.localPosition = new Vector3(0.5f, 1.5f, 0);
        else
            cameraPivot.transform.localPosition = new Vector3(-0.5f, 1.5f, 0);

        cameraRL = !cameraRL;
    }

    //проверка бежит ли персонаж
    private void CheckPlayerSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            CmdSetInAction(false);
            currentSpeed = sprintSpeed;
        }
        else
            currentSpeed = walkSpeed;
    }

    //Изменение Field Of View при прицеливании
    private void SetAimFOV(bool isAiming)
    {
        if (isAiming)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 50f, Time.deltaTime * 10f);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70f, Time.deltaTime * 10f);
        }
    }

    //вращение спины при прицеливании
    private void SpineRotation()
    {
        Vector3 eulerAngleOffset = Vector3.zero;
        eulerAngleOffset = new Vector3(spineOffsetRotation.x, spineOffsetRotation.y, spineOffsetRotation.z);

        Ray ray = new Ray(cameraPivot.transform.position, cameraPivot.transform.forward);
        Debug.DrawRay(cameraPivot.transform.position, cameraPivot.transform.forward, Color.green);
        Vector3 lookPos = ray.GetPoint(50f);

        Quaternion.Slerp(spine.transform.rotation, Quaternion.LookRotation(lookPos), 20f);
        spine.LookAt(lookPos);
        spine.Rotate(eulerAngleOffset);
    }

    //проверка, стоит ли персонаж на поверхности
    private bool CheckIsGrounded()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0), Vector3.down * 0.3f, Color.green);
        return isGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down * 0.3f, 0.15f);
    }

    private void UpdateAnimationStates()
    {
        if (currentSpeed == sprintSpeed)
            animator.SetFloat("Velocity", 1f, 0.3f, Time.deltaTime);
        else if (currentSpeed == walkSpeed)
            animator.SetFloat("Velocity", 0.5f, 0.1f, Time.deltaTime);
        else
            animator.SetFloat("Velocity", 0f, 0.3f, Time.deltaTime);

        animator.SetFloat("HorizontalAxis", horizontalAxis);
        animator.SetFloat("VerticalAxis", verticalAxis);
        animator.SetBool("IsGrounded", isGrounded);

        if (inAction)
        {
            animator.SetBool("IsAiming", true);
        }
        else
        {
            animator.SetBool("IsAiming", isAiming);
        }
    }

    private void MovePlayer()
    {
        verticalAxis = Input.GetAxis("Vertical");
        horizontalAxis = Input.GetAxis("Horizontal");
        angle = Mathf.Atan2(horizontalAxis, verticalAxis) * Mathf.Rad2Deg;

        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            CheckPlayerSpeed();

            cameraForward = Camera.main.transform.TransformDirection(Vector3.forward);
            cameraForward.y = 0;
            cameraForward = cameraForward.normalized;

            cameraRight = new Vector3(cameraForward.z, 0, -cameraForward.x);

            moveDirection = Vector3.zero;
            moveDirection = (horizontalAxis * cameraRight + verticalAxis * cameraForward);

            moveDirection = moveDirection.normalized * currentSpeed * Time.deltaTime;

            thisRigidbody.MovePosition(transform.position + moveDirection);

            if (!isAiming && !inAction)
            {
                rotateTo = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y + angle, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotateTo, playerRotateSpeed * Time.deltaTime);
            }
        }
        else
            currentSpeed = 0;
    }
}
