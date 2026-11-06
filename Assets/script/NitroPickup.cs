using UnityEngine;

public class NitroPickup : MonoBehaviour
{
    public float chargeAmount = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        // Ищем скрипт нитро на объекте, который в нас врезался
        NitroSystem nitro = other.GetComponent<NitroSystem>();
        
        if (nitro != null)
        {
            nitro.AddCharge(chargeAmount);
            Destroy(gameObject); // Уничтожаем банку
        }
    }
}