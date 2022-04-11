using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue; 
//Priority queue from https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
using UnityEngine.UI;
using System.Diagnostics;

public class AStarTree : MonoBehaviour {  

    int maxdepth = 0;
    private class Node {
        public Node(Node _parent, State _state, int d) {
            state = _state;
            depth = d;
            parent = _parent;
            children = new List<Node>();
            fCost = depth + state.cost;
            hashKey = _state.ComputeHashKey();
        }

        public State state;
        public List<Node> children;
        public Node parent { get; set; }
        public int fCost;
        public int depth {get; set; }

        public string hashKey;
    }

    
    //StateHandler stateHandler = new StateHandler();
    Node startNode = new Node(null, StateHandler.GenerateStartState(), 0);
    
    SimplePriorityQueue<Node,int> openList = new SimplePriorityQueue<Node, int>(); //Frontier list
    // List<Node> closedList = new List<Node>(); //Expanded list
    // HashSet<string> _closedList = new HashSet<string>(10000);
    Dictionary<string, Node> _closedList = new Dictionary<string, Node>(200000);

    

    private void Start() {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        UnityEngine.Debug.Log(startNode);
        StateHandler.PrintState(startNode.state);
        //StateHandler.PrintPossibleMoves();
        //StateHandler.PrintPossibleStateMoves(startNode.state);
        InitiateTextElements();
        UpdateTextElements(startNode.state);
        UnityEngine.Debug.LogError("Starting A*");
        Astar();
        stopwatch.Stop();
        UnityEngine.Debug.LogError("Elapsed Time:" + stopwatch.ElapsedMilliseconds);
        elapsedTimetext.text = "Elapsed time [ms]: " + stopwatch.ElapsedMilliseconds  + "";
    }

    public Text textElement;
    public Canvas canvas;
    private List<Text> textElements;

    public Text elapsedTimetext;
    private void InitiateTextElements() {
        textElements = new List<Text>(9);
        UnityEngine.Debug.Log(textElements.Count);
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
            UnityEngine.Debug.Log(move);
            yield return new WaitForSeconds(0.8f);
        }
    }

    List<Move> GetSolutionMoves(Node fromNode) {
        List<Move> moves = new List<Move>();
        while(fromNode.parent != null) {
            moves.Add(fromNode.state.moveMadeHere);
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
        if(fromNode.parent != null)
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
        //Enque start node in Open
        openList.Enqueue(startNode, startNode.fCost);


        bool foundSolution = false;
        int counter = 0;

        while(openList.Count != 0) {
            if(counter > 500000) {
                UnityEngine.Debug.LogError("counter reached limit");
                break;
            }
            ++counter;
            
            // string msg="";
            // foreach(Node n in openList) {
            //     msg += n.fCost + " "; 
            // }

            //Remove node from open list with minimum cost f
            Node node = openList.Dequeue();

            //Check if node is end state -> solution has been found
            if(node.state.cost == 0) {
                UnityEngine.Debug.Log("SOLUTION FOUND");
                foundSolution = true;
                StateHandler.PrintState(node.state);
                UnityEngine.Debug.Log(PrintSolution(node));
                // UpdateTextElements(node.state);
                StartCoroutine(PerformMoves(GetSolutionMoves(node)));

                break;
            }
            
            //Add to closed list and expand node generating all of its children
            // closedList.Add(node);
            _closedList[node.hashKey] = node;
            // _closedList.Add(node.hashKey, node);
            //Debug.Log("Expanding nodes children");
            ExpandNode(node);
        }

        if(!foundSolution)
        {
            UnityEngine.Debug.Log("NO SOLUTION FOUND");
        }
        UnityEngine.Debug.LogError("Max depth" + maxdepth);
        // Debug.Log("Closed list: " + closedList.Count);
        UnityEngine.Debug.LogError("Closed dictionary: " + _closedList.Count);
        UnityEngine.Debug.LogError("Open list:" + openList.Count);
        
    }

    private void ExpandNode(Node parentNode) {
        // Get all possible actions from state in current node
        List<State> childStates = StateHandler.GenerateChildStates(parentNode.state);

        //Generate new nodes for each state, link child node with parent node
        foreach(State state in childStates) {
            //StateHandler.PrintState(state);
            
            Node childNode = new Node(parentNode, state, parentNode.depth + 1);

            maxdepth = maxdepth < parentNode.depth + 1 ? parentNode.depth + 1 : maxdepth;
            //Debug.Log("childNode empty: " + childNode.currentEmpty);
            /*
                A->B->C->D 
                C2 lägre cost än C

                A->B->C2

            For every child node n’ do
            – evaluate h(n’) and compute f(n’) = g(n’) + h(n’) = g(n)+c(n,n’)+h(n’) - V

            – If n’ is already on OPEN or CLOSED compare its new f with the old f. If the
            new value is higher, discard the node. Otherwise, replace old f with new f
            and reopen the node -> flytta från closed till open?

            – Else, put n’ with its f value in the right order in OPEN
 */         
            Node _nodeInClosedList;
            bool _inClosed = _closedList.TryGetValue(childNode.hashKey, out _nodeInClosedList);
            
            
            // (bool inClosed, Node nodeInClosedList) = ExistInClosed(state);
            // (bool inOpen, Node nodeInOpenList) = ExistInOpen(state);

            if(_inClosed) {
                if(childNode.fCost < _nodeInClosedList.fCost) {
                    //Remove the node already in list and add new child node to open list
                    // closedList.Remove(nodeInClosedList);
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
            //     //Debug.Log("Adding childNode to openlist: ");
            //     //StateHandler.PrintState(childNode.state);
            //     openList.Enqueue(childNode, childNode.fCost);
            //     //Debug.Log("# of elements in closed list:" + closedList.Count + " # of elements in open list: "+ openList.Count);
            // }
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

    // private (bool, Node) ExistInOpen(State state) {
    //     foreach(Node node in openList) {
    //         if(CompareStates(state, node.state))  
    //             return (true, node);
    //     }
    //     return (false, null);
    // }
}
