using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int level, skillPoints, attributePoints, lifeDrainLevel, manaDrainLevel, rageLevel, healLevel;
    public float health, maxHealth, mana, maxMana, stamina, maxStamina, XP;
    public Vector3 currentCheckpoint;
    public GameData otherPlayerData;

    public GameData(int pLevel, int pSkillPoints, int pAttributePoints, int pLifeDrainLevel, int pManaDrainLevel, int pRageLevel, int pHealLevel, float pHealth, float pMaxHealth,
        float pMana, float pMaxMana, float pStamina, float pMaxStamina, float pXP, Vector3 pCurrentCheckpoint, GameData pOtherPlayerData)
    {
        level = pLevel;
        skillPoints = pSkillPoints;
        attributePoints = pAttributePoints;
        lifeDrainLevel = pLifeDrainLevel;
        manaDrainLevel = pManaDrainLevel;
        rageLevel = pRageLevel;
        healLevel = pHealLevel;
        health = pHealth;
        maxHealth = pMaxHealth;
        mana = pMana;
        maxMana = pMaxMana;
        stamina = pMaxStamina;
        XP = pXP;
        currentCheckpoint = pCurrentCheckpoint;
        otherPlayerData = pOtherPlayerData;
    }
    public GameData(int pLevel, int pSkillPoints, int pAttributePoints, int pLifeDrainLevel, int pManaDrainLevel, int pRageLevel, int pHealLevel, float pHealth, float pMaxHealth,
        float pMana, float pMaxMana, float pStamina, float pMaxStamina, float pXP, Vector3 pCurrentCheckpoint)
    {
        level = pLevel;
        skillPoints = pSkillPoints;
        attributePoints = pAttributePoints;
        lifeDrainLevel = pLifeDrainLevel;
        manaDrainLevel = pManaDrainLevel;
        rageLevel = pRageLevel;
        healLevel = pHealLevel;
        health = pHealth;
        maxHealth = pMaxHealth;
        mana = pMana;
        maxMana = pMaxMana;
        stamina = pMaxStamina;
        XP = pXP;
        currentCheckpoint = pCurrentCheckpoint;
    }


}