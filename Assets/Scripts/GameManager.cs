using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public float T;

	// Use this for initialization
	void Start () {

        T = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {

        Time.timeScale = T;
	}
}
