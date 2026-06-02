using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    [Header("Identificación Visual")]
    public bool usarIndicadorTecla = false;
    public bool mostrarIndicadorSoloEnRango = false;
    public Vector3 offsetIndicadorAutomatico = new Vector3(0f, 1.2f, 0f);
    public Color colorIndicadorTrampa = new Color(1f, 0.55f, 0.1f, 1f);
    [Range(0f, 0.3f)] public float amplitudPulsoIndicador = 0.08f;
    public float velocidadPulsoIndicador = 4f;

    [Header("Contorno del Escondite Trampa")]
    public bool mostrarContorno = true;
    public bool mostrarContornoSoloEnRango = false;
    public Color colorContornoTrampa = new Color(0.78f, 0.57f, 0.42f, 0.52f);
    [Range(0.01f, 0.25f)] public float grosorContorno = 0.06f;
    [Range(0f, 0.2f)] public float amplitudPulsoContorno = 0.012f;
    public float velocidadPulsoContorno = 1.8f;
    [Range(12, 72)] public int segmentosCirculoContorno = 28;
    public int ordenRenderContorno = 20;

    [Header("Efectos de Muerte")]
    public GameObject efectoFuegoVisual; 
    public AudioSource fuenteAudio;
    public AudioClip sonidoQuemaduraOGrito;
    [Range(0f, 1f)] public float volumenGrito = 1f;
    [Range(0f, 1f)] public float normalizacionVolumenGrito = 0.85f;

    private MiloController milo;
    private Vector3 escalaBaseIndicador = Vector3.one;
    private LineRenderer contornoRenderer;
    private float grosorBaseContorno = 0f;

    void Start()
    {
        // Todos los colliders de la trampa deben ser trigger.
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.isTrigger = true;
        }

        InicializarIndicadorVisual();
        InicializarContornoVisual();

        if (efectoFuegoVisual != null)
        {
            efectoFuegoVisual.SetActive(false);
        }
    }

    void Update()
    {
        ActualizarPulsoIndicador();
        ActualizarPulsoContorno();

        if (jugadorEnRango && !yaActivado && Input.GetKeyDown(teclaInteraccion))
        {
            yaActivado = true;
            
            // Apagamos la [E] porque ya activó la trampa
            if (usarIndicadorTecla && indicadorTeclaE != null) indicadorTeclaE.SetActive(false);

            ActualizarVisibilidadContorno();
            
            StartCoroutine(SecuenciaMuerteFuego());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = true;
            milo = collision.GetComponent<MiloController>();

            // Asegurar que Milo se dibuje por encima del contorno siempre.
            SpriteRenderer miloSprite = collision.GetComponent<SpriteRenderer>();
            if (miloSprite != null && contornoRenderer != null)
            {
                if (miloSprite.sortingOrder <= contornoRenderer.sortingOrder)
                {
                    miloSprite.sortingOrder = contornoRenderer.sortingOrder + 1;
                }
            }

            ActualizarVisibilidadIndicador();
            ActualizarVisibilidadContorno();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            milo = null;
            ActualizarVisibilidadIndicador();
            ActualizarVisibilidadContorno();
        }
    }

    void InicializarIndicadorVisual()
    {
        ValidarReferenciaIndicador();

        if (!usarIndicadorTecla)
        {
            OcultarIndicadoresEExistentes();
            return;
        }

        if (indicadorTeclaE == null)
        {
            TMP_Text textoTmp = GetComponentInChildren<TMP_Text>(true);
            if (textoTmp != null && textoTmp.text.Contains("E"))
            {
                indicadorTeclaE = textoTmp.gameObject;
            }
        }

        if (indicadorTeclaE == null)
        {
            indicadorTeclaE = CrearIndicadorAutomatico();
        }

        if (indicadorTeclaE == null)
        {
            return;
        }

        escalaBaseIndicador = indicadorTeclaE.transform.localScale;
        AplicarColorIndicador();
        ActualizarVisibilidadIndicador();
    }

    void OcultarIndicadoresEExistentes()
    {
        ValidarReferenciaIndicador();

        if (indicadorTeclaE != null)
        {
            indicadorTeclaE.SetActive(false);
        }

        TMP_Text[] textosTmp = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textosTmp.Length; i++)
        {
            if (textosTmp[i] == null)
            {
                continue;
            }

            string contenido = textosTmp[i].text.Trim();
            if (contenido == "E" || contenido == "[E]" || contenido == "(E)")
            {
                textosTmp[i].gameObject.SetActive(false);
            }
        }

        TextMesh[] textos3D = GetComponentsInChildren<TextMesh>(true);
        for (int i = 0; i < textos3D.Length; i++)
        {
            if (textos3D[i] == null)
            {
                continue;
            }

            string contenido = textos3D[i].text.Trim();
            if (contenido == "E" || contenido == "[E]" || contenido == "(E)")
            {
                textos3D[i].gameObject.SetActive(false);
            }
        }
    }

    GameObject CrearIndicadorAutomatico()
    {
        GameObject indicador = new GameObject("IndicadorE_Auto");
        indicador.transform.SetParent(transform, false);
        indicador.transform.localPosition = offsetIndicadorAutomatico;

        TextMesh texto = indicador.AddComponent<TextMesh>();
        texto.text = "[E]";
        texto.fontSize = 72;
        texto.characterSize = 0.06f;
        texto.anchor = TextAnchor.MiddleCenter;
        texto.alignment = TextAlignment.Center;
        texto.color = colorIndicadorTrampa;

        MeshRenderer render = indicador.GetComponent<MeshRenderer>();
        if (render != null)
        {
            render.sortingOrder = 150;
        }

        return indicador;
    }

    void AplicarColorIndicador()
    {
        if (indicadorTeclaE == null)
        {
            return;
        }

        TMP_Text[] textosTmp = indicadorTeclaE.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textosTmp.Length; i++)
        {
            textosTmp[i].color = colorIndicadorTrampa;
        }

        TextMesh[] textos3D = indicadorTeclaE.GetComponentsInChildren<TextMesh>(true);
        for (int i = 0; i < textos3D.Length; i++)
        {
            textos3D[i].color = colorIndicadorTrampa;
        }

        SpriteRenderer[] sprites = indicadorTeclaE.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = colorIndicadorTrampa;
        }
    }

    void ActualizarVisibilidadIndicador()
    {
        ValidarReferenciaIndicador();

        if (!usarIndicadorTecla || indicadorTeclaE == null)
        {
            return;
        }

        bool visible = !mostrarIndicadorSoloEnRango || jugadorEnRango;
        indicadorTeclaE.SetActive(visible && !yaActivado);
    }

    void ActualizarPulsoIndicador()
    {
        ValidarReferenciaIndicador();

        if (!usarIndicadorTecla || indicadorTeclaE == null || !indicadorTeclaE.activeSelf)
        {
            return;
        }

        float pulso = 1f + Mathf.Sin(Time.time * velocidadPulsoIndicador) * amplitudPulsoIndicador;
        indicadorTeclaE.transform.localScale = escalaBaseIndicador * pulso;
    }

    void ValidarReferenciaIndicador()
    {
        if (indicadorTeclaE == gameObject)
        {
            Debug.LogWarning("El indicadorTeclaE apunta al mismo objeto de la trampa en: " + gameObject.name + ". Se ignorará para evitar ocultarlo al iniciar.");
            indicadorTeclaE = null;
        }
    }

    void InicializarContornoVisual()
    {
        if (!mostrarContorno)
        {
            return;
        }

        SpriteRenderer spriteBase = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();
        if (spriteBase == null && col == null)
        {
            Debug.LogWarning("EsconditeTrampa sin sprite/collider para contorno en: " + gameObject.name);
            return;
        }

        GameObject nodoContorno = new GameObject("ContornoEsconditeTrampa");
        nodoContorno.transform.SetParent(transform, false);
        nodoContorno.transform.localPosition = Vector3.zero;
        nodoContorno.transform.localRotation = Quaternion.identity;
        nodoContorno.transform.localScale = Vector3.one;

        contornoRenderer = nodoContorno.AddComponent<LineRenderer>();
        contornoRenderer.useWorldSpace = false;
        contornoRenderer.loop = true;
        contornoRenderer.textureMode = LineTextureMode.Stretch;
        contornoRenderer.alignment = LineAlignment.View;
        contornoRenderer.numCornerVertices = 4;
        contornoRenderer.numCapVertices = 2;
        contornoRenderer.receiveShadows = false;
        contornoRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.hideFlags = HideFlags.HideAndDontSave;
        contornoRenderer.material = mat;

        if (spriteBase != null)
        {
            contornoRenderer.sortingLayerID = spriteBase.sortingLayerID;
            contornoRenderer.sortingOrder = spriteBase.sortingOrder;
        }
        else
        {
            contornoRenderer.sortingOrder = ordenRenderContorno;
        }

        grosorBaseContorno = Mathf.Max(0.01f, grosorContorno);
        contornoRenderer.startWidth = grosorBaseContorno;
        contornoRenderer.endWidth = grosorBaseContorno;

        bool contornoConstruido = ConstruirContornoDesdeSprite(spriteBase);
        if (!contornoConstruido)
        {
            contornoConstruido = ConstruirContornoDesdeCollider(col);
        }
        if (!contornoConstruido && spriteBase != null)
        {
            contornoConstruido = ConstruirContornoDesdeBoundsSprite(spriteBase);
        }
        if (!contornoConstruido && col != null)
        {
            contornoConstruido = ConstruirContornoDesdeBoundsCollider(col);
        }

        if (!contornoConstruido)
        {
            Debug.LogWarning("No se pudo construir contorno para escondite trampa en: " + gameObject.name);
            contornoRenderer.enabled = false;
            return;
        }

        AplicarColorContorno();
        ActualizarVisibilidadContorno();
    }

    bool ConstruirContornoDesdeSprite(SpriteRenderer spriteBase)
    {
        if (contornoRenderer == null || spriteBase == null || spriteBase.sprite == null)
        {
            return false;
        }

        Sprite sprite = spriteBase.sprite;
        int cantidadFormas = sprite.GetPhysicsShapeCount();
        if (cantidadFormas <= 0)
        {
            return false;
        }

        List<Vector2> mejorForma = null;
        float mejorArea = 0f;
        List<Vector2> formaActual = new List<Vector2>(64);

        for (int i = 0; i < cantidadFormas; i++)
        {
            formaActual.Clear();
            sprite.GetPhysicsShape(i, formaActual);
            if (formaActual.Count < 3)
            {
                continue;
            }

            float area = Mathf.Abs(CalcularAreaPoligono(formaActual));
            if (area > mejorArea)
            {
                mejorArea = area;
                if (mejorForma == null)
                {
                    mejorForma = new List<Vector2>(formaActual.Count);
                }
                else
                {
                    mejorForma.Clear();
                }

                for (int p = 0; p < formaActual.Count; p++)
                {
                    mejorForma.Add(formaActual[p]);
                }
            }
        }

        if (mejorForma == null || mejorForma.Count < 3)
        {
            return false;
        }

        float flipX = spriteBase.flipX ? -1f : 1f;
        float flipY = spriteBase.flipY ? -1f : 1f;

        contornoRenderer.positionCount = mejorForma.Count;
        for (int i = 0; i < mejorForma.Count; i++)
        {
            Vector2 p = mejorForma[i];
            contornoRenderer.SetPosition(i, new Vector3(p.x * flipX, p.y * flipY, 0f));
        }

        return true;
    }

    bool ConstruirContornoDesdeCollider(Collider2D col)
    {
        if (contornoRenderer == null || col == null)
        {
            return false;
        }

        if (col is PolygonCollider2D poly && poly.pathCount > 0)
        {
            int mejorIndice = -1;
            float mejorArea = 0f;

            for (int i = 0; i < poly.pathCount; i++)
            {
                Vector2[] path = poly.GetPath(i);
                if (path == null || path.Length < 3)
                {
                    continue;
                }

                float area = Mathf.Abs(CalcularAreaPoligono(path));
                if (area > mejorArea)
                {
                    mejorArea = area;
                    mejorIndice = i;
                }
            }

            if (mejorIndice >= 0)
            {
                Vector2[] mejorPath = poly.GetPath(mejorIndice);
                contornoRenderer.positionCount = mejorPath.Length;
                for (int i = 0; i < mejorPath.Length; i++)
                {
                    contornoRenderer.SetPosition(i, new Vector3(mejorPath[i].x, mejorPath[i].y, 0f));
                }

                return true;
            }
        }

        if (col is CircleCollider2D circle)
        {
            int segs = Mathf.Max(12, segmentosCirculoContorno);
            contornoRenderer.positionCount = segs;
            float paso = Mathf.PI * 2f / segs;
            for (int i = 0; i < segs; i++)
            {
                float ang = i * paso;
                Vector2 p = circle.offset + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * circle.radius;
                contornoRenderer.SetPosition(i, new Vector3(p.x, p.y, 0f));
            }

            return true;
        }

        if (col is CapsuleCollider2D capsule)
        {
            int segs = Mathf.Max(18, segmentosCirculoContorno);
            Vector2 semiejes = capsule.size * 0.5f;
            contornoRenderer.positionCount = segs;
            float paso = Mathf.PI * 2f / segs;
            for (int i = 0; i < segs; i++)
            {
                float ang = i * paso;
                Vector2 p = capsule.offset + new Vector2(Mathf.Cos(ang) * semiejes.x, Mathf.Sin(ang) * semiejes.y);
                contornoRenderer.SetPosition(i, new Vector3(p.x, p.y, 0f));
            }

            return true;
        }

        if (col is BoxCollider2D box)
        {
            Vector2 half = box.size * 0.5f;
            Vector2 o = box.offset;
            Vector3[] puntos = new Vector3[4]
            {
                new Vector3(o.x - half.x, o.y - half.y, 0f),
                new Vector3(o.x - half.x, o.y + half.y, 0f),
                new Vector3(o.x + half.x, o.y + half.y, 0f),
                new Vector3(o.x + half.x, o.y - half.y, 0f)
            };
            contornoRenderer.positionCount = puntos.Length;
            contornoRenderer.SetPositions(puntos);
            return true;
        }

        return false;
    }

    bool ConstruirContornoDesdeBoundsSprite(SpriteRenderer spriteBase)
    {
        if (contornoRenderer == null || spriteBase == null)
        {
            return false;
        }

        return ConstruirRectanguloDesdeBoundsMundo(spriteBase.bounds);
    }

    bool ConstruirContornoDesdeBoundsCollider(Collider2D col)
    {
        if (contornoRenderer == null || col == null)
        {
            return false;
        }

        return ConstruirRectanguloDesdeBoundsMundo(col.bounds);
    }

    bool ConstruirRectanguloDesdeBoundsMundo(Bounds bounds)
    {
        if (bounds.size.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        Vector3 min = transform.InverseTransformPoint(new Vector3(bounds.min.x, bounds.min.y, 0f));
        Vector3 max = transform.InverseTransformPoint(new Vector3(bounds.max.x, bounds.max.y, 0f));
        Vector3[] rect = new Vector3[4]
        {
            new Vector3(min.x, min.y, 0f),
            new Vector3(min.x, max.y, 0f),
            new Vector3(max.x, max.y, 0f),
            new Vector3(max.x, min.y, 0f)
        };
        contornoRenderer.positionCount = rect.Length;
        contornoRenderer.SetPositions(rect);
        return true;
    }

    float CalcularAreaPoligono(IList<Vector2> puntos)
    {
        if (puntos == null || puntos.Count < 3)
        {
            return 0f;
        }

        float area = 0f;
        for (int i = 0; i < puntos.Count; i++)
        {
            int j = (i + 1) % puntos.Count;
            area += (puntos[i].x * puntos[j].y) - (puntos[j].x * puntos[i].y);
        }

        return area * 0.5f;
    }

    void AplicarColorContorno()
    {
        if (contornoRenderer == null)
        {
            return;
        }

        contornoRenderer.startColor = colorContornoTrampa;
        contornoRenderer.endColor = colorContornoTrampa;
    }

    void ActualizarVisibilidadContorno()
    {
        if (contornoRenderer == null)
        {
            return;
        }

        bool visible = mostrarContorno && (!mostrarContornoSoloEnRango || jugadorEnRango) && !yaActivado;
        contornoRenderer.enabled = visible;
    }

    void ActualizarPulsoContorno()
    {
        if (contornoRenderer == null || !contornoRenderer.enabled)
        {
            return;
        }

        float pulso = 1f + Mathf.Sin(Time.time * velocidadPulsoContorno) * amplitudPulsoContorno;
        float ancho = Mathf.Max(0.01f, grosorBaseContorno * pulso);
        contornoRenderer.startWidth = ancho;
        contornoRenderer.endWidth = ancho;
    }

    IEnumerator SecuenciaMuerteFuego()
    {
        MensajeriaJugador.Mostrar("¡Era una trampa!");

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