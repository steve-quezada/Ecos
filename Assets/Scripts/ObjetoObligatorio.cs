using UnityEngine;

public class ObjetoObligatorio : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    // Escribe el nombre del objeto tal como quieres que se valide en la puerta.
    public string nombreDelObjeto; 
    
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No se encontró GameManager para el objeto: " + gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si Milo toca el objeto
        if (collision.CompareTag("Player"))
        {
            if (gameManager == null) return;

            // Le avisamos al GameManager qué objeto recogimos
            gameManager.RecogerObjeto(nombreDelObjeto);
            
            // Destruimos el objeto de la escena para que desaparezca visualmente
            Destroy(gameObject);
        }
    }
}