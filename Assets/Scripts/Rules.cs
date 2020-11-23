using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rules : MonoBehaviour
{
    public abstract bool CheckSolve();
    public abstract List<Piece> GetPieces();
}
