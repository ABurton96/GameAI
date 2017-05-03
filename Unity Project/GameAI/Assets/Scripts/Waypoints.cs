using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour {

	public float delay;
	public Vector3 position;

	void Awake () 
	{
		//Sets position to its world position
		position = transform.position;
	}
}
