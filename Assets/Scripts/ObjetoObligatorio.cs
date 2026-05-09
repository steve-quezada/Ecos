using UnityEngine;

public class ObjetoObligatorio : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    // Aquí escribirás "Mochila" o "Llave" desde el Inspector de Unity
    public string nombreDelObjeto; 
    
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si Milo toca el objeto
        if (collision.CompareTag("Player"))
        {
            // Le avisamos al GameManager qué objeto recogimos
            gameManager.RecogerObjeto(nombreDelObjeto);
            
            // Destruimos el objeto de la escena para que desaparezca visualmente
            Destroy(gameObject);
        }
    }
}