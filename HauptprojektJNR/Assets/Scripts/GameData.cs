using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int level, skillPoints, attributePoints, lifeDrainLevel, manaDrainLevel, rageLevel, healLevel;
    public float health, maxHealth, mana, maxMana, stamina, maxStamina, XP, damage, attackspeed;
    public float currentCheckpointX, currentCheckpointY;
    public GameData otherPlayerData;

    public GameData(int pLevel, int pSkillPoints, int pAttributePoints, int pLifeDrainLevel, int pManaDrainLevel, int pRageLevel, int pHealLevel, float pHealth, float pMaxHealth,
        float pMana, float pMaxMana, float pMaxStamina, float pXP, Vector3 pCurrentCheckpoint, float pDamage, float pAttackspeed, GameData pOtherPlayerData)
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
        maxStamina = pMaxStamina;
        XP = pXP;
        currentCheckpointX = pCurrentCheckpoint.x;
        currentCheckpointY = pCurrentCheckpoint.y;
        damage = pDamage;
        attackspeed = pAttackspeed;
        otherPlayerData = pOtherPlayerData;
    }
    public GameData(int pLevel, int pSkillPoints, int pAttributePoints, int pLifeDrainLevel, int pManaDrainLevel, int pRageLevel, int pHealLevel, float pHealth, float pMaxHealth,
        float pMana, float pMaxMana,  float pMaxStamina, float pXP, Vector3 pCurrentCheckpoint, float pDamage, float pAttackspeed)
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
        currentCheckpointX = pCurrentCheckpoint.x;
        currentCheckpointY = pCurrentCheckpoint.y;
        damage = pDamage;
        attackspeed = pAttackspeed;
    }


}