using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{

    public TextMeshProUGUI textDisplay;
    public TextMeshProUGUI moveDisplay;

    public void UpdateMoveDisplay(int moves) {
        moveDisplay.text = moves.ToString();
    }

}
