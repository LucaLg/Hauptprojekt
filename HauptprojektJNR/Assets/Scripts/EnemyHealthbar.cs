using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class EnemyHealthbar : MonoBehaviour
{
    private Slider Healthbar;
    private Color LowHealth;
    private Color HighHealth;
    private Vector3 Offset = new Vector3(0.3f,0.5f,0);
   
    void Start()
    {
        Healthbar = GetComponentInChildren<Slider>();
        LowHealth = Color.red;
        HighHealth = Color.green;
        
    }

    
    void Update()
    {
        //Healthbar Positioning
        Healthbar.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
    }
    [PunRPC]
    public void SetHealth(float health,float maxHealth)
    {
        Healthbar.gameObject.SetActive(health < maxHealth);
        Healthbar.value = health;
        Healthbar.maxValue = maxHealth;
        Healthbar.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(LowHealth, HighHealth, Healthbar.normalizedValue);
    }
}
