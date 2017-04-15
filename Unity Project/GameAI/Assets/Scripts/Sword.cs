using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {

	public bool attacking;
	public Animator sword;
	public GameObject blood;

	void Update () 
	{
		if(Input.GetMouseButtonDown(0) && !attacking)
		{
			StartCoroutine(Attack());
		}
	}

	IEnumerator Attack()
	{
		attacking = true;
		sword.SetTrigger("Attack");
		yield return new WaitForSeconds(0.2f);
		attacking = false;
	}

	void OnCollisionEnter(Collision other)
	{
		if(attacking && other.gameObject.tag == "AI")
		{
			AI ai = other.gameObject.GetComponent<AI>();
			bool attackOnce = false;

			if(!attackOnce)
			{
				ai.health -= 25;
				ContactPoint contact = other.contacts[0];
				GameObject bloodCreated;
				bloodCreated = Instantiate(blood, contact.point , Quaternion.Euler(new Vector3(-60.5f,0,0)));
				bloodCreated.transform.parent = other.gameObject.transform;
				attackOnce = true;
			}
		}
	}
}
