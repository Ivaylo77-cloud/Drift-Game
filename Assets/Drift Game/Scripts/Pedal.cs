using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Pedals
{
    accelerate,
    brake
}

public class Pedal : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Pedals pedalType;

    public bool isPressed;

    public void OnPointerDown(PointerEventData data)
    {
        isPressed = true;
        CarController cc = FindObjectOfType<CarController>();
        if (pedalType == Pedals.brake)
        {
            cc.moveInput = -1.0f;
        }
        else
        {
            cc.moveInput = 1.0f;
        }
    }
    public void OnPointerUp(PointerEventData data)
    {
        isPressed = false;
        CarController cc = FindObjectOfType<CarController>();
        cc.moveInput = 0.0f;
    }
}
