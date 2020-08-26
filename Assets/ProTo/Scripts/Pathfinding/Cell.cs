using System;
using UnityEngine;

namespace Pathfinding {

    [System.Serializable]
    public class Cell<T> {

        public readonly CellGrid<T> grid;
        public readonly Vector2Int localCoord;
        public T data;

        private Cell() { }
        private Cell(Vector2Int localCoord, CellGrid<T> parent) {
            this.localCoord = localCoord;
            this.grid = parent;
        }
        public static Cell<T> Create(Vector2Int localCoord, CellGrid<T> parent) {
            return new Cell<T>(localCoord, parent);
        }

    }

    // A struct for cell data during an A* algorithm
    public struct AStarData {
        public static readonly Func<AStarData> Init = () => new AStarData { };
    }

    // A struct to represent a sector of a flowfield grid;
    public struct CellGridData<T> {
        public CellGrid<T> cellGrid;
        public Vector3 cellGridWorldPosition;
    }

    // A struct to represent a 'flowfield' cell with data for all points of the algorithm
    public struct FlowFieldData {
        public static readonly Func<FlowFieldData, FlowFieldData, bool> Filter = (c, n) => n.difficulty < 1 && Mathf.Abs(c.height - n.height) <= 0.2f;
        public static readonly Vector3 DirMask = new Vector3(1, 0, 1);

        public float height;
        public float difficulty;
        public float distanceFromGoal;
        public Vector3 flowDirection;
    }


}