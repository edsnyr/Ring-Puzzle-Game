using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLog : MonoBehaviour
{

    public float undoTime = 0.1f; //time of a move when the player undoes an action

    GameController gameController;
    UIController uiController;
    List<Action> log;

    private void Awake() {
        gameController = GetComponent<GameController>();
        uiController = GetComponent<UIController>();
    }

    private void Start() {
        log = new List<Action>();
        GameController.spinEvent.AddListener(LogSpin);
        GameController.shiftEvent.AddListener(LogShift);
    }

    /// <summary>
    /// Adds a spin event to the log. If the log is not empty, attempt
    /// to combine the previous actions.
    /// </summary>
    /// <param name="row">Ring of the spin event.</param>
    /// <param name="dir">Direction of the spin event.</param>
    /// <param name="time">Duration, not used.</param>
    /// <param name="reps">Number of times the spin occurred.</param>
    private void LogSpin(int row, bool dir, float time, int reps) {
        Action newAction = new Action(SelectionMode.Ring, row, dir, reps);
        if(log.Count == 0) {
            log.Add(newAction);
        } else {
            TryAdd(newAction);
        }
        uiController.UpdateMovesDisplay(log.Count);
        //PrintList();
    }

    /// <summary>
    /// Adds a shift event to the log. If the log is not empty, attempt
    /// to combine the previous actions.
    /// </summary>
    /// <param name="column">Ring of the shift event.</param>
    /// <param name="dir">Direction of the shift event.</param>
    /// <param name="time">Duration, not used.</param>
    /// <param name="reps">Number of times the shift occurred.</param>
    private void LogShift(int column, bool dir, float time, int reps) {
        Action newAction = new Action(SelectionMode.Column, column, dir, reps);
        if(log.Count == 0) {
            log.Add(newAction);
        } else {
            TryAdd(newAction);
        }
        uiController.UpdateMovesDisplay(log.Count);
        //PrintList();
    }

    /// <summary>
    /// If the newest action matches the type and location of the previous, combine them.
    /// If the previous action has been undone and now equals 0, remove it. Otherwise,
    /// add the new action to the log.
    /// </summary>
    /// <param name="newAction">The most recent action.</param>
    private void TryAdd(Action newAction) {
        Action lastAction = log[log.Count - 1];
        if(lastAction.mode == newAction.mode && lastAction.location == newAction.location) { //check if move is the same as the last
            int num = lastAction.reps + newAction.reps * (lastAction.dir == newAction.dir ? 1 : -1); //add if direction was the same, subtract if it was undone
            if(num == 0) { //0 means the previous action has returned to its original position
                log.RemoveAt(log.Count - 1); //remove the action from the log, as it has been undone
            } else {
                log[log.Count - 1] = new Action(lastAction.mode, lastAction.location, lastAction.dir, num);
            }
        } else {
            log.Add(newAction);
        }
    }

    public void Undo() {
        StartCoroutine(ReverseMove(undoTime));
    }

    public void UndoAll() {
        StartCoroutine(ReverseAll(undoTime));
    }

    /// <summary>
    /// Replays the last action in reverse. Because this is done by calling a move event,
    /// TryAdd is called naturally and the action is removed from the list.
    /// </summary>
    public IEnumerator ReverseMove(float time) {
        if(GameController.canMove && log.Count > 0) {
            GameController.canMove = false;
            if(log[log.Count - 1].mode == SelectionMode.Ring) {
                GameController.spinEvent.Invoke(log[log.Count - 1].location, !log[log.Count - 1].dir, time, log[log.Count - 1].reps);
            } else {
                GameController.shiftEvent.Invoke(log[log.Count - 1].location, !log[log.Count - 1].dir, time, log[log.Count - 1].reps);
            }
        }
        yield return new WaitForSeconds(time + gameController.waitTime);
    }

    public IEnumerator ReverseAll(float time) {
        while(log.Count > 0) {
            yield return StartCoroutine(ReverseMove(time));
        }
    }

    /// <summary>
    /// Resets the action log.
    /// </summary>
    public void ClearLog() {
        log = new List<Action>();
    }

    /// <summary>
    /// Prints each action in the log as a debug log.
    /// </summary>
    private void PrintList() {
        foreach(Action action in log) {
            Debug.Log(action);
        }
    }

    public void InsertSolve(List<Action> solveLog) {
        log = solveLog;
    }
}

public class Action {
    public SelectionMode mode;
    public int location;
    public bool dir;
    public int reps;

    public Action(SelectionMode mode, int location, bool dir, int reps) {
        this.mode = mode;
        this.location = location;
        this.dir = dir;
        this.reps = reps;
    }

    public override string ToString() {
        return mode + " " + location + " " + dir + " " + reps;
    }
}
