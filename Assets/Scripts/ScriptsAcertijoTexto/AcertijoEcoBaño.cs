using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Acertijo del botiquín del baño
/// El jugador voltea 7 pociones para ver la palabra en el reverso,
/// las arrastra para reordenarlas y escribe la frase resultante.
/// Respuesta correcta: "TU LO VISTE TU LO DEJASTE PASAR"
public class AcertijoEcoBaño : MonoBehaviour
{
    // Referencias de escena
    [Header("Referencias")]
    public GameObject    ecoEnEscena;
    public MiloController milo;

    //Sprites de las pociones
    [Header("Sprites en el orden inicial")]
    public List<Sprite> spritesFrente = new List<Sprite>();

    [Header("Sprites — Reverso (mismo orden que los de frente)")]
    public List<Sprite> spritesReverso = new List<Sprite>();

    [Header("Receta médica")]
    public Sprite imagenReceta;
    public Vector2 posicionReceta = new Vector2(-280f, 60f);
    public Vector2 tamanoReceta   = new Vector2(220f, 320f);

    //Apariencia
    [Header("Apariencia")]
    public Color colorFondo    = new Color(0.92f, 0.89f, 0.84f, 0.98f);
    public Color colorBoton    = new Color(0.22f, 0.20f, 0.18f, 1f);
    public Color colorTexto    = new Color(0.12f, 0.10f, 0.08f, 1f);
    public Color colorInput    = new Color(1f,    1f,    1f,    0.95f);

    [Header("Layout")]
    public Vector2 tamanoPociones    = new Vector2(100f, 160f);
    public float   separacionX       = 110f;
    public Vector2 posicionFila      = new Vector2(0f,  60f);
    public Vector2 posicionInput     = new Vector2(0f, -230f);
    public Vector2 posicionVerificar = new Vector2(-105f, -305f);
    public Vector2 posicionCerrar    = new Vector2( 105f, -305f);

    [Header("Layout Adaptativo")]
    public bool ajustarLayoutSegunPantalla = true;

    private Vector2 tamanoInput = new Vector2(980f, 110f);
    private Vector2 tamanoBoton = new Vector2(320f, 90f);
    private Vector2 tamanoContenedorPociones = new Vector2(1700f, 860f);
    private float separacionY = 135f;
    private float desplazamientoColumnasX = 540f;
    private int tamanoFuenteInput = 42;
    private int tamanoFuenteBoton = 42;
    private float paddingInput = 14f;

    //Privados
    private GameManager      gameManager;
    private InputField       inputRespuesta;
    private RectTransform    contenedorPociones;
    private bool             interfazConstruida = false;

    // Cada poción tiene un índice de slot (posición en la fila) y si está volteada
    private List<PociónSlot> slots = new List<PociónSlot>();

