using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {

    public LayerMask unwalkableMask;
	public LayerMask stairMask;
	public LayerMask walkableMask;
    public float nodeSize;
    public float totalNodesX;
    public float totalNodesY;
	public float totalNodesZ;
	public Vector3 pathSize;
	public Node[,,] nodes;
	public GameObject player;
	public GameObject AI;
	public Vector3 targetPosition;
	public Vector3 playerPosRounded;
	public Vector3 other;
	public List<Node> path = new List<Node>();
	public bool playerSeen;
	public bool drawGizmos;

	public class Node
	{
		public bool walkable;
		public bool active;
		public Vector3 position;

		public int hCost;
		public int gCost;

		public int xposition;
		public int yposition;
		public int zposition;

		public Node previousNode;

		public int fCost()
		{
			return gCost + hCost;
		}

		public Node(bool walk, Vector3 pos, int xPos, int yPos, int zPos)
		{
			walkable = walk;
			position = pos;
			xposition = xPos;
			yposition = yPos;
			zposition = zPos;
		}

		public Node(bool walk, Vector3 pos, int xPos, int yPos, int zPos, bool act)
		{
			walkable = walk;
			position = pos;
			xposition = xPos;
			yposition = yPos;
			zposition = zPos;
			active = act;
		}
	}

    public void Start()
    {
		//Calculates total amount of nodes needed
        totalNodesX = pathSize.x / nodeSize;
        totalNodesY = pathSize.y / nodeSize;
		totalNodesZ = pathSize.z / nodeSize;

		//Finds where the grid will start to generate
		Vector3 botLeft;
        botLeft.x = transform.position.x - pathSize.x / 2;
        botLeft.y = transform.position.x - pathSize.y / 2;

		//Sets the amount of nodes for the array
		nodes = new Node[int.Parse(totalNodesX.ToString()),int.Parse(totalNodesY.ToString()), int.Parse(totalNodesZ.ToString())];


		//Generates all the nodes checking if the node is blocked by any objects
        for (int i = 0; i < totalNodesX; i++)
        {
            for (int j = 0; j < totalNodesY; j++)
            {
				for (int k = 0; k < totalNodesZ; k++)
				{
	                Vector3 position = transform.position;
					position.x = (botLeft.x + nodeSize * i + 1);
					position.y = transform.position.y + k * nodeSize;
					position.z = (botLeft.y + nodeSize * j + 1);

	                bool canWalk = true;

	                if(Physics.CheckSphere(position, nodeSize, unwalkableMask))
	                {
	                    canWalk = false;
						nodes[i,j,k] = new Node(canWalk, position, i, j, k, true);
	                }
					else if(Physics.CheckSphere(position, nodeSize / 2, stairMask))
					{
						nodes[i,j,k] = new Node(canWalk, position, i, j, k, true);

						if(k > 0)
						{
							if(nodes[i,j,k - 1].active && nodes[i,j,k].active)
							{
								nodes[i,j,k - 1].active = false;
							}
						}
					}
					else if(Physics.CheckSphere(position, nodeSize/2, walkableMask))
					{
						nodes[i,j,k] = new Node(canWalk, position, i, j, k, true);

						if(k > 0)
						{
							if(nodes[i,j,k - 1].active && nodes[i,j,k].active)
							{
								nodes[i,j,k - 1].active = false;
							}
						}
					}
					else
					{
						nodes[i,j,k] = new Node(canWalk, position, i, j, k, false);
					}


					Debug.DrawRay(new Vector3(position.x, position.y, position.z), Vector3.down, Color.red);

				}
            }
        }
    }

	public void Update()
	{
		AI aStarOn = AI.GetComponent<AI>();
		if(aStarOn.aStar)
			GetPath(GetClosestNode(targetPosition), GetClosestNode(transform.position), true);
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(pathSize.x, 1, pathSize.y));

		//If the game is playing draw gizmos showing the path and where can/can't be walked
		if(Application.isPlaying && drawGizmos)
		{
			Node nearestNode = GetClosestNode(player.transform.position);

			if (nodes != null)
			{
				foreach (Node node in nodes)
				{
					Vector3 key = node.position;
					key.x = Mathf.RoundToInt(node.position.x);
					key.z = Mathf.RoundToInt(node.position.z);
					Vector3 pos = player.transform.position;
					pos.x = Mathf.RoundToInt(pos.x);
					pos.y = 0;
					pos.z = Mathf.RoundToInt(pos.z);

					if(node.position == nearestNode.position)
					{
						Gizmos.color = Color.blue;
						other = key;
						playerPosRounded = pos;
					}
					else if(node.walkable)
					{
						Gizmos.color = Color.green;
					}
					else
					{
						Gizmos.color = Color.red;
					}

					if(path.Contains(node))
					{
						Gizmos.color = Color.grey;
					}
						
					if(node.position == nearestNode.position)
					{
						Gizmos.color = Color.blue;
					}

					if(node.active)
						Gizmos.DrawCube(node.position, Vector3.one * (nodeSize - 0.1f));
				}
			}
		}
	}

	public Node GetClosestNode(Vector3 playerPos)
	{
		Node nearest = new Node(false, new Vector3(100000,100000,100000), 0 , 0, 0);
		double nearestDiff = 10000000000;

		//Checks all the nodes finding the closes node to the inputed vector3
		foreach (var node in nodes) 
		{
			if(node.active)
			{
				float diff = Vector3.Distance(node.position, playerPos);
				if(diff < nearestDiff)
				{
					nearest = node;
					nearestDiff = diff;
				}
			}
		}

			return nearest;
	}

	public float GetPath(Node end, Node start, bool move)
	{
		//Finds the start and end node
		Node startNode = start;
		Node endNode = end;

		List<Node> openNodes = new List<Node>();
		List<Node> closedNodes = new List<Node>();

		openNodes.Add(startNode);

		//While there are nodes that need checking go through them all finding the shortest path possible
		while (openNodes.Count > 0) 
		{
			Node currentNode = openNodes[0];

			for (int i = 1; i < openNodes.Count; i++) 
			{
				if(openNodes[i].fCost() < currentNode.fCost() || openNodes[i].fCost() == currentNode.fCost() && openNodes[i].hCost < currentNode.hCost)
				{
					currentNode = openNodes[i];
				}
			}

			openNodes.Remove(currentNode);
			closedNodes.Add(currentNode);

			if(currentNode == endNode)
			{
			
				//Removes any path that xwas there before and add all the nodes for the next path
				path.Clear();
				
				while(currentNode != startNode)
				{
					path.Add(currentNode);
					currentNode = currentNode.previousNode;
				}

				path.Reverse();

				break;
			}

			//Check the neighbouring nodes for the best node to move to
			foreach (Node neigh in GetNeighbour(currentNode)) 
			{
				if(!closedNodes.Contains(neigh))
				{
					int moveCost = currentNode.gCost + GetDistance(currentNode, neigh);

					if(moveCost < neigh.gCost || !openNodes.Contains(neigh))
					{
						neigh.gCost = moveCost;
						neigh.hCost = GetDistance(neigh, endNode);
						neigh.previousNode = currentNode;

						if(!openNodes.Contains(neigh))
						{
							openNodes.Add(neigh);
						}
					}
				}
			}
		}

		//Once path has been checked start to move
		if(move)
			Move();
		else
		{
			float soundTravelled = 0f;

			for(int k = 0; k < path.Count - 1; k++)
			{
				soundTravelled += Vector3.Distance(path[k].position, path[k + 1].position);
			}
			return soundTravelled;
		}

		return 0f;
	}

	public void Move()
	{
		//If there is still a path move the game object to the next node
		if(path.Count > 0)
		{
			Vector3 targetPos = path[0].position - transform.position;
			//Vector3 direction = Vector3.RotateTowards(transform.forward, targetPos, 0.5f, 2.0f);

			Quaternion direction;
			//if(playerSeen)
			//	direction = Quaternion.LookRotation((player.transform.position + new Vector3(0,-0.6426952f,0)) - transform.position);
			//else
				direction = Quaternion.LookRotation(path[0].position - transform.position);
			
			transform.rotation = Quaternion.Lerp(transform.rotation, direction, 1);
			transform.position = Vector3.Lerp(transform.position, path[0].position, 0.05f);
		}
	}

	//Get all neighbouring nodes
	public List<Node> GetNeighbour(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int i = -1; i <= 1; i++) 
		{
			for (int j = -1; j <= 1; j++) 
			{
				for (int k = -1; k <= 1; k++) 
				{
					if(i == 0 && j == 0 && k == 0)
					{
						continue;
					}
						
					int x = node.xposition + i;
					int y = node.yposition + j;
					int z = node.zposition + k;

					if(x >= 0 && x < totalNodesX && y >= 0 && y < totalNodesY && z >= 0 && z < totalNodesZ)
					{
						if(nodes[x,y,z].active)
						{
							neighbours.Add(nodes[x,y,z]);
						}
					}
				}
			}
		}

		return neighbours;
	}

	//Calculate the distance from starting node to the next node
	public int GetDistance(Node start, Node check)
	{
		int xDistance = Mathf.Abs(start.xposition - check.xposition);
		int yDistance = Mathf.Abs(start.yposition - check.yposition);

		if(xDistance > yDistance)
		{
			return 14 * yDistance + 10 * (xDistance - yDistance);
		}
		else
		{
			return 14 * xDistance + 10 * (yDistance - xDistance);
		}
	}

	//Returns the total path distance
	public float GetPathDistance(Vector3 startPosition, Vector3 endPosition)
	{
		float pathDistance;
		pathDistance = GetPath(GetClosestNode(startPosition), GetClosestNode(endPosition), false);
		return pathDistance;
	}
}