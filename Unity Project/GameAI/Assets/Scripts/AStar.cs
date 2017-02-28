using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {

    public LayerMask unwalkableMask;
    public float nodeSize;
    public float totalNodesX;
    public float totalNodesY;
    public Vector2 pathSize;
    Dictionary<Vector3, bool> nodes = new Dictionary<Vector3, bool>();
	public GameObject player;
	public Vector3 playerPosRounded;
	public Vector3 other;

    public void Start()
    {
        totalNodesX = pathSize.x / nodeSize;
        totalNodesY = pathSize.y / nodeSize;

        Vector3 botLeft;
        botLeft.x = transform.position.x - pathSize.x / 2;
        botLeft.y = transform.position.x - pathSize.y / 2;

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

                nodes[position] = canWalk;
            }
        }
    }

	public Vector3 GetNodePercent(Vector3 playerPos)
	{
		float percentX = (playerPos.x + pathSize.x / 2) / pathSize.x;
		float percentY = (playerPos.z + pathSize.y / 2) / pathSize.y;

		percentX = Mathf.RoundToInt(percentX);
		percentY = Mathf.RoundToInt(percentY);

		return new Vector3(percentX, playerPos.y, percentY);
	}

	public KeyValuePair<Vector3, bool> GetClosestNode(Vector3 playerPos)
	{
		KeyValuePair<Vector3, bool> nearest = new KeyValuePair<Vector3, bool>();
		double nearestDiff = 10000000000;

		foreach (var node in nodes) 
		{
			float diff = Vector3.Distance(node.Key, playerPos);
			if(diff < nearestDiff)
			{
				nearest = node;
				nearestDiff = diff;
			}
		}

		return nearest;
	}


    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(pathSize.x, 1, pathSize.y));
		KeyValuePair<Vector3, bool> nearestNode = GetClosestNode(player.transform.position);

        if (nodes != null)
        {
            foreach (KeyValuePair<Vector3, bool> node in nodes)
            {
				Vector3 key = node.Key;
				key.x = Mathf.RoundToInt(node.Key.x);
				key.z = Mathf.RoundToInt(node.Key.z);
				Vector3 pos = player.transform.position;
				pos.x = Mathf.RoundToInt(pos.x);
				pos.y = 0;
				pos.z = Mathf.RoundToInt(pos.z);

				if(node.Key == nearestNode.Key)
				{
					Gizmos.color = Color.blue;
					other = key;
					playerPosRounded = pos;
				}
				else if(node.Value)
				{
					Gizmos.color = Color.green;
				}
				else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawCube(node.Key, Vector3.one * (nodeSize - 0.1f));
            }
        }
    }
}