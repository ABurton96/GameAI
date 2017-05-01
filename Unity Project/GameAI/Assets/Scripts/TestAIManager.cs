using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAIManager : MonoBehaviour {

	public GameObject escHint;

	void Update () 
	{
		if(GameObject.Find("AI C Ragdoll") == null)
		{
			escHint.SetActive(true);

		}
	}
}
