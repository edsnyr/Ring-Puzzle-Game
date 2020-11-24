using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{

    public TextMeshProUGUI textDisplay;
    public TextMeshProUGUI moveDisplay;

    int movesToSolve;

    public void EnableAndSetMovesDisplay(int moves) {
        textDisplay.gameObject.SetActive(true);
        moveDisplay.gameObject.SetActive(true);
        movesToSolve = moves;
        moveDisplay.text = moves.ToString();
    }

    /// <summary>
    /// Displays the remaining number of moves to optimally solve the puzzle.
    /// </summary>
    /// <param name="moves">Current number of moves made in the action log.</param>
    public void UpdateMovesDisplay(int moves) {
        moveDisplay.text = (movesToSolve - moves).ToString();
    }

    /// <summary>
    /// Called when unscrambling a puzzle, as it is not needed and will otherwise display incorrectly.
    /// </summary>
    public void DisableMovesDisplay() {
        textDisplay.gameObject.SetActive(false);
        moveDisplay.gameObject.SetActive(false);
    }
}
