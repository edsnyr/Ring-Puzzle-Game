﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{


    public GameController gameController;

    public SpriteRenderer sr;

    public int ring;
    public int location;
    public float radiusOffset;

    private void Start() {
        GameController.spinEvent.AddListener(checkSpin);
        GameController.shiftEvent.AddListener(checkShift);
        PlacePiece(ring, location);
    }

    /// <summary>
    /// Spins the Piece if it is in the chosen ring.
    /// </summary>
    /// <param name="row">The chosen ring, between 1 and numberOfRings.</param>
    /// <param name="dir">Direction of the spin. True is clockwise.</param>
    /// <param name="time">Duration of the spin in seconds.</param>
    private void checkSpin(int row, bool dir, float time, int reps) {
        if(row == ring) {
            StartCoroutine(spinPiece(dir, time, reps));
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
            StartCoroutine(shiftPiece(dir, time, reps));
        }
    }

    /// <summary>
    /// Applied to each Piece individually. Rotates the Piece around the origin
    /// in the given direction by modifying its local euler angles.
    /// </summary>
    /// <param name="dir">Rotation of the spin. True is clockwise.</param>
    /// <param name="time">Duration of the spin in seconds.</param>
    /// <returns></returns>
    private IEnumerator spinPiece(bool dir, float time, int reps) {
        for(int i = reps; i > 0; i--) {
            location += dir ? -1 : 1;
            if(location == -1 || location == 12) {
                location = (location + 12) % 12; //locations are 0-11;
                sr.transform.localPosition *= -1; //position is flipped on second half of ring, makes position consistent at each possible location
                transform.localEulerAngles += new Vector3(0, 0, 180); //flipped again to keep original position
            }
            if((location == 6 && !dir) || (location == 5 && dir)) {
                sr.transform.localPosition *= -1; //flip position for consistent location/position on second half of ring
                transform.localEulerAngles += new Vector3(0, 0, 180); //flip again to keep original position
                
            }
            float start = transform.localEulerAngles.z;
            float target = 30 * (dir ? -1 : 1);
            float timeElapsed = 0;
            while(timeElapsed < time) { //smooth rotation
                timeElapsed += Time.deltaTime;
                float angle = start + (timeElapsed / time * target);
                transform.localEulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }
            transform.localEulerAngles = new Vector3(0, 0, start + target); //ensure the final angle is accurate
            yield return new WaitForSeconds(gameController.waitTime);
        }
    }

    

    /// <summary>
    /// Applied to each Piece individually. Slides the Piece toward or away from the center
    /// in the given direction.
    /// </summary>
    /// <param name="newDir">Direction of the shift. True shifts up for locations 0-5, down for 6-11.</param>
    /// <param name="time">Duration of the shift in seconds.</param>
    /// <returns></returns>
    private IEnumerator shiftPiece(bool dir, float time, int reps) {
        for(int i = reps; i > 0; i--) {
            bool newDir = dir; //save original direction for multiple reps
            if((location / 6) % 2 == 0) { //since direction is based on the angle from the origin, second half is flipped to move all in same direction
                newDir = !newDir;
            }
            ring += (newDir ? -1 : 1);
            if(ring == 0) { //moving to opposite end of ring 1
                ring = 1;
                location = (location + 6) % 12;
                StartCoroutine(JumpAnimation(gameController.smallJumpScale, gameController.jumpHeight, time));
            }
            if(ring == gameController.numberOfRings + 1) { //moving to the opposite end of ring 4
                ring = gameController.numberOfRings;
                location = (location + 6) % 12;
                StartCoroutine(JumpAnimation(gameController.bigJumpScale, gameController.jumpHeight, time));
            }

            float startLocation = sr.transform.localPosition.y;
            float targetLocation = GetRadius();
            float shiftDistance = targetLocation - startLocation; //total distance from start to destination
            float timeElapsed = 0;
            while(timeElapsed < time) { //smooth slide
                timeElapsed += Time.deltaTime;
                float newLocation = startLocation + shiftDistance * (timeElapsed / time);
                sr.transform.localPosition = new Vector3(0, newLocation, 0);
                yield return null;
            }

            sr.transform.localPosition = new Vector3(0, startLocation + shiftDistance, 0); //ensure the final destination is accurate
            yield return new WaitForSeconds(gameController.waitTime);
        }
    }

    private IEnumerator JumpAnimation(float addedScale, float jumpHeight, float time) {
        float startScale = sr.transform.localScale.x;
        float maxScale = startScale + addedScale;
        float startHeight = transform.position.y;
        float maxHeight = startHeight + jumpHeight;
        float timeElapsed = 0;
        while(timeElapsed < time / 2f) {
            timeElapsed += Time.deltaTime;
            float newScale = startScale + (timeElapsed / (time / 2f) * addedScale);
            float newHeight = Mathf.SmoothStep(startHeight, startHeight + jumpHeight, timeElapsed / (time / 2f));
            //float newHeight = startHeight + (timeElapsed / (time / 2f) * jumpHeight);
            sr.transform.localScale = new Vector3(newScale, newScale, 1);
            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
            yield return null;
        }
        timeElapsed = 0;
        sr.transform.localScale = new Vector3(maxScale, maxScale, 1);
        transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
        while(timeElapsed < time / 2f) {
            timeElapsed += Time.deltaTime;
            float newScale = maxScale - (timeElapsed / (time / 2f) * addedScale);
            float newHeight = Mathf.SmoothStep(startHeight + jumpHeight, startHeight, timeElapsed / (time / 2f));
            //float newHeight = maxHeight - (timeElapsed / (time / 2f) * jumpHeight);
            sr.transform.localScale = new Vector3(newScale, newScale, 1);
            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
            yield return null;
        }
        sr.transform.localScale = new Vector3(startScale, startScale, 1);
        transform.position = new Vector3(transform.position.x, startHeight, transform.position.z);
    }

    /// <summary>
    /// Gets the distance of the pieces intended position to the origin.
    /// </summary>
    /// <returns></returns>
    private float GetRadius() {
        float unitScale = (gameController.ringSegmentSizeInPixels / gameController.ringSegmentPixelsPerUnit / 2);
        return (unitScale * gameController.initialRingScale + unitScale * (gameController.ringWidth + gameController.ringSeparation) * (ring - 1) + radiusOffset) * (location < 6 ? 1 : -1);
    }

    /// <summary>
    /// Instantly moves the Piece to a specified ring and location.
    /// </summary>
    /// <param name="newRing"></param>
    /// <param name="newLocation"></param>
    public void PlacePiece(int newRing, int newLocation) {
        ring = newRing;
        location = newLocation;
        float radius = GetRadius();
        float angle = ((location * 30 + gameController.rotationOffset - 15) % 180);
        sr.transform.localPosition = new Vector3(0, radius, 0);
        transform.localEulerAngles = new Vector3(0, 0, angle);
    }
}