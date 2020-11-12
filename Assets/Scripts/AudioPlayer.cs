using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioPlayer : MonoBehaviour
{

    public GameController gameController;
    public AudioSource click;

    public static UnityEvent clickEvent;

    void Awake()
    {
        if(clickEvent == null) { clickEvent = new UnityEvent(); }
        clickEvent.AddListener(() => StartCoroutine(PlayClick()));
    }

    /// <summary>
    /// Plays a clicking sound. Used when a move is made or when the player changes between
    /// spin and shift modes.
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayClick() {
        click.Play();
        yield return null;
    }
}
