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

    public float rayCheckLength = 10;
    public float TimeToRayCheck;
    float rayCheckTime;

    NavMeshAgent agent;

    private SoldierState state = SoldierState.Patrol;

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
                break;

        }

        float angleDif = 0;

        Debug.DrawRay(transform.position, transform.forward, Color.cyan);
        Debug.DrawRay(transform.position, new Vector3(spriteTransform.forward.x, 0, spriteTransform.forward.z), Color.magenta);

        angleDif = Quaternion.FromToRotation(transform.forward, new Vector3(spriteTransform.forward.x,0,spriteTransform.forward.z)).eulerAngles.y;

	    animator.SetFloat("AngleDif", angleDif);
	}

    void LookForPlayer()
    {
        if(Time.time - rayCheckTime >= TimeToRayCheck)
        {
            rayCheckTime = Time.time;

            Debug.DrawLine(transform.position, transform.position + transform.forward * rayCheckLength, Color.green, .25f);

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, rayCheckLength))
            {
                if(hit.collider.gameObject.tag == "Player")
                {
                    print("I see the player!!");
                    SwitchState(SoldierState.Aggro);
                }
            }

            Debug.DrawRay(transform.position, transform.forward, Color.cyan);
        }
    }

    void SwitchState(SoldierState newState)
    {
        curWP = 0;
        if(state == SoldierState.Patrol)
        {
            agent.SetDestination(transform.position);
        }

        state = newState;
    }

    void HandlePlayerMadeNoise(GameObject player, float noiseRange)
    {
        if (state == SoldierState.Alerted)
            noiseRange *= 1.5f;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if(distance <= noiseRange)
        {
            if (distance <= noiseRange * .5f)
                SwitchState(SoldierState.Aggro);
            else
                SwitchState(SoldierState.Alerted);

            print("I hear the player");
        }
    }
}
