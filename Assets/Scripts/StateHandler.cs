using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Move{
    up, down, left, right
}



public class State {

    public State() {
        int count = 1; // 0 -> empty
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                if(row == 2 && col == 2)
                    m[row,col] = 0;
                else 
                    m[row,col] = count;

                count++;
            }
        }
    }
    public State(State oldState, Move move) {
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                m[row,col] = oldState.m[row,col];
            }
        }
        currentEmpty = oldState.currentEmpty;
        MakeMove(move);
        ComputeCost();
            
    }

    private void MakeMove(Move move) {
        int row = currentEmpty.Item1;
        int col = currentEmpty.Item2;
        switch (move)
        {
            case Move.up:
                SwapCells(row-1, col);
                break;
            case Move.down:
                SwapCells(row+1, col);
                break;
            case Move.left: 
                SwapCells(row, col-1);
                break;
            case Move.right:
                SwapCells(row, col+1);
                break;
        }
    }

    private void SwapCells(int row, int col) {
        int tempValue = m[row,col];
        m[currentEmpty.Item1, currentEmpty.Item2] = tempValue; // 3
        m[row, col] = 0;

        currentEmpty.Item1 = row;
        currentEmpty.Item1 = col;
    }

    void ComputeCost() {
        int sum = 0;
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                if(m[row, col] != 0 && m[row, col] != StateHandler.goalState.m[row, col])
                    ++sum;
            }
        }
        cost = sum;
    }
    
    public int[,] m = new int[3,3]; // 3x3 grid of ints
    public (int, int) currentEmpty; // What can I do based on this?
    public int cost;

    static int counter; 
}

public class StateHandler : MonoBehaviour {
    // Start is called before the first frame update'

    State currentState = new State();
    public static State goalState = new State();
    


    void ComputeCost(ref State state) {
        int sum = 0;
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                if(state.m[row, col] != 0 && state.m[row, col] != goalState.m[row, col])
                    ++sum;
            }
        }
        state.cost = sum;
    }

    List<Move>[,] possibleActions = new List<Move>[3,3];


    //State newState = Left(currentState);

    void InitializePossibleMoves() {
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                possibleActions[row,col] = new List<Move>();
                if(row != 0) possibleActions[row,col].Add(Move.up);
                if(row != 2) possibleActions[row,col].Add(Move.down);
                if(col != 0) possibleActions[row,col].Add(Move.left);
                if(col != 2) possibleActions[row,col].Add(Move.right);
            }
        }
    }

    List<Move> GetPossibleMoves(State state) {
        return possibleActions[state.currentEmpty.Item1, state.currentEmpty.Item2];
    }

    void CreateNextLayer() {
        var moves = GetPossibleMoves(currentState);
        foreach(var move in moves) {
            //children.append(new State(currentState, move));
        }
    }
    

    void Start() {
        InitializePossibleMoves();

        //print moves
        Debug.Log(possibleActions.Length);
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                foreach(var item in possibleActions[row,col]) {
                    Debug.Log("index: " + row + "," + col + " " + item);
                    //Debug.Log($"index: {row}, {col} {item}");
                }
                //Debug.Log(possibleActions[row,col]);
            }
        }
        // InitializeGrid();
        InitializeGrid(currentState);
        PrintGrid(currentState);
        Debug.Log("cost of current state: " + currentState.cost);
        ComputeCost(ref currentState);
        Debug.Log("updated cost: " + currentState.cost);
        State childState = new State(currentState, Move.right);
        PrintGrid(childState);
        Debug.Log("child cost: " + childState.cost);

        PrintGrid(StateHandler.goalState);
        Debug.Log(goalState.cost);
        // Debug.Log(goalState.)
    }
    
    // ref State CreateChildState(Move move) { 
    //     ref State childState = new State(currentState, Move.down);
    //     ComputeCost(ref childState);
    //     return ref childState;
    // }
    void InitializeGrid(State state) {
        int count = 0; // 0 -> empty
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                state.m[row,col] = count;
                count++;
            }
        }
    }

    void PrintGrid(State state) {
        string result = "\n";
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                result += state.m[row,col] + " ";
            }
            result += "\n";
        }   
        Debug.Log(result);
    }

    
    // Update is called once per frame
    void Update() {
        
    }
}
