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
			dead.SetActive(true);
			StartCoroutine(WaitAndLoad());
		}

		healthText.text = "Health: " + health.ToString();
	}

	IEnumerator WaitAndLoad()
	{
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene(0);
	}
}
