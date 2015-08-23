using UnityEngine;
using System.Collections;

public enum SoldierState
{
    Patrol,
    Alerted,
    Aggro,
    ComingToAid,

}

public class Soldier : MonoBehaviour 
{
    public Transform[] patrolRoute;

    int curWP = 0;

    public float PatrolSpeed = 2;

    public Transform playerT;

    public Animator animator;
    public Transform spriteTransform;

    public float sightArcDegree;
    public float raysToCast;

    public float rayCheckLength = 10;
    public float TimeToRayCheck;
    float rayCheckTime;

    NavMeshAgent agent;

    private SoldierState state = SoldierState.Patrol;

    public float playerShootRange = 10;

    public float TimeToShoot;
    float shootTime;

	void Start () 
    {
        GameBroadcaster.Instance.PlayerMadeNoise += HandlePlayerMadeNoise;

        agent = GetComponent<NavMeshAgent>();
        rayCheckTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
    {
        switch(state)
        { 
            case SoldierState.Patrol:
                if (Vector3.Distance(transform.position, patrolRoute[curWP].position) < 1.5f)
                {
                    curWP = (curWP + 1) % patrolRoute.Length;
                }

                agent.SetDestination(patrolRoute[curWP].position);
                agent.speed = PatrolSpeed;
                LookForPlayer();
                break;
            case SoldierState.Alerted:
                print("I am alerted");
                
                LookForPlayer();
                break;
            case SoldierState.Aggro:
                print("I am aggroed");
                if(Vector3.Distance(transform.position, playerT.position) <= playerShootRange && CanSeePlayer())
                {
                    animator.SetBool("Walking", false);
                    agent.SetDestination(transform.position);
                    LookAtPlayer();
                    Fire();
                }
                else
                {
                    animator.SetBool("Walking", true);
                    agent.SetDestination(playerT.position);
                }
                break;

        }

        float angleDif = 0;

        Debug.DrawRay(transform.position, transform.forward, Color.cyan);
        Debug.DrawRay(transform.position, new Vector3(spriteTransform.forward.x, 0, spriteTransform.forward.z), Color.magenta);

        angleDif = Quaternion.FromToRotation(transform.forward, new Vector3(spriteTransform.forward.x,0,spriteTransform.forward.z)).eulerAngles.y;

	    animator.SetFloat("AngleDif", angleDif);
	}

    void Fire()
    {
        if(Time.time - shootTime >= TimeToShoot)
        {
            shootTime = Time.time;

            Debug.DrawLine(transform.position, transform.position + transform.forward * playerShootRange, Color.red, .25f);

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            animator.SetTrigger("Fire");

            if (Physics.Raycast(ray, out hit, rayCheckLength))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    if(Random.Range(0f,1f) < .5f)
                    {
                        print("I shot the player!");
                    }
                    else
                    {
                        print("I missed the player by chance!");
                    }
                    
                }
            }
            else
            {
                print("I missed the player!");
            }

        }
    }

    void LookAtPlayer()
    {
        Vector3 fromRot = transform.forward;
        Vector3 toRot = playerT.position - transform.position;

        fromRot.y = 0;
        toRot.y = 0;

        Quaternion refShift = Quaternion.FromToRotation(fromRot, toRot);
        transform.rotation *= refShift;
    }

    bool CanSeePlayer()
    {
        Vector3 dir = transform.forward;
        Debug.DrawLine(transform.position, transform.position + dir * rayCheckLength, Color.green, .25f);

        Ray ray = new Ray(transform.position, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayCheckLength))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                print("I see the player!!");
                return true;
            }
        }

        return false;
    }

    void LookForPlayer()
    {
        if(Time.time - rayCheckTime >= TimeToRayCheck)
        {
            rayCheckTime = Time.time;
            
            Vector3 dir = transform.forward;
            dir = Quaternion.Euler(0, sightArcDegree * .5f, 0) * dir;

            Quaternion refShift = Quaternion.Euler(0, -(sightArcDegree/ raysToCast), 0);

            for (int i = 0; i < raysToCast; ++i)
            {
                
                Debug.DrawLine(transform.position, transform.position + dir * rayCheckLength, Color.green, .25f);

                Ray ray = new Ray(transform.position, dir);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, rayCheckLength))
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        print("I see the player!!");
                        SwitchState(SoldierState.Aggro);
                    }
                }

                dir = refShift * dir;
            }
        }
    }

    void SwitchState(SoldierState newState)
    {
        curWP = 0;
        if(state == SoldierState.Patrol)
        {
            animator.SetTrigger("Alerted");
            agent.SetDestination(transform.position);
        }

        state = newState;
    }

    void HandlePlayerMadeNoise(GameObject player, float noiseRange)
    {
        if (state != SoldierState.Aggro)
        {
            if (state == SoldierState.Alerted)
                noiseRange *= 1.5f;

            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= noiseRange)
            {
                if (distance <= noiseRange * .5f)
                    SwitchState(SoldierState.Aggro);
                else
                    SwitchState(SoldierState.Alerted);

                print("I hear the player");
            }
        }
    }
}
