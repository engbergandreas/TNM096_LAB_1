using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue; 
//Priority queue from https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp

public class AStarTree : MonoBehaviour {  

    int maxdepth = 0;
    private class Node {
        public Node(Node _parent, State _state, int d) {
            state = _state;
            depth = d;
            parent = _parent;
            children = new List<Node>();
            fCost = depth + state.cost;
        }

        
        public State state;
        public List<Node> children;
        public Node parent { get; private set; }
        public int fCost;
        public int depth {get; private set; }
    }

    
    //StateHandler stateHandler = new StateHandler();
    Node startNode = new Node(null, StateHandler.GenerateStartState(), 0);
    
    SimplePriorityQueue<Node,int> openList = new SimplePriorityQueue<Node, int>(); //Frontier list
    List<Node> closedList = new List<Node>(); //Expanded list

    private void Start() {
        Debug.Log(startNode);
        StateHandler.PrintState(startNode.state);
        //StateHandler.PrintPossibleMoves();
        //StateHandler.PrintPossibleStateMoves(startNode.state);
        Astar();
    }

    void Astar() {
        openList.Enqueue(startNode, startNode.fCost);
        Debug.Log("# items in openlist: " + openList.Count);
        bool foundSolution = false;
        int counter = 0;

        while(openList.Count != 0) {
            if(counter > 10000000) {
                Debug.Log("counter reached limit");
                break;
            }
            Debug.Log("Iterator: " + counter++);

            Node node = openList.Dequeue();
            Debug.Log("Dequed node was: ");
            StateHandler.PrintState(node.state);

            Debug.Log("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);

            if(node.state.cost == 0) {
                Debug.Log("SOLUTION FOUND");
                foundSolution = true;
                break;
            }
            Debug.Log("Adding node to closed list");
            closedList.Add(node);
            Debug.Log("Expanding node");
            ExpandNode(node);

        }

        if(!foundSolution)
        {
            Debug.Log("NO SOLUTION FOUND");
        }
        Debug.Log("Max depth" + maxdepth);
    }

    private void ExpandNode(Node node) {
        // Get all possible actions from state in node
        List<State> childStates = StateHandler.GenerateChildStates(node.state);
        Debug.Log("Current node + move to get here was: " + node.state.moveMadeHere );
        StateHandler.PrintState(node.state);
        Debug.Log("Possible Moves: ");
        StateHandler.PrintPossibleStateMoves(node.state);

        Debug.Log("Current Empty node is: " + node.state.currentEmpty);
        Debug.Log("Children states are:");
        // Generate new  nodes for each state, link child node with parent node
        foreach(State state in childStates) {
            StateHandler.PrintState(state);
            
            Node childNode = new Node(node, state, node.depth + 1);
            maxdepth = maxdepth < node.depth + 1 ? node.depth + 1 : maxdepth;
            //Debug.Log("childNode empty: " + childNode.currentEmpty);
            
            // (bool,Node) inClosed = ExistInClosed(state);
            // (bool,Node) inOpen = ExistInOpen(state);

            (bool inClosed, Node closedResult) = ExistInClosed(state);
            (bool inOpen, Node openResult) = ExistInOpen(state);

            if(inClosed) {
                Debug.Log("Child node was found in closed list");
                if(childNode.fCost < closedResult.fCost) {
                    Debug.Log("Updating cost in list for child");
                    //replace cost
                    closedResult.fCost = childNode.fCost;
                    //reopen node?
                    ExpandNode(openResult);
                }
                Debug.Log("Did not add to list, more expensive path");
            }
            else if(inOpen) {
                    Debug.Log("Child node was found in closed list");
                    if(childNode.fCost < openResult.fCost) {
                        Debug.Log("Updating cost in list for child");

                        //replace cost
                        openList.UpdatePriority(openResult, childNode.fCost);
                        // openResult.fCost = childNode.fCost; 
                        //reopen node?
                        ExpandNode(openResult);
                    }
                Debug.Log("Did not add to list, more expensive path");
            }
            else {
                //does not exist in either open or closed list, add to openlist 
                Debug.Log("Adding childNode to openlist: ");
                StateHandler.PrintState(childNode.state);
                openList.Enqueue(childNode, childNode.fCost);
            }
        }

        //Compute F cost (done automatically)
        
        // For each child node check if it exist in either open or closed list

        //if it exists and has cheaper cost than current existing one, replace cost
        //if it exsists and is more expensive discard node

        //if it is not found add the node to open list 

    }
    
    private bool CompareStates(State s1, State s2) {
        for(int row = 0; row < 3; ++row) {
            for(int col = 0; col < 3; ++col) {
                if(s1.m[row, col] != s2.m[row,col])
                    return false; //Exit if state 1 and state 2 differes in any cell
            }
        }
        return true; // All are equal
    }

    private (bool, Node) ExistInOpen(State state) {
        foreach(Node node in openList) {
            if(CompareStates(state, node.state))  
                return (true, node);
        }
        return (false, null);
    }

    private (bool, Node) ExistInClosed(State state) {
        foreach(Node node in closedList) {
            if(CompareStates(state, node.state))
                return (true, node);
        }
        return (false, null);
    }
}
