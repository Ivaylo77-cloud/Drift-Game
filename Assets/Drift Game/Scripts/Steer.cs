using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Directions
{
    left,
    right
}

public class Steer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Directions steerDirection;

    public bool isPressed;

    public void OnPointerDown(PointerEventData data)
    {
        isPressed = true;
        CarController cc = FindObjectOfType<CarController>();
        if (steerDirection == Directions.left)
        {
            cc.turnInput = -1.0f;
        }
        else
        {
            cc.turnInput = 1.0f;
        }
    }
    public void OnPointerUp(PointerEventData data)
    {
        isPressed = false;
        CarController cc = FindObjectOfType<CarController>();
        cc.turnInput = 0.0f;
    }
}
