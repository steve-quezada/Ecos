using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EsconditeTrampa : MonoBehaviour
{
    [Header("Interacción")]
    public KeyCode teclaInteraccion = KeyCode.E;
    private bool jugadorEnRango = false;
    private bool yaActivado = false;

    [Header("Efectos de Muerte")]
    [Tooltip("Arrastra aquí el dibujo del fuego por si quieres que aparezca de golpe")]
    public GameObject efectoFuegoVisual; 
    public AudioSource fuenteAudio;
    public AudioClip sonidoQuemaduraOGrito;

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
        if (fuenteAudio != null && sonidoQuemaduraOGrito != null)
        {
            fuenteAudio.PlayOneShot(sonidoQuemaduraOGrito);
        }

        // 4. Esperamos exactamente los 2.5 segundos que pediste
        yield return new WaitForSeconds(2.5f);

        // 5. ¡Game Over! Mandamos a la pantalla de perder
        SceneManager.LoadScene("PantallaPerder");
    }
}