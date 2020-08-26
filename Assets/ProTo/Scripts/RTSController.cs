using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSController : MonoBehaviour {
    Camera cam;
    Attackable unit;



    private void Start() {
        cam = Camera.main;
    }
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            print("Trying to select unit");
            TrySelectUnit();
        }

        if (Input.GetMouseButtonDown(1)) {
            print("Trying to move unit");
            if (unit != null) TryMoveUnit();
        }
    }

    void TrySelectUnit() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Unit"))) {
            print("hit");
            unit = hit.transform.gameObject.GetComponent<Attackable>();
        }
    }

    void TryMoveUnit() {

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Terrain"))) {
            // unit.Command(Authority.TEAM_A, MoveTo(targetPos));
        }
    }

    public enum Selected {
        UNIT,
        BUILDING,
        ENEMY_UNIT,
        ENEMY_BUILDING
    }
}
