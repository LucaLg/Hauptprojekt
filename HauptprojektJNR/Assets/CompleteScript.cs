using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteScript : MonoBehaviour
{
    Collider2D player1Collider;
    Collider2D player2Collider;
    // Start is called before the first frame update
    private void Update()
    {
        if(player1Collider != null && player2Collider != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CompleteScene");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" )
        {
            if (player1Collider == null) { 
             player1Collider = collision;
            }
            else if( player2Collider == null && !collision.Equals(player1Collider))
            {
                player2Collider = collision;
            }
        }
    }
}
