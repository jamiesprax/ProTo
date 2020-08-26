using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Resource {

    public Type resourceType;
    public int value;

    public enum Type {
        NONE,
        WOOD,
        STONE
    }
}
