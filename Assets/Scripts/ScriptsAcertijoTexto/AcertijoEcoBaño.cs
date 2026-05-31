using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// Acertijo del botiquin del bano.
/// Requiere seguir una secuencia de colores para construir el mensaje final.
public class AcertijoEcoBaño : MonoBehaviour
{
    private static readonly string[] SecuenciaColoresReceta =
    {
        "rojo", "negro", "blanco", "rojo", "negro", "verde", "gris"
    };

    private static readonly string[] SecuenciaPalabrasReceta =
    {
        "TU", "LO", "VISTE", "TU", "LO", "DEJASTE", "PASAR"
    };

    [Header("Referencias")]
    public GameObject ecoEnEscena;
    public MiloController milo;

    [Header("UI General a ocultar")]
    [SerializeField] private GameObject uiObjetosObligatorios;
    [SerializeField] private GameObject uiEco;

    [Header("Sprites en el orden inicial")]
    public List<Sprite> spritesFrente = new List<Sprite>();

    [Header("Sprites en el reverso (mismo orden)")]
    public List<Sprite> spritesReverso = new List<Sprite>();

    [Header("Receta medica")]
    public Sprite imagenReceta;
    public Vector2 posicionReceta = new Vector2(0f, 80f);
    public Vector2 tamanoReceta = new Vector2(480f, 680f);

    [Header("Mensaje Revelado")]
    [TextArea(2, 4)] public string mensajeFinal = "TU LO VISTE\nTU LO DEJASTE PASAR";
    public Vector2 posicionInstruccion = new Vector2(0f, -220f);
    public Vector2 posicionMensaje = new Vector2(0f, -300f);
    public Vector2 tamanoMensaje = new Vector2(1100f, 280f);
    public Vector2 posicionContinuar = new Vector2(160f, -340f);

    [Header("Apariencia")]
    public Color colorFondo = new Color(0.92f, 0.89f, 0.84f, 0.98f);
    public Color colorBoton = new Color(0.22f, 0.20f, 0.18f, 1f);
    public Color colorTexto = new Color(0.12f, 0.10f, 0.08f, 1f);
    public Color colorMensaje = new Color(0.22f, 0.12f, 0.08f, 1f);

    [Header("Layout")]
    public Vector2 tamanoPociones = new Vector2(140f, 220f);
    public Vector2 posicionFila = new Vector2(0f, 36f);
    public Vector2 posicionCerrar = new Vector2(-160f, -340f);

    [Header("Botones (estilo habitacion)")]
    public bool usarAnclajeBotonesHabitacion = true;
    public float margenLateralBotones = 190f;
    public float margenInferiorBotones = 52f;

    [Header("Layout Adaptativo")]
    public bool ajustarLayoutSegunPantalla = true;

    private Vector2 tamanoBoton = new Vector2(340f, 94f);
    private Vector2 tamanoContenedorPociones = new Vector2(1800f, 900f);
    private float separacionY = 260f;
    private float desplazamientoColumnasX = 620f;
    private int tamanoFuenteInstruccion = 40;
    private int tamanoFuenteMensaje = 48;
    private int tamanoFuenteBoton = 44;

    private GameManager gameManager;
    private RectTransform contenedorPociones;
    private bool interfazConstruida = false;
    private bool ecoCompletado = false;

