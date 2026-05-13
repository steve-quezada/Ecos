using UnityEngine;
using UnityEngine.SceneManagement; // CRUCIAL para poder cambiar escenas

public class CambioHabitacion : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("Escribe el nombre EXACTO de la escena a la que quieres ir")]
    public string nombreSiguienteEscena; 

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si el que tocó la zona de la puerta fue Milo
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Milo cruzó la puerta. Viajando a: " + nombreSiguienteEscena);
            
            // Esta es la línea mágica que carga el nuevo nivel
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
    }
}