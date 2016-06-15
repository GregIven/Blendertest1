/* This is a simple move and attack script which gives the functionality
    to move a NavMeshAgent attached to a Player GameObject upon clicking/holding
    the mouse button. Additional functionality is upon clicking an enemy, it moves
    to attack this enemy with an attack animation. 

    -Added GetHit functionality for enemy to take damage (5/16)

    greg drew - 5/9/16
    */






using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickToMove : MonoBehaviour
{
    //Player vars
    private NavMeshAgent agent;
    public int damage;
    public int health;
    public int maxHealth;
    private int healthPercentage;


    //Anim vars
    private AnimatorStateInfo currentBaseState; //Gets the current state of the mecanim anim controller
    private AnimatorTransitionInfo currentTransition;
    public HashIDs hashes;
    private Animator anim;
    private bool walking;                       //Sets bool to true when NavMeshAgent is moving to destination
    private bool attacked;                      //Sets false to end exit out of the attack animation after entered in the anim state.
    private bool runAttacked;                   //Bool that sets the ability of the player to click to move

    //Attack vars
    public Transform Target;                    //Sets the target for attacking, set true on enemy clicked.
    private bool enemyClicked;                  //Checks that an enemy has been clicked, moved to their position to attack
    public bool impacted;
    private bool attackCheck;

    //Movement vars
    private Quaternion _lookRotation;           //Placeholder for the direction for the transform to turn
    private Vector3 _direction;                 //Gets the difference between the quaternions, Player and Target
    public float RotationSpeed;                 //The speed that is set for rotating to the target


    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        //intiatize the variable to use it.
        hashes = GetComponent<HashIDs>();

    }

    // Update is called once per frame
    void Update()
    {
        //Checks to keep in running state if destination has not been reached
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || Mathf.Abs(agent.velocity.sqrMagnitude) < float.Epsilon)
            {
                walking = false;
            }
        }
        else
        {
            walking = true;
        }


        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool(hashes.attackBool, attacked);
        }

        //Moves to enemy to attack, or if in range attacks.
        if (enemyClicked)
        {
            Attack();
        }

        //Sets the Target(enemy) variable to null if idle state.
        if (currentBaseState.fullPathHash == hashes.idleState && !enemyClicked)
        {
            //Target = null;
            //attacked = false;
        }


        //Debug.Log(" is" + runAttacked);
        //Debug.Log("attacked is " + attacked);

        //if (IsFacing())
        //{
        //    Debug.Log("Facing");
        //}

        //if (!IsFacing())
        //{
        //    Debug.Log("Not facing");
        //}

        //Checks the transition 
        currentTransition = anim.GetAnimatorTransitionInfo(0);

        //This logs to the console when the anim state is transitioning from running to attacking,
        //keep this around because when the transition time is beyond a certain length the player is
        //able to click away at the moment of attack. Setting runAttacked true so the player cannot click away.
        if (currentTransition.IsName("Run -> Attack"))
        {
            Debug.Log("Attack transition");
            runAttacked = true;

        }

        //float attackDist = 2f;
        //float distance = Vector3.Distance(Target.transform.position, transform.position);
        //if (distance <= attackDist)
        //{
        //    Debug.Log("In attack range");
        //}
    }

    void FixedUpdate()
    {
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);             //Gets the current base state of the anim state machine

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
            {
                if (hit.collider.CompareTag("Enemy") && !runAttacked)
                {
                    Target = hit.transform;
                    enemyClicked = true;
                    //agent.Stop();

                }
                else if (!hit.collider.CompareTag("Player") && !runAttacked)
                {
                    // agent.Stop();
                    agent.destination = hit.point;
                    enemyClicked = false;
                    anim.SetBool(hashes.attackBool, enemyClicked);
                    agent.Resume();

                }


            }
        }

        //This checks the range difference between the targets to set the attacked var false.
        //The player can still attack if out of range, this checks to make sure the player's
        //Attack anim only plays when attacking.
        float attackDist = 2f;
        float distance = Vector3.Distance(Target.transform.position, transform.position);
        if (distance <= attackDist || Target == null)
        {
            attacked = false;
        }

        float playBackTime = currentBaseState.normalizedTime % 1;       //This sets the time of the animation (normalized) to a var



        //This if statement checks if the mecanim animation state has entered the attack state. If the attack state has been reached
        // it sets the enemyClicked false, sets the attacked bool false. (This wouldn't exit unless 
        // set up in this statement.) Sets the navAgent destination to the current in world destination.
        if (currentBaseState.fullPathHash == hashes.attackState)
        {
            enemyClicked = false;
            attacked = false;
            //runAttacked = true;
            anim.SetBool(hashes.attackBool, attacked);
            agent.destination = this.transform.position;
            // agent.Stop();

            if (playBackTime > 0.5f && !impacted)
            {
                // Debug.Log("Playback time is: " + playBackTime);
                Target.GetComponent<EnemyScript>().GetHit(damage);
                impacted = true;
            }
        }

        //This function checks if player is not in attack state, and if so
        //Finds the vector pointing from our position to the target (_direction),
        //Creates a rotation we need to be in to look at the target (_lookRotation),
        //Then rotates over time towards enemy position.
        if (enemyClicked && currentBaseState.fullPathHash != hashes.attackState)
        {
            _direction = (Target.position - transform.position).normalized;
            _lookRotation = Quaternion.LookRotation(_direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
        }

        //Sets impact false when the impacted is true and the state machine is in idle state
        if (impacted && currentBaseState.fullPathHash == hashes.idleState)
        {
            impacted = false;

        }

        //This function sets runAttacked to false so the players can only move if in run/idle state
        if (currentBaseState.fullPathHash == hashes.idleState || currentBaseState.fullPathHash == hashes.runState)
        {
            runAttacked = false;
            Debug.Log("eeeeeeee");
        }

        anim.SetBool(hashes.runBool, walking);

    }

    // This function checks first if any enemy object is targeted. Sets the navAgent destination to the target enemy transform
    // if not returned. If the distance is greater than the attacking distance, it moves the navAgent to that destination. If 
    // within attack range, sets the attack bool true for state change. Sets walking bool false to stop walking anim.
    private void Attack()
    {
        float attackDist = 1.5f;
        if (Target == null)
            return;

        agent.destination = Target.position;
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

        if (agent.remainingDistance >= attackDist)
        {
            agent.Resume();
        }

        if (agent.remainingDistance <= attackDist)
        {
            agent.Stop();
            runAttacked = true;
            walking = false;


            anim.SetBool(hashes.attackBool, attacked);
            attacked = true;

        }
    }

    //Bool function to check if the tansform of the player GO is facing the Target variable. Angle of 30 gives a good approx.
    private bool IsFacing()
    {
        float angle = 30f;

        return (Vector3.Angle(transform.forward, Target.transform.position - transform.position) < angle);
    }


}