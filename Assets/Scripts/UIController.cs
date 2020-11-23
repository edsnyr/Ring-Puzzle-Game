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

    public void UpdateMovesDisplay(int moves) {
        moveDisplay.text = (movesToSolve - moves).ToString();
    }

    public void DisableMovesDisplay() {
        textDisplay.gameObject.SetActive(false);
        moveDisplay.gameObject.SetActive(false);
    }
}
