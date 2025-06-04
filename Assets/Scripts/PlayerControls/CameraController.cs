using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction rotateAction;
    private GameObject cameraArm;
    
    [SerializeField] private float moveSpeed = 5f; // Movement speed
    [SerializeField] private float rotateSpeed = 100f; // Rotation speed

    private Vector2 moveInput; // Stores the current input
    private float rotationInput; //Stores the rotation input

    //UI Elements
    [SerializeField] private Button regenerateButton;
    [SerializeField] private Button confirmButton;
    
    private void Awake() {
        var inputActionAsset = GetComponent<PlayerInput>().actions;
        moveAction = inputActionAsset["CameraMovement/WASD"];
        rotateAction = inputActionAsset["CameraMovement/Rotate"];

        cameraArm = transform.GetChild(0).gameObject;
        
    }

    private void Update() {
        // Apply movement
        Vector3 forward = cameraArm.transform.forward;
        Vector3 right = cameraArm.transform.right;

        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
        
        //Apply rotation
        cameraArm.transform.Rotate(Vector3.up, rotationInput * rotateSpeed * Time.deltaTime);
    }

    private void OnEnable() {
        // Enable the action and subscribe to the performed callback
        moveAction.Enable();
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;

        rotateAction.Enable();
        rotateAction.performed += OnRotationPerformed;
        rotateAction.canceled += OnRotateCanceled;
    }

    private void OnDisable() {
        // Disable the action and unsubscribe from the callback
        moveAction.Disable();
        // moveAction.performed -= OnMovePerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context) {
        // Update the move input when the action is performed
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context) {
        // Reset the move input when the action is canceled
        moveInput = Vector2.zero;
    }
    
    private void OnRotationPerformed(InputAction.CallbackContext context) {
        // Update the rotation input when the action is performed
        rotationInput = context.ReadValue<float>(); 
    }
    
    private void OnRotateCanceled(InputAction.CallbackContext context) {
        // Reset the rotation input when the action is canceled
        rotationInput = 0f;
    }

    public void OnConfirmPressed() {
        regenerateButton.enabled = false;
        regenerateButton.gameObject.SetActive(false);
        confirmButton.enabled = false;
        confirmButton.gameObject.SetActive(false);
    }
}