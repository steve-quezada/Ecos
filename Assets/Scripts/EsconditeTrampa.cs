using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EsconditeTrampa : MonoBehaviour
{
    private const string EscenaPantallaTrampa = "PantallaTrampa";
    private const float DuracionFundidoMuerte = 2.5f;

    [Header("Interacción")]
    public KeyCode teclaInteraccion = KeyCode.E;
    private bool jugadorEnRango = false;
    private bool yaActivado = false;

    [Header("Efectos de Muerte")]
    [Tooltip("Arrastra aquí el dibujo del fuego por si quieres que aparezca de golpe")]
    public GameObject efectoFuegoVisual; 
    public AudioSource fuenteAudio;
    public AudioClip sonidoQuemaduraOGrito;
    [Range(0f, 1f)] public float volumenGrito = 1f;

    private MiloController milo;

    void Start()
    {
        // Nos aseguramos de que el fuego empiece apagado
        if (efectoFuegoVisual != null)
        {
            efectoFuegoVisual.SetActive(false);
        }
    }

    void Update()
    {
        // Si Milo está cerca, presiona la tecla y la trampa no se ha activado aún
        if (jugadorEnRango && !yaActivado && Input.GetKeyDown(teclaInteraccion))
        {
            yaActivado = true;
            StartCoroutine(SecuenciaMuerteFuego());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = true;
            milo = collision.GetComponent<MiloController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            milo = null;
        }
    }

    IEnumerator SecuenciaMuerteFuego()
    {
        Debug.Log("¡Muerte por trampa! Milo se escondió en el fuego.");

        // 1. Destruimos a Milo por completo (desaparece de la pantalla al instante)
        if (milo != null)
        {
            Destroy(milo.gameObject);
        }

        // 2. Encendemos las llamas visuales
        if (efectoFuegoVisual != null)
        {
            efectoFuegoVisual.SetActive(true);
        }

        // 3. Reproducimos el grito o sonido de quemadura
        ReproducirGritoTrampa();

        if (!Application.CanStreamedLevelBeLoaded(EscenaPantallaTrampa))
        {
            Debug.LogError("No se puede cargar " + EscenaPantallaTrampa + ". Verifica Build Settings.");
            yield break;
        }

        // 4. Fundido progresivo a negro
        yield return EfectoMuertePantalla.FundirANegro(DuracionFundidoMuerte);

        // 5. ¡Game Over! Mandamos a la pantalla de trampa
        SceneManager.LoadScene(EscenaPantallaTrampa);
    }

    void ReproducirGritoTrampa()
    {
        AudioClip clip = sonidoQuemaduraOGrito;
        AudioSource fuente = fuenteAudio;

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
            Debug.LogWarning("EsconditeTrampa sin clip de grito configurado en: " + gameObject.name);
            return;
        }

        if (fuente != null)
        {
            fuente.PlayOneShot(clip, volumenGrito);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volumenGrito);
        }
    }
}