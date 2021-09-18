using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public List<Node> rootNodes;
    public List<Node> nodes;
    public int nodeCount = 5;
    public bool debug = true;

    // Start is called before the first frame update
    void Start()
    {
        // First add root node.
        Node rootNode = Instantiate(rootNodes[0].gameObject, new Vector3(0,0,0), Quaternion.identity).GetComponent<Node>();
        rootNode.gameObject.name = "Root";
        // Set up root node physics.
        Rigidbody rootBody = rootNode.gameObject.GetComponent<Rigidbody>();
        rootBody.useGravity = false;
        rootBody.mass = Mathf.Infinity;
        
        // Add all connections of root node to the queue.
        // Use the queue for breadth first creation.
        Queue<Connection> allConnections = new Queue<Connection>();
        foreach(Connection connection in rootNode.connections) {
            allConnections.Enqueue(connection);
        }

        Node currentNode;
        for(int i=0; i<nodeCount; i++) {
            // If no connection to dequeue, end.
            if(allConnections.Count == 0) {
                break;
            }

            // Pull connection from the queue and set our current node.
            Connection currentNodeConnection = allConnections.Dequeue();
            currentNode = currentNodeConnection.transform.parent.gameObject.GetComponent<Node>();
            Log("current node: " + currentNode.name);

            Log("current node connection: " + currentNodeConnection.name + " of: " + currentNodeConnection.transform.parent.gameObject.name);
            Vector3 currentNodeConnectionPosition = currentNodeConnection.transform.position;

            // Select next node to add.
            Node nextNode = Instantiate(nodes[Random.Range(0, nodes.Count)].gameObject, new Vector3(0,0,0), Quaternion.identity).GetComponent<Node>();
            nextNode.gameObject.name = "Node " + i;
            nextNode.enabled = false;

            // Choose a random connection point on the node we will add.
            Connection nextNodeConnection = nextNode.connections[Random.Range(0, nextNode.connections.Count)];
            Log("next node connection: " + nextNodeConnection.name);
            Vector3 nextConnectionPosition = nextNodeConnection.transform.position;
            nextNode.gameObject.transform.Translate(currentNodeConnectionPosition - nextConnectionPosition, Space.Self);

            // Rotate the node into correct orientation by using the Out plane of connection point of current node.
            // QuaternionRotateAround(nextNode.gameObject.transform, currentNodeConnectionPosition, currentNodeConnection.In.transform.rotation);
            Quaternion rotation = Quaternion.FromToRotation(nextNodeConnection.Out.up, currentNodeConnection.In.up);
            QuaternionRotateAround(nextNode.gameObject.transform, currentNodeConnectionPosition, rotation);

            // Add joint between the two nodes at the connection point.
            CharacterJoint joint = currentNode.gameObject.AddComponent<CharacterJoint>();
            joint.anchor = currentNodeConnection.transform.localPosition;
            joint.connectedBody = nextNode.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = nextNodeConnection.transform.localPosition;
            joint.enableCollision = true;

            // Add connections that are NOT the current connection point to the queue.
            foreach(Connection c in nextNode.connections) {
                if(c != nextNodeConnection) {
                    allConnections.Enqueue(c);
                }
            }

            nextNode.enabled = true;
        }
    }

    void AddJoints(Node n) {
        foreach(Connection connection in n.connections) {
            CharacterJoint joint = n.gameObject.AddComponent<CharacterJoint>();
            joint.anchor = connection.transform.localPosition;
            joint.enableCollision = true;
        }
    }

    void QuaternionRotateAround(Transform transform, Vector3 pivotPoint, Quaternion rot){
        transform.position = rot * (transform.position - pivotPoint) + pivotPoint;
        transform.rotation = rot * transform.rotation;
    }

    void Log(string message) {
        if(debug) Debug.Log(message);
    }
}
