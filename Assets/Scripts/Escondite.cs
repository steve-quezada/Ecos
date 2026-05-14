using System.Collections; // Necesario para usar Corrutinas (el temporizador)
using UnityEngine;
using UnityEngine.SceneManagement;

public class Escondite : MonoBehaviour
{
    private const string EscenaPantallaTrampa = "PantallaTrampa";
    private const float DuracionFundidoMuerte = 2.5f;

    [Header("Configuración de Salida")]
    public Vector2 offsetSalida = new Vector2(1.5f, 0f); 

    [Header("Modo Trampa")]
    public bool esTrampaMortal = false;
    public bool ocultarSpriteAlMorir = true;

    [Header("Audio de Muerte Trampa")]
    public AudioSource fuenteAudioMuerte;
    public AudioClip sonidoGritoMuerte;
    [Range(0f, 1f)] public float volumenGritoMuerte = 1f;
    [Range(0f, 1f)] public float normalizacionVolumenGritoMuerte = 0.85f;

    private GameManager gameManager;
    private bool enZonaDeEscondite = false;
    
    // Referencias al jugador
    private Transform jugadorTransform;
    private MiloController miloController;
    private SpriteRenderer miloSprite;

    // Esta variable guarda nuestro temporizador
    private Coroutine rutinaReduccionRuido;
    private bool muerteTrampaEnCurso = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No se encontró GameManager para el escondite: " + gameObject.name);
        }
    }

    void Update()
    {
        if (gameManager == null) return;

        if (enZonaDeEscondite && Input.GetKeyDown(KeyCode.E))
        {
            if (esTrampaMortal)
            {
                ActivarTrampaMortal();
                return;
            }

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

    void ActivarTrampaMortal()
    {
        if (muerteTrampaEnCurso)
        {
            return;
        }

        muerteTrampaEnCurso = true;

        if (rutinaReduccionRuido != null)
        {
            StopCoroutine(rutinaReduccionRuido);
            rutinaReduccionRuido = null;
        }

        gameManager.miloOculto = false;

        if (miloController != null)
        {
            miloController.puedeMoverse = false;
        }

        if (ocultarSpriteAlMorir && miloSprite != null)
        {
            miloSprite.enabled = false;
        }

        enZonaDeEscondite = false;
        Debug.Log("\u00a1TRAMPA! Ese escondite era falso. Milo murió al instante.");

        ReproducirGritoMuerte();
        StartCoroutine(SecuenciaMuerteTrampa());
    }

    IEnumerator SecuenciaMuerteTrampa()
    {
        if (!Application.CanStreamedLevelBeLoaded(EscenaPantallaTrampa))
        {
            Debug.LogError("No se puede cargar " + EscenaPantallaTrampa + ". Verifica Build Settings.");
            muerteTrampaEnCurso = false;
            yield break;
        }

        yield return EfectoMuertePantalla.FundirANegro(DuracionFundidoMuerte);
        SceneManager.LoadScene(EscenaPantallaTrampa);
    }

    void ReproducirGritoMuerte()
    {
        AudioClip clip = sonidoGritoMuerte;
        AudioSource fuente = fuenteAudioMuerte;
        float volumenFinal = Mathf.Clamp01(volumenGritoMuerte * normalizacionVolumenGritoMuerte);

        EstaticaController estatica = FindObjectOfType<EstaticaController>();
        if (clip == null && estatica != null)
        {
            clip = estatica.sonidoGrito;
        }

        if (fuente == null && estatica != null)
        {
            fuente = estatica.fuenteAudio;
        }

        if (clip == null)
        {
            Debug.LogWarning("Escondite trampa sin clip de grito configurado en: " + gameObject.name);
            return;
        }

        if (fuente != null)
        {
            fuente.PlayOneShot(clip, volumenFinal);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volumenFinal);
        }
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