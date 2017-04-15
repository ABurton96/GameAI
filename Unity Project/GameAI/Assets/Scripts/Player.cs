using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	
	public enum soundLevel {Nothing, Quite, Loud};

	public soundLevel sound;
	public string soundLevelAI;
	public float health;

	void Update () 
	{
		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
		{
			if( Input.GetKey(KeyCode.LeftShift))
			{
				sound = soundLevel.Loud;
			}
			else
			{
				sound = soundLevel.Quite;
			}
		}
		else
		{
			sound = soundLevel.Nothing;
		}

		soundLevelAI = sound.ToString();

		if(health <= 0)
		{
			Debug.Log("Dead");
		}
	}
}
