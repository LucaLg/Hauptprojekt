using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    public void newGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    public void loadGame()
    {
        StaticData.load = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
