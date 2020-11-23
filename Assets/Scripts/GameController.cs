using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{

    public Segment segmentPrefab;
    public SpriteMask segmentMask;
    public SpriteRenderer ringSelectionHighlightPrefab;
    public GameObject columnSelectionHighlightPrefab;
    public SpriteMask selectionMaskPrefab;

    public Color ringColor1;
    public Color ringColor2;
    public Color selectionMaskColor;

    public static bool canMove = true; //determines if the player can move again

    public int numberOfRings = 4; //the number of visible rings
    public float initialRingScale = 1f; //the local scale of the inner most ring, ring 1
    public float ringWidth = 0.2f; //the difference in scale between a ring segment and its mask
    public float ringSeparation = 0.05f; //the difference in scale between a ring and the next ring's mask
    public int rotationOffset = 15; //an additional rotation applied to all segments
    public float moveTime = 0.3f; //the time for an action to take place
    public float waitTime = 0.05f; //the time between moves for multiple step actions

    public float smallJumpScale = 1f;
    public float bigJumpScale = 1.5f;
    public float jumpHeight = 1;

    public int ringSegmentSizeInPixels = 2000; //the size of the segment sprite, used for scaling safety
    public int ringSegmentPixelsPerUnit = 250; //the pixels per unit chosen in the sprite settings

    public Button modeButton;
    public Button clockwiseButton;
    public Button clockwiseButton1;
    public Button counterClockwiseButton;
    public Button counterClockwiseButton1;
    public Button shiftUpButton;
    public Button shiftDownButton;
    public Button checkSolveButton;

    public RectTransform shiftButtonPivot; //UI object that holds the shift buttons
    public RectTransform spinButtonPivot; //UI object that holds the spin buttons

    public static ParameterEvent spinEvent; //called when the player wants to spin a ring
    public static ParameterEvent shiftEvent; //called when the player wants to slide a column

    Rules rules;

    SelectionMode mode; //decides whether the player can select rings or columns
    int selectedRing;
    int selectedColumn;
    TextMeshProUGUI modeButtonText;
    SpriteRenderer ringSelectionHighlight; //sprite that highlights the currently selected ring
    SpriteMask ringSelectionMask; //masks the ring highlight sprite, like how segments are made
    GameObject columnSelectionHighlight; //sprite that highlights the currently selected column
    SpriteMask columnSelectionMask; //masks the center of the column highlight sprite

    void Awake() {
        rules = GetComponent<Rules>();
        InitializeEvents();
        BuildRings();
        SetButtonPivotPoints();
        InitializeRingSelectionMask();
        InitializeColumnSelectionMask();
        InitializeButtonEvents();
        InitializeSelectionMode();
    }

    private void Update() {
        CheckClicked();
    }

    /// <summary>
    /// Initializes the events that the segments subscribe to.
    /// </summary>
    private static void InitializeEvents() {
        if(spinEvent == null) { spinEvent = new ParameterEvent(); }
        if(shiftEvent == null) { shiftEvent = new ParameterEvent(); }
    }

    /// <summary>
    /// Adds onClick behavior to each button.
    /// </summary>
    private void InitializeButtonEvents() {
        modeButton.onClick.AddListener(() => SwitchSelectionMode());
        clockwiseButton.onClick.AddListener(() => TrySpin(true));
        clockwiseButton1.onClick.AddListener(() => TrySpin(true));
        counterClockwiseButton.onClick.AddListener(() => TrySpin(false));
        counterClockwiseButton1.onClick.AddListener(() => TrySpin(false));
        shiftUpButton.onClick.AddListener(() => TryShift(true));
        shiftDownButton.onClick.AddListener(() => TryShift(false));
    }

    /// <summary>
    /// Sets the selection mode to ring, hides the column highlight, and updates the mode button.
    /// </summary>
    private void InitializeSelectionMode() {
        modeButtonText = modeButton.GetComponentInChildren<TextMeshProUGUI>();
        mode = SelectionMode.Ring;
        ringSelectionHighlight.gameObject.SetActive(true);
        spinButtonPivot.gameObject.SetActive(true);
        columnSelectionHighlight.SetActive(false);
        shiftButtonPivot.gameObject.SetActive(false);
        ModifyModeButton();
    }

    /// <summary>
    /// Centers the button pivots over the rings
    /// </summary>
    private void SetButtonPivotPoints() {
        shiftButtonPivot.position = Camera.main.WorldToScreenPoint(Vector3.zero);
        spinButtonPivot.position = Camera.main.WorldToScreenPoint(Vector3.zero);

        //add functionality to scale distances based on ring and screen size? Probably new function?
    }

    /// <summary>
    /// If the player can make a move, spin the Segments on the selected ring.
    /// The player cannot move again until the spin is completed.
    /// </summary>
    /// <param name="dir"></param>
    private void TrySpin(bool dir) {
        if(canMove) {
            canMove = false;
            AudioPlayer.clickEvent.Invoke();
            spinEvent.Invoke(selectedRing, dir, moveTime, 1);
        }
    }

    /// <summary>
    /// If the player can make a move, shift the Segments on the selected column.
    /// The player cannot move again until the shift is completed.
    /// </summary>
    /// <param name="dir"></param>
    private void TryShift(bool dir) {
        if(canMove) {
            canMove = false;
            shiftEvent.Invoke(selectedColumn, dir, moveTime, 1);
            AudioPlayer.clickEvent.Invoke();
        }
    }

    /// <summary>
    /// Builds the game board with the parameters given in the GameController.
    /// </summary>
    private void BuildRings() {
        int num = 0;
        for(int i = 0; i < numberOfRings + 2; i++) { //rings 0 and x+1 are hidden, so that segments can slide in and out of sight smoothly
            for(int j = 0; j < 12; j++) {
                Segment segment = Instantiate(segmentPrefab);
                SpriteMask mask = Instantiate(segmentMask);

                segment.name = "Segment " + num;
                segment.gameController = this;

                SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
                sr.color = (num % 2 == i % 2 ? ringColor1 : ringColor2);
                segment.mask = mask;
                segment.mask.name = segment.name + " Mask";

                sr.sortingOrder = num; //ensures the mask only effects its own segment
                segment.mask.frontSortingOrder = num;
                segment.mask.backSortingOrder = num - 1;

                segment.PlaceSegment(i, j);

                num++;
            }
        }
    }

    /// <summary>
    /// Initializes the highlight sprite and mask for the selected ring.
    /// </summary>
    private void InitializeRingSelectionMask() {
        ringSelectionHighlight = Instantiate(ringSelectionHighlightPrefab);
        ringSelectionHighlight.color = selectionMaskColor;

        ringSelectionMask = Instantiate(selectionMaskPrefab);

        selectedRing = 1;

        SetRingSelectionMask();
    }

    /// <summary>
    /// Scales the ring highlight sprite and mask to show the currently selected ring.
    /// </summary>
    private void SetRingSelectionMask() {
        float newScale = initialRingScale + ringWidth * (selectedRing - 1) + ringSeparation * (selectedRing - 1);
        float newMaskScale = newScale - ringWidth;

        ringSelectionHighlight.transform.localScale = new Vector3(newScale, newScale, 1);
        ringSelectionMask.transform.localScale = new Vector3(newMaskScale, newMaskScale, 1);
    }

    /// <summary>
    /// Initializes the column highlight sprite and mask to show the currently selected column.
    /// </summary>
    private void InitializeColumnSelectionMask() {
        columnSelectionHighlight = Instantiate(columnSelectionHighlightPrefab);
        SpriteRenderer[] srs = columnSelectionHighlight.GetComponentsInChildren<SpriteRenderer>(); //get both sprite renderers
        foreach(SpriteRenderer sr in srs) {
            sr.color = selectionMaskColor;
            sr.sortingOrder = 600;
        }
        float highlightScale = initialRingScale + ringWidth * (numberOfRings - 1) + ringSeparation * (numberOfRings - 1); //scale out to the outer most ring
        columnSelectionHighlight.transform.localScale = new Vector3(highlightScale, highlightScale, 1);

        columnSelectionMask = Instantiate(selectionMaskPrefab);
        float maskScale = initialRingScale - ringWidth; //this mask remains constant
        columnSelectionMask.transform.localScale = new Vector3(maskScale, maskScale, 1);
        columnSelectionMask.frontSortingOrder = 600; //ensures the mask only affects the column highlight sprite
        columnSelectionMask.backSortingOrder = 599;

        selectedColumn = 0;

        SetColumnSelectionMask();
    }

    /// <summary>
    /// Rotates the column highlight sprite to show the currently selected column.
    /// </summary>
    private void SetColumnSelectionMask() {
        float newRotation = 30 * selectedColumn + rotationOffset;
        columnSelectionHighlight.transform.localEulerAngles = new Vector3(0, 0, newRotation);
    }

    /// <summary>
    /// When a click is detected, determine the selection behavior.
    /// </summary>
    private void CheckClicked() {
        if(Input.GetButtonDown("Fire1")) {
            switch(mode) {
                case SelectionMode.Ring:
                    GetClickedRing();
                    break;
                case SelectionMode.Column:
                    GetClickedColumn();
                    break;
                default:
                    Debug.Log("No Selection Mode");
                    break;
            }
        }
    }

    /// <summary>
    /// Determines which ring the player clicked on, and sets it as the selected ring.
    /// </summary>
    private void GetClickedRing() {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //get the position the player clicked at
        float distanceFromOrigin = new Vector3(pos.x, pos.y, 0).magnitude; //distance from the origin
        float initialRingRadius = ringSegmentSizeInPixels / ringSegmentPixelsPerUnit / 2;
        float ringWidthByRadius = initialRingRadius * ringWidth;
        for(int i = 1; i <= numberOfRings; i++) {
            float radius = initialRingRadius * (initialRingScale + (ringWidth + ringSeparation) * (i-1)); //outer radius of the current ring in units
            if(distanceFromOrigin >= radius - ringWidthByRadius && distanceFromOrigin <= radius) { //the click is between the inner radius and outer radius of the selected ring
                selectedRing = i;
                SetRingSelectionMask();
                break; //stop searching once found
            }
        }
    }

    /// <summary>
    /// Determines which column the player clicked on, and sets it as the selected column.
    /// </summary>
    private void GetClickedColumn() {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //get the position the player clicked at
        float distanceFromOrigin = new Vector3(pos.x, pos.y, 0).magnitude; //distance from the origin
        float initialRingRadius = ringSegmentSizeInPixels / ringSegmentPixelsPerUnit / 2;
        float minRadius = initialRingRadius * initialRingScale - initialRingRadius * ringWidth;
        float maxRadius = initialRingRadius * (initialRingScale + (ringWidth + ringSeparation) * (numberOfRings - 1));
        if(distanceFromOrigin >= minRadius && distanceFromOrigin <= maxRadius) { //ensure the player clicked somewhere on the rings
            float angle = Vector3.Angle(Vector3.up, new Vector3(pos.x,pos.y,0) * (pos.x > 0 ? -1 : 1)) + rotationOffset; //flipping the position at locations 6-11 will get the correct column
            selectedColumn = (int)((angle) / 30) % 6; //only columns 0-5
            SetColumnSelectionMask();
            SetPivotAngle();
        }
    }

    /// <summary>
    /// Aligns the shift buttons with the column that is currently selected.
    /// </summary>
    private void SetPivotAngle() {
        shiftButtonPivot.localEulerAngles = new Vector3(0, 0, selectedColumn * 30);
    }

    /// <summary>
    /// Flips the Selection mode, and the enabled buttons and selection highlights.
    /// </summary>
    public void SwitchSelectionMode() {
        if(mode == SelectionMode.Ring) {
            mode = SelectionMode.Column;
            columnSelectionHighlight.SetActive(true);
            shiftButtonPivot.gameObject.SetActive(true);
            ringSelectionHighlight.gameObject.SetActive(false);
            spinButtonPivot.gameObject.SetActive(false);
        } else if(mode == SelectionMode.Column) {
            mode = SelectionMode.Ring;
            columnSelectionHighlight.SetActive(false);
            shiftButtonPivot.gameObject.SetActive(false);
            ringSelectionHighlight.gameObject.SetActive(true);
            spinButtonPivot.gameObject.SetActive(true);
        }
        AudioPlayer.clickEvent.Invoke();
        ModifyModeButton();
    }

    /// <summary>
    /// Changes the text on the mode button to reflect the current selection mode.
    /// </summary>
    private void ModifyModeButton() {
        modeButtonText.text = mode == SelectionMode.Ring ? "Spin Mode" : "Slide Mode";
    }

    public void DisplaySolve() {
        rules.CheckSolve();
        foreach(Piece piece in rules.GetPieces()) {
            piece.DisplayHighlight();
        }
    }
}

/// <summary>
/// Event allows for Segments to recieve information about which ones should move,
/// in which direction, and in how long.
/// </summary>
public class ParameterEvent : UnityEvent<int, bool, float, int> {

}

/// <summary>
/// Decides whether the player can manipulate columns or rings.
/// </summary>
public enum SelectionMode {Ring, Column};