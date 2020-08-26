using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {

    [ExecuteAlways]
    public class Pathfinder : MonoBehaviour {

        public Vector2Int gridCount = new Vector2Int(1, 1);
        public Vector2Int cellCount = new Vector2Int(30, 30);
        public float cellSpacing = 1;

        [Range(0f, 1.5f)]
        public float heightDiffLimit;
        [Range(0, 1)]
        public float steepnessAllowance;

        public GridManager<FlowFieldData> gridManager;

        public GameObject ob;

        public bool update = false;
        public bool rebuild = true;
        private bool settingsUpdated = true;

        [Header("Gizmos")]
        public bool showGrid;
        public bool showNodes;
        public bool showPaths;
        public bool showDistanceField;
        public bool showVectorField;

        private void Start() {
            settingsUpdated = true;
        }

        private void Update() {
            if (!settingsUpdated) {
                return;
            }

            if (rebuild || gridManager == null) {
                gridManager = GridManager<FlowFieldData>.Create(gridCount, cellCount, cellSpacing, transform.position);
                UpdateAllCells();
                rebuild = false;
            }

            /*

            if (update) {
                UpdateAllGrids();
                update = false;
            }

            */

            settingsUpdated = false;
        }

        public void UpdateGrid(Vector3 position) {
            foreach (Cell<FlowFieldData> cell in gridManager.FindGridFromPosition(position).cellGrid.GetCells()) {
                UpdateCell(cell);
            }
        }

        public void UpdateAllCells() {
            for (int x = 0; x < gridCount.x; x++) {
                for (int y = 0; y < gridCount.x; y++) {
                    foreach (Cell<FlowFieldData> cell in gridManager.GetGridCells(x, y)) {
                        UpdateCell(cell);
                    }
                }
            }
        }

        public void UpdateCell(Cell<FlowFieldData> cell) {
            Vector3 orig = gridManager.WorldPos(cell) + Vector3.up * 10f;
            RaycastHit hit;
            if (Physics.Raycast(orig, Vector3.down, out hit, 15f, ~LayerMask.GetMask("Unit"))) {
                Terrain ter;
                if (hit.transform.gameObject.TryGetComponent(out ter)) {
                    if (Vector3.Dot(hit.normal, Vector3.up) >= steepnessAllowance) {
                        // If we hit some terrain and it passed steepness check
                        cell.data.height = hit.point.y;
                        cell.data.difficulty = ter.difficulty;
                    } else {
                        // The angle is too steep
                        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 1f);
                        cell.data.difficulty = 1f;
                    }
                } else {
                    // Didn't hit any terrain
                    Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.red, 1f);
                    cell.data.difficulty = 1f;
                }
            } else {
                // Fell off the map? out of range?
                Debug.DrawLine(orig, orig + Vector3.down * 15f, Color.cyan, 1f);
                cell.data.difficulty = 1f;
            }
        }

        private void OnValidate() {
            settingsUpdated = true;
        }

        private void OnDrawGizmos() {
            if (settingsUpdated) return;

            foreach (Cell<FlowFieldData> cell in gridManager.GetAllCells()) {
                Vector3 currentCellWorldPos = gridManager.WorldPos(cell) + cell.data.height * Vector3.up;
                if (showGrid) {
                    float x = (float) cell.localCoord.x / cellCount.x;
                    float y = (float) cell.localCoord.y / cellCount.y;
                    Gizmos.color = Color.Lerp(Color.blue, Color.red, (x + y) / 2f);
                    Gizmos.DrawCube(currentCellWorldPos, cellSpacing * new Vector3(.5f, .5f, .5f));
                }

                if (showNodes) {
                    Gizmos.color = Color.Lerp(Color.green, Color.red, cell.data.difficulty);
                    Gizmos.DrawSphere(currentCellWorldPos, 0.2f);
                }

                if (showPaths) {
                    if (cell.data.difficulty == 1) continue;

                    Cell<FlowFieldData>[] nbrs = gridManager.GetFilteredMooreNeighbours(cell, FlowFieldData.Filter);

                    for (int i = 0; i < nbrs.Length; i++) {
                        if (nbrs[i] != null) {
                            if (i == 0 || i == 1 || i == 5 || i == 7) {
                                Gizmos.color = Color.blue;
                            } else {
                                Gizmos.color = Color.red;
                            }
                            Vector3 dir = currentCellWorldPos - gridManager.WorldPos(nbrs[i]);
                            dir.y = 0;
                            Gizmos.DrawLine(currentCellWorldPos, currentCellWorldPos - dir * 0.4f);
                        }
                    }
                }

                if (showDistanceField) {
                    if (cell.data.distanceFromGoal == 0 || cell.data.difficulty == 1) continue;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(gridManager.WorldPos(cell) + Vector3.up * (cell.data.distanceFromGoal * 0.1f), 0.2f);
                }

                if (showVectorField) {
                    if (cell.data.flowDirection == Vector3.zero || cell.data.difficulty == 1) continue;
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(currentCellWorldPos, 0.15f);
                    Gizmos.DrawLine(currentCellWorldPos, currentCellWorldPos + cell.data.flowDirection);
                }
            }

        }
    }
}
