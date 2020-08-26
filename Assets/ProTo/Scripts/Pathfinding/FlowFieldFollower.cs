using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
    [SelectionBase]
    public class FlowFieldFollower : MonoBehaviour {
        public float speed;
        public bool shouldMove;
        [Range(0.001f, 1)]
        public float moveLerp;


        private Pathfinder pathfinder;
        private FlowField field;
        private Camera cam;
        private Cell<FlowFieldData> currCell;
        private Cell<FlowFieldData> targetCell;
        private Vector3 targetPoint;
        private Vector3 velocity;
        private void Awake() {
            pathfinder = FindObjectOfType<Pathfinder>();
            cam = Camera.main;
        }

        private void Update() {

            if (Input.GetMouseButtonDown(0)) {
                print("clicked");
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"))) {
                    targetPoint = hit.point;
                    StartCoroutine("UpdateFieldToTarget", hit.point);
                }
            }

            if (field != null) {
                if (currCell == targetCell) {
                    field = null;
                    return;
                }

                Cell<FlowFieldData> cell = pathfinder.gridManager.FindCellFromPosition(transform.position);
                if (cell != currCell) {
                    Vector3 newVel = Vector3.Scale(cell.data.flowDirection, FlowFieldData.DirMask) * (speed * 1 - cell.data.difficulty);
                    velocity = newVel;
                    currCell = cell;
                }

                Vector3 newPos = Vector3.Lerp(transform.position, transform.position + (velocity * Time.deltaTime), moveLerp);
                newPos.y = currCell.data.height;
                transform.position = newPos;
            } else {
                if (targetPoint != null) {
                    // transform.position = Vector3.Lerp(transform.position, targetPoint, moveLerp);
                }
            }
        }


        public IEnumerator UpdateFieldToTarget(Vector3 target) {
            field = new FlowField(pathfinder.gridManager, target);
            yield return null;
        }

        private void OnDrawGizmos() {
            if (field != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(field.targetPos, 0.2f);
            }
        }
    }
}