    private readonly List<PotionSlot> slots = new List<PotionSlot>();
    private Text textoInstruccion;
    private Button botonContinuar;
    private int pasoSecuenciaActual = 0;
    private string avisoSecuencia = string.Empty;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        ResolverHudMinijuego();
        interfazConstruida = false;
    }

    void OnEnable()
    {
        AsegurarPanelPantallaCompleta();

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OcultarHudProgresoEnMinijuego();
        }

        ResolverHudMinijuego();
        AplicarVisibilidadHudMinijuego(false);

        if (!interfazConstruida)
        {
            ConstruirInterfaz();
            interfazConstruida = true;
        }

        ReiniciarEstadoVisual();

        Time.timeScale = 0f;
        if (milo != null)
        {
            milo.puedeMoverse = false;
        }
    }

    void OnDisable()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.RestaurarHudProgresoTrasMinijuego();
        }

        AplicarVisibilidadHudMinijuego(true);
    }

    public void CerrarPanelSinResolver()
    {
        DescongelarJuego();
        gameObject.SetActive(false);
    }

    public void VerificarRespuesta()
    {
        // Compatibilidad con botones viejos: redirige al flujo nuevo.
        ContinuarTrasMensajeRevelado();
    }

    public void ContinuarTrasMensajeRevelado()
    {
        if (ecoCompletado)
        {
            return;
        }

        if (!SecuenciaCompletadaCorrectamente())
        {
            MensajeriaJugador.Mostrar("Aún no completas la receta correcta. Revisa el orden de colores.");
            return;
        }

        ecoCompletado = true;
        MensajeriaJugador.Mostrar("¡Muy bien! Descifraste el mensaje y recogiste el eco.");

        if (gameManager != null)
        {
            gameManager.RegistrarEcoRecolectado();
        }

        gameObject.SetActive(false);
        DescongelarJuego();

        if (ecoEnEscena != null)
        {
            Destroy(ecoEnEscena);
        }
    }

    private void DescongelarJuego()
    {
        Time.timeScale = 1f;

        if (milo != null)
        {
            milo.puedeMoverse = true;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ResolverHudMinijuego()
    {
        if (uiObjetosObligatorios == null)
        {
            uiObjetosObligatorios = GameObject.Find("ObjetosObligatorios");
        }

        if (uiEco == null)
        {
            uiEco = GameObject.Find("ECO");
            if (uiEco == null)
            {
                uiEco = GameObject.Find("ECO_Cocina");
            }
        }
    }

    private void AplicarVisibilidadHudMinijuego(bool visible)
    {
        if (uiObjetosObligatorios != null)
        {
            uiObjetosObligatorios.SetActive(visible);
        }

        if (uiEco != null)
        {
            uiEco.SetActive(visible);
        }
    }

    private void AsegurarPanelPantallaCompleta()
    {
        RectTransform rectPanel = transform as RectTransform;
        if (rectPanel != null)
        {
            rectPanel.anchorMin = Vector2.zero;
            rectPanel.anchorMax = Vector2.one;
            rectPanel.offsetMin = Vector2.zero;
            rectPanel.offsetMax = Vector2.zero;
            rectPanel.localScale = Vector3.one;
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Image fondo = GetComponent<Image>();
        if (fondo == null)
        {
            fondo = gameObject.AddComponent<Image>();
        }

        Color colorFondoOpaco = colorFondo;
        colorFondoOpaco.a = 1f;
        fondo.color = colorFondoOpaco;
        fondo.raycastTarget = true;

        transform.SetAsLastSibling();
    }

    private void ReiniciarEstadoVisual()
    {
        ecoCompletado = false;
        pasoSecuenciaActual = 0;
        avisoSecuencia = string.Empty;

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ResetVolteo();
        }

        if (textoInstruccion != null)
        {
            ActualizarTextoProgresivo();
        }

        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(false);
        }
    }

    void ConstruirInterfaz()
    {
        Transform raiz = transform;

        if (ajustarLayoutSegunPantalla)
        {
            AplicarLayoutResponsivo();
        }

        Image fondo = GetComponent<Image>();
        if (fondo != null)
        {
            Color colorFondoOpaco = colorFondo;
            colorFondoOpaco.a = 1f;
            fondo.color = colorFondoOpaco;
        }

        int cantidad = Mathf.Min(spritesFrente.Count, spritesReverso.Count);
        cantidad = Mathf.Min(cantidad, 7);

        CrearReceta(raiz);
        ReubicarCamposEntradaLegacy(raiz);

        GameObject contenedorGO = new GameObject("Pociones", typeof(RectTransform));
        contenedorGO.transform.SetParent(raiz, false);
        contenedorPociones = contenedorGO.GetComponent<RectTransform>();
        contenedorPociones.anchorMin = contenedorPociones.anchorMax = new Vector2(0.5f, 0.5f);
        contenedorPociones.pivot = new Vector2(0.5f, 0.5f);
        contenedorPociones.anchoredPosition = posicionFila;
        contenedorPociones.sizeDelta = tamanoContenedorPociones;

        for (int i = 0; i < cantidad; i++)
        {
            CrearSlotPocion(i, cantidad);
        }

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        textoInstruccion = CrearTexto(
            raiz,
            "Instruccion",
            "",
            posicionInstruccion,
            tamanoMensaje,
            tamanoFuenteInstruccion,
            colorMensaje);
        ActualizarTextoProgresivo();

        botonContinuar = CrearBoton(raiz, "Continuar", posicionContinuar, fuente, false);
        botonContinuar.onClick.AddListener(ContinuarTrasMensajeRevelado);
        botonContinuar.gameObject.SetActive(false);
    }

    void ReubicarCamposEntradaLegacy(Transform raiz)
    {
        float yBase = posicionMensaje.y - Mathf.Max(70f, tamanoMensaje.y * 0.55f);

        InputField[] camposLegacy = raiz.GetComponentsInChildren<InputField>(true);
        for (int i = 0; i < camposLegacy.Length; i++)
        {
            RectTransform rect = camposLegacy[i].GetComponent<RectTransform>();
            if (rect == null)
            {
                continue;
            }

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, yBase - (i * 88f));
            rect.sizeDelta = new Vector2(Mathf.Max(680f, tamanoMensaje.x * 0.9f), Mathf.Max(72f, rect.sizeDelta.y));
        }

        TMP_InputField[] camposTmp = raiz.GetComponentsInChildren<TMP_InputField>(true);
        for (int i = 0; i < camposTmp.Length; i++)
        {
            RectTransform rect = camposTmp[i].GetComponent<RectTransform>();
            if (rect == null)
            {
                continue;
            }

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, yBase - (i * 88f));
            rect.sizeDelta = new Vector2(Mathf.Max(680f, tamanoMensaje.x * 0.9f), Mathf.Max(72f, rect.sizeDelta.y));
        }
    }

    void CrearReceta(Transform parent)
    {
        if (imagenReceta == null)
        {
            return;
        }

        GameObject go = new GameObject("Receta", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicionReceta;
        rect.sizeDelta = tamanoReceta;

        Image img = go.GetComponent<Image>();
        img.sprite = imagenReceta;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    void CrearSlotPocion(int indice, int total)
    {
        Vector2 posicionSlot = ObtenerPosicionSlot(indice, total);

        GameObject go = new GameObject("Pocion_" + indice, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(contenedorPociones, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicionSlot;
        rect.sizeDelta = tamanoPociones;

        Image img = go.GetComponent<Image>();
        img.sprite = spritesFrente[indice];
        img.preserveAspect = true;

        string colorDetectado = DetectarColorPocion(spritesFrente[indice]);
        if (colorDetectado == "desconocido")
        {
            Debug.LogWarning("No se pudo detectar color para " + spritesFrente[indice].name + " en " + gameObject.name);
        }

        PotionSlot slot = new PotionSlot(img, spritesFrente[indice], spritesReverso[indice], colorDetectado);
        slots.Add(slot);

        Button btn = go.GetComponent<Button>();
        int idx = indice;
        btn.onClick.AddListener(() => AlternarPocion(idx));
    }

    void AlternarPocion(int indice)
    {
        if (indice < 0 || indice >= slots.Count || ecoCompletado)
        {
            return;
        }

        PotionSlot slot = slots[indice];
        if (slot.estaVolteada)
        {
            return;
        }

        if (pasoSecuenciaActual >= SecuenciaColoresReceta.Length)
        {
            EvaluarMensajeRevelado();
            return;
        }

        string colorEsperado = SecuenciaColoresReceta[pasoSecuenciaActual];
        if (slot.colorClave == colorEsperado)
        {
            slot.Revelar();
            pasoSecuenciaActual++;
            avisoSecuencia = string.Empty;
        }
        else
        {
            avisoSecuencia = "Orden incorrecto. Sigue la receta e intenta de nuevo.";
            ReiniciarIntentoSecuencia();
            return;
        }

        EvaluarMensajeRevelado();
    }

    void ReiniciarIntentoSecuencia()
    {
        pasoSecuenciaActual = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ResetVolteo();
        }

        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(false);
        }

        ActualizarTextoProgresivo();
    }

    void EvaluarMensajeRevelado()
    {
        bool secuenciaCompleta = SecuenciaCompletadaCorrectamente();
        ActualizarTextoProgresivo();

        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(secuenciaCompleta);
        }
    }

    void ActualizarTextoProgresivo()
    {
        if (textoInstruccion == null)
        {
            return;
        }

        textoInstruccion.text = ConstruirMensajeProgresivo();
    }

    string ConstruirMensajeProgresivo()
    {
        StringBuilder sb = new StringBuilder(128);
        int pasosVisibles = Mathf.Clamp(pasoSecuenciaActual, 0, SecuenciaPalabrasReceta.Length);

        for (int i = 0; i < SecuenciaPalabrasReceta.Length; i++)
        {
            if (i == 3)
            {
                sb.Append('\n');
            }
            else if (i > 0)
            {
                sb.Append(' ');
            }

            if (i < pasosVisibles)
            {
                sb.Append(SecuenciaPalabrasReceta[i]);
            }
            else
            {
                sb.Append(new string('_', SecuenciaPalabrasReceta[i].Length));
            }
        }

        if (SecuenciaCompletadaCorrectamente())
        {
            sb.Append("\n\nPresiona Continuar para recoger el eco.");
        }
        else if (!string.IsNullOrEmpty(avisoSecuencia))
        {
            sb.Append("\n\n");
            sb.Append(avisoSecuencia);
        }

        return sb.ToString();
    }

    string DetectarColorPocion(Sprite sprite)
    {
        if (sprite == null || string.IsNullOrEmpty(sprite.name))
        {
            return "desconocido";
        }

        string nombre = sprite.name.ToLowerInvariant();
        if (nombre.Contains("roj")) return "rojo";
        if (nombre.Contains("negr")) return "negro";
        if (nombre.Contains("blanc")) return "blanco";
        if (nombre.Contains("verd")) return "verde";
        if (nombre.Contains("gris") || nombre.Contains("gray")) return "gris";

        return "desconocido";
    }

    bool SecuenciaCompletadaCorrectamente()
    {
        if (pasoSecuenciaActual < SecuenciaColoresReceta.Length)
        {
            return false;
        }

        string fraseEsperada = string.Join(" ", SecuenciaPalabrasReceta);
        string normalizadaEsperada = NormalizarFrase(fraseEsperada);
        string normalizadaFinal = NormalizarFrase(mensajeFinal);
        return normalizadaEsperada == normalizadaFinal;
    }

    string NormalizarFrase(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(texto.Length);
        bool ultimoFueEspacio = false;

        for (int i = 0; i < texto.Length; i++)
        {
            char c = char.ToUpperInvariant(texto[i]);
            if (char.IsWhiteSpace(c))
            {
                if (!ultimoFueEspacio)
                {
                    sb.Append(' ');
                    ultimoFueEspacio = true;
                }
            }
            else
            {
                sb.Append(c);
                ultimoFueEspacio = false;
            }
        }

        return sb.ToString().Trim();
    }

    Vector2 ObtenerPosicionSlot(int indice, int total)
    {
        int cantidadIzquierda = Mathf.Min(4, total);
        int cantidadDerecha = Mathf.Max(0, total - cantidadIzquierda);

        if (indice < cantidadIzquierda)
        {
            float yInicioIzquierda = (cantidadIzquierda - 1) * 0.5f * separacionY;
            float y = yInicioIzquierda - indice * separacionY;
            return new Vector2(-desplazamientoColumnasX, y);
        }

        int indiceDerecha = indice - cantidadIzquierda;
        float yInicioDerecha = (cantidadDerecha - 1) * 0.5f * separacionY;
        float yDerecha = yInicioDerecha - indiceDerecha * separacionY;
        return new Vector2(desplazamientoColumnasX, yDerecha);
    }

    Text CrearTexto(
        Transform parent,
        string nombre,
        string contenido,
        Vector2 posicion,
        Vector2 tamano,
        int tamanoFuente,
        Color color)
    {
        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;

        Text texto = go.GetComponent<Text>();
        texto.font = fuente;
        texto.fontSize = tamanoFuente;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = color;
        texto.text = contenido;
        texto.raycastTarget = false;

        return texto;
    }

    Button CrearBoton(Transform parent, string label, Vector2 posicion, Font fuente, bool anclarAbajoIzquierda)
    {
        GameObject go = new GameObject(label + "_Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();

        if (usarAnclajeBotonesHabitacion)
        {
            rect.anchorMin = rect.anchorMax = anclarAbajoIzquierda ? new Vector2(0f, 0f) : new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anclarAbajoIzquierda
                ? new Vector2(margenLateralBotones, margenInferiorBotones)
                : new Vector2(-margenLateralBotones, margenInferiorBotones);
        }
        else
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
        }

        rect.sizeDelta = tamanoBoton;

        Image imagen = go.GetComponent<Image>();
        imagen.color = colorBoton;

        GameObject textoGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textoGO.transform.SetParent(go.transform, false);
        RectTransform textoRect = textoGO.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        Text texto = textoGO.GetComponent<Text>();
        texto.font = fuente;
        texto.fontSize = tamanoFuenteBoton;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
        texto.text = label;
        texto.raycastTarget = false;

        return go.GetComponent<Button>();
    }

    void AplicarLayoutResponsivo()
    {
        RectTransform rectPanel = transform as RectTransform;
        float anchoPantalla = rectPanel != null ? rectPanel.rect.width : 0f;
        float altoPantalla = rectPanel != null ? rectPanel.rect.height : 0f;

        if (anchoPantalla <= 1f || altoPantalla <= 1f)
        {
            anchoPantalla = Screen.width;
            altoPantalla = Screen.height;
        }

        float escalaAncho = Mathf.Clamp(anchoPantalla / 1920f, 0.6f, 1.3f);
        float escalaAlto = Mathf.Clamp(altoPantalla / 1080f, 0.6f, 1.3f);
        float escala = Mathf.Min(escalaAncho, escalaAlto);

        tamanoPociones = new Vector2(138f * escala, 222f * escala);
        separacionY = Mathf.Clamp(tamanoPociones.y * 1.18f, 170f, 320f);
        tamanoContenedorPociones = new Vector2(anchoPantalla * 0.98f, altoPantalla * 0.82f);

        tamanoReceta = new Vector2(
            Mathf.Clamp(anchoPantalla * 0.72f, 900f, 1500f),
            Mathf.Clamp(altoPantalla * 0.70f, 600f, 1000f));

        posicionReceta = new Vector2(
            0f,
            Mathf.Clamp(altoPantalla * 0.14f, 100f, 180f));

        posicionFila = new Vector2(
            0f,
            Mathf.Clamp(altoPantalla * 0.03f, 20f, 56f));

        float margenColumnas = Mathf.Clamp(anchoPantalla * 0.04f, 46f, 86f);
        desplazamientoColumnasX = tamanoReceta.x * 0.5f + tamanoPociones.x * 0.65f + margenColumnas;
        float maxOffset = anchoPantalla * 0.5f - tamanoPociones.x * 0.55f - 16f;
        desplazamientoColumnasX = Mathf.Min(desplazamientoColumnasX, maxOffset);

        tamanoMensaje = new Vector2(
            Mathf.Clamp(anchoPantalla * 0.72f, 880f, 1450f),
            Mathf.Clamp(altoPantalla * 0.24f, 180f, 320f));

        posicionInstruccion = new Vector2(0f, -altoPantalla * 0.35f);
        posicionMensaje = new Vector2(0f, -altoPantalla * 0.43f);

        tamanoBoton = new Vector2(
            Mathf.Clamp(anchoPantalla * 0.20f, 260f, 420f),
            Mathf.Clamp(altoPantalla * 0.084f, 68f, 106f));

        margenLateralBotones = Mathf.Clamp(anchoPantalla * 0.105f, 170f, 280f);
        margenInferiorBotones = Mathf.Clamp(altoPantalla * 0.05f, 44f, 92f);

        posicionCerrar = new Vector2(-anchoPantalla * 0.17f, -altoPantalla * 0.44f);
        posicionContinuar = new Vector2(anchoPantalla * 0.17f, -altoPantalla * 0.44f);

        tamanoFuenteInstruccion = Mathf.RoundToInt(Mathf.Clamp(altoPantalla * 0.036f, 30f, 48f));
        tamanoFuenteMensaje = Mathf.RoundToInt(Mathf.Clamp(altoPantalla * 0.048f, 38f, 62f));
        tamanoFuenteBoton = Mathf.RoundToInt(Mathf.Clamp(altoPantalla * 0.038f, 30f, 50f));
    }
}

public class PotionSlot
{
    public Sprite spriteFrente;
    public Sprite spriteReverso;
    public bool estaVolteada;
    public string colorClave;

    private readonly Image imagen;

    public PotionSlot(Image img, Sprite frente, Sprite reverso, string color)
    {
        imagen = img;
        spriteFrente = frente;
        spriteReverso = reverso;
        colorClave = color;
        estaVolteada = false;
    }

    public void Revelar()
    {
        estaVolteada = true;
        imagen.sprite = spriteReverso;
    }

    public void ResetVolteo()
    {
        estaVolteada = false;
        imagen.sprite = spriteFrente;
    }
}
