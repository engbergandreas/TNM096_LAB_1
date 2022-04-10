using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Move{
    up, down, left, right
}

public class State {
    //Default constructor will create goal state 1-8 + 0 at the lower right corner 
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
    //Makes a copy of oldState and performs a move, move
    public State(State oldState, Move move) {
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                m[row,col] = oldState.m[row,col];
            }
        }
        currentEmpty = oldState.currentEmpty;
        //Debug.Log("Old empty was: " + oldState.currentEmpty + " this empty is: " + currentEmpty);
        moveMadeHere = move;
        MakeMove(move);
        ComputeCost(); 
    }

    // Move empty square in the direction of move
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
    //Swap value in cell at [row,int] with the currently empty cell
    private void SwapCells(int row, int col) {
        //Debug.Log("Swapping cells at:" + currentEmpty + " with " + row + "," + col);
        int tempValue = m[row,col];
        m[currentEmpty.Item1, currentEmpty.Item2] = tempValue; // 3
        m[row, col] = 0;

        currentEmpty.Item1 = row;
        currentEmpty.Item2 = col;
        //Debug.Log("Empty for this node is now :" + currentEmpty);
    }

    // Compute heuristic cost for this state based on the number of wrong positions
    public void ComputeCost() {
        int sum = 0;
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                if(m[row, col] != 0 && m[row, col] != StateHandler.goalState.m[row, col])
                    ++sum;
            }
        }
        cost = sum;
    }
    
    // This is the actual state
    public int[,] m = new int[3,3]; // 3x3 grid of ints
    // Keep track of where the empty square is
    public (int, int) currentEmpty; 
    // The cost for reaching this state
    public int cost;

    public Move moveMadeHere;
}

public class StateHandler {

    public static State goalState = new State();
    //public State startState = new State();
    //List of possible action at every [y,x] coordinate in the grid
    private static List<Move>[,] possibleActions = InitializePossibleMoves();


    public static State GenerateStartState() {
        State start = new State();
        InitalizeRandomGrid(start);

        return start;
    }
    private static List<Move>[,] InitializePossibleMoves() {
        List<Move>[,] actions = new List<Move>[3,3];
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                actions[row,col] = new List<Move>();
                if(row != 0) actions[row,col].Add(Move.up);
                if(row != 2) actions[row,col].Add(Move.down);
                if(col != 0) actions[row,col].Add(Move.left);
                if(col != 2) actions[row,col].Add(Move.right);
            }
        }
        return actions;
    }
    //Returns the available moves for a current state
    private static List<Move> GetPossibleMoves(State state) {
        return possibleActions[state.currentEmpty.Item1, state.currentEmpty.Item2];
    }

    public static List<State> GenerateChildStates(State state) {
        List<Move> moves = GetPossibleMoves(state);
        List<State> states = new List<State>();

        foreach(Move move in moves) {
            states.Add(new State(state, move));
        }
        
        return states;
    }

    public static void PrintPossibleMoves() {
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                foreach(var item in possibleActions[row,col]) {
                    Debug.Log("index: " + row + "," + col + " " + item);
                }
                //Debug.Log(possibleActions[row,col]);
            }
        }
    }

    public static void PrintPossibleStateMoves(State state) {
        List<Move> moves = GetPossibleMoves(state);
        foreach(Move move in moves) {
            Debug.Log(move);
        }
    }

    private static void InitalizeRandomGrid(State state) {
        int count = 0; // 0 -> empty
        state.m[0,0] = 1;
        state.m[0,1] = 2;
        state.m[0,2] = 3;
        state.m[1,0] = 5;
        state.m[1,1] = 0;
        state.m[1,2] = 4;
        state.m[2,0] = 6;
        state.m[2,1] = 8;
        state.m[2,2] = 7;
        state.currentEmpty = (1,1);
        // for(int row = 0; row < 3; row++) {
        //     for(int col = 0; col < 3; col++) {
        //         state.m[row,col] = count;
        //         count++;
        //     }
        // }

        /* hardest: 
        state.m[0,0] = 6;
        state.m[0,1] = 4;
        state.m[0,2] = 7;
        state.m[1,0] = 8;
        state.m[1,1] = 5;
        state.m[1,2] = 0;
        state.m[2,0] = 3;
        state.m[2,1] = 2;
        state.m[2,2] = 1; 
        state.currentEmpty = (1,2);
        */
        state.ComputeCost();
    }

    // Utility function for printing m of a state
    public static void PrintState(State state) {
        string result = "\n";
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                result += state.m[row,col] + " ";
            }
            result += "\n";
        }
        result += "cost: " + state.cost + "\n";
        Debug.Log(result);
    }
}
