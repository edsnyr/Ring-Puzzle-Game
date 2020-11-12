﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrigamiRules : Rules
{

    /*
     * Rules based on Paper Mario: The Origami King, the game this came from.
     * Align pieces in columns of 4, or in squares of 2x2 on the inner two rings.
     * Currently no logic in place to check number of moves made or if a puzzle is solved,
     * so the player must recognize for themselves for now.
     * A move is defined as a series of shifts or spins on the same column or ring, respectively;
     * for example: spinning the same ring 4 times counts as one move, shifting a column would be
     * a second move, and spinning the original ring again would be a third move.
     */

    GameController gameController;
    UIController uiController;

    public Piece piecePrefab;
    public int maxSets = 3; //number of groups of 4 pieces in puzzle
    public int maxMoves = 3; //number of moves needed to solve
    public List<Color> colors; //possible piece colors
    public Color defaultColor;

    List<Piece> pieces;
    List<Action> solveLog; //holds the solution to the current puzzle
    ActionLog actionLog;

    private void Awake() {
        pieces = new List<Piece>();
        solveLog = new List<Action>();
        gameController = GetComponent<GameController>();
        uiController = GetComponent<UIController>();
        actionLog = GetComponent<ActionLog>();
    }

    void Start()
    {
        BuildPuzzle();
    }

    /// <summary>
    /// Creates a solved puzzle, then scrambles it. The pieces are then placed on the board
    /// at their appropriate positions.
    /// </summary>
    private void BuildPuzzle() {
        int sets = Random.Range(1, maxSets + 1); //determine how many sets of pieces there will be
        for(int i = 0; i < sets;) {
            int type = Random.Range(0, 2); //which type of set
            int column = Random.Range(0, 12); //which column to place the set
            Color color;
            if(colors.Count - 1 >= i) {
                color = colors[i];
            } else {
                color = defaultColor;
            }
            switch(type) {
                case 0:
                    if(CheckSquare(column)) {
                        PlaceSquare(column, color);
                        i++; //only increment loop if placement is successful
                    }
                    break;
                case 1:
                    if(CheckColumn(column)) {
                        PlaceColumn(column, color);
                        i++; //only increment loop if placement is successful
                    }
                    break;
                default:
                    Debug.Log("Invalid type.");
                    break;
            }
        }
        Scramble();
    }

    /// <summary>
    /// Ensure no spaces in the chosen column are already occupied.
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    private bool CheckColumn(int column) {
        foreach(Piece piece in pieces) {
            if(piece.location == column) {
                return false;
            }
        }
        return true;
    }

    //Place 4 pieces on a specified column.
    private void PlaceColumn(int column, Color color) {
        //Debug.Log("Place Column " + column);
        for(int i = 1; i <= 4; i++) {
            Piece newPiece = Instantiate(piecePrefab);
            newPiece.gameController = gameController;
            newPiece.sr.color = color;
            newPiece.PlacePiece(i, column);
            pieces.Add(newPiece);
        }
    }

    /// <summary>
    /// Ensure there are no pieces in the specified column or next.
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    private bool CheckSquare(int column) {
        foreach(Piece piece in pieces) {
            if(piece.location == column || piece.location == (column + 1) % 12) {
                if(piece.ring == 1 || piece.ring == 2) {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Place 4 pieces in the specified column and next, in a square formation.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="color"></param>
    private void PlaceSquare(int column, Color color) {
        //Debug.Log("Place Square: " + column);
        for(int i = 1; i <= 2; i++) {
            for(int j = column; j <= column + 1; j++) {
                Piece newPiece = Instantiate(piecePrefab);
                newPiece.gameController = gameController;
                newPiece.sr.color = color;
                newPiece.PlacePiece(i, j%12);
                pieces.Add(newPiece);
            }
        }
    }

    /// <summary>
    /// Scrambles the puzzle once it has been created. Currently no checks to ensure duplicate moves
    /// do not happen, or that the puzzle isn't already solved.
    /// </summary>
    private void Scramble() {
        int moves = Random.Range(1, maxMoves + 1);
        if(uiController != null)
            uiController.UpdateMoveDisplay(moves);
        for(int i = 0; i < moves;) {
            int type = Random.Range(0, 2); //randomly select either shift or spin
            Piece target = pieces[Random.Range(0, pieces.Count)]; //by selecting a piece to manipulate, ensures every move affects at least one piece
            switch(type) {
                case 0:
                    if(TrySpin(target.ring, Random.Range(1, 7), Random.value > 0.5f ? true : false)) { i++; } //spin a random direction a random number of times
                    break;
                case 1:
                    if(TryShift(target.location % 6, Random.Range(1, gameController.numberOfRings + 1), Random.value > 0.5f ? true : false)) { i++; } //shift a random direction a random number of times
                    break;
                default:
                    Debug.Log("Invalid Type.");
                    break;
            }
        }

    }

    /// <summary>
    /// Checks if the given spin instruction is allowed.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="reps"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool TrySpin(int location, int reps, bool dir) {
        if(solveLog.Count != 0) {
            if(solveLog[solveLog.Count - 1].mode == SelectionMode.Ring && solveLog[solveLog.Count - 1].location == location) { //do not perform if it is the same action as the last
                return false;
            }
        }
        ScrambleSpin(location, reps, dir);
        return true;
    }

    /// <summary>
    /// Spins the pieces as part of the scramble and assigns pieces their new locations.
    /// Adds the move to the solve log.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="reps"></param>
    /// <param name="dir"></param>
    private void ScrambleSpin(int location, int reps, bool dir) {
        solveLog.Add(new Action(SelectionMode.Ring, location, dir, reps));
        foreach(Piece piece in pieces) {
            if(piece.ring == location) {
                int newLocation = ((piece.location + 12 + reps * (dir ? -1 : 1)) % 12);
                piece.PlacePiece(piece.ring, newLocation); 
            }
        }
    }

    /// <summary>
    /// Checks if the given shift instruction is allowed.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="reps"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool TryShift(int location, int reps, bool dir) {
        if(solveLog.Count != 0) {
            if(solveLog[solveLog.Count - 1].mode == SelectionMode.Column && solveLog[solveLog.Count - 1].location == location) { //do not perform if it is the same action as the last
                return false;
            }
        }
        ScrambleShift(location, reps, dir);
        return true;
    }

    /// <summary>
    /// Shifts the pieces as part of the scramble and assigns pieces their new locations.
    /// Adds the move to the solve log.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="reps"></param>
    /// <param name="dir"></param>
    private void ScrambleShift(int location, int reps, bool dir) {
        solveLog.Add(new Action(SelectionMode.Column, location, dir, reps));
        foreach(Piece piece in pieces) {
            if(piece.location % 6 == location) {
                bool newDir = ((piece.location / 6) % 2) == 0 ? !dir : dir;
                int newRing = piece.ring + (reps * (newDir ? -1 : 1));
                int newLocation = piece.location;
                if(newRing <= 0) {
                    newRing = (newRing * -1) + 1;
                    newLocation = (newLocation + 6) % 12;
                }
                if(newRing > gameController.numberOfRings) {
                    newRing = gameController.numberOfRings - (newRing - (gameController.numberOfRings + 1));
                    newLocation = (newLocation + 6) % 12;
                }
                piece.PlacePiece(newRing, newLocation);
            }
        }
    }

    /// <summary>
    /// Called by a button to solve the puzzle.
    /// </summary>
    public void StartUnscramble() {
        StartCoroutine(Unscramble());
    }

    /// <summary>
    /// Undoes all moves made by the player, then gives the solution to the action log to solve.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Unscramble() {
        yield return StartCoroutine(actionLog.ReverseAll(0));
        yield return new WaitForSeconds(0.25f);
        actionLog.InsertSolve(solveLog);
        yield return StartCoroutine(actionLog.ReverseAll(actionLog.undoTime));
    }

    /// <summary>
    /// Removes all pieces and clears the pieces and solutions lists 
    /// </summary>
    private void ClearPuzzle() {
        foreach(Piece piece in pieces) {
            Destroy(piece.gameObject);
        }
        pieces = new List<Piece>();
        solveLog = new List<Action>();
        actionLog.ClearLog();
    }

    /// <summary>
    /// Clears the current puzzle and builds a new one.
    /// </summary>
    public void ResetPuzzle() {
        ClearPuzzle();
        BuildPuzzle();
    }
}
