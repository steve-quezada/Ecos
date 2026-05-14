using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public float velocidadBase = 2.0f; 
    public float incrementoVelocidadPorRuido = 0.18f; 
    
    [Header("Aparición Periódica")]
    public float tiempoAparicion = 15f; 
    public float duracionPatrullaje = 10f; 
    private float temporizadorAparicion = 0f;
    private float temporizadorPatrullaje = 0f;

    [Header("Patrullaje")]
    public float rangoMovimientoAleatorio = 5f;
    private Vector2 puntoDestinoAleatorio;

    [Header("Persecución Curva")]
    [Tooltip("Qué tanto se desvía lateralmente la trayectoria al perseguir a Milo")]
    public float radioCurva = 1.2f;
    [Tooltip("Cada cuánto recalcula una nueva curva de persecución")]
    public float intervaloCambioCurva = 0.35f;
    [Tooltip("Suavizado del giro; mayor valor = giros más amplios")]
    public float suavizadoCurva = 0.14f;
    [Range(0f, 1f)] public float probabilidadCambioLado = 0.3f;

    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    [Tooltip("Placeholder: asigna aqui el sonido de aparicion de La Estatica")]
    public AudioClip sonidoAparicion;
    [Range(0f, 1f)] public float volumenAparicion = 1f;
    public AudioClip sonidoGrito;
    [Range(0f, 1f)] public float volumenGrito = 1f;
    private bool yaAtrapado = false;

    private Transform jugador;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;
    
    private bool estaActiva = false;
    private Vector2 objetivoCurvoPersecucion;
    private Vector2 velocidadSuavizadaPersecucion;
    private float temporizadorCurva = 0f;
    private int ladoCurva = 1;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        GameObject jugadorEncontrado = GameObject.FindGameObjectWithTag("Player");
        if (jugadorEncontrado != null)
        {
            jugador = jugadorEncontrado.transform;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
        ConfigurarAudioAparicion();

        if (duracionPatrullaje <= 0f || duracionPatrullaje >= tiempoAparicion)
        {
            // Evita dejarla activa demasiado tiempo por valores viejos en escena.
            duracionPatrullaje = 10f;
        }

        DesactivarEstatica();
    }

    void Update()
    {
        if (ManagerRuido.instancia == null || yaAtrapado || jugador == null || gameManager == null)
        {
            return;
        }

        float nivelDeRuido = ManagerRuido.instancia.ruidoActual;
        float ruidoMaximo = ManagerRuido.instancia.ruidoMaximo;

        if (nivelDeRuido >= ruidoMaximo)
        {
            ForzarAparicionYDerrota();
            return;
        }

        if (!estaActiva)
        {
            temporizadorAparicion += Time.deltaTime;

            if (temporizadorAparicion >= tiempoAparicion)
            {
                IniciarAparicionPeriodica();
            }

            return;
        }

        temporizadorPatrullaje += Time.deltaTime;
        if (temporizadorPatrullaje >= duracionPatrullaje)
        {
            DesactivarEstatica();
            return;
        }

        if (!gameManager.miloOculto)
        {
            PerseguirAMilo(nivelDeRuido);
        }
        else
        {
            PatrullarHabitacion();
        }
    }

    void IniciarAparicionPeriodica()
    {
        temporizadorAparicion = 0f;
        temporizadorPatrullaje = 0f;
        temporizadorCurva = 0f;
        velocidadSuavizadaPersecucion = Vector2.zero;

        float xA = Random.Range(limiteMinX, limiteMaxX);
        float yA = Random.Range(limiteMinY, limiteMaxY);
        transform.position = new Vector2(xA, yA);
        
        AsignarNuevoDestinoAleatorio();

        ActivarEstatica();
        ReproducirSonidoAparicion();
    }

    void ForzarAparicionYDerrota()
    {
        if (yaAtrapado)
        {
            return;
        }

        transform.position = jugador.position;
        ActivarEstatica();
        ReproducirSonidoAparicion();

        yaAtrapado = true;
        StartCoroutine(SecuenciaMuerte());
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
        temporizadorPatrullaje = 0f;
        temporizadorCurva = 0f;
        velocidadSuavizadaPersecucion = Vector2.zero;
        spriteRenderer.enabled = false;
        colisionador.enabled = false;
    }

    void PerseguirAMilo(float nivelDeRuido)
    {
        Vector2 posicionActual = transform.position;
        Vector2 posicionMilo = jugador.position;
        Vector2 haciaMilo = posicionMilo - posicionActual;

        if (haciaMilo.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        temporizadorCurva += Time.deltaTime;
        if (temporizadorCurva >= intervaloCambioCurva || Vector2.Distance(posicionActual, objetivoCurvoPersecucion) < 0.18f)
        {
            temporizadorCurva = 0f;

            if (Random.value < probabilidadCambioLado)
            {
                ladoCurva *= -1;
            }

            Vector2 direccion = haciaMilo.normalized;
            Vector2 perpendicular = new Vector2(-direccion.y, direccion.x);
            float distancia = haciaMilo.magnitude;

            float desvioLateral = Mathf.Clamp(distancia * 0.45f, 0.45f, radioCurva) * ladoCurva;
            float adelanto = Mathf.Clamp(distancia * 0.55f, 0.4f, 1.35f);

            objetivoCurvoPersecucion = posicionMilo + (direccion * adelanto) + (perpendicular * desvioLateral);
            objetivoCurvoPersecucion = LimitarPosicion(objetivoCurvoPersecucion);
        }

        float velocidadExtra = (nivelDeRuido - ruidoParaCazar) * incrementoVelocidadPorRuido;
        if (velocidadExtra < 0) velocidadExtra = 0; 
        
        float velocidadActual = velocidadBase + velocidadExtra;
        transform.position = Vector2.SmoothDamp(posicionActual, objetivoCurvoPersecucion, ref velocidadSuavizadaPersecucion, suavizadoCurva, velocidadActual);
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

    Vector2 LimitarPosicion(Vector2 posicion)
    {
        posicion.x = Mathf.Clamp(posicion.x, limiteMinX, limiteMaxX);
        posicion.y = Mathf.Clamp(posicion.y, limiteMinY, limiteMaxY);
        return posicion;
    }

    void ConfigurarAudioAparicion()
    {
        if (fuenteAudio == null)
        {
            fuenteAudio = GetComponent<AudioSource>();
        }

        if (fuenteAudio == null)
        {
            fuenteAudio = gameObject.AddComponent<AudioSource>();
        }

        fuenteAudio.playOnAwake = false;
        fuenteAudio.loop = false;
        fuenteAudio.spatialBlend = 0f;
    }

    void ReproducirSonidoAparicion()
    {
        if (sonidoAparicion == null)
        {
            return;
        }

        if (fuenteAudio != null)
        {
            fuenteAudio.PlayOneShot(sonidoAparicion, volumenAparicion);
        }
        else
        {
            AudioSource.PlayClipAtPoint(sonidoAparicion, transform.position, volumenAparicion);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && gameManager != null && !gameManager.miloOculto && !yaAtrapado)
        {
            yaAtrapado = true;
            StartCoroutine(SecuenciaMuerte());
        }
    }

    IEnumerator SecuenciaMuerte()
    {
        Debug.Log("¡La Estática te atrapó!");

        MiloController milo = FindObjectOfType<MiloController>();
        if (milo != null) milo.puedeMoverse = false;

        if (sonidoGrito != null)
        {
            if (fuenteAudio != null)
            {
                fuenteAudio.PlayOneShot(sonidoGrito, volumenGrito);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sonidoGrito, transform.position, volumenGrito);
            }
        }

        if (!Application.CanStreamedLevelBeLoaded(EscenaPantallaPerder))
        {
            Debug.LogError("No se puede cargar " + EscenaPantallaPerder + ". Verifica Build Settings.");
            yield break;
        }

        yield return EfectoMuertePantalla.FundirANegro(DuracionFundidoMuerte);

        SceneManager.LoadScene(EscenaPantallaPerder);
    }
}