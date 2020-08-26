using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Pathfinding {
    public class GridManager<T> {

        private Vector2Int gridCount = new Vector2Int(1, 1);
        private Vector2Int cellCount;
        public readonly Vector3 navWorldOrigin;
        private float cellSpacing;
        private float gridSpacingX;
        private float gridSpacingY;

        private CellGrid<CellGridData<T>> gridCells;
        private Cell<T>[] cellCache;
        private int hash;

        private GridManager() { }

        private GridManager(Vector2Int gridCount, Vector2Int cellCount, float cellSpacing, Vector3 positionOrigin) {
            this.gridCount = gridCount;
            this.cellCount = cellCount;
            this.cellSpacing = cellSpacing;

            navWorldOrigin = positionOrigin;
            gridSpacingX = cellCount.x * cellSpacing;
            gridSpacingY = cellCount.y * cellSpacing;

            InitCellGrids();
            PopulateAllCellsCache();
        }

        public static GridManager<T> Create(Vector2Int gridCount, Vector2Int cellCount, float cellSpacing, Vector3 positionOrigin) {
            return new GridManager<T>(gridCount, cellCount, cellSpacing, positionOrigin);
        }

        #region Init

        private void InitCellGrids() {
            gridCells = CellGrid<CellGridData<T>>.Create(gridCount.x, gridCount.y);

            for (int x = 0; x < gridCount.x; x++) {
                for (int y = 0; y < gridCount.y; y++) {
                    var gridCell = gridCells.GetCell(x, y);

                    var subGrid = CellGrid<T>.Create(cellCount.x, cellCount.y, gridCell);
                    var gridData = new CellGridData<T>() {
                        cellGrid = subGrid,
                        cellGridWorldPosition = navWorldOrigin + new Vector3(x * gridSpacingX, 0, y * gridSpacingY)
                    };

                    gridCell.data = gridData;
                }
            }
            Debug.Log(gridCells.GetCells().Length + " grids created");
        }

        private void PopulateAllCellsCache() {
            var cells = new List<Cell<T>>(gridCount.sqrMagnitude * cellCount.sqrMagnitude);
            for (int x = 0; x < gridCount.x; x++) {
                for (int y = 0; y < gridCount.y; y++) {
                    cells.AddRange(gridCells.GetCell(x, y).data.cellGrid.GetCells());
                }
            }
            cellCache = cells.ToArray();
            hash = cellCache.GetHashCode();

        }

        #endregion

        #region Cells
        public Cell<T>[] GetAllCells() {
            if (cellCache == null) return new Cell<T>[0];

            if (cellCache.GetHashCode() != hash) {
                PopulateAllCellsCache();
            }
            return cellCache;
        }

        public Cell<CellGridData<T>>[] GetAllGridCells() {
            return gridCells.GetCells();
        }

        public Cell<T>[] GetFilteredMooreNeighbours(Cell<T> current, Func<T, T, bool> filter) {
            var allNbrs = GetGridAwareMooreNeighbours(current);
            var result = new Cell<T>[8];

            for (int i = 0; i < allNbrs.Length; i++) {
                if (allNbrs[i] != null && filter(current.data, allNbrs[i].data)) {
                    result[i] = allNbrs[i];
                }
            }

            return result;
        }

        public Cell<T>[] GetFilteredNeumannNeighbours(Cell<T> current, Func<T, T, bool> filter) {
            var allNbrs = GetGridAwareNeumannNeighbours(current);
            var result = new Cell<T>[4];

            for (int i = 0; i < allNbrs.Length; i++) {
                if (allNbrs[i] != null && filter(current.data, allNbrs[i].data)) {
                    result[i] = allNbrs[i];
                }
            }

            return result;
        }

        #region Cell lookups

        public Vector3 WorldPos(Cell<T> cell) {
            if (cell.grid.parent == null) {
                return navWorldOrigin + new Vector3(
                    cell.localCoord.x * cellSpacing,
                    0,
                    cell.localCoord.y * cellSpacing
                );
            }

            return navWorldOrigin + new Vector3(
                cell.grid.parent.localCoord.x * gridSpacingX + cell.localCoord.x * cellSpacing,
                0,
                cell.grid.parent.localCoord.y * gridSpacingY + cell.localCoord.y * cellSpacing
            );
        }

        public Vector2Int WorldCoord(Cell<T> cell) {
            if (cell.grid.parent == null) {
                return cell.localCoord;
            }

            return new Vector2Int(
                cell.grid.parent.localCoord.x * gridCount.x + cell.localCoord.x,
                cell.grid.parent.localCoord.y * gridCount.y + cell.localCoord.y
            );
        }

        public Cell<T>[] GetGridCells(Vector2Int gridCoord) {
            return gridCells.GetCell(gridCoord).data.cellGrid.GetCells();
        }

        public Cell<T>[] GetGridCells(int x, int y) {
            return gridCells.GetCell(x, y).data.cellGrid.GetCells();
        }

        public Cell<T> FindCellFromPosition(Vector3 pos) {
            var grid = FindGridFromPosition(pos);



            int cellX = PosOverlap(grid.cellGridWorldPosition.x, pos.x, cellSpacing);
            int cellY = PosOverlap(grid.cellGridWorldPosition.z, pos.z, cellSpacing);

            Cell<T> cell = grid.cellGrid.GetCell(cellX, cellY);

            Debug.DrawLine(grid.cellGridWorldPosition, WorldPos(cell), Color.yellow, 3f);

            return cell;
        }
        public CellGridData<T> FindGridFromPosition(Vector3 pos) {

            int gridX = PosOverlap(navWorldOrigin.x, pos.x, gridSpacingX);
            int gridY = PosOverlap(navWorldOrigin.z, pos.z, gridSpacingY);

            return gridCells.GetCell(gridX, gridY).data;
        }


        #endregion

        #endregion

        #region Internal
        private Cell<T>[] GetGridAwareNeumannNeighbours(Cell<T> currCell) {
            int cellLocalX = currCell.localCoord.x;
            int cellLocalY = currCell.localCoord.y;

            var currGrid = currCell.grid.parent;

            var result = currGrid.data.cellGrid.GetNeumannNeighbours(currCell);

            // If it's not on an edge, don't worry about crossings
            if (cellLocalX > 0 && cellLocalY > 0 && cellLocalX < cellCount.x - 1 && cellLocalY < cellCount.y - 1) {
                return result;
            }

            // Fill in blanks if possible from grid neighbours

            int gridX = currGrid.localCoord.x;
            int gridY = currGrid.localCoord.y;

            var gridNbrs = gridCells.GetNeumannNeighbours(currGrid);

            // North, get north grids south cell
            if (cellLocalY == cellCount.y - 1) { // if on northern row of cells
                if (gridY < gridCount.y - 1) { // if this grid is not the most northern grid
                    result[0] = gridNbrs[0].data.cellGrid.GetCell(cellLocalX, 0); // add the northern grids southern cell as this cells northern neighbour
                }
            }

            // East, get east grids west cell
            if (cellLocalX == cellCount.x - 1) {
                if (gridX < gridCount.x - 1) {
                    result[1] = gridNbrs[1].data.cellGrid.GetCell(0, cellLocalY);
                }
            }

            // South, get south grids north cell
            if (cellLocalY == 0) {
                if (gridY > 0) {
                    result[2] = gridNbrs[2].data.cellGrid.GetCell(cellLocalX, cellCount.y - 1);
                }
            }

            // West, get west grids east cell
            if (cellLocalX == 0) {
                if (gridX > 0) {
                    result[3] = gridNbrs[3].data.cellGrid.GetCell(cellCount.x - 1, cellLocalY);
                }
            }

            return result;
        }

        private Cell<T>[] GetGridAwareMooreNeighbours(Cell<T> currCell) {
            int cellLocalX = currCell.localCoord.x;
            int cellLocalY = currCell.localCoord.y;

            var currGrid = currCell.grid.parent;

            var result = currGrid.data.cellGrid.GetMooreNeighbours(currCell);

            // If it's not on an edge, don't worry about crossings
            if (cellLocalX > 0 && cellLocalY > 0 && cellLocalX < cellCount.x - 1 && cellLocalY < cellCount.y - 1) {
                return result;
            }

            // Fill in blanks if possible from grid neighbours

            int gridX = currGrid.localCoord.x;
            int gridY = currGrid.localCoord.y;

            var gridNbrs = gridCells.GetMooreNeighbours(currGrid);

            /*
            var gI = new int[] {
                gridX * gridCount.y + (gridY + 1),        // N  0
                (gridX + 1) * gridCount.y + gridY,        // E  1
                gridX * gridCount.y + (gridY - 1),        // S  2
                (gridX - 1) * gridCount.y + gridY,        // W  3
                (gridX - 1) * gridCount.y + (gridY + 1),  // NW 4
                (gridX + 1) * gridCount.y + (gridY + 1),  // NE 5
                (gridX - 1) * gridCount.y + (gridY - 1),  // SW 6
                (gridX + 1) * gridCount.y + (gridY - 1)   // SE 7                
            };
            */

            // North, get north grids south cell
            if (cellLocalY == cellCount.y - 1) { // if on northern row of cells
                if (gridY < gridCount.y - 1) { // if this grid is not the most northern grid
                    result[0] = gridNbrs[0].data.cellGrid.GetCell(cellLocalX, 0); // add the northern grids southern cell as this cells northern neighbour
                }
            }

            // East, get east grids west cell
            if (cellLocalX == cellCount.x - 1) {
                if (gridX < gridCount.x - 1) {
                    result[1] = gridNbrs[1].data.cellGrid.GetCell(0, cellLocalY);
                }
            }

            // South, get south grids north cell
            if (cellLocalY == 0) {
                if (gridY > 0) {
                    result[2] = gridNbrs[2].data.cellGrid.GetCell(cellLocalX, cellCount.y - 1);
                }
            }

            // West, get west grids east cell
            if (cellLocalX == 0) {
                if (gridX > 0) {
                    result[3] = gridNbrs[3].data.cellGrid.GetCell(cellCount.x - 1, cellLocalY);
                }
            }

            // North-West
            if (result[4] == null) {
                //  Debug.DrawLine(currCell.worldPos, currCell.worldPos + Vector3.up, Color.white);
                if (cellLocalX != 0) { // N row not NW corner
                    if (gridY < gridCount.y) { // if we not at the top
                        result[4] = gridNbrs[0].data.cellGrid.GetCell(cellLocalX - 1, 0);
                    }
                } else if (cellLocalY != cellCount.y - 1) { // W column, not NW corner
                    if (gridX != 0) {
                        result[4] = gridNbrs[3].data.cellGrid.GetCell(cellCount.x - 1, cellLocalY + 1);
                    }
                } else if (gridY < gridCount.y && gridX != 0) {

                }


                if (gridY != gridCount.y - 1) {
                    result[4] = gridNbrs[0].data.cellGrid.GetCell(cellLocalX - 1, 0);
                } else if (gridX != 0 && cellLocalY < cellCount.y - 1) {
                    result[4] = gridNbrs[3].data.cellGrid.GetCell(cellCount.x - 1, cellLocalY + 1);
                } else if (gridX != 0 && gridY != gridCount.y - 1) {
                    result[4] = gridNbrs[4].data.cellGrid.GetCell(cellCount.x - 1, 0);
                }
            }

            // North East
            if (result[5] == null) {
                if (cellLocalX != cellCount.x - 1 && gridY != gridCount.y - 1) {
                    result[5] = gridNbrs[0].data.cellGrid.GetCell(cellLocalX + 1, 0);
                } else if (gridX < gridCount.x - 1 && cellLocalY < cellCount.y - 1) {
                    result[5] = gridNbrs[1].data.cellGrid.GetCell(0, cellLocalY + 1);
                } else if (gridX != gridCount.x - 1 && gridY != gridCount.y - 1) {
                    result[5] = gridNbrs[5].data.cellGrid.GetCell(0, 0);
                }
            }

            // South-West
            if (result[6] == null) {
                if (cellLocalX != 0 && gridY != 0) {
                    result[6] = gridNbrs[2].data.cellGrid.GetCell(cellLocalX - 1, cellCount.y - 1);
                } else if (gridX != 0 && cellLocalY > 0) {
                    result[6] = gridNbrs[3].data.cellGrid.GetCell(cellCount.x - 1, cellLocalY - 1);
                } else if (gridX != 0 && gridY != 0) {
                    result[6] = gridNbrs[6].data.cellGrid.GetCell(cellCount.x - 1, cellCount.y - 1);
                }
            }

            // South East
            if (result[7] == null) {
                if (cellLocalX != cellCount.x - 1 && gridY != 0) {
                    result[7] = gridNbrs[2].data.cellGrid.GetCell(cellLocalX + 1, cellCount.y - 1);
                } else if (gridX < gridCount.x - 1 && cellLocalY > 0) {
                    result[7] = gridNbrs[1].data.cellGrid.GetCell(0, cellLocalY - 1);
                } else if (gridX != gridCount.x - 1 && gridY != 0) {
                    result[7] = gridNbrs[7].data.cellGrid.GetCell(0, cellCount.y - 1);
                }
            }

            return result;
        }

        private Cell<CellGridData<T>> GetGrid(Vector2Int coord) {
            return gridCells.GetCell(coord);
        }

        private int PosOverlap(float a, float b, float spacing) {
            if (a > b) return 0;
            int index = 0;
            while (a + (spacing * index) < b) {
                index++;
            }
            return index - 1;
        }

        #endregion
    }
}
