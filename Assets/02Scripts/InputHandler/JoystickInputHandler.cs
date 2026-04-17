using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickInputHandler : MonoBehaviour, IInputHandler
{
    [SerializeField] private DynamicJoystick joystick;

    public Vector2 GetMovement => joystick != null ? joystick.Direction : Vector2.zero;
}
