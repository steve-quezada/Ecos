using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AcertijoEcoSalaVisual : MonoBehaviour
{
    [Header("Elementos de la UI a ocultar")]
    public GameObject uiObjetosObligatorios;
    public GameObject uiEco;

    [Header("Compatibilidad con panel actual")]
    public int totalPiezas = 0;
    public EcoInteractuable ecoOso;

    [Header("Respuesta")]
    [SerializeField] private string respuestaCorrecta = "FUE UN ACCIDENTE";

    [Header("Sprites del criptograma")]
    [SerializeField] private List<Sprite> equivalenciasVisuales = new List<Sprite>();
    [SerializeField] private Sprite fraseCompletaEncriptada;

    [Header("Carga automatica de finales")]
    [SerializeField] private bool cargarSpritesFinalesDesdeAssetsAcertijos = false;
    [SerializeField] private string carpetaCriptogramaRelativa = "AssetsAcertijos/Acertijo4_Criptograma";
    [SerializeField] private string nombreImagenFrase = "Criptograma.jpeg";

    [Header("Distribucion")]
    [SerializeField] private float radioEquivalencias = 290f;
    [SerializeField] private Vector2 tamanoEquivalencia = new Vector2(116f, 64f);
    [SerializeField] private Vector2 tamanoFraseCompleta = new Vector2(680f, 320f);
    [SerializeField] private bool usarDistribucionLateralesEnSala = true;
    [SerializeField] private float separacionHorizontalFilas = 190f;
    [SerializeField] private float alturaFilaSuperior = 255f;
    [SerializeField] private float alturaFilaInferior = -255f;
    [SerializeField] private Vector2 posicionInputRespuesta = new Vector2(0f, -360f);
    [SerializeField] private Vector2 posicionBotonVerificar = new Vector2(0f, -434f);
    [SerializeField] private Vector2 posicionBotonCerrar = new Vector2(0f, -500f);

    [Header("Apariencia")]
    [SerializeField] private Color colorFondo = new Color(0.92f, 0.89f, 0.84f, 0.98f);
    [SerializeField] private Color colorPlaceholderEquivalencia = new Color(0.25f, 0.21f, 0.17f, 0.2f);
    [SerializeField] private Color colorPlaceholderFrase = new Color(0.2f, 0.2f, 0.2f, 0.2f);
    [SerializeField] private Color colorInput = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color colorBoton = new Color(0.22f, 0.2f, 0.18f, 1f);
    [SerializeField] private Color colorTexto = new Color(0.12f, 0.1f, 0.08f, 1f);

    private const int EquivalenciasTotales = 10;

    private static readonly string[] NombresEquivalenciasFinales =
    {
        "A.jpeg",
        "C.jpeg",
        "D.jpeg",
        "E.jpeg",
        "F.jpeg",
        "I.jpeg",
        "N.jpeg",
        "O.jpeg",
        "T.jpeg",
        "U.jpeg"
    };

    private GameManager gameManager;
    private RectTransform raizGenerada;
    private InputField inputRespuesta;
    private Font fuenteUi;
    private Sprite spriteBlanco;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnEnable()
    {
        if (uiObjetosObligatorios != null) uiObjetosObligatorios.SetActive(false);
        if (uiEco != null) uiEco.SetActive(false);

        ConstruirInterfazSiHaceFalta();
        LimpiarInput();
    }

    void OnDisable()
    {
        if (uiObjetosObligatorios != null) uiObjetosObligatorios.SetActive(true);
        if (uiEco != null) uiEco.SetActive(true);
    }

    public void VerificarRespuesta()
    {
        if (inputRespuesta == null)
        {
            Debug.LogWarning("AcertijoEcoSalaVisual: no se encontro InputField para validar.");
            return;
        }

        string respuestaJugador = NormalizarTexto(inputRespuesta.text);
        string respuestaSecreta = NormalizarTexto(respuestaCorrecta);

        if (respuestaJugador == respuestaSecreta)
        {
            Debug.Log("Correcto. Milo resolvio el criptograma de Sala y recogio el Eco.");

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (gameManager != null)
            {
                gameManager.RegistrarEcoRecolectado();
            }

            CerrarYConsumirEco();
        }
        else
        {
            Debug.Log("Respuesta incorrecta. Intenta de nuevo.");
            inputRespuesta.text = string.Empty;
            inputRespuesta.ActivateInputField();
        }
    }

    public void CerrarSinResolver()
    {
        CerrarPanel();
    }

    private void ConstruirInterfazSiHaceFalta()
    {
        if (raizGenerada != null)
        {
            return;
        }

        CargarSpritesFinales();

        fuenteUi = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuenteUi == null)
        {
            fuenteUi = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        spriteBlanco = CrearSpriteBlanco();

        GameObject raiz = new GameObject("CriptogramaSala_UI", typeof(RectTransform), typeof(Image));
        raiz.transform.SetParent(transform, false);
        raiz.transform.SetAsLastSibling();

        raizGenerada = raiz.GetComponent<RectTransform>();
        raizGenerada.anchorMin = new Vector2(0f, 0f);
        raizGenerada.anchorMax = new Vector2(1f, 1f);
        raizGenerada.offsetMin = Vector2.zero;
        raizGenerada.offsetMax = Vector2.zero;

        Image fondo = raiz.GetComponent<Image>();
        fondo.sprite = spriteBlanco;
        fondo.color = colorFondo;
        fondo.raycastTarget = true;

        CrearCirculoEquivalencias(raizGenerada);
        CrearMensajeCentral(raizGenerada);
        CrearInputYBotones(raizGenerada);
    }

    private void CargarSpritesFinales()
    {
        if (!cargarSpritesFinalesDesdeAssetsAcertijos)
        {
            return;
        }

        string carpetaAbsoluta = ObtenerRutaAbsolutaCarpetaCriptograma();
        if (!Directory.Exists(carpetaAbsoluta))
        {
            Debug.LogWarning("AcertijoEcoSalaVisual: no existe carpeta de criptograma en " + carpetaAbsoluta);
            return;
        }

        List<Sprite> equivalenciasCargadas = new List<Sprite>(EquivalenciasTotales);
        for (int i = 0; i < NombresEquivalenciasFinales.Length; i++)
        {
            string nombreArchivo = NombresEquivalenciasFinales[i];
            string ruta = Path.Combine(carpetaAbsoluta, nombreArchivo);
            Sprite sprite = CargarSpriteDesdeArchivo(ruta, "Eq_" + nombreArchivo);
            if (sprite != null)
            {
                equivalenciasCargadas.Add(sprite);
            }
        }

        if (equivalenciasCargadas.Count > 0)
        {
            equivalenciasVisuales.Clear();
            equivalenciasVisuales.AddRange(equivalenciasCargadas);
        }

        string rutaFrase = Path.Combine(carpetaAbsoluta, nombreImagenFrase);
        Sprite fraseCargada = CargarSpriteDesdeArchivo(rutaFrase, "FraseCriptogramaSala");
        if (fraseCargada != null)
        {
            fraseCompletaEncriptada = fraseCargada;
        }
    }

    private string ObtenerRutaAbsolutaCarpetaCriptograma()
    {
        string relativa = carpetaCriptogramaRelativa.Replace('\\', '/').Trim();
        if (relativa.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            relativa = relativa.Substring("Assets/".Length);
        }

        string relativaSistema = relativa.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(Application.dataPath, relativaSistema);
    }

    private Sprite CargarSpriteDesdeArchivo(string ruta, string nombreSprite)
    {
        if (!File.Exists(ruta))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(ruta);
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        Texture2D textura = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        textura.name = nombreSprite + "_Tex";
        textura.hideFlags = HideFlags.HideAndDontSave;

        if (!textura.LoadImage(bytes, false))
        {
            Destroy(textura);
            return null;
        }

        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;

        Sprite sprite = Sprite.Create(textura, new Rect(0f, 0f, textura.width, textura.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = nombreSprite;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private void CrearCirculoEquivalencias(RectTransform parent)
    {
        int cantidad = EquivalenciasTotales;
        if (cantidad <= 0)
        {
            return;
        }

        for (int i = 0; i < cantidad; i++)
        {
            GameObject nodo = new GameObject("Equivalencia_" + i, typeof(RectTransform), typeof(Image));
            nodo.transform.SetParent(parent, false);

            RectTransform rect = nodo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = tamanoEquivalencia;
            rect.anchoredPosition = ObtenerPosicionEquivalencia(i, cantidad);

            Image imagen = nodo.GetComponent<Image>();
            Sprite sprite = i < equivalenciasVisuales.Count ? equivalenciasVisuales[i] : null;
            if (sprite != null)
            {
                imagen.sprite = sprite;
                imagen.color = Color.white;
                imagen.preserveAspect = true;
            }
            else
            {
                imagen.sprite = spriteBlanco;
                imagen.color = colorPlaceholderEquivalencia;
            }

            imagen.raycastTarget = false;
        }
    }

    private Vector2 ObtenerPosicionEquivalencia(int indice, int cantidad)
    {
        if (usarDistribucionLateralesEnSala && cantidad == EquivalenciasTotales)
        {
            int indiceEnFila = indice % 5;
            float x = (indiceEnFila - 2) * separacionHorizontalFilas;
            float y = indice < 5 ? alturaFilaSuperior : alturaFilaInferior;
            return new Vector2(x, y);
        }

        float angulo = (360f / cantidad) * indice - 90f;
        float rad = angulo * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radioEquivalencias;
    }

    private void CrearMensajeCentral(RectTransform parent)
    {
        GameObject mensaje = new GameObject("MensajeEncriptado", typeof(RectTransform), typeof(Image));
        mensaje.transform.SetParent(parent, false);

        RectTransform rect = mensaje.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 10f);
        rect.sizeDelta = tamanoFraseCompleta;

        Image imagenMensaje = mensaje.GetComponent<Image>();
        if (fraseCompletaEncriptada != null)
        {
            imagenMensaje.sprite = fraseCompletaEncriptada;
            imagenMensaje.color = Color.white;
            imagenMensaje.preserveAspect = true;
        }
        else
        {
            imagenMensaje.sprite = spriteBlanco;
            imagenMensaje.color = colorPlaceholderFrase;
            imagenMensaje.preserveAspect = false;
        }

        imagenMensaje.raycastTarget = false;
    }

    private void CrearInputYBotones(RectTransform parent)
    {
        RectTransform inputRect;
        inputRespuesta = CrearInputField(parent, posicionInputRespuesta, new Vector2(480f, 54f), out inputRect);

        Button botonVerificar = CrearBoton(parent, "Verificar", posicionBotonVerificar, new Vector2(190f, 52f));
        botonVerificar.onClick.AddListener(VerificarRespuesta);

        Button botonCerrar = CrearBoton(parent, "Cerrar", posicionBotonCerrar, new Vector2(190f, 44f));
        botonCerrar.onClick.AddListener(CerrarSinResolver);
    }

    private InputField CrearInputField(RectTransform parent, Vector2 posicion, Vector2 tamano, out RectTransform rect)
    {
        GameObject inputGo = new GameObject("RespuestaInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputGo.transform.SetParent(parent, false);

        rect = inputGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;

        Image fondo = inputGo.GetComponent<Image>();
        fondo.sprite = spriteBlanco;
        fondo.color = colorInput;

        InputField input = inputGo.GetComponent<InputField>();
        input.characterLimit = 32;

        GameObject textoGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textoGo.transform.SetParent(inputGo.transform, false);
        RectTransform textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0f, 0f);
        textoRect.anchorMax = new Vector2(1f, 1f);
        textoRect.offsetMin = new Vector2(14f, 8f);
        textoRect.offsetMax = new Vector2(-14f, -8f);

        Text texto = textoGo.GetComponent<Text>();
        texto.font = fuenteUi;
        texto.fontSize = 28;
        texto.alignment = TextAnchor.MiddleLeft;
        texto.color = colorTexto;

        GameObject placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        placeholderGo.transform.SetParent(inputGo.transform, false);
        RectTransform placeholderRect = placeholderGo.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0f, 0f);
        placeholderRect.anchorMax = new Vector2(1f, 1f);
        placeholderRect.offsetMin = new Vector2(14f, 8f);
        placeholderRect.offsetMax = new Vector2(-14f, -8f);

        Text placeholder = placeholderGo.GetComponent<Text>();
        placeholder.font = fuenteUi;
        placeholder.fontSize = 24;
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.color = new Color(colorTexto.r, colorTexto.g, colorTexto.b, 0.45f);
        placeholder.text = "Escribe aqui...";

        input.textComponent = texto;
        input.placeholder = placeholder;
        input.targetGraphic = fondo;

        return input;
    }

    private Button CrearBoton(RectTransform parent, string textoBoton, Vector2 posicion, Vector2 tamano)
    {
        GameObject botonGo = new GameObject(textoBoton + "_Boton", typeof(RectTransform), typeof(Image), typeof(Button));
        botonGo.transform.SetParent(parent, false);

        RectTransform rect = botonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;

        Image fondo = botonGo.GetComponent<Image>();
        fondo.sprite = spriteBlanco;
        fondo.color = colorBoton;

        Button boton = botonGo.GetComponent<Button>();

        GameObject textoGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textoGo.transform.SetParent(botonGo.transform, false);
        RectTransform textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0f, 0f);
        textoRect.anchorMax = new Vector2(1f, 1f);
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        Text texto = textoGo.GetComponent<Text>();
        texto.font = fuenteUi;
        texto.fontSize = 24;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
        texto.text = textoBoton;

        return boton;
    }

    private void LimpiarInput()
    {
        if (inputRespuesta == null)
        {
            return;
        }

        inputRespuesta.text = string.Empty;
        inputRespuesta.ActivateInputField();
    }

    private void CerrarYConsumirEco()
    {
        CerrarPanel();

        if (ecoOso != null)
        {
            Destroy(ecoOso.gameObject);
        }
    }

    private void CerrarPanel()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (ecoOso == null)
        {
            ecoOso = FindObjectOfType<EcoInteractuable>();
        }

        if (ecoOso != null)
        {
            ecoOso.CerrarRompecabezas();
        }
        else
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

    private string NormalizarTexto(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(texto.Length);
        for (int i = 0; i < texto.Length; i++)
        {
            char c = texto[i];
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(char.ToUpperInvariant(c));
            }
        }

        return sb.ToString();
    }

    private Sprite CrearSpriteBlanco()
    {
        Texture2D textura = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        textura.name = "CriptogramaSala_White";
        textura.hideFlags = HideFlags.HideAndDontSave;
        textura.SetPixel(0, 0, Color.white);
        textura.Apply();

        Sprite sprite = Sprite.Create(textura, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "CriptogramaSala_WhiteSprite";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}