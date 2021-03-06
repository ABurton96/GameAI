﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
	
	public enum soundLevel {Nothing, Quite, Loud};

	public soundLevel sound;
	public string soundLevelAI;
	public float health;
	public GameObject dead;
	public TextMesh healthText;
	public CharacterController characterCon;

	void Update () 
	{
		//Takes inputs and sets sound level depending on keys pressed
		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
		{
			if(Input.GetKey(KeyCode.LeftShift))
			{
				sound = soundLevel.Loud;
			}
			else if(Input.GetKey(KeyCode.LeftControl))
			{
				sound = soundLevel.Nothing;
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

		//Chages heigh of collider if crouching or standing
		if(Input.GetKey(KeyCode.LeftShift))
		{
			characterCon.height = 1.8f;
		}
		else if(Input.GetKey(KeyCode.LeftControl))
		{
			characterCon.height = 1;
		}
		else
		{
			characterCon.height = 1.8f;
		}

		soundLevelAI = sound.ToString();

		//If dead display test then load menu
		if(health <= 0)
		{
			dead.SetActive(true);
			StartCoroutine(WaitAndLoad());
		}

		healthText.text = "Health: " + health.ToString();

		//Shortcut keys for loading scenes
		if(Input.GetKeyDown(KeyCode.Keypad1))
		{
			SceneManager.LoadScene(1);
		}
		else if(Input.GetKeyDown(KeyCode.Keypad2))
		{
			SceneManager.LoadScene(2);
		}
		else if(Input.GetKeyDown(KeyCode.Keypad3))
		{
			SceneManager.LoadScene(3);
		}
		else if(Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene(0);
		}
	}

	IEnumerator WaitAndLoad()
	{
		//Wait 2 seconds then load menu
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene(0);
	}
}
