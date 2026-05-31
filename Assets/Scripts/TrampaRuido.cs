using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrampaRuido : MonoBehaviour
{
    private const float VolumenMaximoMetalGlobal = 0.102f;

    [Header("Configuración de la Trampa")]
    public AudioClip sonidoTrampa;
    public float cantidadDeRuido = 1f; // Sumará exactamente 1 punto

    [Header("Normalización de Volumen")]
    [Tooltip("Tope de volumen para sonidos de metal/trampa para evitar picos exagerados")]
    [Range(0f, 1f)] public float volumenMaximoMetal = VolumenMaximoMetalGlobal;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ForzarVolumenMetalGlobal();
    }

    void OnValidate()
    {
        ForzarVolumenMetalGlobal();
    }

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("TrampaRuido sin AudioSource en: " + gameObject.name);
            return;
        }

        audioSource.clip = sonidoTrampa;
        audioSource.playOnAwake = false;
        AplicarNormalizacionVolumen();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            AplicarNormalizacionVolumen();

            if (sonidoTrampa != null && audioSource.clip != sonidoTrampa)
            {
                audioSource.clip = sonidoTrampa;
            }

            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        if (ManagerRuido.instancia != null)
        {
            ManagerRuido.instancia.AgregarRuido(cantidadDeRuido);
        }
        else
        {
            Debug.LogWarning("No existe ManagerRuido.instancia en la escena para: " + gameObject.name);
        }

        MensajeriaJugador.Mostrar("Pisaste una trampa de ruido.");
    }

    void AplicarNormalizacionVolumen()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.volume = Mathf.Min(audioSource.volume, volumenMaximoMetal);
    }

    void ForzarVolumenMetalGlobal()
    {
        volumenMaximoMetal = VolumenMaximoMetalGlobal;
    }
}