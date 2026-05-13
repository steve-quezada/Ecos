using UnityEngine;
using UnityEngine.SceneManagement; // NUEVO
using System.Collections; // NUEVO: Para poder hacer pausas (Corrutinas)

public class EstaticaController : MonoBehaviour
{
    private const string EscenaPantallaPerder = "PantallaPerder";
    private const float DuracionFundidoMuerte = 2.5f;

    [Header("Límites de la Habitación")]
    public float limiteMinX = -7f;
    public float limiteMaxX = 7f;
    public float limiteMinY = -4f;
    public float limiteMaxY = 4f;

    [Header("Configuración de Caza (Ruido)")]
    public float radioDeteccion = 3f;
    public int ruidoParaCazar = 5; 
    public float velocidadBase = 1.5f; 
    public float incrementoVelocidadPorRuido = 0.4f; 
    
    [Header("Aparición Aleatoria (15s)")]
    public float tiempoAparicion = 15f; 
    public float duracionPatrullaje = 5f; 
    private float temporizadorAparicion = 0f;
    private float temporizadorPatrullaje = 0f;

    [Header("Patrullaje")]
    public float rangoMovimientoAleatorio = 5f;
    private Vector2 puntoDestinoAleatorio;

    [Header("Susto y Game Over (NUEVO)")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoGrito;
    private bool yaAtrapado = false; // Seguro para que no grite varias veces

    private Transform jugador;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;
    
    private bool estaActiva = false;
    private bool estaCazandoPorRuido = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();

        DesactivarEstatica();
    }

    void Update()
    {
        if (ManagerRuido.instancia == null || yaAtrapado) return; // Si ya lo atrapó, deja de moverse

        float nivelDeRuido = ManagerRuido.instancia.ruidoActual;
        float ruidoMaximo = ManagerRuido.instancia.ruidoMaximo;

        if (nivelDeRuido >= ruidoMaximo)
        {
            transform.position = jugador.position;
            ActivarEstatica();
            this.enabled = false; 
            return; 
        }

        if (nivelDeRuido >= ruidoParaCazar)
        {
            estaCazandoPorRuido = true;
            if (!estaActiva) ActivarEstatica(); 
            PerseguirAMilo();
            return; 
        }
        else 
        {
            if (estaCazandoPorRuido)
            {
                estaCazandoPorRuido = false;
                DesactivarEstatica();
            }
        }

        if (!estaActiva)
        {
            temporizadorAparicion += Time.deltaTime;
            if (temporizadorAparicion >= tiempoAparicion)
            {
                IniciarRecorridoAleatorio();
                temporizadorAparicion = 0f; 
            }
        }
        else
        {
            float distanciaAMilo = Vector2.Distance(transform.position, jugador.position);

            if (!gameManager.miloOculto && (nivelDeRuido > 0 || distanciaAMilo <= radioDeteccion))
            {
                PerseguirAMilo();
                temporizadorPatrullaje = 0f; 
            }
            else
            {
                PatrullarHabitacion();
                temporizadorPatrullaje += Time.deltaTime;
                if (temporizadorPatrullaje >= duracionPatrullaje)
                {
                    DesactivarEstatica(); 
                }
            }
        }
    }

    void IniciarRecorridoAleatorio()
    {
        ActivarEstatica();
        temporizadorPatrullaje = 0f; 
        
        float xA = Random.Range(limiteMinX, limiteMaxX);
        float yA = Random.Range(limiteMinY, limiteMaxY);
        transform.position = new Vector2(xA, yA);
        
        AsignarNuevoDestinoAleatorio();
    }

    void ActivarEstatica()
    {
        estaActiva = true;
        spriteRenderer.enabled = true;
        colisionador.enabled = true;
    }

    void DesactivarEstatica()
    {
        estaActiva = false;
        spriteRenderer.enabled = false;
        colisionador.enabled = false;
    }

    void PerseguirAMilo()
    {
        float nivelDeRuido = ManagerRuido.instancia.ruidoActual;
        float velocidadExtra = (nivelDeRuido - ruidoParaCazar) * incrementoVelocidadPorRuido;
        if (velocidadExtra < 0) velocidadExtra = 0; 
        
        float velocidadActual = velocidadBase + velocidadExtra;
        transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidadActual * Time.deltaTime);
    }

    void PatrullarHabitacion()
    {
        transform.position = Vector2.MoveTowards(transform.position, puntoDestinoAleatorio, velocidadBase * Time.deltaTime);
        if (Vector2.Distance(transform.position, puntoDestinoAleatorio) < 0.1f)
        {
            AsignarNuevoDestinoAleatorio();
        }
    }

    void AsignarNuevoDestinoAleatorio()
    {
        float xAleatorio = Random.Range(limiteMinX, limiteMaxX);
        float yAleatorio = Random.Range(limiteMinY, limiteMaxY);
        puntoDestinoAleatorio = new Vector2(xAleatorio, yAleatorio);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ¡NUEVO! Detecta a Milo, verifica que no esté escondido y que no lo haya atrapado ya
        if (collision.CompareTag("Player") && !gameManager.miloOculto && !yaAtrapado)
        {
            yaAtrapado = true;
            StartCoroutine(SecuenciaMuerte());
        }
    }

    // ¡NUEVO! La corrutina que maneja el susto y el cambio de pantalla
    IEnumerator SecuenciaMuerte()
    {
        Debug.Log("¡La Estática te atrapó!");

        // 1. Congelamos a Milo en su lugar
        MiloController milo = FindObjectOfType<MiloController>();
        if (milo != null) milo.puedeMoverse = false;

        // 2. Reproducimos el grito
        if (fuenteAudio != null && sonidoGrito != null)
        {
            fuenteAudio.PlayOneShot(sonidoGrito);
        }

        if (!Application.CanStreamedLevelBeLoaded(EscenaPantallaPerder))
        {
            Debug.LogError("No se puede cargar " + EscenaPantallaPerder + ". Verifica Build Settings.");
            yield break;
        }

        // 3. Fundido progresivo a negro mientras suena el susto
        yield return EfectoMuertePantalla.FundirANegro(DuracionFundidoMuerte);

        // 4. Cambiamos a la pantalla de perder
        SceneManager.LoadScene(EscenaPantallaPerder);
    }
}