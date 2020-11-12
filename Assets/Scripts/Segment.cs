using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    [HideInInspector]
    public GameController gameController;

    public SpriteMask mask;
    public int ring;
    public int location;

    private void Start() {
        GameController.spinEvent.AddListener(checkSpin);
        GameController.shiftEvent.AddListener(checkShift);
    }

    /// <summary>
    /// Spins the Segment if it is in the chosen ring.
    /// </summary>
    /// <param name="row">The chosen ring, between 1 and numberOfRings.</param>
    /// <param name="dir">Direction of the spin. True is clockwise.</param>
    /// <param name="time">Duration of the spin in seconds.</param>
    private void checkSpin(int row, bool dir, float time, int reps) {
        if(row == ring) {
            StartCoroutine(spinSegment(dir, time, reps));
        }
    }

    /// <summary>
    /// Slides the Segment if it is in the chosen column.
    /// </summary>
    /// <param name="column">The chosen column.</param>1
    /// <param name="dir">Direction of the shift. True increases for locations 0-5, decreases 6-11.</param>
    /// <param name="time">Duration of the shift in seconds.</param>
    private void checkShift(int column, bool dir, float time, int reps) {
        if(column == location % 6) {
            StartCoroutine(shiftSegment(dir, time, reps));
        }
    }

    /// <summary>
    /// Applied to each Segment individually. Rotates the Segment around the origin
    /// in the given direction by modifying its local euler angles.
    /// </summary>
    /// <param name="dir">Rotation of the spin. True is clockwise.</param>
    /// <param name="time">Duration of the spin in seconds.</param>
    /// <returns></returns>
    private IEnumerator spinSegment(bool dir, float time, int reps) {
        for(int i = reps; i > 0; i--) {
            location = (location + 12 + (dir ? -1 : 1)) % 12; //bounds are 0-11 and loops around
            float start = transform.localEulerAngles.z;
            float target = 30 * (dir ? -1 : 1); //360 degrees/12 segments = 30 degrees
            float timeElapsed = 0;

            while(timeElapsed < time) { //smooth shift over time
                timeElapsed += Time.deltaTime;
                float angle = start + (timeElapsed / time * target); //new angle since start
                transform.localEulerAngles = new Vector3(0, 0, angle);
                mask.transform.localEulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }
            transform.localEulerAngles = new Vector3(0, 0, start + target); //ensure intended angle
            mask.transform.localEulerAngles = new Vector3(0, 0, start + target);
            yield return new WaitForSeconds(gameController.waitTime);
        }
        GameController.canMove = true;
    }

    /// <summary>
    /// Applied to each Segment individually. Slides the Segment toward or away from the center
    /// in the given direction, done by scaling both the Segment and its Mask.
    /// </summary>
    /// <param name="newDir">Direction of the shift. True shifts up for sections 0-5, down 6-11.</param>
    /// <param name="time">Time for the shift to take place, in seconds.</param>
    /// <returns></returns>
    private IEnumerator shiftSegment(bool dir, float time, int reps) {
        for(int i = reps; i > 0; i--) {
            bool newDir = dir;
            if((location / 6) % 2 == 0) { newDir = !newDir; } //flip direction on one half of column
            ring += (newDir ? -1 : 1); //add or subtract from ring accordingly

            if(ring < 0 || ring > gameController.numberOfRings + 1) { //cycles segment to other side of column if moved out of range
                PlaceSegment((ring + gameController.numberOfRings + 2) % (gameController.numberOfRings + 2), location);
                yield return new WaitForSeconds(time);
            } else {
                float startSegment = transform.localScale.x;
                float startMask = mask.transform.localScale.x;
                float shiftDistance = (gameController.ringWidth + gameController.ringSeparation) * (newDir ? -1 : 1); //total difference in scale and direction
                float timeElapsed = 0;
                while(timeElapsed < time) { //smooth shift over given time
                    timeElapsed += Time.deltaTime;
                    float newScale = timeElapsed / time * shiftDistance; //adjustment since start
                    if(!(ring == gameController.numberOfRings && newDir) && !(ring == gameController.numberOfRings + 1 && !newDir)) { //don't scale when unnecesary - between ring x (outer most) and x+1 (hidden)
                        transform.localScale = new Vector3(startSegment + newScale, startSegment + newScale, 1); //new scale is original + adjustment
                    }
                    if(!(ring == 0 && newDir) && !(ring == 1 && !newDir)) { //don't scale when unnecessary - between ring 0 (hidden) and ring 1 (inner most)
                        mask.transform.localScale = new Vector3(startMask + newScale, startMask + newScale, 1); //new scale is original + adjustment
                    }
                    yield return null;
                }
                if(!(ring == gameController.numberOfRings && newDir) && !(ring == gameController.numberOfRings + 1 && !newDir)) {
                    transform.localScale = new Vector3(startSegment + shiftDistance, startSegment + shiftDistance, 1); //ensure intended scale
                }
                if(!(ring == 0 && newDir) && !(ring == 1 && !newDir)) {
                    mask.transform.localScale = new Vector3(startMask + shiftDistance, startMask + shiftDistance, 1); //ensure intended scale
                }
            }
            yield return new WaitForSeconds(gameController.waitTime);
        }
        GameController.canMove = true; //enable next move
    }

    /// <summary>
    /// Moves the Segment to a new location based on a given ring and location.
    /// </summary>
    /// <param name="newRing">The ring, between 0 and numberOfRings+1, for the Segment to be placed in.</param>
    /// <param name="newLocation">The location, between 0 and 11, for the Segment to be placed in.</param>
    public void PlaceSegment(int newRing, int newLocation) {
        float currentScale = gameController.initialRingScale + (gameController.ringWidth + gameController.ringSeparation) * (newRing - 1); //set scale based on what ring the Segment is in

        transform.localEulerAngles = new Vector3(0, 0, newLocation * 30 + gameController.rotationOffset);
        if(newRing == gameController.numberOfRings + 1) {
            transform.localScale = new Vector3(currentScale - gameController.ringWidth - gameController.ringSeparation, currentScale - gameController.ringWidth - gameController.ringSeparation, 1);
        } else {
            transform.localScale = new Vector3(currentScale, currentScale, 1);
        }

        mask.gameObject.transform.localEulerAngles = new Vector3(0, 0, newLocation * 30 + gameController.rotationOffset);
        if(newRing == 0) {
            mask.gameObject.transform.localScale = new Vector3(currentScale + gameController.ringSeparation, currentScale + gameController.ringSeparation, 0);
        } else {
            mask.gameObject.transform.localScale = new Vector3(currentScale - gameController.ringWidth, currentScale - gameController.ringWidth, 0);
        }

        ring = newRing;
        location = newLocation;
    }

}
