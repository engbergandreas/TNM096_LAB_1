using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue; 
//Priority queue from https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
using UnityEngine.UI;
using System.Diagnostics;

public class AStarTree : MonoBehaviour {  
    private class Node {
        public Node(Node _parent, State _state, int d) {
            state = _state;
            depth = d;
            parent = _parent;
            fCost = depth + state.cost;
            hashKey = _state.ComputeHashKey();
        }

        public State state;
        public Node parent { get; set; }
        public int fCost;
        public int depth {get; set; }

        public string hashKey;
    }

    Node startNode = new Node(null, StateHandler.GenerateStartState(), 0);
    
    SimplePriorityQueue<Node,int> openList = new SimplePriorityQueue<Node, int>(); //Frontier list
    Dictionary<string, Node> _closedList = new Dictionary<string, Node>(200000);

    

    private void Start() {
        InitiateTextElements();
        UpdateTextElements(startNode.state);
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Astar();
        stopwatch.Stop();
        elapsedTimeText.text = "Elapsed time [ms]: " + stopwatch.ElapsedMilliseconds  + "";
    }

    // Prefab text element object
    public Text textPrefab;
    public Text elapsedTimeText;
    public Canvas canvas;
    
    // List contains text tiles for the puzzle
    private List<Text> textElements;

    //Create the text tiles for each number
    private void InitiateTextElements() {
        textElements = new List<Text>(9);
        for(int row = 0; row < 3; row++) {
            for(int col =0; col < 3; ++col) {
                var text = Instantiate(textPrefab);
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
    //Updates text element with current state value
    private void UpdateTextElements(State state) {
        for(int row = 0; row < 3; row++) {
            for(int col =0; col < 3; ++col) 
                textElements[row*3+col].text = state.m[row,col] + "";
        }
    }

    // Display solution from list of moves
    IEnumerator PerformMoves(List<Move> moves) {
        State nextState = startNode.state;
        foreach (Move move in moves)
        {
            nextState = new State(nextState, move);
            UpdateTextElements(nextState);
            UnityEngine.Debug.Log(move);
            yield return new WaitForSeconds(0.8f);
        }
    }

    //Retrieve the solution from start to end node
    List<Move> GetSolutionMoves(Node fromNode) {
        List<Move> moves = new List<Move>();
        while(fromNode.parent != null) {
            moves.Add(fromNode.state.moveMadeHere);
            fromNode = fromNode.parent;
        }
        moves.Reverse();
        textPrefab.text = "Number of moves to solution: " + moves.Count;

        return moves;
    }
    
    // Return solution of puzzle as string
    private string PrintSolution(Node fromNode) {
        string msg = "";
        if(fromNode.parent != null) {
            msg += PrintSolution(fromNode.parent);
        }
        if(fromNode.parent != null)
            msg += fromNode.state.moveMadeHere + " ";
            
        return msg;
    }


    // Perform the A* algorithm
    void Astar() {
        //Enque start node in Open
        openList.Enqueue(startNode, startNode.fCost);

        bool foundSolution = false;
        int counter = 0;

        while(openList.Count != 0) {
            if(counter > 500000) {
                UnityEngine.Debug.LogError("Can't find possible solution after 500k iterations of A*");
                break;
            }
            ++counter;

            //Remove node from open list with minimum cost f
            Node node = openList.Dequeue();

            //Check if node is end state -> solution has been found
            if(node.state.cost == 0) {
                UnityEngine.Debug.Log("SOLUTION FOUND");
                foundSolution = true;
                UnityEngine.Debug.Log(PrintSolution(node));
                // UpdateTextElements(node.state);
                StartCoroutine(PerformMoves(GetSolutionMoves(node)));
                break;
            }
            
            //Add to closed list and expand node generating all of its children
            _closedList[node.hashKey] = node;
            ExpandNode(node);
        }

        if(!foundSolution) {
            UnityEngine.Debug.LogError("OPEN LIST IS EMPTY NO SOLUTION FOUND!");
        }        
    }
    //Expand a node to find all possible moves from current state, adds children to open list if they
    //don't already exist 
    private void ExpandNode(Node parentNode) {
        // Get all possible actions from state in current node
        List<State> childStates = StateHandler.GenerateChildStates(parentNode.state);

        //Generate new nodes for each state, link child node with parent node
        foreach(State state in childStates) {
            //StateHandler.PrintState(state);
            Node childNode = new Node(parentNode, state, parentNode.depth + 1);
            
            //Check if child node exist in closed list already
            Node _nodeInClosedList;
            bool _inClosed = _closedList.TryGetValue(childNode.hashKey, out _nodeInClosedList);
            //Check if child node exist in open list already
            // (bool inOpen, Node nodeInOpenList) = ExistInOpen(state);

            // – If n is already on OPEN or CLOSED compare its new f with the old f. If the
            // new value is higher, discard the node. Otherwise, replace old f with new f
            // and reopen the node
            if(_inClosed) {
                if(childNode.fCost < _nodeInClosedList.fCost) {
                    //Remove the node already in list and add new child node to open list
                    _closedList.Remove(_nodeInClosedList.hashKey);
                    openList.Enqueue(childNode, childNode.fCost);
                }
            }
            // else if(inOpen) {
            //         if(childNode.fCost < nodeInOpenList.fCost) {
            //             //Remove the node already in list and add new child node to open list
            //             openList.Remove(nodeInOpenList);
            //             openList.Enqueue(childNode, childNode.fCost);
            //         }
            // }
            else 
                openList.Enqueue(childNode, childNode.fCost);
            // if(!inOpen && !_inClosed) {
            //     //does not exist in either open or closed list, add to openlist
            //     openList.Enqueue(childNode, childNode.fCost);
            // }
        }
    }
    
    // Are states s1 and s2 equal in terms of state matrix?
    // private bool CompareStates(State s1, State s2) {
    //     for(int row = 0; row < 3; ++row) {
    //         for(int col = 0; col < 3; ++col) {
    //             if(s1.m[row, col] != s2.m[row,col])
    //                 return false; //Exit if state 1 and state 2 differes in any cell
    //         }
    //     }
    //     return true; // All are equal
    // }

    // private (bool, Node) ExistInOpen(State state) {
    //     foreach(Node node in openList) {
    //         if(CompareStates(state, node.state))  
    //             return (true, node);
    //     }
    //     return (false, null);
    // }
}
