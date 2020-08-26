using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Attackable;
using static ResourceStorage;

public class Gatherer : MonoBehaviour {

    // Optional setting
    public ResourceStorage locatedResource;
    public ResourceStorage locatedStorage;

    // Gatherer Specific
    private State state;
    public Resource.Type resourceType;
    public float harvestSpeed;
    public float deliverSpeed;

    private float harvestTime;
    private float deliverTime;
    private bool deliveryComplete;
    private bool commanded;
    public bool autonamous;
    // Components
    private Moveable moveable;
    private ResourceStorage inventory;
    private Attackable attackable;
    void Start() {
        harvestTime = harvestSpeed;
        deliverTime = deliverSpeed;
        if (!TryGetComponent(out inventory)) InitInventory();
        if (!TryGetComponent(out moveable)) InitMovable();
        if (!TryGetComponent(out attackable)) InitAttackable();
    }

    void Update() {
        if (locatedResource == null || !locatedResource.CanStore(resourceType)) locatedResource = FindClosestResource();
        if (locatedStorage == null) locatedStorage = FindClosestStorage();

        if (state == State.IDLE) DetermineState();

        switch (state) {
            case State.COMMANDED:
                CommandedActions();
                break;
            case State.TRAVELLING:
                MoveTowardsTarget();
                break;
            case State.HARVESTING:
                HarvestResource();
                break;
            case State.DELIVERING:
                DeliverResource();
                break;
        }
    }

    void DetermineState() {

        if (!deliveryComplete) {
            state = State.DELIVERING;
            return;
        }

        // If gather cannot store resource
        if (!inventory.CanStore(resourceType)) {
            if (locatedStorage == null) {
                state = State.SEARCHING_DELIVERY;
            } else {
                if (moveable.InRangeOf(locatedStorage.gameObject)) {
                    // If we are in range of the storage place
                    state = State.DELIVERING;
                    deliveryComplete = false;
                } else {
                    state = State.TRAVELLING;
                    locatedStorage = FindClosestStorage();
                    moveable.SetTarget(locatedStorage.transform.position);
                }
            }
        } else {
            // If we have space to store resources
            if (locatedResource == null) {
                state = State.SEARCHING_RESOURCE;
            } else {
                if (moveable.InRangeOf(locatedResource.gameObject)) {
                    // If we are in range of the resource we want
                    state = State.HARVESTING;
                } else {
                    // Otherwise, set the target to the resource
                    state = State.TRAVELLING;
                    moveable.SetTarget(locatedResource.transform.position);
                }
            }
        }
    }

    void CommandedActions() {
        if (!moveable.isCommanded) state = State.IDLE;
    }

    void MoveTowardsTarget() {
        moveable.MoveTowardsTarget();
        if (moveable.AtTarget()) {
            state = State.IDLE;
        }
    }

    void HarvestResource() {
        harvestTime -= Time.deltaTime;
        if (harvestTime <= 0) {
            if (locatedResource.MoveResourceTo(resourceType, 1, ref inventory) == 0) {
                state = State.IDLE;
            }
            harvestTime = harvestSpeed;
            state = State.IDLE;
        }
    }

    void DeliverResource() {
        deliverTime -= Time.deltaTime;
        if (deliverTime <= 0) {
            if (locatedStorage.AddResourceFrom(resourceType, 1, ref inventory) == 0) {
                state = State.IDLE;
            }

            deliverTime = deliverSpeed;
        }
    }

    ResourceStorage FindClosestResource() {
        ResourceStorage[] storages = FindObjectsOfType<ResourceStorage>();
        float shortestDistance = float.MaxValue;
        ResourceStorage closestStorage = null;
        foreach (ResourceStorage rs in storages) {
            if (rs.isPersonal) continue;
            if (!rs.CanStore(resourceType)) continue;
            if (!rs.isResource) continue;

            float dist = Vector3.Distance(transform.position, rs.transform.position);
            if (dist < shortestDistance) {
                closestStorage = rs;
                shortestDistance = dist;
            }
        }
        return closestStorage;
    }

    ResourceStorage FindClosestStorage() {
        ResourceStorage[] storages = FindObjectsOfType<ResourceStorage>();
        float shortestDistance = float.MaxValue;
        ResourceStorage closestStorage = null;
        foreach (ResourceStorage rs in storages) {
            if (rs.isPersonal) continue;
            if (rs.isResource) continue;
            if (!rs.CanStore(resourceType)) continue;

            float dist = Vector3.Distance(transform.position, rs.transform.position);
            if (dist < shortestDistance) {
                closestStorage = rs;
                shortestDistance = dist;
            }
        }
        return closestStorage;
    }

    void InitInventory() {
        inventory = gameObject.AddComponent<ResourceStorage>();

        Store[] st = new Store[2];
        st[0] = new Store() { maxAmount = 10, type = Resource.Type.WOOD };
        st[1] = new Store() { maxAmount = 10, type = Resource.Type.STONE };

        inventory.Init(st, true);
    }

    void InitMovable() {
        moveable = gameObject.AddComponent<Moveable>();

        moveable.Init(5f, transform.position);
    }

    void InitAttackable() {
        attackable = gameObject.AddComponent<Attackable>();

        Resistance[] res = new Resistance[1];
        res[0] = new Resistance() { type = Type.SIEGE, amount = 0.9f };

        attackable.Init(15f, res);
    }

    public enum State {
        IDLE,
        COMMANDED,
        DELIVERING,
        HARVESTING,
        TRAVELLING,
        SEARCHING_DELIVERY,
        SEARCHING_RESOURCE
    }
}
