using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum Move{
    up, down, left, right
}

public class State {
    // Default constructor will create goal state 1-8 + 0 at the lower right corner 
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
        currentEmpty = (2,2);
    }
    // Makes a copy of oldState and performs a move, move
    public State(State oldState, Move move) {
        for(int row = 0; row < 3; row++) {
            for(int col = 0; col < 3; col++) {
                m[row,col] = oldState.m[row,col];
            }
        }
        currentEmpty = oldState.currentEmpty;
        moveMadeHere = move;
        MakeMove(move);
        ComputeCost(); 
    }

    // Move empty square in the direction of move
    public void MakeMove(Move move) {
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
    // Swap value in cell at [row,int] with the currently empty cell
    private void SwapCells(int row, int col) {
        //Debug.Log("Swapping cells at:" + currentEmpty + " with " + row + "," + col);
        int tempValue = m[row,col];
        m[currentEmpty.Item1, currentEmpty.Item2] = tempValue; // 3
        m[row, col] = 0;

        currentEmpty.Item1 = row;
        currentEmpty.Item2 = col;
    }

    // Compute heuristic cost h1 or h2
    // h1 is the number of wrong positions, h2 manhattan distance to correct solution
    public void ComputeCost() {
        if(StateHandler.useH1) {
            int sum = 0;
            for(int row = 0; row < 3; row++) {
                for(int col = 0; col < 3; col++) {
                    if(m[row, col] != 0 && m[row, col] != StateHandler.goalState.m[row, col])
                        ++sum;
                }
            }
            cost = sum;
        }
        else {
            int sum = 0;
            for(int row = 0; row < 3; row++) {
                for(int col = 0; col < 3; col++) {
                    sum += ManhattanDistance(m[row,col], row, col);
                }
            }
            cost = sum;
        }

    }

    // For a given value, return the correct position
    (int, int) GetIndexFromValue(int val) {
        if(val == 1) return (0,0);
        if(val == 2) return (0,1);
        if(val == 3) return (0,2);
        if(val == 4) return (1,0);
        if(val == 5) return (1,1);
        if(val == 6) return (1,2);
        if(val == 7) return (2,0);
        if(val == 8) return (2,1);
        if(val == 0) return (2,2);
        return (-1, -1);
    }

    // Compute manhattan distance for a given value,
    // compared to where it should be.
    int ManhattanDistance(int value, int y0, int x0) {
        (int y1, int x1) = GetIndexFromValue(value);
        int distance = Math.Abs(x0 - x1) + Math.Abs(y0 - y1);
        return distance;
    }

    // Compute the hash key for this matrix
    public string ComputeHashKey() {
        string result = "";
        for(int row =0; row < 3; row++) {
            for(int col =0; col < 3; ++col) {
                result += m[row,col];
            }        
        }
        return result;
    }
    
    // This is the actual state
    public int[,] m = new int[3,3]; // 3x3 grid of ints
    // Keep track of where the empty square is
    public (int, int) currentEmpty; 
    // The cost for reaching this state
    public int cost;
    // Use h1 or h2 heuristic?
    // private bool h1 = false;
    // The latest move to reach this state.
    public Move moveMadeHere;
}

public class StateHandler {
    //Define goalstate as 1-8, 0 at bottom right corner (default State ctor)
    public static bool useH1 = false;
    public static State goalState = new State();

    public static State startState = new State();
    //List of possible action at every [y,x] coordinate in the grid
    private static List<Move>[,] possibleActions = InitializePossibleMoves();

    //Generate random start state
    // public static State GenerateStartState() {
    //     State start = new State();
    //     InitalizeRandomGrid(start);

    //     return start;
    // }

    // Store possible moves at each location
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
    // Returns all available moves for a current state
    private static List<Move> GetPossibleMoves(State state) {
        return possibleActions[state.currentEmpty.Item1, state.currentEmpty.Item2];
    }
    // Generates possible child states for current state
    public static List<State> GenerateChildStates(State state) {
        List<Move> moves = GetPossibleMoves(state);
        List<State> states = new List<State>();

        foreach(Move move in moves) {
            states.Add(new State(state, move));
        }
        
        return states;
    }


    public static State EasyGrid() {
        startState.m[0,1] = 2;
        startState.m[0,0] = 1;
        startState.m[0,2] = 3;
        startState.m[1,0] = 5;
        startState.m[1,1] = 0;
        startState.m[1,2] = 4;
        startState.m[2,0] = 6;
        startState.m[2,1] = 8;
        startState.m[2,2] = 7;
        startState.currentEmpty = (1,1);
        startState.ComputeCost();
        return startState;
    }

    public static State HardGrid() {
        startState.m[0,0] = 6;
        startState.m[0,1] = 4;
        startState.m[0,2] = 7;
        startState.m[1,0] = 8;
        startState.m[1,1] = 5;
        startState.m[1,2] = 0;
        startState.m[2,0] = 3;
        startState.m[2,1] = 2;
        startState.m[2,2] = 1; 
        startState.currentEmpty = (1,2);
        
        startState.ComputeCost();
        return startState;
    }

    // Generate a random state
    public static State InitalizeRandomGrid() {

        System.Random r = new System.Random();

        int iterations = 30;
        for(int i = 0; i < iterations; i++) {
            var moves = GetPossibleMoves(startState);
            int moveIdx = r.Next(0, moves.Count);
            startState.MakeMove(moves[moveIdx]);
        }

        startState.ComputeCost();
        
        return startState;

    }
}
