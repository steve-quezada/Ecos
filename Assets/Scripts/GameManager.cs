using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Sistema de Ruido")]
    public int nivelDeRuido = 0;
    public int ruidoMaximo = 10;
    public bool miloOculto = false; 

    [Header("Reducción de Ruido")]
    public float tiempoParaRestar = 1f; // 1 segundo
    private float temporizador = 0f;

    [Header("Inventario por Habitación")]
    [SerializeField] private List<string> objetosRequeridos = new List<string>();
    [SerializeField] private List<string> objetosRecolectados = new List<string>();

    [Header("UI de Progreso")]
    [SerializeField] private Image iconoObjetosObligatorios;
    [SerializeField] private Image iconoEco;
    [SerializeField] private Color colorLineaTomado = new Color(0.86f, 0.18f, 0.18f, 0.82f);
    [SerializeField] private float anguloLineaDiagonal = -24f;
    [SerializeField] private float grosorLinea = 4f;
    [SerializeField] private float largoRelativoLinea = 0.72f;
    [SerializeField] private float margenHorizontalLinea = 16f;
    [SerializeField] private float desplazamientoVerticalLineaObjetos = -8f;
    [SerializeField] private float desplazamientoHorizontalLineaPrimerObjeto = -6f;
    [SerializeField] private float desplazamientoHorizontalLineaSegundoObjeto = 6f;

    [Header("Escala UI 1080p")]
    [SerializeField] private bool ajustarObjetosObligatoriosEn1080 = true;
    [SerializeField] private Vector2 tamanoObjetosObligatorios1080 = new Vector2(84f, 84f);
    [SerializeField] private float escalaObjetosObligatorios1080 = 3.8f;

    private readonly List<Image> lineasObjetosTomados = new List<Image>();
    private readonly List<string> objetosOrdenVisual = new List<string>();
    private Image lineaEcoTomado;
    private Sprite spriteLinea;
    private static Sprite spriteLineaCompartido;
    private bool ecoRecolectado = false;
    private int bloqueosHudMinijuego = 0;
    private EstaticaController estaticaController;

    [Header("Compatibilidad (Legacy)")]
    public bool tieneMochila = false;
    public bool tieneLlave = false;

    void Start()
    {
        RegistrarObjetosRequeridosEnEscena();
        ResolverReferenciasUI();
        AjustarRecuadroObjetosPara1080();
        PrepararMarcasEstado();
        ActualizarEstadoUI();
    }

    void Update()
    {
        if (ManagerRuido.instancia != null)
        {
            // Con ManagerRuido activo, el ruido se gestiona en un solo lugar.
            temporizador = 0f;
            return;
        }

        // Si Milo está escondido y el ruido es mayor a 0, empezamos a contar el tiempo
        if (miloOculto && nivelDeRuido > 0)
        {
            temporizador += Time.deltaTime; // Time.deltaTime cuenta los segundos reales

            if (temporizador >= tiempoParaRestar)
            {
                nivelDeRuido--; // Restamos 1 punto
                temporizador = 0f; // Reiniciamos el cronómetro
                Debug.Log("Relajándose... Nivel de ruido bajó a: " + nivelDeRuido);
            }
        }
        else
        {
            // Si Milo sale del escondite o el ruido llega a 0, reiniciamos el temporizador
            temporizador = 0f;
        }
    }

    public void AgregarRuido(int cantidad)
    {
        if (miloOculto) return;

        if (ManagerRuido.instancia != null)
        {
            ManagerRuido.instancia.AgregarRuido(cantidad);
            return;
        }

        nivelDeRuido += cantidad;
        MensajeriaJugador.Mostrar("Hiciste ruido. Nivel actual: " + nivelDeRuido + "/" + ruidoMaximo + ".");

        if (nivelDeRuido >= ruidoMaximo)
        {
            MensajeriaJugador.Mostrar("¡Cuidado! El ruido llegó al máximo.");
        }
    }

    // Esta función la llamaremos cuando Milo toque un objeto
    public void RecogerObjeto(string nombreObjeto)
    {
        string nombreNormalizado = NormalizarNombreObjeto(nombreObjeto);

        if (string.IsNullOrEmpty(nombreNormalizado))
        {
            Debug.LogWarning("Intentaste recoger un objeto sin nombre.");
            return;
        }

        if (!objetosRequeridos.Contains(nombreNormalizado))
        {
            objetosRequeridos.Add(nombreNormalizado);
        }

        if (objetosRecolectados.Contains(nombreNormalizado))
        {
            return;
        }

        objetosRecolectados.Add(nombreNormalizado);

        if (nombreNormalizado == "Mochila")
        {
            tieneMochila = true;
        }
        else if (nombreNormalizado == "Llave")
        {
            tieneLlave = true;
        }

        MensajeriaJugador.Mostrar("Recogiste: " + nombreNormalizado + ".");
        ActualizarEstadoUI();
    }

    public void RegistrarEcoRecolectado()
    {
        if (ecoRecolectado)
        {
            return;
        }

        ecoRecolectado = true;
        MensajeriaJugador.Mostrar("Eco recolectado.");
        ActualizarEstadoUI();
    }

    // Esta función la usará la puerta para saber si te deja salir
    public bool TieneTodosLosObjetos()
    {
        if (objetosRequeridos.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < objetosRequeridos.Count; i++)
        {
            if (!objetosRecolectados.Contains(objetosRequeridos[i]))
            {
                return false;
            }
        }

        return true;
    }

    public string ObtenerObjetosFaltantes()
    {
        List<string> faltantes = new List<string>();

        for (int i = 0; i < objetosRequeridos.Count; i++)
        {
            string objeto = objetosRequeridos[i];
            if (!objetosRecolectados.Contains(objeto))
            {
                faltantes.Add(objeto);
            }
        }

        return string.Join(", ", faltantes);
    }

    private void RegistrarObjetosRequeridosEnEscena()
    {
        objetosRequeridos.Clear();
        objetosRecolectados.Clear();
        tieneMochila = false;
        tieneLlave = false;
        ecoRecolectado = false;

        ObjetoObligatorio[] objetos = FindObjectsOfType<ObjetoObligatorio>();

        for (int i = 0; i < objetos.Length; i++)
        {
            string nombreNormalizado = NormalizarNombreObjeto(objetos[i].nombreDelObjeto);

            if (string.IsNullOrEmpty(nombreNormalizado))
            {
                continue;
            }

            if (!objetosRequeridos.Contains(nombreNormalizado))
            {
                objetosRequeridos.Add(nombreNormalizado);
            }
        }
    }

    private string NormalizarNombreObjeto(string nombreObjeto)
    {
        if (string.IsNullOrWhiteSpace(nombreObjeto))
        {
            return string.Empty;
        }

        return nombreObjeto.Trim();
    }

    private void ResolverReferenciasUI()
    {
        if (iconoObjetosObligatorios == null)
        {
            GameObject objetoUI = GameObject.Find("ObjetosObligatorios");
            if (objetoUI != null)
            {
                iconoObjetosObligatorios = objetoUI.GetComponent<Image>();
            }
        }

        if (iconoEco == null)
        {
            GameObject ecoUI = GameObject.Find("ECO");
            if (ecoUI != null)
            {
                iconoEco = ecoUI.GetComponent<Image>();
            }
        }

        if (iconoEco == null)
        {
            GameObject ecoUICocina = GameObject.Find("ECO_Cocina");
            if (ecoUICocina != null)
            {
                iconoEco = ecoUICocina.GetComponent<Image>();
            }
        }
    }

    private void AjustarRecuadroObjetosPara1080()
    {
        if (!ajustarObjetosObligatoriosEn1080 || iconoObjetosObligatorios == null)
        {
            return;
        }

        RectTransform rect = iconoObjetosObligatorios.rectTransform;
        rect.sizeDelta = tamanoObjetosObligatorios1080;
        rect.localScale = new Vector3(escalaObjetosObligatorios1080, escalaObjetosObligatorios1080, 1f);
    }

    private void PrepararMarcasEstado()
    {
        spriteLinea = ObtenerSpriteLineaBasica();

        AsegurarMascaraRecorte(iconoObjetosObligatorios);
        AsegurarMascaraRecorte(iconoEco);

        PrepararMarcasObjetos();
        lineaEcoTomado = CrearMarcaDiagonal(iconoEco, "MarcaEcoTomado", 0.5f, ObtenerAnchoRect(iconoEco, 100f), 0f, 0f);
    }

    private Sprite ObtenerSpriteLineaBasica()
    {
        if (spriteLineaCompartido != null)
        {
            return spriteLineaCompartido;
        }

        Texture2D textura = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        textura.name = "LineaDiagonalUI_Tex";
        textura.hideFlags = HideFlags.HideAndDontSave;
        textura.SetPixel(0, 0, Color.white);
        textura.Apply();
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;

        Sprite sprite = Sprite.Create(textura, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "LineaDiagonalUI_Sprite";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        spriteLineaCompartido = sprite;

        return spriteLineaCompartido;
    }

    private void PrepararMarcasObjetos()
    {
        lineasObjetosTomados.Clear();
        ReconstruirOrdenVisualObjetos();

        if (iconoObjetosObligatorios == null)
        {
            return;
        }

        LimpiarMarcasObjetosExistentes();

        int totalMarcas = Mathf.Max(0, objetosOrdenVisual.Count);
        float anchoIcono = ObtenerAnchoRect(iconoObjetosObligatorios, 100f);
        float anchoFranja = totalMarcas > 0 ? anchoIcono / totalMarcas : anchoIcono;

        for (int i = 0; i < totalMarcas; i++)
        {
            float posicionHorizontal = ObtenerPosicionHorizontalMarca(i, totalMarcas);
            float desplazamientoHorizontal = ObtenerDesplazamientoHorizontalMarca(i, totalMarcas);
            Image marca = CrearMarcaDiagonal(iconoObjetosObligatorios, "MarcaObjetoTomado_" + i, posicionHorizontal, anchoFranja, desplazamientoHorizontal, desplazamientoVerticalLineaObjetos);
            lineasObjetosTomados.Add(marca);
        }
    }

    private void ReconstruirOrdenVisualObjetos()
    {
        objetosOrdenVisual.Clear();

        for (int i = 0; i < objetosRequeridos.Count; i++)
        {
            string nombre = objetosRequeridos[i];
            if (string.IsNullOrEmpty(nombre))
            {
                continue;
            }

            if (!objetosOrdenVisual.Contains(nombre))
            {
                objetosOrdenVisual.Add(nombre);
            }
        }

        objetosOrdenVisual.Sort(CompararObjetosVisualmente);
    }

    private int CompararObjetosVisualmente(string a, string b)
    {
        bool aEsLlave = EsLlave(a);
        bool bEsLlave = EsLlave(b);

        if (aEsLlave && !bEsLlave)
        {
            return 1;
        }

        if (!aEsLlave && bEsLlave)
        {
            return -1;
        }

        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private bool EsLlave(string nombreObjeto)
    {
        return string.Equals(nombreObjeto, "Llave", StringComparison.OrdinalIgnoreCase);
    }

    private void AsegurarMascaraRecorte(Image icono)
    {
        if (icono == null)
        {
            return;
        }

        RectMask2D mascara = icono.GetComponent<RectMask2D>();
        if (mascara == null)
        {
            icono.gameObject.AddComponent<RectMask2D>();
        }
    }

    private void LimpiarMarcasObjetosExistentes()
    {
        if (iconoObjetosObligatorios == null)
        {
            return;
        }

        List<Transform> nodosAEliminar = new List<Transform>();
        Transform iconoTransform = iconoObjetosObligatorios.transform;

        for (int i = 0; i < iconoTransform.childCount; i++)
        {
            Transform child = iconoTransform.GetChild(i);
            if (child.name.StartsWith("MarcaObjetoTomado_"))
            {
                nodosAEliminar.Add(child);
            }
        }

        for (int i = 0; i < nodosAEliminar.Count; i++)
        {
            Destroy(nodosAEliminar[i].gameObject);
        }
    }

    private Image CrearMarcaDiagonal(Image icono, string nombreNodo, float posicionHorizontal, float anchoFranja, float desplazamientoHorizontal, float desplazamientoVertical)
    {
        if (icono == null)
        {
            return null;
        }

        Transform existente = icono.transform.Find(nombreNodo);
        GameObject nodo;
        Image imagenMarca;

        if (existente != null)
        {
            nodo = existente.gameObject;
            imagenMarca = existente.GetComponent<Image>();
            if (imagenMarca == null)
            {
                imagenMarca = nodo.AddComponent<Image>();
            }
        }
        else
        {
            nodo = new GameObject(nombreNodo);
            nodo.transform.SetParent(icono.transform, false);
            imagenMarca = nodo.AddComponent<Image>();
        }

        nodo.transform.SetAsLastSibling();

        RectTransform rect = nodo.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = nodo.AddComponent<RectTransform>();
        }

        float anchoPadre = ObtenerAnchoRect(icono, 100f);
        float altoPadre = ObtenerAltoRect(icono, 100f);
        float anchoDisponible = Mathf.Max(grosorLinea + 2f, anchoFranja * 0.72f);
        float altoDisponible = Mathf.Max(grosorLinea + 2f, altoPadre * 0.72f);
        float largoMaximoSeguro = CalcularLargoMaximoSeguro(anchoDisponible, altoDisponible);
        float largoObjetivo = Mathf.Min(anchoDisponible, altoDisponible) * Mathf.Clamp01(largoRelativoLinea);
        float largoLinea = Mathf.Clamp(largoObjetivo, grosorLinea + 2f, largoMaximoSeguro * 0.96f);

        if (float.IsNaN(largoLinea) || float.IsInfinity(largoLinea) || largoLinea <= grosorLinea)
        {
            largoLinea = Mathf.Max(grosorLinea + 2f, Mathf.Min(anchoPadre, altoPadre) * 0.5f);
        }

        rect.anchorMin = new Vector2(posicionHorizontal, 0.5f);
        rect.anchorMax = new Vector2(posicionHorizontal, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(desplazamientoHorizontal, desplazamientoVertical);
        rect.sizeDelta = new Vector2(grosorLinea, largoLinea);
        rect.localRotation = Quaternion.Euler(0f, 0f, anguloLineaDiagonal);

        if (spriteLinea != null)
        {
            imagenMarca.sprite = spriteLinea;
        }

        imagenMarca.type = Image.Type.Simple;
        imagenMarca.preserveAspect = false;
        imagenMarca.raycastTarget = false;
        imagenMarca.color = colorLineaTomado;
        imagenMarca.enabled = false;

        return imagenMarca;
    }

    private float CalcularLargoMaximoSeguro(float anchoDisponible, float altoDisponible)
    {
        float anguloRad = Mathf.Abs(anguloLineaDiagonal) * Mathf.Deg2Rad;
        float sin = Mathf.Abs(Mathf.Sin(anguloRad));
        float cos = Mathf.Abs(Mathf.Cos(anguloRad));

        float maxPorAncho = float.PositiveInfinity;
        if (sin > 0.0001f)
        {
            float restanteAncho = anchoDisponible - (grosorLinea * cos);
            maxPorAncho = restanteAncho > 0f ? restanteAncho / sin : 0f;
        }

        float maxPorAlto = float.PositiveInfinity;
        if (cos > 0.0001f)
        {
            float restanteAlto = altoDisponible - (grosorLinea * sin);
            maxPorAlto = restanteAlto > 0f ? restanteAlto / cos : 0f;
        }

        float largoSeguro = Mathf.Min(maxPorAncho, maxPorAlto);
        if (float.IsNaN(largoSeguro) || float.IsInfinity(largoSeguro) || largoSeguro <= 0f)
        {
            largoSeguro = Mathf.Min(anchoDisponible, altoDisponible);
        }

        return Mathf.Max(grosorLinea + 2f, largoSeguro);
    }

    private float ObtenerAnchoRect(Image icono, float fallback)
    {
        if (icono == null)
        {
            return fallback;
        }

        float ancho = icono.rectTransform.rect.width;
        return ancho > 0.1f ? ancho : fallback;
    }

    private float ObtenerAltoRect(Image icono, float fallback)
    {
        if (icono == null)
        {
            return fallback;
        }

        float alto = icono.rectTransform.rect.height;
        return alto > 0.1f ? alto : fallback;
    }

    private float ObtenerPosicionHorizontalMarca(int indice, int total)
    {
        if (total <= 1)
        {
            return 0.5f;
        }

        float anchoIcono = 100f;
        if (iconoObjetosObligatorios != null)
        {
            float anchoRect = iconoObjetosObligatorios.rectTransform.rect.width;
            if (anchoRect > 0.1f)
            {
                anchoIcono = anchoRect;
            }
        }

        float margenNormalizado = Mathf.Clamp01(margenHorizontalLinea / anchoIcono);
        float inicio = margenNormalizado;
        float fin = 1f - margenNormalizado;
        float t = (indice + 0.5f) / total;

        return Mathf.Lerp(inicio, fin, t);
    }

    private float ObtenerDesplazamientoHorizontalMarca(int indice, int total)
    {
        if (total == 2)
        {
            if (indice == 0)
            {
                return desplazamientoHorizontalLineaPrimerObjeto;
            }

            if (indice == 1)
            {
                return desplazamientoHorizontalLineaSegundoObjeto;
            }
        }

        return 0f;
    }

    private void AsegurarCantidadMarcasObjetos()
    {
        ReconstruirOrdenVisualObjetos();

        if (lineasObjetosTomados.Count != objetosOrdenVisual.Count)
        {
            PrepararMarcasObjetos();
        }
    }

    private void AsegurarMarcaEco()
    {
        if (lineaEcoTomado == null && iconoEco != null)
        {
            lineaEcoTomado = CrearMarcaDiagonal(iconoEco, "MarcaEcoTomado", 0.5f, ObtenerAnchoRect(iconoEco, 100f), 0f, 0f);
        }
    }

    private void ActualizarLineasObjetos()
    {
        AsegurarCantidadMarcasObjetos();

        for (int i = 0; i < lineasObjetosTomados.Count; i++)
        {
            Image linea = lineasObjetosTomados[i];
            if (linea == null)
            {
                continue;
            }

            bool objetoTomado = i < objetosOrdenVisual.Count && objetosRecolectados.Contains(objetosOrdenVisual[i]);
            linea.enabled = objetoTomado;
        }
    }

    private void ActualizarLineaEco()
    {
        AsegurarMarcaEco();
        if (lineaEcoTomado != null)
        {
            lineaEcoTomado.enabled = ecoRecolectado;
        }
    }

    private void ActualizarEstadoUI()
    {
        ActualizarLineasObjetos();
        ActualizarLineaEco();
    }

    public void OcultarHudProgresoEnMinijuego()
    {
        ResolverReferenciasUI();

        if (bloqueosHudMinijuego == 0)
        {
            AplicarPausaEstaticaEnMinijuego(true);
        }

        bloqueosHudMinijuego++;
        AplicarVisibilidadHudProgreso(false);
    }

    public void RestaurarHudProgresoTrasMinijuego()
    {
        ResolverReferenciasUI();

        if (bloqueosHudMinijuego > 0)
        {
            bloqueosHudMinijuego--;
        }

        if (bloqueosHudMinijuego == 0)
        {
            AplicarPausaEstaticaEnMinijuego(false);
            AplicarVisibilidadHudProgreso(true);
        }
    }

    private void AplicarPausaEstaticaEnMinijuego(bool pausar)
    {
        if (estaticaController == null)
        {
            estaticaController = FindObjectOfType<EstaticaController>();
        }

        if (estaticaController == null)
        {
            return;
        }

        if (pausar)
        {
            estaticaController.PausarPorMinijuego();
        }
        else
        {
            estaticaController.ReanudarTrasMinijuego();
        }
    }

    private void AplicarVisibilidadHudProgreso(bool visible)
    {
        if (iconoObjetosObligatorios != null)
        {
            iconoObjetosObligatorios.gameObject.SetActive(visible);
        }

        if (iconoEco != null)
        {
            iconoEco.gameObject.SetActive(visible);
        }
    }
}
