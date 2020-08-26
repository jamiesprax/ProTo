using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NavGrid;

public class Moveable : MonoBehaviour {

    public float speed;
    public Vector3 targetPos;

    public bool isCommanded;

    private Node[,] nodePath;

    // Components
    private NavGrid navGrid;

    public void Init(float speed, Vector3 targetPos) {
        this.speed = speed;
        this.targetPos = targetPos;
    }

    private void Start() {
        navGrid = FindObjectOfType<NavGrid>();
    }

    private void Update() {
        //NavGrid.Node node = navGrid.PositionToIndex(transform.position);
        //Debug.DrawLine(node.position, node.position + Vector3.up * 5, Color.cyan);

    }
    public void SetTarget(Vector3 pos) {
        targetPos = pos;
    }

    public void MoveTowardsTarget() {
        if (nodePath == null) {
            nodePath = navGrid.CloneNavGrid();
        }

        Debug.DrawLine(transform.position, targetPos, Color.blue);

        Vector3 dir = (targetPos - transform.position).normalized;
        //Vector3 dir2 = navGrid.PositionToIndex(transform.position).direction;

        //transform.position = transform.position += dir2 * speed * Time.deltaTime;
    }
    public bool AtTarget() {

        return InRangeOf(targetPos);
    }


    public bool InRangeOf(Vector3 position) {
        return Vector3.Distance(transform.position, position) < 1f;
    }

    public bool InRangeOf(GameObject gO) {
        Collider[] cols = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider col in cols) {
            if (col.gameObject == gO) {
                return true;
            }
        }
        return false;
    }

    public static void Gatherer(ref Moveable mov) {
        mov.speed = 5f;
        mov.targetPos = mov.gameObject.transform.position;
    }

    public static void Scout(ref Moveable mov) {
        mov.speed = 10f;
        mov.targetPos = mov.gameObject.transform.position;
    }
}
