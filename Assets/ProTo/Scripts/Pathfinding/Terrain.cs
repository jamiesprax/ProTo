using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pathfinding {

    [RequireComponent(typeof(Collider))]
    public class Terrain : MonoBehaviour {
        [Range(0, 1)]
        public float difficulty = 0;
    }
}
