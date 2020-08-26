using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pathfinding {

    [System.Serializable]
    public class CellGrid<T> {
        public readonly int width, length;
        public readonly Cell<CellGridData<T>> parent;

        private Cell<T>[] cells;

        private CellGrid() { }

        private CellGrid(int width, int length, Cell<CellGridData<T>> parent) {
            this.width = width;
            this.length = length;
            this.parent = parent;

            InitCells();
        }

        public static CellGrid<T> Create(int width, int length, Cell<CellGridData<T>> parent) {
            return new CellGrid<T>(width, length, parent);
        }

        public static CellGrid<T> Create(int width, int length) {
            return new CellGrid<T>(width, length, null);
        }

        public void InitCells() {
            cells = new Cell<T>[width * length];

            for (int w = 0; w < width; w++) {
                for (int l = 0; l < length; l++) {
                    int index = w * length + l;
                    var cell = Cell<T>.Create(new Vector2Int(w, l), this);

                    cells[index] = cell;
                }
            }
        }

        public void SetDataAtIndex(int index, T data) {
            cells[index].data = data;
        }

        public Cell<T>[] GetNeumannNeighbours(Cell<T> current) {
            Cell<T>[] result = new Cell<T>[4];

            int x = current.localCoord.x;
            int y = current.localCoord.y;

            var indices = new int[]            {
                x * length + (y + 1),
                (x + 1) * length + y,
                x * length + (y - 1),
                (x - 1) * length + y
            };


            // North
            if (y < length - 1)
                result[0] = cells[indices[0]];

            // East
            if (x < width - 1)
                result[1] = cells[indices[1]];

            // South
            if (y > 0)
                result[2] = cells[indices[2]];

            // West
            if (x > 0)
                result[3] = cells[indices[3]];

            return result;
        }

        public Cell<T>[] GetMooreNeighbours(Cell<T> current) {
            var result = new Cell<T>[8];

            int x = current.localCoord.x;
            int y = current.localCoord.y;

            var indices = new int[] {
                x * length + (y + 1),
                (x + 1) * length + y,
                x * length + (y - 1),
                (x - 1) * length + y,
                (x - 1) * length + (y + 1),
                (x + 1) * length + (y + 1),
                (x - 1) * length + (y - 1),
                (x + 1) * length + (y - 1)
            };


            // North
            if (y < length - 1)
                result[0] = cells[indices[0]];

            // East
            if (x < width - 1)
                result[1] = cells[indices[1]];

            // South
            if (y > 0)
                result[2] = cells[indices[2]];

            // West
            if (x > 0)
                result[3] = cells[indices[3]];


            // North-West
            if (y < length - 1 && x > 0)
                result[4] = cells[indices[4]];

            // North-East
            if (y < length - 1 && x < width - 1)
                result[5] = cells[indices[5]];

            // South-West
            if (y > 0 && x > 0)
                result[6] = cells[indices[6]];

            // South-East
            if (y > 0 && x < width - 1)
                result[7] = cells[indices[7]];

            return result;
        }

        public Cell<T> GetCell(int x, int y) {
            return cells[x * length + y];
        }

        public Cell<T> GetCell(Vector2Int pos) {
            return cells[pos.x * length + pos.y];
        }

        public Cell<T> GetCell(int index) {
            return cells[index];
        }

        public Cell<T>[] GetCells() {
            return cells;
        }
    }
}