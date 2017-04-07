using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.Cameras;

public class PlayerController : NetworkBehaviour {

    protected Animator animator;
    protected Rigidbody thisRigidbody;

    [SyncVar]
    protected float currentSpeed;
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

    public Vector3 spineOffsetRotation;
    private Transform spine;

    [SyncVar]
    public bool isGrounded;
    [SyncVar]
    protected bool isAiming;
    [SyncVar]
    private bool inAction;
    

    private Text debugText;

    private FreeLookCam freeLookCam;
    private Transform cameraPivot;
    private bool cameraRL;

    Vector3 cameraForward;
    Vector3 cameraRight;
    Vector3 moveDirection;

    Quaternion rotateTo; // rotate player to movement direction

    // Use this for initialization
    protected virtual void Start ()
    {
        animator  = GetComponent<Animator>();
        thisRigidbody = GetComponent<Rigidbody>();
        freeLookCam = GameObject.Find("FreeLookCameraRig").GetComponent<FreeLookCam>();
        cameraPivot = GameObject.Find("PivotCamera").GetComponent<Transform>();
        spine = transform.Find("root/pelvis/spine_01");

        debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>();
        transform.name = "Player " + this.netId;
        freeLookCam.m_LockCursor = true;
    }
	
	// Update is called once per frame
	protected virtual void Update ()
    {
        if (currentSpeed == sprintSpeed)
            animator.SetFloat("Velocity", 1f, 0.3f, Time.deltaTime);
        else if (currentSpeed == walkSpeed)
            animator.SetFloat("Velocity", 0.5f, 0.3f, Time.deltaTime);
        else
            animator.SetFloat("Velocity", 0f, 0.3f, Time.deltaTime);

        animator.SetFloat("HorizontalAxis", horizontalAxis);
        animator.SetFloat("VerticalAxis", verticalAxis);
        animator.SetBool("IsGrounded", isGrounded);

        if(inAction)
        {
            animator.SetBool("IsAiming", true);
        }
        else
        {
            animator.SetBool("IsAiming", isAiming);
        }
        

        if (!isLocalPlayer)
            return;

        CheckIsGrounded();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            ChangeCameraSide();

        CmdSetIsAiming(Input.GetKey(KeyCode.Mouse1));

        if(Input.GetKey(KeyCode.Mouse0))
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
        else
        {
            CmdDoFire(false, Vector3.forward);
        }
        
        if (isAiming || inAction)
        {
            CmdSetMovementAxis(verticalAxis, horizontalAxis);
            rotateTo = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotateTo, playerRotateSpeed * Time.deltaTime);
            
        }
        SetAimFOV(isAiming);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            thisRigidbody.velocity = new Vector3(0, jumpHeight, 0);
            isGrounded = false;
        }


        if (isServer) /// Логика на стороне хоста ///
        {

        }
        else /// Логика на стороне клиента ///в
        {
            CmdSetCurrentSpeed(currentSpeed);
            CmdSetIsGrounded(isGrounded);
        }

    }
    

    protected virtual void FixedUpdate()
    {

        if (!isLocalPlayer)
            return;

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
            moveDirection = (horizontalAxis * cameraRight + verticalAxis * cameraForward) * currentSpeed;

            if (currentSpeed == walkSpeed)
                moveDirection = Vector3.ClampMagnitude(moveDirection, walkSpeed);
            if (currentSpeed == sprintSpeed)
                moveDirection = Vector3.ClampMagnitude(moveDirection, sprintSpeed);

            moveDirection.y = thisRigidbody.velocity.y;

            thisRigidbody.velocity = moveDirection;


            if (!isAiming && !inAction)
            {
                rotateTo = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y + angle, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotateTo, playerRotateSpeed * Time.deltaTime);
            }
        }
        else
            currentSpeed = 0;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer)
            return;
        if (isAiming || inAction)
            SpineRotation();
    }

    [Command(channel = 0)]
    protected void CmdSetIsAiming(bool isAiming)
    {
        this.isAiming = isAiming;
    }

    [Command(channel = 0)]
    protected void CmdDoFire(bool isFiring, Vector3 HitPosition)
    {
        if(isFiring)
        {
            Weapon weapon = GetComponentInChildren<Weapon>();
            weapon.CmdFire(HitPosition);
            inAction = true;
        }

        CmdSetAnimatorValueBool("Fire", isFiring);
    }

    [Command(channel = 0)]
    protected void CmdSetIsGrounded(bool isGrounded)
    {
        this.isGrounded = isGrounded;
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

    [Command(channel = 1)]
    protected void CmdSetCurrentSpeed(float speed)
    {
        currentSpeed = speed;
    }

    /// <summary>
    /// Синхронизация Axis значений
    /// </summary>
    /// <param name="verticalAxis">W,S клавиши</param>
    /// <param name="horizontalAxis">A,D клавиши</param>
    [Command(channel = 1)]
    protected void CmdSetMovementAxis(float verticalAxis, float horizontalAxis)
    {
        this.verticalAxis = verticalAxis;
        this.horizontalAxis = horizontalAxis;
    }

    [Command(channel = 1)]
    private void CmdSetInAction(bool inAction)
    {
        this.inAction = inAction;
    }

    void ChangeCameraSide()
    {
        if (cameraRL)
            cameraPivot.transform.localPosition = new Vector3(0.5f, 1.5f, 0);
        else
            cameraPivot.transform.localPosition = new Vector3(-0.5f, 1.5f, 0);

        cameraRL = !cameraRL;
    }

    void CheckPlayerSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            CmdSetInAction(false);
            currentSpeed = sprintSpeed;
        }
        else
            currentSpeed = walkSpeed;
    }
    void SetAimFOV(bool isAiming)
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

    void SpineRotation()
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

    protected bool CheckIsGrounded()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0), Vector3.down * 0.3f, Color.green);
        return isGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down * 0.3f, 0.15f);
    }

}
