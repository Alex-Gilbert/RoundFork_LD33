using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour 
{
    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    public float walkSpeed = 2;
    public float maxJumpForce;    
    public float timeToChargeJump;
    public float leapForce = 3;

    public LayerMask groundMask;

    Transform cameraT;
    float verticalLookRotation;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    Vector3 jumpDir;
    Vector3 jumpDirVel;

    float jumpforce;
    float jumpVel;

    Rigidbody rb;

    bool grounded;

    bool chargingJump = false;

    bool checkingForLeap = false;

    float _doubleTapTimeD;
    float _doubleTapTimeA;

	// Use this for initialization
	void Start () 
    {
        jumpDir = new Vector3(1, 0, 0);
        cameraT = Camera.main.transform;
        rb = this.GetComponent<Rigidbody>();
	}

    public void FixedUpdate()
    {
        if(!chargingJump)
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
	
	// Update is called once per frame
	void Update () 
    {


        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
        cameraT.localEulerAngles = Vector3.left * verticalLookRotation;

        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount = moveDir * walkSpeed;

        bool doubleTapA = DoubleTapA();
        bool doubleTapD = DoubleTapD();

        moveAmount =  Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        grounded = false;


        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1 + .1f , groundMask))
        {
            grounded = true;
        }
        Debug.DrawLine(transform.position, transform.position - transform.up * 1.1f, chargingJump ? Color.green : Color.red);

        if(Input.GetButton("Jump") && !chargingJump && grounded)
        {
            StopCoroutine(ChargeJump());
            StartCoroutine(ChargeJump());
        }

        if(!chargingJump && grounded)
        {
            if (doubleTapA)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(transform.rotation * new Vector3(-1, .35f, 0) * 100 * leapForce);
            }
            else if (doubleTapD)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(transform.rotation * new Vector3(1, .35f, 0) * 100 * leapForce);
            }
        }
	}

    bool DoubleTapD()
    {
        bool doubleTapD = false;

        #region doubleTapD

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time < _doubleTapTimeD + .25f)
            {
                doubleTapD = true;
            }
            _doubleTapTimeD = Time.time;
        }

        #endregion

        if (doubleTapD)
        {
            Debug.Log("DoubleTapD");
            return true;
        }

        return false;
    }

    bool DoubleTapA()
    {
        bool doubleTapA = false;

        #region doubleTapA

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time < _doubleTapTimeA + .25f)
            {
                doubleTapA = true;
            }
            _doubleTapTimeA = Time.time;
        }

        #endregion

        if (doubleTapA)
        {
            Debug.Log("DoubleTapA");
            return true;
        }

        return false;
    }


    IEnumerator ChargeJump()
    {
        GetComponent<JumpPreview>().SetDrawPath(true);
        chargingJump = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        jumpforce = 0;
        jumpDir = new Vector3(0, 0, 1);
        while(Input.GetButton("Jump"))
        {
            jumpforce = Mathf.SmoothDamp(jumpforce, maxJumpForce, ref jumpVel, timeToChargeJump);
            jumpDir = Vector3.SmoothDamp(jumpDir, new Vector3(0, .4f, 1), ref jumpDirVel, timeToChargeJump);

            GetComponent<JumpPreview>().force = transform.rotation * jumpDir * jumpforce * 100;

            Debug.DrawLine(transform.position, transform.position + (transform.rotation * jumpDir * jumpforce));
            print(string.Format("JumpForce: {0}", jumpforce));
            yield return null;
        }

        rb.AddForce((transform.rotation * jumpDir * jumpforce * 100));

        chargingJump = false;
        GetComponent<JumpPreview>().SetDrawPath(false);
        yield return null;
    }
}
