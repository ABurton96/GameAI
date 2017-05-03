using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour {

	[System.Serializable]
	public class viewcones 
	{
		[Header ("Set the distance of the viewcone, angle of viewcone and alert status of it")]
		public float distance;
		public float angle;
		public statuses status;
	}

	public enum statuses {patrolling, suspicious, alert};

	public GameObject player; 
	public SphereCollider trigger; 
	public statuses overallStatus;
	private statuses sightAlertStatus;
	private statuses soundAlertStatus;
	private Vector3 lastKnownPos;
	public Animator point;
	public Animator playerAnim;
	public float suspiciousTimer;
	private bool suspiciousCheck;
	private float suspiciousTimerEnd;
	public UnityEngine.AI.NavMeshAgent nav;
	public Transform eyePos;
	[Header(" If using AStar uncheck Nav Mesh Agent")]
	public bool aStar;

	[Space(10)]
	[Header(" - Sight variables")]
	public viewcones[] viewconeInfo;
	public Vector3 playerDistance;
	public float viewAngle;
	public RaycastHit hit ;
	public bool playerSeen;
	private int viewconeSeen;
	public float loseSightTimer;
	private bool loseSightCheck;
	private float loseSightTimerEnd;
	private float currentTime;
	private Vector3 AIPosition;

	[Space(10)]
	[Header(" - Sound variables")]
	public float noAudioTimer;
	public bool noAudioCheck;
	public float noAudioTimerEnd;
	public float hearingDistance;
	public float soundTravelled;
	private bool noMoreNoise;
	private float suspiciousAudioTimerEnd;
	private bool suspiciousAudioCheck;

	[Space(10)]
	[Header(" - Pathing variables")]
	public GameObject waypointsGameObj;
	public List<Waypoints> patrolPoints;
	public int patrolNumber;
	public Vector3 startPos;
	public bool startedWait = false;
	public float waitEnd;

	[Space(10)]
	[Header(" - Attacking variables")]
	public float attackSpeed;
	public float attackDistance;
	public float lastAttack;
	public float health;
	public float healthStart;
	public float healthBarPercent; 
	public Transform healthBar; 
	public bool dead;
	public GameObject ragdoll; 

	public CapsuleCollider capCol;
	public GameObject sword;
	public Vector3 pos;



	void Start () 
	{
		//Sets sphere collider to the length of the max viewcone distance
		trigger.radius = viewconeInfo[viewconeInfo.Length - 1].distance;

		//Gets start position
		startPos = transform.position;

		//Looks through all of the child game objects. If the object is a waypoint then at it to the list of patrol points
		for(int i = 0; i < waypointsGameObj.transform.childCount; i++)
		{
			if(waypointsGameObj.transform.GetChild(i).tag == "Waypoint")
			{
				patrolPoints.Add(waypointsGameObj.transform.GetChild(i).GetComponent<Waypoints>());
				waypointsGameObj.transform.GetChild(i).gameObject.SetActive(false);
			}
		}

		//Sets health to the starting health
		health = healthStart;
	}

	void Update () 
	{
		//Calls patrol function
		Patrol();

		//Sets overal alert status depending on if the AI can hear/see the player
		if(soundAlertStatus == statuses.alert || sightAlertStatus == statuses.alert)
		{
			overallStatus = statuses.alert;
		}
		else if(soundAlertStatus == statuses.suspicious || sightAlertStatus == statuses.suspicious)
		{
			overallStatus = statuses.suspicious;
		}
		else if(soundAlertStatus == statuses.patrolling || sightAlertStatus == statuses.patrolling)
		{
			overallStatus = statuses.patrolling;
		}

		//If health is 0 or less then call dead function
		if(health <= 0 && !dead)
		{
			Dead();
		}


		AIPosition = transform.position;

		//Sets health bar
		healthBarPercent = (health / healthStart) * 0.2542284f;
		healthBar.localScale = new Vector3(healthBarPercent, healthBar.localScale.y, healthBar.localScale.z);
	}

	void Dead()
	{
		dead = true;

		//Spawns ragdoll character then deletes current AI
		Instantiate(ragdoll, transform.position, transform.rotation);
		Destroy(this.gameObject);
	}

	void OnTriggerStay(Collider other) 
	{
		//Checks if player is inside of the trigger
		if(other.gameObject.tag == "Player")
		{
			//Sight

			//Calculates an angle from the AI player to the playable character
			playerDistance = player.transform.position - transform.position;
			viewAngle = Vector3.Angle(playerDistance, transform.forward);

			//Sets playerSeen to false. If it doesnt get set back to true then the player wasn't seen in the last checks.
			playerSeen = false;
			//Starts a for loop running through each viewcone
			for(int i = 0; i < viewconeInfo.Length; i++)
			{	
				//Checks to see if the player is within the viewcone angle and is close enough
				if(viewAngle < viewconeInfo[i].angle * 0.5f && Vector3.Distance(transform.position, player.transform.position) <= viewconeInfo[i].distance || Vector3.Distance(transform.position, player.transform.position) < 3)
				{				
					Vector3 norm;
					norm = playerDistance.normalized;
					norm.y = (norm.y - norm.y);

					AIPosition.y = AIPosition.y + 1;

					//Raycasts from the AI to the playable character
					if (Physics.Raycast(AIPosition, norm, out hit))
					{

						//Draws a debug ray inside of the scene view. Only use it to help see where the raycast is going
						Debug.DrawRay(AIPosition, norm, Color.red);

						//Checks to see if the raycast hits the player. If it doesn't then the player is behind an object
						if(hit.collider != null && hit.collider.gameObject.tag == "Player")
						{
							//If the player hasn't already been seen then they have been now
							if(!playerSeen)
							{
								SetSightAlert(viewconeInfo[i].status);
								Debug.Log(viewconeInfo[i].angle);

								playerSeen = true;
								viewconeSeen = i;
							}
							//If player has been seen but viewcone is closer to the player then change spotted viewcone
							else if (playerSeen && viewconeSeen > i)
							{
								SetSightAlert(viewconeInfo[i].status);
								Debug.Log(viewconeInfo[i].angle);

								playerSeen = true;
								viewconeSeen = i;
							}
						}
					}
				}

				//If it is the last viewcone being checked and player wasn't seen then set patrolling
				if(i == viewconeInfo.Length - 1 && !playerSeen)
				{
					SetSightAlert(statuses.patrolling);
				}
			}

			//Hearing

			//Checks the distance from the player to the AI
			if(Vector3.Distance(transform.position, player.transform.position) <= hearingDistance)
			{
				if(!aStar)
				{
					//Create variables for navigation
					Vector3[] navWaypoints;
					NavMeshPath navPath = new NavMeshPath();

					//Calculates the path from player to enemy
					nav.CalculatePath(player.transform.position, navPath);

					//Adds 2 points to the waypoints array. One to add players position and one for AI position.
					navWaypoints = new Vector3[navPath.corners.Length + 2];

					//Adds positions to array
					navWaypoints[0] = transform.position;
					navWaypoints[navWaypoints.Length - 1] = player.transform.position;


					//Resets the sound travelled to 0
					soundTravelled = 0f;

					//Runs through for loop setting array to each corner from navPath calculation
					for(int j = 1; j < navWaypoints.Length - 1; j++)
					{
						navWaypoints[j] = navPath.corners[j - 1];
					}
					
					//Calculates each of the distances between the point adding to soundTravelled
					for(int k = 0; k < navWaypoints.Length - 1; k++)
					{
						soundTravelled += Vector3.Distance(navWaypoints[k], navWaypoints[k + 1]);
					}
				}
				else
				{
					AStar aStarPath = GetComponent<AStar>();
					soundTravelled = aStarPath.GetPathDistance(transform.position, player.transform.position);
				}
					
			}
			else
			{
				soundTravelled = 25;
			}

			Player play =  player.GetComponent<Player>();

			//If less that hearingDistance - 10% then alert
			if(soundTravelled <= hearingDistance - (hearingDistance / 80) && play.soundLevelAI == "Loud" || soundTravelled <= hearingDistance - (hearingDistance /70) && play.soundLevelAI == "Quite")
			{
				Debug.Log("Alert");
				SetSoundAlert(statuses.alert);
			}
			//If between 80% and 100% of hearing distance then suspicious. Needs to add alert timer.
			else if(soundTravelled > hearingDistance - (hearingDistance / 80) && soundTravelled < hearingDistance && play.soundLevelAI == "Loud")
			{
				Debug.Log("Suspicious");
				SetSoundAlert(statuses.suspicious);
			}
			else
			{
				SetSoundAlert(statuses.patrolling);
			}
		}
	}

	void OnCollisionStay(Collision other)
	{
		if(other.gameObject.tag == "Player)")
		{
			SetSightAlert(statuses.alert);
		}
	}

	void SetSightAlert(statuses status)
	{

		//Check inputed function
		switch(status)
		{
		//If status was alert set the AI alert status to alert and make sure that loseSight isn't true
		case statuses.alert:	
			sightAlertStatus = status;
			loseSightCheck = false;
			point.SetBool("Alert", true);
			point.SetBool("Suspicious", false);
			break;

		//If suspicious then check if already alert. If not check how long they have been in the viewcone for. If more than the check timer then become alert 
		case statuses.suspicious:	
			if(sightAlertStatus == statuses.alert)
			{
				sightAlertStatus = statuses.alert;
			}
			else
			{
				sightAlertStatus = status;
				point.SetBool("Alert", false);
				point.SetBool("Suspicious", true);
			}

			if(!suspiciousCheck)
			{
				suspiciousCheck = true;
				suspiciousTimerEnd = suspiciousTimer + Time.time;
			}
			else if(suspiciousCheck && Time.time > suspiciousTimerEnd)
			{
				sightAlertStatus = statuses.alert;
			}

			lastKnownPos = player.transform.position;
			loseSightCheck = false;

			break;
		//If player is no longer in a viewcone check how long they have not been seen for. If more than lose sight time then set back to partrolling
		case statuses.patrolling:	
			if(!loseSightCheck)
			{
				loseSightCheck = true;
				loseSightTimerEnd = loseSightTimer + Time.time;
			}
			else if(sightAlertStatus == statuses.alert && loseSightCheck && Time.time < loseSightTimerEnd)
			{
				sightAlertStatus = statuses.alert;
			}
			else
			{
				sightAlertStatus = status;
				point.SetBool("Alert", false);
				point.SetBool("Suspicious", false);
			}
			suspiciousCheck = false;
			break;
		}
	}

	void SetSoundAlert(statuses status)
	{
		//Check inputed function
		switch(status)
		{
		//If status was alert set the AI audio alert status to alert and make sure that no audio is false.
		case statuses.alert:	
			soundAlertStatus = status;
			noAudioCheck = false;
			break;
		//Check if the AI is already alert. If not check how long they have been in the the hearable distance for. If more than the check timer then audio status is alert
		case statuses.suspicious:	
			if(soundAlertStatus == statuses.alert)
			{
				soundAlertStatus = statuses.alert;
			}
		else
			{
				soundAlertStatus = status;
			}

			if(!suspiciousAudioCheck)
			{
				suspiciousAudioCheck = true;
				suspiciousAudioTimerEnd = suspiciousTimer + Time.time;
			}
			else if(suspiciousAudioCheck && Time.time > suspiciousAudioTimerEnd)
			{
				soundAlertStatus = statuses.alert;
			}

			noAudioCheck = false;
			lastKnownPos = player.transform.position;
			break;
		//If player is no making noise then audio check back to patrolling
		case statuses.patrolling:	
			if(!noAudioCheck)
			{
				noAudioCheck = true;
				noAudioTimerEnd = noAudioTimer + Time.time;
			}
			else if(soundAlertStatus == statuses.alert && noAudioCheck && Time.time < noAudioTimerEnd)
			{
				soundAlertStatus = statuses.alert;
			}
			else
			{
				soundAlertStatus = status;
			}

			suspiciousAudioCheck = false;
			break;
		}
	}

	void Patrol()
	{
		AStar aStarPathPatrol = GetComponent<AStar>();
		Waypoints time;
		float delayTime = 0;

		//All patrol functions have parts for both unityNav mesh and my A* algorithm

		//If waypoints are set up then patrol between them
		if(patrolPoints.Count >= 1)
		{
			//If already patrolling and near waypoint then change to next waypoint. After this then path to next waypoint
			if(overallStatus == statuses.patrolling)
			{
				if(Vector3.Distance(patrolPoints[patrolNumber].position, transform.position) < 2)
				{
					if(!startedWait)
					{
						startedWait = true;
						nav.angularSpeed = 0;

						time = patrolPoints[patrolNumber];
						delayTime = time.GetComponent<Waypoints>().delay;
						waitEnd = Time.time + delayTime;
					}
				}

				if(startedWait && Time.time > waitEnd)
				{
					nav.angularSpeed = 200;

					startedWait = false;

					patrolNumber ++;

					if(patrolNumber >= patrolPoints.Count)
					{
						patrolNumber = 0;
					}
				}

				if(!startedWait)
				{
					playerAnim.SetBool("Walking", true);
					playerAnim.SetBool("Running", false);
				}
				else
				{
					playerAnim.SetBool("Walking", false);
					playerAnim.SetBool("Running", false);
				}

				nav.speed = 1.5f;

				if(!dead  && !startedWait)
				{
					if(!aStar)
					{
						nav.SetDestination(patrolPoints[patrolNumber].position);
					}
					else
						aStarPathPatrol.targetPosition = patrolPoints[patrolNumber].position;
				}
			}
			//If suspicious then move to last known position of player
			else if(overallStatus == statuses.suspicious)
			{
				nav.angularSpeed = 200;

				playerAnim.SetBool("Walking", true);
				playerAnim.SetBool("Running", false);
				nav.speed = 1.5f;
				if(!dead)
				{
					if(!aStar)
						nav.SetDestination(lastKnownPos);
					else
					{
						aStarPathPatrol.SetDest(lastKnownPos, transform.position, true, this);
						aStarPathPatrol.targetPosition = lastKnownPos;
					}
				}
			}
			//If alert then path to player
			else if(overallStatus == statuses.alert)
			{
				nav.angularSpeed = 200;

				if(Vector3.Distance(transform.position, player.transform.position) > attackDistance)
				{
					playerAnim.SetBool("Walking", false);
					playerAnim.SetBool("Running", true);
					nav.speed = 3.5f;
					if(!dead)
					{
						if(!aStar)
							nav.SetDestination(player.transform.position);
						else
							aStarPathPatrol.targetPosition = player.transform.position;
					}
				}
				else if(Vector3.Distance(transform.position, player.transform.position) <=  attackDistance && playerSeen)
				{
					Attack();
				}
			}
		}
		//If no way points are created
		else
		{
			if(overallStatus == statuses.patrolling)
			{
				if(Vector3.Distance(transform.position, startPos) > 5)
				{
					if(!dead)
					{
						if(!aStar)
							nav.SetDestination(startPos);
						else
							aStarPathPatrol.targetPosition = startPos;
					}
					playerAnim.SetBool("Walking", true);
					playerAnim.SetBool("Running", false);
				}
				else
				{
					playerAnim.SetBool("Walking", false);
					playerAnim.SetBool("Running", false);
				}
				nav.speed = 1.5f;
			}
			//If suspicious then move to last known position of player
			else if(overallStatus == statuses.suspicious)
			{
				playerAnim.SetBool("Walking", true);
				playerAnim.SetBool("Running", false);
				nav.speed = 1.5f;
				if(!dead)
				{
					if(!aStar)
						nav.SetDestination(lastKnownPos);
					else
						aStarPathPatrol.targetPosition = lastKnownPos;
				}
			}
			//If alert then path to player
			else if(overallStatus == statuses.alert)
			{
				playerAnim.SetBool("Walking", false);
				playerAnim.SetBool("Running", true);
				nav.speed = 3.5f;
				if(Vector3.Distance(transform.position, player.transform.position) > attackDistance)
				{
					if(!dead)
					{
						if(!aStar)
							nav.SetDestination(player.transform.position);
						else
							aStarPathPatrol.targetPosition = player.transform.position;
					}
				}
				else if(Vector3.Distance(transform.position, player.transform.position) <=  attackDistance)
				{
					if(!dead)
					{
						if(!aStar)
							nav.SetDestination(player.transform.position);
						else
							aStarPathPatrol.targetPosition = player.transform.position;
					}

					Attack();
				}
			}
		}
	}

	public void Attack()
	{
		//Stops the walking/running animations
		playerAnim.SetBool("Walking", false);
		playerAnim.SetBool("Running", false);

		//Makes sure AI is moving to the player
		nav.SetDestination(player.transform.position);

		//If time since attack is longer than attack speed then attack the player. Trigger the attacking animation, set attack time and remove health from player.
		if(Time.time > lastAttack + attackSpeed)
		{
			Player playerHealth = player.GetComponent<Player>();

			playerHealth.health -= 10;

			lastAttack = Time.time;
			playerAnim.SetTrigger("Attack");
		}
	}
}

