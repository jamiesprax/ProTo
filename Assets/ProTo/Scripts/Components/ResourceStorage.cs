using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStorage : MonoBehaviour {

    public bool isPersonal;
    public bool isResource;
    [SerializeField]
    private Store[] stores;

    public void Init(Store[] stores, bool isPersonal) {
        this.stores = stores;
        this.isPersonal = isPersonal;
    }

    public int MoveResourceTo(Resource.Type type, int value, ref ResourceStorage other) {
        if (Stores(type) && other.Stores(type)) {
            Store st = stores[StoreIndexOf(type)];
            int amount = Mathf.Min(st.currAmount, value);

            Store stO = other.stores[other.StoreIndexOf(type)];
            int space = stO.maxAmount - stO.currAmount;


            st.currAmount -= Mathf.Min(space, amount);
            stO.currAmount += Mathf.Min(space, amount);


            stores[StoreIndexOf(type)] = st;
            other.stores[other.StoreIndexOf(type)] = stO;
        }

        return 0;
    }
    public int AddResourceFrom(Resource.Type type, int value, ref ResourceStorage other) {
        if (other.CanStore(type)) {
            stores[StoreIndexOf(type)].currAmount += value;
            return -value;
        }

        return 0;
    }

    private int StoreIndexOf(Resource.Type type) {
        for (int i = 0; i < stores.Length; i++) {
            if (stores[i].type == type) {
                return i;
            }
        }
        return -1;
    }

    public bool Stores(Resource.Type type) {
        return StoreIndexOf(type) != -1;
    }

    public Resource.Type[] AllowedTypes() {
        Resource.Type[] types = new Resource.Type[stores.Length];
        for (int i = 0; i < types.Length; i++) {
            types[i] = stores[i].type;
        }
        return types;
    }

    public bool CanStore(Resource.Type type) {
        int index = StoreIndexOf(type);
        if (index == -1) return false;
        return stores[index].currAmount < stores[index].maxAmount;
    }

    [Serializable]
    public struct Store {
        public int currAmount;
        public int maxAmount;
        public Resource.Type type;
    }
}
