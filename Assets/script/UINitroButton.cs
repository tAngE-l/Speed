using UnityEngine;
using UnityEngine.EventSystems;

public class UINitroButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Nitro, Gas, Brake, Left, Right }
    public ButtonType buttonType;

    private CarController car;
    private NitroSystem nitro;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            car = player.GetComponent<CarController>();
            nitro = player.GetComponent<NitroSystem>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (car == null) return;

        switch (buttonType)
        {
            case ButtonType.Nitro: if (nitro != null) nitro.TryActivateNitro(); break;
            case ButtonType.Gas: car.SetGas(true); break;
            case ButtonType.Brake: car.SetBrake(true); break;
            case ButtonType.Left: car.SetLeft(true); break;
            case ButtonType.Right: car.SetRight(true); break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (car == null) return;

        switch (buttonType)
        {
            case ButtonType.Gas: car.SetGas(false); break;
            case ButtonType.Brake: car.SetBrake(false); break;
            case ButtonType.Left: car.SetLeft(false); break;
            case ButtonType.Right: car.SetRight(false); break;
        }
    }
}