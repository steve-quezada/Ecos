using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EstaticaController : MonoBehaviour
{
    private const string EscenaPantallaPerder = "PantallaPerder";
    private const float DuracionFundidoMuerte = 2.5f;
    private const float VolumenBaseEstaticaMinimoGlobal = 1.0f;
    private const float VolumenAparicionMinimoGlobal = 1.0f;
    private const float MultiplicadorAparicionMinimoGlobal = 1.6f;

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
    [Tooltip("Distancia mínima respecto a Milo para elegir punto de aparición")]
    public float distanciaMinimaAparicionJugador = 4.5f;
    [Tooltip("Tiempo de gracia tras cerrar un minijuego antes de volver a poder aparecer")]
    public float enfriamientoTrasMinijuego = 4f;
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

    [Header("Señal Visual de Aparición")]
    public bool mostrarSenalAparicion = false;
    public float duracionSenalAparicion = 1.0f;
    public float escalaInicialSenal = 0.35f;
    public float escalaFinalSenal = 2.25f;
    public float grosorCruzSenal = 0.16f;
    public Color colorSenalAparicion = new Color(1f, 0.36f, 0.12f, 0.95f);

    [Header("Señal Direccional en Milo")]
    public bool mostrarSenalDireccionEnMilo = true;
    public float duracionSenalDireccion = 1.2f;
    [Range(1, 6)] public int cantidadLineasDireccion = 3;
    public float distanciaBaseSenalDireccion = 0.5f;
    public float separacionSenalDireccion = 0.18f;
    public float largoLineaSenalDireccion = 0.44f;
    public float grosorLineaSenalDireccion = 0.06f;
    public float amplitudRespiracionSenal = 0.06f;
    public float velocidadRespiracionSenal = 6.2f;
    public Color colorSenalDireccion = new Color(1f, 0.84f, 0.68f, 0.9f);

    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    [Tooltip("Placeholder: asigna aqui el sonido de aparicion de La Estatica")]
    public AudioClip sonidoAparicion;
    [Range(0f, 1f)] public float volumenAparicion = 1f;
    public AudioClip sonidoGrito;
    [Range(0f, 1f)] public float volumenGrito = 1f;
    [Range(0f, 1f)] public float volumenBaseFuenteEstatica = 0.9f;
    [Range(0f, 1f)] public float volumenMinimoAparicion = 0.9f;
    [Range(0f, 1f)] public float volumenMinimoGrito = 0.9f;
    [Range(1f, 2f)] public float multiplicadorPrioridadAparicion = 1.6f;
    private bool yaAtrapado = false;
    private bool muerteForzadaEnCurso = false;

    private Transform jugador;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private Collider2D colisionador;
    
    private bool estaActiva = false;
    private bool pausadaPorMinijuego = false;
    private Vector2 objetivoCurvoPersecucion;
    private Vector2 velocidadSuavizadaPersecucion;
    private float temporizadorCurva = 0f;
    private int ladoCurva = 1;
    private static Sprite spriteSenalAparicion;

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

        if (pausadaPorMinijuego)
        {
            return;
        }

        float nivelDeRuido = ManagerRuido.instancia.ruidoActual;

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

        Vector2 puntoAparicion = ObtenerPuntoAparicionLejanoDelJugador();
        transform.position = puntoAparicion;
        
        AsignarNuevoDestinoAleatorio();

        if (mostrarSenalAparicion)
        {
            StartCoroutine(MostrarSenalAparicion(puntoAparicion));
        }

        if (mostrarSenalDireccionEnMilo)
        {
            StartCoroutine(MostrarSenalDireccionEnMilo(puntoAparicion));
        }

        ActivarEstatica();
        ReproducirSonidoAparicion();
    }

    public void PausarPorMinijuego()
    {
        if (yaAtrapado)
        {
            return;
        }

        pausadaPorMinijuego = true;
        DesactivarEstatica();
        temporizadorAparicion = 0f;
    }

    public void ReanudarTrasMinijuego()
    {
        if (yaAtrapado)
        {
            return;
        }

        pausadaPorMinijuego = false;
        float enfriamiento = Mathf.Clamp(enfriamientoTrasMinijuego, 0f, Mathf.Max(0.01f, tiempoAparicion));
        temporizadorAparicion = Mathf.Max(0f, tiempoAparicion - enfriamiento);
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

    Vector2 ObtenerPuntoAparicionLejanoDelJugador()
    {
        Vector2 puntoMasLejanoEncontrado = new Vector2(Random.Range(limiteMinX, limiteMaxX), Random.Range(limiteMinY, limiteMaxY));
        float mejorDistancia = -1f;
        float distanciaMinima = Mathf.Max(0.5f, distanciaMinimaAparicionJugador);
        Vector2 posicionJugador = jugador != null ? (Vector2)jugador.position : Vector2.zero;

        for (int intento = 0; intento < 48; intento++)
        {
            Vector2 candidato = new Vector2(Random.Range(limiteMinX, limiteMaxX), Random.Range(limiteMinY, limiteMaxY));
            float distancia = Vector2.Distance(candidato, posicionJugador);

            if (distancia >= distanciaMinima)
            {
                return candidato;
            }

            if (distancia > mejorDistancia)
            {
                mejorDistancia = distancia;
                puntoMasLejanoEncontrado = candidato;
            }
        }

        return puntoMasLejanoEncontrado;
    }

    IEnumerator MostrarSenalAparicion(Vector2 posicion)
    {
        if (duracionSenalAparicion <= 0f)
        {
            yield break;
        }

        GameObject senal = new GameObject("SenalAparicionEstatica");
        senal.transform.position = new Vector3(posicion.x, posicion.y, transform.position.z - 0.1f);

        Sprite sprite = ObtenerSpriteSenal();
        int orden = spriteRenderer != null ? spriteRenderer.sortingOrder + 20 : 100;

        SpriteRenderer horizontal = CrearSegmentoSenal(senal.transform, "Horizontal", sprite, orden);
        SpriteRenderer vertical = CrearSegmentoSenal(senal.transform, "Vertical", sprite, orden);

        horizontal.transform.localScale = new Vector3(1f, Mathf.Max(0.02f, grosorCruzSenal), 1f);
        vertical.transform.localScale = new Vector3(Mathf.Max(0.02f, grosorCruzSenal), 1f, 1f);

        float tiempo = 0f;
        while (tiempo < duracionSenalAparicion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracionSenalAparicion);
            float escala = Mathf.Lerp(escalaInicialSenal, escalaFinalSenal, t);
            float alfa = Mathf.Lerp(colorSenalAparicion.a, 0f, t);

            senal.transform.localScale = new Vector3(escala, escala, 1f);

            Color colorActual = colorSenalAparicion;
            colorActual.a = alfa;
            horizontal.color = colorActual;
            vertical.color = colorActual;

            yield return null;
        }

        Destroy(senal);
    }

    IEnumerator MostrarSenalDireccionEnMilo(Vector2 puntoAparicion)
    {
        if (jugador == null || duracionSenalDireccion <= 0f)
        {
            yield break;
        }

        Vector2 direccion = puntoAparicion - (Vector2)jugador.position;
        if (direccion.sqrMagnitude <= 0.0001f)
        {
            yield break;
        }
        direccion.Normalize();

        Vector2 perpendicular = new Vector2(-direccion.y, direccion.x);
        float angulo = Mathf.Atan2(perpendicular.y, perpendicular.x) * Mathf.Rad2Deg;

        GameObject raiz = new GameObject("SenalDireccionMilo");
        raiz.transform.position = jugador.position;

        Sprite sprite = ObtenerSpriteSenal();
        int orden = spriteRenderer != null ? spriteRenderer.sortingOrder + 25 : 120;
        int cantidadLineas = Mathf.Clamp(cantidadLineasDireccion, 1, 6);
        SpriteRenderer[] lineas = new SpriteRenderer[cantidadLineas];

        for (int i = 0; i < cantidadLineas; i++)
        {
            lineas[i] = CrearSegmentoSenal(raiz.transform, "Respiracion_" + i, sprite, orden);
            lineas[i].transform.localRotation = Quaternion.Euler(0f, 0f, angulo);
            lineas[i].transform.localScale = new Vector3(Mathf.Max(0.08f, largoLineaSenalDireccion), Mathf.Max(0.02f, grosorLineaSenalDireccion), 1f);
            lineas[i].color = colorSenalDireccion;
        }

        float tiempo = 0f;
        while (tiempo < duracionSenalDireccion)
        {
            tiempo += Time.deltaTime;

            if (jugador == null)
            {
                break;
            }

            raiz.transform.position = jugador.position;
            float t = Mathf.Clamp01(tiempo / duracionSenalDireccion);
            float desvanecimiento = Mathf.Lerp(colorSenalDireccion.a, 0f, t);

            for (int i = 0; i < cantidadLineas; i++)
            {
                float fase = tiempo * velocidadRespiracionSenal + i * 0.7f;
                float respiracion = Mathf.Sin(fase) * amplitudRespiracionSenal;
                float distancia = distanciaBaseSenalDireccion + (separacionSenalDireccion * i) + respiracion;
                Vector2 offset = direccion * Mathf.Max(0.05f, distancia);
                lineas[i].transform.localPosition = new Vector3(offset.x, offset.y, 0f);

                Color c = colorSenalDireccion;
                c.a = Mathf.Max(0f, desvanecimiento);
                lineas[i].color = c;
            }

            yield return null;
        }

        Destroy(raiz);
    }

    SpriteRenderer CrearSegmentoSenal(Transform parent, string nombre, Sprite sprite, int orden)
    {
        GameObject segmento = new GameObject(nombre);
        segmento.transform.SetParent(parent, false);

        SpriteRenderer render = segmento.AddComponent<SpriteRenderer>();
        render.sprite = sprite;
        render.sortingOrder = orden;
        render.color = colorSenalAparicion;
        return render;
    }

    Sprite ObtenerSpriteSenal()
    {
        if (spriteSenalAparicion != null)
        {
            return spriteSenalAparicion;
        }

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.name = "EstaticaSenalTex";
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "EstaticaSenalSprite";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        spriteSenalAparicion = sprite;
        return spriteSenalAparicion;
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
        float volumenBaseForzado = Mathf.Max(volumenBaseFuenteEstatica, VolumenBaseEstaticaMinimoGlobal);
        fuenteAudio.volume = Mathf.Max(fuenteAudio.volume, volumenBaseForzado);
    }

    void ReproducirSonidoAparicion()
    {
        if (sonidoAparicion == null)
        {
            return;
        }

        float volumenBaseAparicion = Mathf.Max(volumenAparicion, volumenMinimoAparicion, VolumenAparicionMinimoGlobal);
        float multiplicador = Mathf.Max(multiplicadorPrioridadAparicion, MultiplicadorAparicionMinimoGlobal);
        float volumenFinalAparicion = Mathf.Clamp(volumenBaseAparicion * multiplicador, 0f, 2f);

        if (fuenteAudio != null)
        {
            fuenteAudio.PlayOneShot(sonidoAparicion, volumenFinalAparicion);
        }
        else
        {
            AudioSource.PlayClipAtPoint(sonidoAparicion, transform.position, volumenFinalAparicion);
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

    public bool ForzarMuertePorRuido()
    {
        if (yaAtrapado || muerteForzadaEnCurso)
        {
            return true;
        }

        if (jugador == null)
        {
            GameObject jugadorEncontrado = GameObject.FindGameObjectWithTag("Player");
            if (jugadorEncontrado != null)
            {
                jugador = jugadorEncontrado.transform;
            }
        }

        if (jugador == null)
        {
            return false;
        }

        StartCoroutine(SecuenciaAtaqueForzadoPorRuido());
        return true;
    }

    IEnumerator SecuenciaAtaqueForzadoPorRuido()
    {
        muerteForzadaEnCurso = true;
        pausadaPorMinijuego = false;

        temporizadorAparicion = 0f;
        temporizadorPatrullaje = 0f;
        temporizadorCurva = 0f;
        velocidadSuavizadaPersecucion = Vector2.zero;

        Vector2 posicionJugador = jugador != null ? (Vector2)jugador.position : (Vector2)transform.position;
        Vector2 direccion = Random.insideUnitCircle;
        if (direccion.sqrMagnitude <= 0.0001f)
        {
            direccion = Vector2.right;
        }

        direccion.Normalize();
        float distancia = Mathf.Clamp(distanciaMinimaAparicionJugador * 0.5f, 1.4f, 3.2f);
        Vector2 puntoAparicion = LimitarPosicion(posicionJugador + (direccion * distancia));

        transform.position = puntoAparicion;

        if (mostrarSenalAparicion)
        {
            StartCoroutine(MostrarSenalAparicion(puntoAparicion));
        }

        if (mostrarSenalDireccionEnMilo)
        {
            StartCoroutine(MostrarSenalDireccionEnMilo(puntoAparicion));
        }

        ActivarEstatica();
        ReproducirSonidoAparicion();

        yield return new WaitForSecondsRealtime(0.35f);

        if (!yaAtrapado)
        {
            yaAtrapado = true;
            yield return StartCoroutine(SecuenciaMuerte());
        }

        muerteForzadaEnCurso = false;
    }

    IEnumerator SecuenciaMuerte()
    {
        MensajeriaJugador.Mostrar("La Estática te alcanzó.");

        MiloController milo = FindObjectOfType<MiloController>();
        if (milo != null) milo.puedeMoverse = false;

        if (sonidoGrito != null)
        {
            float volumenFinalGrito = Mathf.Clamp01(Mathf.Max(volumenGrito, volumenMinimoGrito));

            if (fuenteAudio != null)
            {
                fuenteAudio.PlayOneShot(sonidoGrito, volumenFinalGrito);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sonidoGrito, transform.position, volumenFinalGrito);
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