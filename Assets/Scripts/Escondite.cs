using System.Collections; // Necesario para usar Corrutinas (el temporizador)
using UnityEngine;

public class Escondite : MonoBehaviour
{
    [Header("Configuración de Salida")]
    public Vector2 offsetSalida = new Vector2(1.5f, 0f); 

    private GameManager gameManager;
    private bool enZonaDeEscondite = false;
    
    // Referencias al jugador
    private Transform jugadorTransform;
    private MiloController miloController;
    private SpriteRenderer miloSprite;

    // Esta variable guarda nuestro temporizador
    private Coroutine rutinaReduccionRuido;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (enZonaDeEscondite && Input.GetKeyDown(KeyCode.E))
        {
            if (!gameManager.miloOculto)
            {
                EsconderMilo();
            }
            else
            {
                SalirDelEscondite();
            }
        }
    }

    void EsconderMilo()
    {
        gameManager.miloOculto = true;
        miloController.puedeMoverse = false;
        jugadorTransform.position = transform.position;
        miloSprite.enabled = false;

        // Si ya había un reloj corriendo por error, lo matamos primero
        if (rutinaReduccionRuido != null) 
        {
            StopCoroutine(rutinaReduccionRuido);
        }
        // Iniciamos el temporizador de reducción de ruido
        rutinaReduccionRuido = StartCoroutine(ReducirRuidoPorSegundo());

        Debug.Log("Milo se escondió. Iniciando reducción de ruido...");
    }

    void SalirDelEscondite()
    {
        gameManager.miloOculto = false;
        jugadorTransform.position = (Vector2)transform.position + offsetSalida;
        miloController.puedeMoverse = true;
        miloSprite.enabled = true;

        // Detenemos el temporizador en el instante que Milo sale
        if (rutinaReduccionRuido != null)
        {
            StopCoroutine(rutinaReduccionRuido);
            rutinaReduccionRuido = null;
        }

        Debug.Log("Milo salió del escondite. El ruido dejó de bajar.");
    }

    // El temporizador que se ejecuta cada 1 segundo
    IEnumerator ReducirRuidoPorSegundo()
    {
        // Mientras Milo siga oculto, este ciclo se repetirá
        while (gameManager.miloOculto)
        {
            // Espera exactamente 1 segundo real
            yield return new WaitForSeconds(1f);

            // Le avisa al Manager que baje 1 punto
            if (ManagerRuido.instancia != null)
            {
                ManagerRuido.instancia.ReducirRuido(1f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enZonaDeEscondite = true;
            
            if (jugadorTransform == null)
            {
                jugadorTransform = collision.transform;
                miloController = collision.GetComponent<MiloController>();
                miloSprite = collision.GetComponent<SpriteRenderer>();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enZonaDeEscondite = false;
        }
    }
}