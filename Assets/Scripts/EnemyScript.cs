using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {

    //Enemy object vars
    public int health;
    public int maxHealth;
    public float healthPercentage;

    private NavMeshAgent agent;

    public ClickToMove opponent;

    public Transform player;

    private float oppRadius = 1.5f;

    public int range;

    private Vector3 oppPos;

	// Use this for initialization
	void Start () {

        agent = GetComponent<NavMeshAgent>();
        opponent = player.GetComponent<ClickToMove>();
        oppPos = (opponent.transform.position * oppRadius);
        health = maxHealth;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate ()
    {
        //Debug.Log("Enemy Health: " + health);
    }

    //Basic function to take damage from enemys attack script
    public void GetHit(int damage)
    {
        health = health - damage;
        if(health < 0)
        {
            health = 0;
        }
    }

    bool InRange()
    {
        return (Vector3.Distance(transform.position, player.position) < range);
    }
}
