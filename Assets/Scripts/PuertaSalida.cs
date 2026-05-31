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

        if (gameManager == null)
        {
            Debug.LogError("No se encontró GameManager en la escena.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager == null) return;

        if (collision.CompareTag("Player"))
        {
            // Le preguntamos al GameManager si Milo ya tiene todo
            if (gameManager.TieneTodosLosObjetos())
            {
                if (string.IsNullOrWhiteSpace(nombreSiguienteEscena))
                {
                    Debug.LogWarning("La puerta no tiene escena de destino configurada.");
                    return;
                }

                MensajeriaJugador.Mostrar("¡Excelente! Pasaste a la siguiente habitación.");
                
                // Esta es la línea mágica que carga el nuevo nivel
                SceneManager.LoadScene(nombreSiguienteEscena);
            }
            else
            {
                string faltantes = gameManager.ObtenerObjetosFaltantes();
                if (string.IsNullOrEmpty(faltantes))
                {
                    MensajeriaJugador.Mostrar("La puerta está cerrada. Aún faltan objetos obligatorios.");
                }
                else
                {
                    MensajeriaJugador.Mostrar("La puerta está cerrada. Te faltan: " + faltantes);
                }
            }
        }
    }
}