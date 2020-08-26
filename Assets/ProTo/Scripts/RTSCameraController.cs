using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCameraController : MonoBehaviour {

    public float fastSpeed;
    public float normalSpeed;
    public float movementTime;
    public float rotationAmount;
    public float zoomSpeed;
    public float dragSpeed;

    public Quaternion newRotation;
    public Vector3 newPosition;

    private Camera cam;

    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 planarFoward;
    public Vector3 planarRight;

    private GameObject selected;

    // Start is called before the first frame update
    void Start() {
        if (transform.GetChild(0) != null) {
            cam = transform.GetChild(0).gameObject.GetComponent<Camera>();
        }

        if (cam == null) {
            GameObject camera = Instantiate(new GameObject("Camera"), transform);
            cam = camera.AddComponent<Camera>();
            cam.fieldOfView = 10;
            camera.transform.rotation = Quaternion.Euler(new Vector3(45, 45, 0));
        }

        newPosition = transform.position;
        newRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update() {
        planarFoward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        planarRight = Vector3.Scale(cam.transform.right, new Vector3(1, 0, 1)).normalized;
        HandleMovementInput();
        HandleMouseInput();

        transform.position = Vector3.Lerp(transform.position, newPosition, movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, movementTime);
    }

    void HandleMouseInput() {
        if (Input.mouseScrollDelta.y != 0) {
            newPosition += cam.transform.forward * Input.mouseScrollDelta.y * zoomSpeed * 50f;
        }

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Unit"))) {
                selected = hit.collider.gameObject;
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            if (selected != null) {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"))) {
                    Moveable mov;
                    if (selected.TryGetComponent(out mov)) {
                        // mov.CommandMoveTo(hit.point);
                    }
                }
            }
        }

        // Drag panning
        if (Input.GetMouseButtonDown(2)) {
            dragStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2)) {
            dragCurrentPosition = Input.mousePosition;
            Vector3 diff = (dragStartPosition - dragCurrentPosition);
            transform.position = transform.position + ((planarFoward * diff.y) + (planarRight * diff.x)) * dragSpeed * Time.deltaTime;
            newPosition = transform.position;
            dragStartPosition = dragCurrentPosition;
        }
    }

    void HandleMovementInput() {
        float speed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : normalSpeed;

        if (Input.GetKey(KeyCode.UpArrow)) {
            newPosition += (planarFoward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            newPosition += (-planarFoward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            newPosition += (planarRight * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            newPosition += (-planarRight * speed * Time.deltaTime);
        }

        // TODO: Better Rotation. Rotate around target axis
        if (Input.GetKey(KeyCode.Q)) {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E)) {
            newRotation *= Quaternion.Euler(-Vector3.up * rotationAmount * Time.deltaTime);
        }
    }

    private void OnDrawGizmos() {
        if (selected != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(selected.transform.position + Vector3.up * 3f, 0.3f);
        }
    }
}
