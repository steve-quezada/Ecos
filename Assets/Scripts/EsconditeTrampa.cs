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

    [Header("UI Flotante")]
    [Tooltip("Arrastra aquí el objeto 3D de la [E]. Se quedará siempre visible.")]
    public GameObject indicadorTeclaE; 

    [Header("Efectos de Muerte")]
    public GameObject efectoFuegoVisual; 
    public AudioSource fuenteAudio;
    public AudioClip sonidoQuemaduraOGrito;
    [Range(0f, 1f)] public float volumenGrito = 1f;
    [Range(0f, 1f)] public float normalizacionVolumenGrito = 0.85f;

    private MiloController milo;

    void Start()
    {
        if (efectoFuegoVisual != null)
        {
            efectoFuegoVisual.SetActive(false);
        }
    }

    void Update()
    {
        if (jugadorEnRango && !yaActivado && Input.GetKeyDown(teclaInteraccion))
        {
            yaActivado = true;
            
            // Apagamos la [E] porque ya activó la trampa
            if (indicadorTeclaE != null) indicadorTeclaE.SetActive(false);
            
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

        if (milo != null) Destroy(milo.gameObject);
        if (efectoFuegoVisual != null) efectoFuegoVisual.SetActive(true);

        ReproducirGritoTrampa();

        if (!Application.CanStreamedLevelBeLoaded(EscenaPantallaTrampa))
        {
            Debug.LogError("No se puede cargar " + EscenaPantallaTrampa + ". Verifica Build Settings.");
            yield break;
        }

        yield return EfectoMuertePantalla.FundirANegro(DuracionFundidoMuerte);
        SceneManager.LoadScene(EscenaPantallaTrampa);
    }

    void ReproducirGritoTrampa()
    {
        AudioClip clip = sonidoQuemaduraOGrito;
        AudioSource fuente = fuenteAudio;
        float volumenFinal = Mathf.Clamp01(volumenGrito * normalizacionVolumenGrito);

        EstaticaController estatica = FindObjectOfType<EstaticaController>();
        if (clip == null && estatica != null) clip = estatica.sonidoGrito;
        if (fuente == null && estatica != null) fuente = estatica.fuenteAudio;

        if (clip == null)
        {
            Debug.LogWarning("EsconditeTrampa sin clip de grito configurado en: " + gameObject.name);
            return;
        }

        if (fuente != null) fuente.PlayOneShot(clip, volumenFinal);
        else AudioSource.PlayClipAtPoint(clip, transform.position, volumenFinal);
    }
}