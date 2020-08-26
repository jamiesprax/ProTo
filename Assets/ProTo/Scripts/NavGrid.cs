using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[ExecuteInEditMode]
public class NavGrid : MonoBehaviour {
    public bool drawGizmos = false;

    [Range(1, 100)]
    public float gridSize;
    [Range(2, 100)]
    public int cells;

    private Node[,] nodeGrid;
    public bool settingsUpdated;
    private float div;
    private float halfDiv;

    private void Awake() {
        settingsUpdated = true;
        div = gridSize / cells;
        halfDiv = div / 2f;
    }

    private void OnValidate() {
        settingsUpdated = true;
    }

    void Init() {
        nodeGrid = new Node[cells, cells];

        for (int x = 0; x < cells; x++) {
            for (int y = 0; y < cells; y++) {
                Node node = new Node();
                Vector3 orig = transform.position + new Vector3(x * div, 10f, y * div);
                RaycastHit hit;
                if (Physics.Raycast(orig, Vector3.down, out hit, 15f, 1 << 9)) {
                    node.worldPosition = hit.point;

                    Collider[] cols = Physics.OverlapSphere(hit.point, halfDiv, ~(3 << 9)); // 11 shifted left for layers 9&10
                    if (cols.Length > 0) {
                        node.impassable = true;
                    } else {
                        node.impassable = false;
                    }

                }
                nodeGrid[x, y] = node;
            }
        }
        settingsUpdated = false;
    }

    public Node PositionToIndex(Vector3 pos, out int iX, out int iY) {
        Vector3 gridPos = transform.position;
        float x = gridPos.x;
        iX = 0;

        while (x < pos.x) {
            x += div;
            iX++;
        }

        float y = gridPos.z;
        iY = 0;
        while (y < pos.z) {
            y += div;
            iY++;
        }

        return nodeGrid[iX, iY];
    }

    public Node[,] CloneNavGrid() {
        return nodeGrid.Clone() as Node[,];
    }

    // Update is called once per frame
    void Update() {
        if (settingsUpdated) Init();
    }

    private void OnDrawGizmos() {
        if (!drawGizmos || settingsUpdated) return;

        Vector3 size = Vector3.one * halfDiv;

        for (int x = 0; x < cells; x++) {
            for (int y = 0; y < cells; y++) {
                if (!nodeGrid[x, y].impassable) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(nodeGrid[x, y].worldPosition, 0.1f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(nodeGrid[x, y].worldPosition, nodeGrid[x, y].worldPosition + nodeGrid[x, y].direction);
                } else {
                    /* if (nodeGrid[x, y].allowedTypes != null && nodeGrid[x, y].allowedTypes.Length > 0) {
                         foreach (Resource.Type t in nodeGrid[x, y]) {

                             if (t.Equals(Resource.Type.STONE)) {
                                 Gizmos.color = Color.gray;
                                 Gizmos.DrawSphere(nodeGrid[x, y].worldPosition + new Vector3(0, 10, 0), 0.2f);
                             }

                             if (t.Equals(Resource.Type.WOOD)) {
                                 Gizmos.color = new Color(.5f, .25f, 0f);
                                 Gizmos.DrawSphere(nodeGrid[x, y].worldPosition + new Vector3(0, 11, 0), 0.2f);
                             }
                         }
                     }*/

                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(nodeGrid[x, y].worldPosition, size);
                }
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(nodeGrid[0, 0].worldPosition + Vector3.up * 2f, .2f);
    }

    public struct Node {
        public Vector3 worldPosition;
        public Vector3 direction;
        public bool impassable;
    }
}
