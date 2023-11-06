using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Controls controls;
    private InputAction move;

    private void Awake() => controls = new Controls();

    private void OnEnable() => controls.Enable();

    private void OnDisable() => controls.Disable();

    void Start()
    {
        move = controls.Player.Move;
    }

    void FixedUpdate() // перемещение игрока
    {
        Vector2 moveVector = move.ReadValue<Vector2>() / 10;
        transform.Translate(moveVector.x, 0, moveVector.y);
    }
}
