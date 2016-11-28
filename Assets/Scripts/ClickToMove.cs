/* This is a simple move and attack script which gives the functionality
    to move a NavMeshAgent attached to a Player GameObject upon clicking/holding
    the mouse button. Additional functionality is upon clicking an enemy, it moves
    to attack this enemy with an attack animation. 

    -Added GetHit functionality for enemy to take damage (5/16)
    -Reworked statemachine to transition from attack to run (7/16)
    -Fixed the agent.stop() function in Queued attack phase (7/24)

    greg drew - 5/9/16
    */






using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickToMove : MonoBehaviour
{
    //Player vars
    private NavMeshAgent agent;                         //This moves the player
    public PlayerStats pStats;                          //This is the player stat class
    
    //Anim vars
    private AnimatorStateInfo currentBaseState;         //Gets the current state of the mecanim anim controller
    private AnimatorTransitionInfo currentTransition;   //This is the current state of the mecanim tranistion.
    public HashIDs hashes;
    private Animator anim;
    private bool isWalking;                               //Sets bool to true when NavMeshAgent is moving to destination
    private bool isAttacking;                              //Sets false to exit out of the attack animation after entered in the anim state.
    private bool attackBreak;                           //If false; the player can click to move. Should only be true before impact.
    private bool clickAway;                                 //Set for if the player has clicked to move during the attack anim

    //Attack vars
    public Transform Target = null;                    //Sets the target for attacking, set true on enemy clicked.
    public float attackBreakMeasure;                   //Sets at which point in the attack state clicking away breaks the animation.
    public bool impacted;                      //Turns true when the player "lands" a hit. Deals damage at this point in the anim to the enemy. This should only be true once per attack anim.
    public GameObject Enemy;
    private bool enemyClicked;                  //Checks that an enemy has been clicked, moved to their position to attack. When true this loops the attack function. Should only be true once per impact.
    private bool attackToRun;               //Checks when the player has landed a hit and can transition back to running. True to return to run state from attack state.

    //Movement vars
    private Quaternion _lookRotation;           //Placeholder for the direction for the transform to turn
    private Vector3 _direction;                 //Gets the difference between the quaternions, Player and Target
    public float RotationSpeed;                 //The speed that is set for rotating to the target


    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        hashes = GetComponent<HashIDs>();
        pStats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        //Basic init
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);             //Gets the current base state of the anim state machine
        currentTransition = anim.GetAnimatorTransitionInfo(0);


        //Checks to keep in running state if destination has not been reached
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || Mathf.Abs(agent.velocity.sqrMagnitude) < float.Epsilon)
            {
                isWalking = false;
                //Debug.Log("Yet another log");
            }
        }
        else
        {
            isWalking = true;
        }


        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool(hashes.attackBool, isAttacking);
        }

        //Moves to enemy to attack, or if in range attacks.
        //if (enemyClicked)
        //{
        //    Attack();
        //    Debug.Log("Attack1");
        //}

        //Logic statements

        //Debug.Log("enemyclicked is " + enemyClicked);
        //Debug.Log("impacted is " + impacted);
        //Debug.Log("attackBreak is " + attackBreak);
        //Debug.Log("isAttacking " + isAttacking);
        //Debug.Log("attacktorun" + attackToRun);

        //checks 
        if (agent.remainingDistance >= agent.stoppingDistance)
        {
            //Debug.Log("Path not complete");
        }
        if (agent.hasPath)
        {
            //Debug.Log("Has path");
        }


        //Checks the transition 

        //
        //if (anim.GetAnimatorTransitionInfo(0).IsName("Base Layer.Run -> Base Layer.Attack"))
        //{
        //    Debug.Log("iii00");
        //}

        //This sets the bool false when transitioning out of attack state.
        if (anim.GetAnimatorTransitionInfo(0).IsName("Attack -> Idle") || anim.GetAnimatorTransitionInfo(0).IsName("Attack -> Run"))
        {
            attackToRun = false;
        }
        //float attackDist = 2f;
        //float distance = Vector3.Distance(Target.transform.position, transform.position);
        //if (distance <= attackDist)
        //{
        //    Debug.Log("In attack range");
        //}

        //Update function state sets.
        anim.SetBool(hashes.runBool, isWalking);
        //anim.SetBool(hashes.attackBool, attackBreak);


        //Misc updates
        Enemy = Target.gameObject;     
        
    }

    void FixedUpdate()
    {
        //currentBaseState = anim.GetCurrentAnimatorStateInfo(0);             //Gets the current base state of the anim state machine

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    if (!attackBreak)
                    {
                        Target = hit.transform;
                        enemyClicked = true;
                    }
                    if (attackBreak)
                    {
                        // Target = hit.transform;
                        //Debug.Log("uppp");
                    }

                }


                else if (!hit.collider.CompareTag("Player"))
                {
                    if (!attackBreak)
                    {
                        agent.destination = hit.point;
                        enemyClicked = false;
                        //anim.SetBool(hashes.attackBool, enemyClicked);
                        agent.Resume();
                    }
                    if (attackBreak && !clickAway)
                    {
                        agent.destination = hit.point;
                        //Debug.Log("wewewe");
                    }
                }
            }
        }

        if (enemyClicked)
        {
            float attackDist = 1.3f;
            if (Target == null)
                return;

            agent.destination = Target.position;

            if (agent.remainingDistance >= attackDist)
            {
                agent.Resume();
                //Debug.Log("resume");
            }

            if (agent.remainingDistance <= attackDist)
            {
                Attack();
                //Debug.Log("attack");
            }
            //Debug.Log("Attack1");
        }

        if (anim.GetAnimatorTransitionInfo(0).IsName("Run -> Attack"))
        {
            Debug.Log("iii00");
        }

        if (currentBaseState.fullPathHash == hashes.attackState && currentBaseState.normalizedTime > .20)
        {
            anim.ResetTrigger(hashes.attackTrig);
            enemyClicked = false;
            attackBreak = false;

            Strike(pStats.damage);
            //Debug.Log("poop1");
        }
        //Doesnt allow attacking if out of distance
        //float attackDist = 2f;
        //float distance = Vector3.Distance(Target.transform.position, transform.position);
        //if (distance <= attackDist || Target == null)
        //{
        //    isAttacking = false;
        //}

        ////PRE ATTACK
        //if (currentBaseState.fullPathHash == hashes.attackState && currentBaseState.normalizedTime < .09)
        //{
        //    enemyClicked = false;
        //    attackBreak = true;
        //    //Debug.Log("Pre attack");
        //}
        ////ATTACK PHASE 1
        ////This if statement checks if the mecanim animation state has entered the attack state. If the attack state has been reached
        //// it, this sets the enemyClicked false to exit attack state, sets the isAttacking bool false. (This won't exit unless 
        //// set up in this statement.) Sets the navAgent to current destination.
        //if (currentBaseState.fullPathHash == hashes.attackState && !impacted && currentBaseState.normalizedTime < .11)
        //{
        //    enemyClicked = false;
        //    //attackBreak = false;
        //    isAttacking = false;
        //    anim.SetBool(hashes.attackBool, isAttacking);
        //    agent.ResetPath();
        //    Debug.Log("Phase 1");
        //}

        ////ATTACK PHASE 2
        ////This statement sets impact and does damage to enemy, allows the player to move once they have impacted.
        //if (currentBaseState.fullPathHash == hashes.attackState && currentBaseState.normalizedTime > attackBreakMeasure)
        //{
        //    Target.GetComponent<GetDamaged>().GetHit(pStats.damage);
        //    impacted = true;
        //    attackBreak = false;
        //    clickAway = true;
        //    //enemyClicked = false;
        //    agent.Resume();
        //    //if (agent.hasPath)
        //    //{
        //    //    enemyClicked = true;
        //    //}
        //    Debug.Log("Phase2");
        //}

        ////ATTACK PHASE 3
        ////This resets the impact, it begins right before the mecanim transition to run state if attackToRun was true.
        //if (currentBaseState.fullPathHash == hashes.attackState && !agent.hasPath && currentBaseState.normalizedTime > .95)
        //{
        //    impacted = false;
        //    enemyClicked = false;
        //    attackToRun = false;
        //    Debug.Log("phase 3");
        //}

        ////This statement faces the target while attacking.
        //if (enemyClicked && currentBaseState.fullPathHash != hashes.attackState)
        //{
        //    _direction = (Target.position - transform.position).normalized;
        //    _lookRotation = Quaternion.LookRotation(_direction);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
        //}

        ////RESET PHASE
        ////This function sets attackBreak to false when the player is not in attack state, allowing the player to move.
        //if (impacted && currentBaseState.fullPathHash == hashes.idleState || currentBaseState.fullPathHash == hashes.runState)
        //{
        //    attackBreak = false;
        //    impacted = false;
        //    clickAway = false;
        //    anim.SetBool(hashes.attackBreak, attackBreak);
        //    //enemyClicked = false;
        //    //Debug.Log("Reset");

        //}

        ////QUEUED RUN PHASE (RUNS AFTER A DESTINATION HAS BEEN SET DURING ATTACK PHASE)
        ////This statement stops the player from moving during the attack animation.
        //if (!agent.hasPath && currentBaseState.fullPathHash == hashes.attackState && attackBreak)
        //{
        //    agent.Stop();
        //    //Debug.Log("Queued");
        //}

        //if (currentBaseState.fullPathHash == hashes.attackState && currentBaseState.normalizedTime > attackBreakMeasure 
        //    && agent.hasPath == true && agent.velocity.sqrMagnitude > 0)
        //{
        //    attackToRun = true;
        //    anim.SetBool(hashes.attackBreak, attackToRun);
        //    Debug.Log("Transition run");
        //}


        //if (impacted && currentBaseState.fullPathHash == hashes.attackState)
        //{
        //    attackBreak = false;
        //    impacted = false;
        //    //attackToRun = false;
        //    Debug.Log("poop");
        //}
        //currentTransition = anim.GetAnimatorTransitionInfo(0);

        ////
        //if (anim.GetAnimatorTransitionInfo(0).IsName("Run -> Attack"))
        //{
        //    Debug.Log("iii00");
        //}

    }

    // This function checks first if any enemy object is targeted. Sets the navAgent destination to the target enemy transform
    // if not returned. If the distance is greater than the attacking distance, it moves the navAgent to that destination. If 
    // within attack range, sets the attack bool true for state change. Sets isWalking bool false to stop isWalking anim.
    private void Attack()
    {
        //float attackDist = 1.3f;
        //if (Target == null)
        //    return;

        //agent.destination = Target.position;
        ////currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

        //if (agent.remainingDistance >= attackDist)
        //{
        //    agent.Resume();
        //    //Debug.Log("Running");
        //}

        //if (agent.remainingDistance <= attackDist)
        //{
        //    //agent.Stop();
        //    attackBreak = true;
        //    isWalking = false;
        //    isAttacking = true;
        //    //enemyClicked = false;
        //    //anim.SetBool(hashes.attackBool, isAttacking);
        //    anim.SetTrigger(hashes.attackTrig);
        //    //isAttacking = true;
        //    Debug.Log("slap");
        //}

        anim.SetTrigger(hashes.attackTrig);
        //Debug.Log("in it");
    }

    //Bool function to check if the tansform of the player GO is facing the Target variable. Angle of 30 gives a good approx.
    private bool IsFacing()
    {
        float angle = 30f;

        return (Vector3.Angle(transform.forward, Target.transform.position - transform.position) < angle);
    }

    void Strike(int damage)
    {
        Target.GetComponent<GetDamaged>().GetHit(damage);

    }
}