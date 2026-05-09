using UnityEngine;

public class FuenteRuido : MonoBehaviour
{
    public int puntosDeRuido = 1;
    private GameManager gameManager;

    void Start()
    {
        // Encontramos el cerebro del juego en la escena automáticamente
        gameManager = FindObjectOfType<GameManager>();
    }

    // Unity llama a esta función automáticamente cuando algo entra al Trigger
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si el objeto que pisó esta zona tiene la etiqueta "Player" (Milo)
        if (collision.CompareTag("Player"))
        {
            gameManager.AgregarRuido(puntosDeRuido);
            
            // Opcional: Destruir el objeto para que el vidrio solo suene una vez al pisarlo
            // Destroy(gameObject); 
        }
    }
}