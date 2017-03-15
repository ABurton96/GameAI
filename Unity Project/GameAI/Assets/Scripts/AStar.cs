using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {

    public LayerMask unwalkableMask;
    public float nodeSize;
    public float totalNodesX;
    public float totalNodesY;
    public Vector2 pathSize;
	Node[,] nodes;
	public GameObject player;
	public Vector3 targetPosition;
	public Vector3 playerPosRounded;
	public Vector3 other;
	List<Node> path = new List<Node>();

	public class Node
	{
		public bool walkable;
		public Vector3 position;

		public int hCost;
		public int gCost;

		public int xposition;
		public int yposition;

		public Node previousNode;

		public int fCost()
		{
			return gCost + hCost;
		}

		public Node(bool walk, Vector3 pos, int xPos, int yPos)
		{
			walkable = walk;
			position = pos;
			xposition = xPos;
			yposition = yPos;

		}
	}

    public void Start()
    {
		//Calculates total amount of nodes needed
        totalNodesX = pathSize.x / nodeSize;
        totalNodesY = pathSize.y / nodeSize;

		//Finds where the grid will start to generate
		Vector3 botLeft;
        botLeft.x = transform.position.x - pathSize.x / 2;
        botLeft.y = transform.position.x - pathSize.y / 2;

		//Sets the amount of nodes for the array
		nodes = new Node[int.Parse(totalNodesX.ToString()),int.Parse(totalNodesY.ToString())];


		//Generates all the nodes checking if the node is blocked by any objects
        for (int i = 0; i < totalNodesX; i++)
        {
            for (int j = 0; j < totalNodesY; j++)
            {
                Vector3 position = transform.position;
				position.x = (botLeft.x + nodeSize * i + 1);
                position.y = transform.position.y;
				position.z = (botLeft.y + nodeSize * j + 1);
                bool canWalk = true;

                if(Physics.CheckSphere(position, nodeSize, unwalkableMask))
                {
                    canWalk = false;
                }

				nodes[i,j] = new Node(canWalk, position, i, j);
            }
        }
    }

	public void Update()
	{
		//Gets path needed
		GetPath();
	}

	void OnDrawGizmos()
	{
		//If the game is playing draw gizmos showing the path and where can/can't be walked
		if(Application.isPlaying)
		{
			Gizmos.DrawWireCube(transform.position, new Vector3(pathSize.x, 1, pathSize.y));
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
					Gizmos.DrawCube(node.position, Vector3.one * (nodeSize - 0.1f));
				}
			}
		}
	}

	public Node GetClosestNode(Vector3 playerPos)
	{
		Node nearest = new Node(false, new Vector3(100000,100000,100000), 0 ,0);
		double nearestDiff = 10000000000;

		//Checks all the nodes finding the closes node to the inputed vector3
		foreach (var node in nodes) 
		{
			float diff = Vector3.Distance(node.position, playerPos);
			if(diff < nearestDiff)
			{
				nearest = node;
				nearestDiff = diff;
			}
		}

			return nearest;
	}

	public void GetPath()
	{
		//Finds the start and end node
		Node startNode = GetClosestNode(transform.position);
		Node endNode = GetClosestNode(targetPosition);

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
			
				//Removes any path that was there before and add all the nodes for the next path
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
		Move();
	}

	public void Move()
	{
		//If there is still a path move the game object to the next node
		if(path.Count > 0)
		{
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
				if(i == 0 && j == 0)
				{
					continue;
				}
					
				int x = node.xposition + i;
				int y = node.yposition + j;

				if(x >= 0 && x < totalNodesX && y >= 0 && y < totalNodesY)
				{
					neighbours.Add(nodes[x,y]);
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
}