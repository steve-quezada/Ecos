using UnityEngine;
using UnityEngine.SceneManagement; // CRUCIAL para poder cambiar de escenas

public class PuertaSalida : MonoBehaviour
{
    [Header("Destino")]
    public string nombreSiguienteEscena; // Para que escribas "Baño" (o como se llame) en el Inspector

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Le preguntamos al GameManager si Milo ya tiene todo
            if (gameManager.TieneTodosLosObjetos())
            {
                Debug.Log("¡NIVEL COMPLETADO! Cruzaste la puerta. Viajando a: " + nombreSiguienteEscena);
                
                // Esta es la línea mágica que carga el nuevo nivel
                SceneManager.LoadScene(nombreSiguienteEscena);
            }
            else
            {
                Debug.Log("La puerta está cerrada. Te faltan objetos obligatorios.");
            }
        }
    }
}