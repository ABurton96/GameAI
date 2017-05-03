using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAIManager : MonoBehaviour {

	public GameObject escHint;

	void Update () 
	{
		//If no AI game object is in the scene then show prompt to escape back to menu
		if(GameObject.Find("AI C Ragdoll") == null)
		{
			escHint.SetActive(true);
		}
	}
}
