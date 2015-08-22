using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour 
{
    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    public float walkSpeed = 2;
    public float maxJumpForce;
    public float timeToChargeJump;
    
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

	// Use this for initialization
	void Start () 
    {
        jumpDir = new Vector3(1, 0, 0);
        cameraT = Camera.main.transform;
        rb = this.GetComponent<Rigidbody>();
	}

    public void FixedUpdate()
    {
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

        moveAmount =  Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        grounded = false;


        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1 + .1f , groundMask))
        {
            grounded = true;
        }
        Debug.DrawLine(transform.position, transform.position - transform.up * 1.1f, chargingJump ? Color.green : Color.red);

        if(Input.GetButton("Jump") && !chargingJump)
        {
            StopCoroutine(ChargeJump());
            StartCoroutine(ChargeJump());
        }
	}

    IEnumerator Leap(bool left)
    {
        checkingForLeap = true;
        float timeSinceTap = 0;

        bool letGo = false;
        while(timeSinceTap < .25f)
        {
            timeSinceTap += Time.deltaTime;
            if(Input.GetAxisRaw("Horizontal") == 0)
            {
                letGo = true;
                break;
            }
            yield return null;
        }

        bool leap = false;
        if(letGo)
        {
            timeSinceTap = 0;
            while (timeSinceTap < .25f)
            {
                timeSinceTap += Time.deltaTime;
                if ((left && Input.GetAxisRaw("Horizontal") == -1) || (!left && Input.GetAxisRaw("Horizontal") == 1))
                {
                    leap = true;
                    break;
                }
                yield return null;
            }

            if(leap)
            {
                
                rb.AddForce(transform.rotation * new Vector3(left?-1:1, .5f, 0) * 100);
            }
        }

        checkingForLeap = false;
        yield return null;
    }

    IEnumerator ChargeJump()
    {
        chargingJump = true;
        jumpforce = 0;
        jumpDir = new Vector3(0, 0, 1);
        while(Input.GetButton("Jump"))
        {
            jumpforce = Mathf.SmoothDamp(jumpforce, maxJumpForce, ref jumpVel, timeToChargeJump);
            jumpDir = Vector3.SmoothDamp(jumpDir, new Vector3(0, .5f, 1), ref jumpDirVel, timeToChargeJump);

            Debug.DrawLine(transform.position, transform.position + (transform.rotation * jumpDir * jumpforce));
            print(string.Format("JumpForce: {0}", jumpforce));
            yield return null;
        }

        rb.AddForce((transform.rotation * jumpDir * jumpforce * 75));

        chargingJump = false;
        yield return null;
    }
}
