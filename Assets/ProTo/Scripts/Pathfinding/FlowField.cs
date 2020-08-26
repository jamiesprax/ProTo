using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {

    public class FlowField {
        [SerializeField]
        public GridManager<FlowFieldData> gridManager;
        public float[] flowField;

        public Vector3 targetPos;

        private FlowField() { }

        public FlowField(GridManager<FlowFieldData> gridManager, Vector3 targetPos) {
            this.gridManager = gridManager;
            this.targetPos = targetPos;

            GenerateDistanceField();
            GenerateVectorFields();

            Debug.Log("Flowfield Creation Complete");
        }

        public Vector3 FlowDir(Vector3 pos) {
            Cell<FlowFieldData> currCell = gridManager.FindCellFromPosition(pos);
            if (currCell != null) {
                Debug.Log("found dir");
                return currCell.data.flowDirection;
            } else {
                Debug.LogWarning("no dir");
                return Vector3.zero;
            }
        }

        private void GenerateDistanceField() {
            Debug.Log("Populating distance fields");

            var markedCells = new List<Cell<FlowFieldData>>();

            var goalCell = gridManager.FindCellFromPosition(targetPos);

            if (goalCell == null) {
                Debug.LogError("No goals found !");
                return;
            }

            goalCell.data.distanceFromGoal = 0;
            markedCells.Add(goalCell);

            bool shouldRun = true;
            while (shouldRun) {
                int startCount = markedCells.Count;
                for (int i = 0; i < markedCells.Count; i++) {
                    if (markedCells[i].data.difficulty == 0)
                        continue;

                    var neighbours = gridManager.GetFilteredMooreNeighbours(markedCells[i], FlowFieldData.Filter);
                    for (int j = 0; j < 8; j++) {
                        var nbr = neighbours[j];
                        if (nbr == null || markedCells.Contains(nbr))
                            continue;

                        nbr.data.distanceFromGoal = markedCells[i].data.distanceFromGoal;
                        nbr.data.distanceFromGoal += (gridManager.WorldCoord(nbr) - gridManager.WorldCoord(markedCells[i])).magnitude;

                        markedCells.Add(nbr);
                    }
                }

                if (startCount == markedCells.Count) { //didnt add any new cells
                    shouldRun = false;
                }
            }

        }

        private void GenerateVectorFields() {
            Debug.Log("Populating vector fields");
            Cell<FlowFieldData>[] allCells = gridManager.GetAllCells();
            for (int i = 0; i < allCells.Length; i++) {
                var cur = allCells[i];

                var neighbours = gridManager.GetFilteredNeumannNeighbours(cur, FlowFieldData.Filter);

                float west, east, north, south;
                west = east = north = south = cur.data.distanceFromGoal;

                if (neighbours[0] != null && neighbours[0].data.difficulty != 1) north = neighbours[0].data.distanceFromGoal;
                if (neighbours[1] != null && neighbours[1].data.difficulty != 1) east = neighbours[1].data.distanceFromGoal;
                if (neighbours[2] != null && neighbours[2].data.difficulty != 1) south = neighbours[2].data.distanceFromGoal;
                if (neighbours[3] != null && neighbours[3].data.difficulty != 1) west = neighbours[3].data.distanceFromGoal;


                float x = west - east;
                float y = south - north;

                cur.data.flowDirection = new Vector3(x, 0, y).normalized;
            }
        }
    }
}