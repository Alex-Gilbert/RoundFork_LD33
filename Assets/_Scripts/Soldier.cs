using UnityEngine;
using System.Collections;

public class Soldier : MonoBehaviour 
{

    public Animator animator;

    public Transform spriteTransform;

	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        float angleDif = 0;

        Debug.DrawRay(transform.position, transform.forward, Color.cyan);
        Debug.DrawRay(transform.position, new Vector3(spriteTransform.forward.x, 0, spriteTransform.forward.z), Color.magenta);

        angleDif = Quaternion.FromToRotation(transform.forward, new Vector3(spriteTransform.forward.x,0,spriteTransform.forward.z)).eulerAngles.y;

	    animator.SetFloat("AngleDif", angleDif);
	}
}
