using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue; 
//Priority queue from https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
using UnityEngine.UI;

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
        public Node parent { get; set; }
        public int fCost;
        public int depth {get; set; }
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
        InitiateTextElements();
        UpdateTextElements(startNode.state);
        Astar();
    }

    public Text textElement;
    public Canvas canvas;
    private List<Text> textElements;

    private void InitiateTextElements() {
        textElements = new List<Text>(9);
        Debug.Log(textElements.Count);
        for(int row = 0; row < 3; row++) {
            for(int col =0; col < 3; ++col) {
                var text = Instantiate(textElement);
                text.transform.SetParent(canvas.transform);
                text.text =  row * 3 + col + "";
                

                RectTransform rectTransform = text.GetComponent<RectTransform>();
                Vector2 pos = rectTransform.anchoredPosition;
                pos.x = col * 100; 
                pos.y = -row * 100;

                rectTransform.anchoredPosition = pos;
                textElements.Add(text);
            }
        }
    }
    private void UpdateTextElements(State state) {
        for(int row = 0; row < 3; row++) {
            for(int col =0; col < 3; ++col) {
                textElements[row*3+col].text = state.m[row,col] + "";
                // var text = Instantiate(textElement);
                // text.transform.SetParent(canvas.transform);
                // text.text =  row * 3 + col + "";
                // text.text = state.m[row,col] + "";

                // RectTransform rectTransform = text.GetComponent<RectTransform>();
                // Vector2 pos = rectTransform.anchoredPosition;
                // pos.x = col * 100; 
                // pos.y = -row * 100;

                // rectTransform.anchoredPosition = pos;
            }
        }
    }

    IEnumerator PerformMoves(List<Move> moves) {
        State nextState = startNode.state;
        foreach (Move move in moves)
        {
            nextState = new State(nextState, move);
            UpdateTextElements(nextState);
            Debug.Log(move);
            yield return new WaitForSeconds(2);
        }
    }

    List<Move> GetSolutionMoves(Node fromNode) {
        List<Move> moves = new List<Move>();
        while(fromNode.parent != null) {
            moves.Add(fromNode.parent.state.moveMadeHere);
            fromNode = fromNode.parent;
        }
        moves.Reverse();
        return moves;
    }

    private string PrintSolution(Node fromNode) {
        string msg = "";
        if(fromNode.parent != null) {
            msg += PrintSolution(fromNode.parent);
        }
        msg += fromNode.state.moveMadeHere + " ";
        // string msg = fromNode.state.moveMadeHere + ":" + fromNode.state.cost + " ";
        // while(fromNode.parent != null) {
        //     msg += fromNode.parent.state.moveMadeHere + ":" + fromNode.parent.state.cost + " ";
        //     fromNode = fromNode.parent;
        // }

        //msg += fromNode.state.moveMadeHere + ":" + fromNode.state.cost + " " ;

        return msg;
    }

    void Astar() {
        openList.Enqueue(startNode, startNode.fCost);
        //Debug.Log("# items in openlist: " + openList.Count);
        bool foundSolution = false;
        int counter = 0;

        while(openList.Count != 0) {
            if(counter > 10000) {
                Debug.Log("counter reached limit");
                break;
            }
            //Debug.Log("Iterator: " + counter++);

            //Debug.Log("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
            string msg="";
            foreach(Node n in openList) {
                msg += n.fCost + " "; 
            }
            //Debug.Log("Fcost in open list : " + msg);
            Node node = openList.Dequeue();
            //Debug.Log("Dequed node with fcost: " + node.fCost + " was: ");
            //StateHandler.PrintState(node.state);
            //Debug.Log("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);


            if(node.state.cost == 0) {
                Debug.Log("SOLUTION FOUND");
                foundSolution = true;
                Debug.Log(PrintSolution(node));
                // UpdateTextElements(node.state);
                StartCoroutine(PerformMoves(GetSolutionMoves(node)));

                break;
            }
            //Debug.Log("Adding the node to closed list");
            closedList.Add(node);
            //Debug.Log("Expanding nodes children");
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
        //Debug.Log("Current node + move to get here was: " + node.state.moveMadeHere );
        //StateHandler.PrintState(node.state);
        //Debug.Log("Current Empty cell is: " + node.state.currentEmpty);
        //Debug.Log("Possible moves for current node: ");
        List<State> childStates = StateHandler.GenerateChildStates(node.state);
        //StateHandler.PrintPossibleStateMoves(node.state);

        //Debug.Log("Children states are:");
        // Generate new  nodes for each state, link child node with parent node
        foreach(State state in childStates) {
            //StateHandler.PrintState(state);
            
            Node childNode = new Node(node, state, node.depth + 1);
            maxdepth = maxdepth < node.depth + 1 ? node.depth + 1 : maxdepth;
            //Debug.Log("childNode empty: " + childNode.currentEmpty);
            
            // (bool,Node) inClosed = ExistInClosed(state);
            // (bool,Node) inOpen = ExistInOpen(state);
            //Debug.Log("child node fcost:" + childNode.fCost);
            (bool inClosed, Node nodeInClosedList) = ExistInClosed(state);
            (bool inOpen, Node nodeInOpenList) = ExistInOpen(state);

            if(inClosed) {
                //Debug.Log("Child node was found in closed list");
                if(childNode.fCost < nodeInClosedList.fCost) {
                    //Debug.Log("Updating cost in list which was: " + nodeInClosedList.fCost);
                    //replace cost
                    //Debug.LogError("Removing node in closed list");
                    //Debug.LogError("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
                    //closedList.Remove(nodeInClosedList);
                    //Debug.LogError("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
                    //Debug.LogError("Expanding the child node again");
                    //ExpandNode(childNode);
                    // nodeInClosedList.fCost = childNode.fCost;
                    // nodeInClosedList.parent = node;
                    // nodeInClosedList.depth = childNode.depth;
                    //reopen node?
                    // ExpandNode(nodeInOpenList);
                }
                //Debug.Log("Did not add to list, more expensive path");
            }
            else if(inOpen) {
                    //Debug.Log("Child node was found in open list");
                    if(childNode.fCost < nodeInOpenList.fCost) {
                        //Debug.Log("Updating cost in list which was: " + nodeInOpenList.fCost);
                        //Debug.LogError("Removing node in open list");
                        //Debug.LogError("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
                        //openList.Remove(nodeInOpenList);
                        //Debug.LogError("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
                        //Debug.LogError("Expanding the child node again");
                        //ExpandNode(childNode);
                        //replace cost
                        // openList.UpdatePriority(nodeInOpenList, childNode.fCost);
                        // nodeInOpenList.parent = node;
                        // nodeInClosedList.depth = childNode.depth;

                        // openResult.fCost = childNode.fCost; 
                        //reopen node?
                        // ExpandNode(nodeInOpenList);
                    }
                //Debug.Log("Did not add to list, more expensive path");
            }
            if(!inOpen && !inClosed) {
                //does not exist in either open or closed list, add to openlist 
                //Debug.Log("Adding childNode to openlist: ");
                //StateHandler.PrintState(childNode.state);
                openList.Enqueue(childNode, childNode.fCost);
                //Debug.Log("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
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