    private const string RespuestaCorrecta = "TULOVISTETULODEJASTEPASAR";

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        interfazConstruida = false;
    }

    // EcoInteractuable activa el Panel con SetActive(true),
    // lo que dispara OnEnable — aquí construimos la interfaz y congelamos.
    void OnEnable()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
 
        if (!interfazConstruida)
        {
            ConstruirInterfaz();
            interfazConstruida = true;
        }
 
        foreach (var s in slots) s.ResetVolteo();
        if (inputRespuesta != null) inputRespuesta.text = string.Empty;
 
        Time.timeScale = 0f;
        if (milo != null) milo.puedeMoverse = false;
    }
    public void CerrarPanelSinResolver()
    {
        DescongelarJuego();
        gameObject.SetActive(false);
    }

    private void DescongelarJuego()
    {
        Time.timeScale = 1f;
        if (milo != null) milo.puedeMoverse = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    //Verificar
    public void VerificarRespuesta()
    {
        string jugador = Normalizar(inputRespuesta.text);
        if (jugador == RespuestaCorrecta)
        {
            Debug.Log("¡Correcto! Eco del baño recolectado.");

            if (gameManager != null) gameManager.RegistrarEcoRecolectado();

            gameObject.SetActive(false);
            DescongelarJuego();

            if (ecoEnEscena != null) Destroy(ecoEnEscena);
        }
        else
        {
            Debug.Log("Incorrecto. Intenta de nuevo.");
            inputRespuesta.text = string.Empty;
            inputRespuesta.ActivateInputField();
        }
    }

    //Construcción de la UI
    void ConstruirInterfaz()
    {
    Transform raiz = transform;

    if (ajustarLayoutSegunPantalla)
    {
        AplicarLayoutResponsivo();
    }

    // Fondo
    Image fondo = GetComponent<Image>();
    if (fondo != null) fondo.color = colorFondo;

        int cantidad = Mathf.Min(spritesFrente.Count, spritesReverso.Count);
        cantidad = Mathf.Min(cantidad, 7);

        // Receta médica
        CrearReceta(raiz);

        // Contenedor de pociones
        GameObject contenedorGO = new GameObject("Pociones", typeof(RectTransform));
        contenedorGO.transform.SetParent(raiz, false);
        contenedorPociones = contenedorGO.GetComponent<RectTransform>();
        contenedorPociones.anchorMin = contenedorPociones.anchorMax = new Vector2(0.5f, 0.5f);
        contenedorPociones.pivot     = new Vector2(0.5f, 0.5f);
        contenedorPociones.anchoredPosition = posicionFila;
        contenedorPociones.sizeDelta = tamanoContenedorPociones;

        // Crear slots de pociones

        for (int i = 0; i < cantidad; i++)
        {
            CrearSlotPocion(i, cantidad);
        }

        // Input de respuesta
        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        inputRespuesta = CrearInputField(raiz, fuente);

        // Botones
        Button btnVerificar = CrearBoton(raiz, "Verificar", posicionVerificar, fuente);
        btnVerificar.onClick.AddListener(VerificarRespuesta);

        Button btnCerrar = CrearBoton(raiz, "Cerrar", posicionCerrar, fuente);
        btnCerrar.onClick.AddListener(CerrarPanelSinResolver);
    }

    void CrearReceta(Transform parent)
    {
        if (imagenReceta == null) return;

        GameObject go = new GameObject("Receta", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicionReceta;
        rect.sizeDelta        = tamanoReceta;

        Image img = go.GetComponent<Image>();
        img.sprite        = imagenReceta;
        img.preserveAspect = true;
        img.raycastTarget  = false;
    }

    void CrearSlotPocion(int indice, int total)
    {
        Vector2 posicionSlot = ObtenerPosicionSlot(indice, total);

        // GameObject de la poción
        GameObject go = new GameObject("Pocion_" + indice,
            typeof(RectTransform), typeof(Image),
            typeof(Button));

        go.transform.SetParent(contenedorPociones, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicionSlot;
        rect.sizeDelta = tamanoPociones;

        Image img = go.GetComponent<Image>();
        img.sprite = spritesFrente[indice];
        img.preserveAspect = true;

        // Drag & Drop
        DraggablePocion drag = go.AddComponent<DraggablePocion>();
        drag.Init(this, indice);

        // Flip al click
        PociónSlot slot = new PociónSlot(img, spritesFrente[indice], spritesReverso[indice]);
        slots.Add(slot);

        Button btn = go.GetComponent<Button>();
        int idx = indice; // captura para lambda
        btn.onClick.AddListener(() => slots[idx].Voltear());
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

    InputField CrearInputField(Transform parent, Font fuente)
    {
        GameObject go = new GameObject("InputRespuesta",
            typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicionInput;
        rect.sizeDelta = tamanoInput;

        go.GetComponent<Image>().color = colorInput;

        // Texto
        GameObject textoGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textoGO.transform.SetParent(go.transform, false);
        SetFullRect(textoGO.GetComponent<RectTransform>(), paddingInput);
        Text texto = textoGO.GetComponent<Text>();
        texto.font = fuente; texto.fontSize = tamanoFuenteInput;
        texto.alignment = TextAnchor.MiddleLeft;
        texto.color = colorTexto;

        // Placeholder
        GameObject phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        phGO.transform.SetParent(go.transform, false);
        SetFullRect(phGO.GetComponent<RectTransform>(), paddingInput);
        Text ph = phGO.GetComponent<Text>();
        ph.font = fuente; ph.fontSize = Mathf.Max(24, tamanoFuenteInput - 4);
        ph.alignment = TextAnchor.MiddleLeft;
        ph.color = new Color(colorTexto.r, colorTexto.g, colorTexto.b, 0.4f);
        ph.text = "Escribe la frase en orden...";

        InputField input = go.GetComponent<InputField>();
        input.textComponent = texto;
        input.placeholder   = ph;
        input.characterLimit = 60;

        return input;
    }

    Button CrearBoton(Transform parent, string label, Vector2 pos, Font fuente)
    {
        GameObject go = new GameObject(label + "_Btn",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = tamanoBoton;

        go.GetComponent<Image>().color = colorBoton;

        GameObject textoGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textoGO.transform.SetParent(go.transform, false);
        SetFullRect(textoGO.GetComponent<RectTransform>(), 0f);
        Text t = textoGO.GetComponent<Text>();
        t.font = fuente; t.fontSize = tamanoFuenteBoton;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text  = label;

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

        tamanoPociones = new Vector2(102f * escala, 162f * escala);
        separacionY = Mathf.Clamp(tamanoPociones.y * 1.04f, 108f, 185f);
        tamanoContenedorPociones = new Vector2(anchoPantalla * 0.96f, altoPantalla * 0.74f);

        tamanoReceta = new Vector2(
            Mathf.Clamp(anchoPantalla * 0.70f, 640f, 1280f),
            Mathf.Clamp(altoPantalla * 0.66f, 430f, 800f));

        posicionReceta = new Vector2(
            0f,
            Mathf.Clamp(altoPantalla * 0.04f, 10f, 70f));

        posicionFila = new Vector2(
            0f,
            -altoPantalla * 0.01f);

        float margenColumnas = Mathf.Clamp(anchoPantalla * 0.03f, 30f, 62f);
        desplazamientoColumnasX = tamanoReceta.x * 0.5f + tamanoPociones.x * 0.65f + margenColumnas;
        float maxOffset = anchoPantalla * 0.5f - tamanoPociones.x * 0.55f - 16f;
        desplazamientoColumnasX = Mathf.Min(desplazamientoColumnasX, maxOffset);

        tamanoInput = new Vector2(
            Mathf.Clamp(anchoPantalla * 0.52f, 640f, 1080f),
            Mathf.Clamp(altoPantalla * 0.090f, 70f, 116f));

        tamanoBoton = new Vector2(
            Mathf.Clamp(anchoPantalla * 0.19f, 230f, 380f),
            Mathf.Clamp(altoPantalla * 0.072f, 58f, 90f));

        posicionInput = new Vector2(0f, -altoPantalla * 0.35f);
        posicionVerificar = new Vector2(-anchoPantalla * 0.11f, -altoPantalla * 0.44f);
        posicionCerrar = new Vector2(anchoPantalla * 0.11f, -altoPantalla * 0.44f);

        tamanoFuenteInput = Mathf.RoundToInt(Mathf.Clamp(altoPantalla * 0.037f, 24f, 42f));
        tamanoFuenteBoton = Mathf.RoundToInt(Mathf.Clamp(altoPantalla * 0.034f, 22f, 38f));
        paddingInput = Mathf.Clamp(altoPantalla * 0.009f, 8f, 14f);
    }

    void SetFullRect(RectTransform r, float padding)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(padding, padding);
        r.offsetMax = new Vector2(-padding, -padding);
    }

    //Drag & Drop (intercambia posiciones)
    public void IntercambiarSlots(int desde, int hasta)
    {
        if (desde == hasta) return;
        if (desde < 0 || hasta < 0 || desde >= slots.Count || hasta >= slots.Count) return;

        // Intercambiar los sprites de los slots (frente y reverso)
        PociónSlot a = slots[desde];
        PociónSlot b = slots[hasta];

        Sprite tmpFrente  = a.spriteFrente;
        Sprite tmpReverso = a.spriteReverso;
        bool   tmpVolteo  = a.estaVolteada;

        a.Reemplazar(b.spriteFrente, b.spriteReverso, b.estaVolteada);
        b.Reemplazar(tmpFrente, tmpReverso, tmpVolteo);
    }

    //Utilidades
    static string Normalizar(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in s)
            if (!char.IsWhiteSpace(c))
                sb.Append(char.ToUpperInvariant(c));
        return sb.ToString();
    }
}

public class PociónSlot
{
    public Sprite spriteFrente;
    public Sprite spriteReverso;
    public bool   estaVolteada;

    private Image imagen;

    public PociónSlot(Image img, Sprite frente, Sprite reverso)
    {
        imagen        = img;
        spriteFrente  = frente;
        spriteReverso = reverso;
        estaVolteada  = false;
    }

    public void Voltear()
    {
        estaVolteada  = !estaVolteada;
        imagen.sprite = estaVolteada ? spriteReverso : spriteFrente;
    }

    public void ResetVolteo()
    {
        estaVolteada  = false;
        imagen.sprite = spriteFrente;
    }

    public void Reemplazar(Sprite frente, Sprite reverso, bool volteada)
    {
        spriteFrente  = frente;
        spriteReverso = reverso;
        estaVolteada  = volteada;
        imagen.sprite = volteada ? reverso : frente;
    }
}

// Componente que maneja el drag de una poción dentro del panel.
// Al soltarla sobre otra poción, las intercambia.
public class DraggablePocion : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private AcertijoEcoBaño manager;
    private int                  miIndice;

    private RectTransform  rect;
    private Canvas         canvas;
    private CanvasGroup    canvasGroup;
    private Vector2        posicionOriginal;
    private Transform      parentOriginal;

    public void Init(AcertijoEcoBaño mgr, int indice)
    {
        manager  = mgr;
        miIndice = indice;
        rect     = GetComponent<RectTransform>();
    }

    void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas      = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        posicionOriginal = rect.anchoredPosition;
        parentOriginal   = transform.parent;

        // Subir al Canvas raíz para que se vea encima de todo
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        rect.anchoredPosition += e.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(parentOriginal, true);

        // Buscar si soltamos sobre otra poción
        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(e, resultados);

        int destino = -1;
        foreach (var r in resultados)
        {
            DraggablePocion otro = r.gameObject.GetComponent<DraggablePocion>();
            if (otro != null && otro != this)
            {
                destino = otro.miIndice;
                break;
            }
        }

        if (destino >= 0)
            manager.IntercambiarSlots(miIndice, destino);

        // Regresar a posición visual (el manager ya actualizó los sprites)
        rect.anchoredPosition = posicionOriginal;
    }
}
