#pragma strict

public class viewcones 
{
	public var distance: float;
	public var angle: float;
	public var status: statuses;
}

enum statuses {patrolling, suspicious, alert};

public var player: GameObject; 
public var trigger: SphereCollider; 
public var alertStatus: statuses;
public var viewconeInfo: viewcones[];
public var playerDistance: Vector3;
public var viewAngle: float;
public var hit: RaycastHit;
public var playerSeen: boolean;
public var viewconeSeen: int;
public var leftRay: Vector3;
public var rightRay: Vector3;


function Start () 
{
	//Finds player
	player = GameObject.FindGameObjectWithTag("Player");
	
	//Sets sphere collider to the length of the max viewcone distance
	trigger.radius = viewconeInfo[viewconeInfo.Length - 1].distance;
}

function Update () 
{
	
}

function OnTriggerStay(other: Collider) 
{
	//Checks if player is inside of the trigger
	if(other.gameObject.tag == "Player")
	{
		//Calculates an angle from the AI player to the playable character
		playerDistance = player.transform.position - transform.position;
		//viewAngle = Vector3.Angle(Vector3.forward, playerDistance);
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
				
					//Checks to see if the raycast hits the player. If it doesnt then the player is behind an object
					if(hit.collider.gameObject.tag == "Player")
					{
						//If the player hasn't already been seen then they have been now
						if(!playerSeen)
						{
							alertStatus = viewconeInfo[i].status;
							Debug.Log(viewconeInfo[i].angle);
							
							playerSeen = true;
							viewconeSeen = i;
						}
						//If player has been seen but viewcone is closer to the player then change spotted viewcone
						else if (playerSeen && viewconeSeen > i)
						{
							alertStatus = viewconeInfo[i].status;
							Debug.Log(viewconeInfo[i].angle);
							
							playerSeen = true;
							viewconeSeen = i;
						}
					}
				}
			}
		}
	}
}