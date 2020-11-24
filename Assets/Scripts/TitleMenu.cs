using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{

    public Transform mainMenu;
    public Transform tutorialMenu;
    public List<Transform> submenus;

    public void PlayGame() {
        SceneManager.LoadScene("Game");
    }

    public void DisplayMainMenu() {
        mainMenu.gameObject.SetActive(true);
        tutorialMenu.gameObject.SetActive(false);
    }

    public void DisplayTutorialMenu() {
        mainMenu.gameObject.SetActive(false);
        tutorialMenu.gameObject.SetActive(true);
    }

    public void DisplayTutorialSubmenu(Transform menu) {
        foreach(Transform t in submenus) {
            if(menu == t)
                t.gameObject.SetActive(true);
            else
                t.gameObject.SetActive(false);
        }
    }

}
