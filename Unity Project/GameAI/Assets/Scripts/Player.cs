using System.Collections;
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

	void Update () 
	{
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

		soundLevelAI = sound.ToString();
		
		if(health <= 0)
		{
			dead.SetActive(true);
			StartCoroutine(WaitAndLoad());
		}

		healthText.text = "Health: " + health.ToString();

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
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene(0);
	}
}
