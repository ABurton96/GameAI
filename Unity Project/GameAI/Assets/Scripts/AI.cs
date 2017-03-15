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

	[Space(10)]
	[Header(" - Sound variables")]
	public float noAudioTimer;
	public bool noAudioCheck;
	public float noAudioTimerEnd;
	public float hearingDistance;
	private float soundTravelled;
	private bool noMoreNoise;
	private float suspiciousAudioTimerEnd;
	private bool suspiciousAudioCheck;

	[Space(10)]
	[Header(" - Pathing variables")]
	public GameObject waypointsGameObj;
	public List<Vector3> patrolPoints;
	public int patrolNumber;
	public Vector3 startPos;

	[Space(10)]
	[Header(" - Attacking variables")]
	public float shotSpeed;
	public float attackDistance;
	public bool meele;
	public Vector3 shotPosition;
	public float shotInAc;
	private float lastShot;
	public float reloadTime;
	private float finishReloading;
	public int bulletMag;
	private int currentMag;
	private bool shooting;
	public GameObject bullet;
	private bool reloading;

	void Start () 
	{
		//Finds player
		//player = GameObject.FindGameObjectWithTag("Player");

		//Sets sphere collider to the length of the max viewcone distance
		trigger.radius = viewconeInfo[viewconeInfo.Length - 1].distance;

		//Gets start position
		startPos = transform.position;

		//Looks through all of the child game objects. If the object is a waypoint then at it to the list of patrol points
		for(int i = 0; i < waypointsGameObj.transform.childCount; i++)
		{
			if(waypointsGameObj.transform.GetChild(i).tag == "Waypoint")
			{
				patrolPoints.Add(waypointsGameObj.transform.GetChild(i).position);
				waypointsGameObj.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
	}
	
	void Update () 
	{
		//Calls patrol function
		Patrol();

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
	}


	void OnTriggerStay(Collider other) 
	{
		//Checks if player is inside of the trigger
		if(other.gameObject.tag == "Player")
		{

			//Sight

			//Calculates an angle from the AI player to the playable character
			playerDistance = player.transform.position - transform.position;
			//ViewAngle = Vector3.Angle(Vector3.forward, playerDistance);
			viewAngle = Vector3.Angle(playerDistance, transform.forward);

			//Sets playerSeen to false. If it doesnt get set back to true then the player wasn't seen in the last checks.
			playerSeen = false;
			//Starts a for loop running through each viewcone
			for(int i = 0; i < viewconeInfo.Length; i++)
			{	
				//Checks to see if the player is within the viewcone angle and is close enough
				if(viewAngle < viewconeInfo[i].angle * 0.5f && Vector3.Distance(transform.position, player.transform.position) <= viewconeInfo[i].distance)
				{				
					Vector3 AIPos = new Vector3(0,0,0);
					AIPos.y += 1.4f;

					Vector3 playerPos = player.transform.position;
					playerPos.y  = playerPos.y - 1;
					//Raycasts from the AI to the playable character
					//if (Physics.Raycast(AIPos, playerPos, hit));
					if (Physics.Raycast(transform.position, playerDistance.normalized, out hit))
					{
						//Draws a debug ray inside of the scene view. Only use it to help see where the raycast is going
						Debug.DrawRay(transform.position, playerDistance.normalized, Color.red);

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
				soundTravelled = 25;
			}

			Player play =  player.GetComponent<Player>();

			//If less that hearingDistance - 10% then alert
			if(soundTravelled <= hearingDistance - (hearingDistance / 20) && play.soundLevelAI == "Loud" || soundTravelled <= hearingDistance - (hearingDistance /10) && play.soundLevelAI == "Quite")
			{
				//soundAlertStatus = statuses.alert;
				Debug.Log("Alert");
				SetSoundAlert(statuses.alert);
			}
			//If between 90% and 100% of hearing distance then suspicious. Needs to add alert timer.
			else if(soundTravelled > hearingDistance - (hearingDistance / 10) && soundTravelled < hearingDistance && play.soundLevelAI == "Loud")
			{
				//soundAlertStatus = statuses.suspicious;
				Debug.Log("Suspicious");
				SetSoundAlert(statuses.suspicious);
			}
			else
			{
				SetSoundAlert(statuses.patrolling);
			}
		}
	}

	void SetSightAlert(statuses status)
	{

		//Check inputed function
		switch(status)
		{
		//If status was alert set the AI alert status to alert and make sure that loseSight isn't true
		case statuses.alert:	sightAlertStatus = status;
			loseSightCheck = false;
			point.SetBool("Alert", true);
			point.SetBool("Suspicious", false);
			break;

			//If suspicious then check if already alert. If not check how long they have been in the viewcone for. If more than the check timer then become alert 
		case statuses.suspicious:	if(sightAlertStatus == statuses.alert)
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
		case statuses.patrolling:	if(!loseSightCheck)
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

		//Wait for half a second before any decision is made
		//yield return new WaitForSeconds(0.5f);

		//Check inputed function
		switch(status)
		{
		//If status was alert set the AI audio alert status to alert and make sure that no audio is false.
		case statuses.alert:	soundAlertStatus = status;
			noAudioCheck = false;
			break;
			//Check if the AI is already alert. If not check how long they have been in the the hearable distance for. If more than the check timer then audio status is alert
		case statuses.suspicious:	if(soundAlertStatus == statuses.alert)
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
		case statuses.patrolling:	if(!noAudioCheck)
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
		AStar aStarPath2 = GetComponent<AStar>();

		//If waypoints are set up then patrol between them
		if(patrolPoints.Count >= 1)
		{
			//If already patrolling and near waypoint then change to next waypoint. After this then path to next waypoint
			if(overallStatus == statuses.patrolling)
			{
				if(Vector3.Distance(patrolPoints[patrolNumber], transform.position) < 5)
				{
					patrolNumber ++;

					if(patrolNumber >= patrolPoints.Count)
					{
						patrolNumber = 0;
					}
				}

				playerAnim.SetBool("Walking", true);
				playerAnim.SetBool("Running", false);
				nav.speed = 1.5f;
				if(!aStar)
					nav.SetDestination(patrolPoints[patrolNumber]);
				else
					aStarPath2.targetPosition = patrolPoints[patrolNumber];
			}
			//If suspicious then move to last known position of player
			else if(overallStatus == statuses.suspicious)
			{
				playerAnim.SetBool("Walking", true);
				playerAnim.SetBool("Running", false);
				nav.speed = 1.5f;
				if(!aStar)
					nav.SetDestination(lastKnownPos);
				else
					aStarPath2.targetPosition = lastKnownPos;
			}
			//If alert then path to player
			else if(overallStatus == statuses.alert)
			{
				//TODO This should stop the player from getting too close depeing on weapon type. Once close enough begin shooting.
				if(Vector3.Distance(transform.position, player.transform.position) > attackDistance)
				{
					playerAnim.SetBool("Walking", false);
					playerAnim.SetBool("Running", true);
					nav.speed = 3.5f;
					if(!aStar)
						nav.SetDestination(player.transform.position);
					else
						aStarPath2.targetPosition = player.transform.position;
				}
				else if(Vector3.Distance(transform.position, player.transform.position) <=  attackDistance && playerSeen)
				{
					Shoot();
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
					nav.SetDestination(startPos);
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
				if(!aStar)
					nav.SetDestination(lastKnownPos);
				else
					aStarPath2.targetPosition = lastKnownPos;
			}
			//If alert then path to player
			else if(overallStatus == statuses.alert)
			{
				playerAnim.SetBool("Walking", false);
				playerAnim.SetBool("Running", true);
				nav.speed = 3.5f;
				if(!aStar)
					nav.SetDestination(player.transform.position);
				else
					aStarPath2.targetPosition = player.transform.position;

				if(Vector3.Distance(transform.position, player.transform.position) <=  attackDistance)
				{
					Shoot();
				}

			}
		}
	}

	void Shoot()
	{
		playerAnim.SetBool("Walking", false);
		playerAnim.SetBool("Running", false);

		//Checks if you can see the player
		if(playerSeen)
		{
			//Is there bullets in the mag
			if(currentMag > 0)
			{
				//Adds small delay to the next shot and creates a bullet
				if(Time.time >= lastShot + shotSpeed)
				{
					shotPosition = transform.position;
					Instantiate(bullet, shotPosition, transform.rotation);
					lastShot = Time.time;
					currentMag --;
					shooting = true;
				}
			}
			//If no bullets in mag and not reloading then start reloading
			else if(!reloading)
			{
				reloading = true;
				finishReloading = Time.time + reloadTime;
			}
			//If reloading stop reloading after reload time
			else if(reloading && Time.time >= finishReloading)
			{
				reloading = false;
				currentMag = bulletMag;
			}
		}
		//If raycast isn't directly hitting player contnue shooting for a short period then stop
		else if(shooting && lastShot + 2  < Time.time)
		{
			shooting = false;
		}
	}
}

