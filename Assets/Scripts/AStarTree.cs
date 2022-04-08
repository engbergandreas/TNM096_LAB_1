using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarTree : MonoBehaviour {  
    private class Node {
        public Node(Node _parent, State _state, int d) {
            state = _state;
            depth = d;
            parent = _parent;
            children = new List<Node>();
        }

        
        public State state;
        public List<Node> children;
        public Node parent { get; private set; }
        public int fCost => depth + state.cost;
        private int depth;
    }

    private void ExpandNode(Node node) {
        
    }
    
    private bool CompareNodes(Node n1, Node n2) {
        return false; //TODO
    }

    private bool ExistInOpen(State s) {
        return false; //TODO
    }

    private bool ExistInClosed(State s) {
        return false; //TODO
    }

    //StateHandler stateHandler = new StateHandler();
    Node startNode = new Node(null, StateHandler.GenerateStartState(), 0);

    private void Start() {
        Debug.Log(startNode);
        StateHandler.PrintGrid(startNode.state);
        StateHandler.PrintPossibleMoves();
        StateHandler.PrintPossibleMoves(startNode.state);
    }
}
