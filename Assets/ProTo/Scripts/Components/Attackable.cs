using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attackable : MonoBehaviour {
    public float currentHealth;
    public float maxHealth;

    public Resistance[] resistances;

    public void Init(float currentHealth, float maxHealth, Resistance[] res) {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        this.resistances = res;
    }

    public void Init(float maxHealth) {
        Init(maxHealth, maxHealth, null);
    }

    public void Init(float maxHealth, params Resistance[] res) {
        Init(maxHealth, maxHealth, res);
    }

    public void Attack(Type type, float damage) {
        currentHealth -= damage * ResistanceTo(type);

        if (currentHealth <= 0) {
            Die();
        }
    }

    public void Heal(float heal) {
        currentHealth += heal;
        currentHealth = Mathf.Max(currentHealth, maxHealth);
    }

    private void Die() {
        Destroy(gameObject);
    }

    private int ResistanceIndexOf(Type type) {
        if (resistances == null) return -1;

        for (int i = 0; i < resistances.Length; i++) {
            if (resistances[i].type == type) {
                return i;
            }
        }
        return -1;
    }

    public bool IsResistantTo(Type type) {
        return ResistanceIndexOf(type) != -1;
    }

    public float ResistanceTo(Type type) {
        int index = ResistanceIndexOf(type);
        if (index == -1) return 1f;
        return resistances[index].amount;
    }



    public enum Type {
        BLUNT,
        PIERCE,
        SLASH,
        SIEGE
    }

    [Serializable]
    public struct Resistance {
        public float amount;
        public Type type;
    }

}
