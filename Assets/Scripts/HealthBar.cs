using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    public Image bar;
    public Image frame;
    public EnemyScript enemy;
    public float healthPercentage;
    public Image fullOrb;
    public GameObject[] player;
    public GameObject[] eN;
    public ClickToMove go_user;                    //This is for game object that holds this script
	// Use this for initialization
	void Start () {
        bar.enabled = false;
        frame.enabled = false;
        go_user = GetComponent<ClickToMove>();
        player = GameObject.FindGameObjectsWithTag("Player");
        eN = GameObject.FindGameObjectsWithTag("Enemy");
	}
	
	// Update is called once per frame
	void Update () {
	    if(go_user.impacted == true)
        {
            bar.enabled = true;
            frame.enabled = true;
            UpdateBar();
        }

        CheckHealth();
	}

    void UpdateBar()
    {
        if(enemy.health <= enemy.maxHealth)
        {
            bar.fillAmount = enemy.healthPercentage;
        }
    }

    void CheckHealth()
    {
        if (go_user.health <= go_user.maxHealth)
        {
            healthPercentage = (float)go_user.health / go_user.maxHealth;
            fullOrb.fillAmount = healthPercentage;
        }
    }
}
