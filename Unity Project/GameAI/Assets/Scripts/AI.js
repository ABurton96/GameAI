#pragma strict
import UnityEngine.AI;

public class viewcones 
{
	@header ("Set the distance of the viewcone, angle of viewcone and alert status of it")
	public var distance: float;
	public var angle: float;
	public var status: statuses;
}

enum statuses {patrolling, suspicious, alert};

public var player: GameObject; 
public var trigger: SphereCollider; 
public var alertStatus: statuses;
public var soundAlertStatus: statuses;
public var viewconeInfo: viewcones[];
public var playerDistance: Vector3;
public var viewAngle: float;
public var hit: RaycastHit;
public var playerSeen: boolean;
public var viewconeSeen: int;
public var suspiciousTimer: float;
public var suspiciousCheck: boolean;
public var suspiciousTimerEnd: float;
public var loseSightTimer: float;
public var loseSightCheck: boolean;
public var loseSightTimerEnd: float;
public var currentTime: float;
public var point: Animator;

public var hearingDistance: float;
public var nav: UnityEngine.AI.NavMeshAgent;
public var soundTravelled: float;

function Start () 
{
	//Finds player
	player = GameObject.FindGameObjectWithTag("Player");
	
	//Sets sphere collider to the length of the max viewcone distance
	trigger.radius = viewconeInfo[viewconeInfo.Length - 1].distance;
}

function Update () 
{
	//Temp. Just to see current scene time in inspector
	currentTime = Time.time;

}

function OnTriggerStay(other: Collider) 
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
		for(var i: int = 0; i < viewconeInfo.Length; i++)
		{	
			//Checks to see if the player is within the viewcone angle and is close enough
			if(viewAngle < viewconeInfo[i].angle * 0.5 && Vector3.Distance(transform.position, player.transform.position) <= viewconeInfo[i].distance)
			{				
				//Raycasts from the AI to the playable character
				if (Physics.Raycast(transform.position, playerDistance.normalized, hit));
				{
					//Draws a debug ray inside of the scene view. Only use it to help see where the raycast is going
					Debug.DrawRay(transform.position, playerDistance.normalized, Color.red);
				
					//Checks to see if the raycast hits the player. If it doesn't then the player is behind an object
					if(hit.collider.gameObject.tag == "Player")
					{
						//If the player hasn't already been seen then they have been now
						if(!playerSeen)
						{
							SetAlert(viewconeInfo[i].status);
							Debug.Log(viewconeInfo[i].angle);
							
							playerSeen = true;
							viewconeSeen = i;
						}
						//If player has been seen but viewcone is closer to the player then change spotted viewcone
						else if (playerSeen && viewconeSeen > i)
						{
							SetAlert(viewconeInfo[i].status);
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
				SetAlert(statuses.patrolling);
			}
		}

		//Hearing

		//Checks the distance from the player to the AI
		if(Vector3.Distance(transform.position, player.transform.position) <= hearingDistance)
		{
			//Create variables for navigation
			var navWaypoints: Vector3[];
			var navPath: NavMeshPath  = new NavMeshPath();

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
			for(var j: int = 1; j < navWaypoints.Length - 1; j++)
			{
				navWaypoints[j] = navPath.corners[j - 1];
			}

			//Calculates each of the distances between the point adding to soundTravelled
			for(var k: int = 0; k < navWaypoints.Length - 1; k++)
			{
				soundTravelled += Vector3.Distance(navWaypoints[k], navWaypoints[k + 1]);
			}
		}

		//If less that hearingDistance - 10% then alert
		if(soundTravelled <= hearingDistance - (hearingDistance /10))
		{
			soundAlertStatus = statuses.alert;
		}
		//If between 90% and 100% of hearing distance then suspicious. Needs to add alert timer.
		else if(soundTravelled > hearingDistance - (hearingDistance /10) && soundTravelled < hearingDistance)
		{
			soundAlertStatus = statuses.suspicious;
		}
		// Alert status should check against sight for overall alert status
	}
}

function SetAlert(status: statuses)
{

//Wait for half a second before any decision is made
	yield WaitForSeconds(0.5);

	//Check inputed function
	switch(status)
	{
		//If status was alert set the AI alert status to alert and make sure that loseSight isn't true
		case statuses.alert:	alertStatus = status;
								loseSightCheck = false;
								point.SetBool("Alert", true);
								point.SetBool("Suspicious", false);
								break;
		//If suspicious then check if already alert. If not check how long they have been in the viewcone for. If more than the check timer then become alert 
		case statuses.suspicious:	if(alertStatus == statuses.alert)
									{
										alertStatus = statuses.alert;
									}
									else
									{
										alertStatus = status;
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
										alertStatus = statuses.alert;
									}
									loseSightCheck = false;
									break;
		//If player is no longer in a viewcone check how long they have not been seen for. If more than lose sight time then set back to partrolling
		case statuses.patrolling:	if(!loseSightCheck)
									{
										loseSightCheck = true;
										loseSightTimerEnd = loseSightTimer + Time.time;
									}
									else if(alertStatus == statuses.alert && loseSightCheck && Time.time < loseSightTimerEnd)
									{
										alertStatus = statuses.alert;
									}
									else
									{
										alertStatus = status;
										point.SetBool("Alert", false);
										point.SetBool("Suspicious", false);
									}
									suspiciousCheck = false;
									break;
	}
}