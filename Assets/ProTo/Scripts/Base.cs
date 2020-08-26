using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResourceStorage;

public class Base : MonoBehaviour {
    private ResourceStorage storage;
    private Attackable attackable;

    void Start() {
        if (!TryGetComponent<ResourceStorage>(out storage)) InitInventory();
        if (!TryGetComponent<Attackable>(out attackable)) InitAttackable();

    }

    void InitInventory() {
        storage = gameObject.AddComponent<ResourceStorage>();

        Store[] st = new Store[2];
        st[0] = new Store() { maxAmount = 1000, type = Resource.Type.WOOD };
        st[1] = new Store() { maxAmount = 1000, type = Resource.Type.STONE };

        storage.Init(st, false);
    }

    void InitAttackable() {
        attackable = gameObject.AddComponent<Attackable>();
    }
}
