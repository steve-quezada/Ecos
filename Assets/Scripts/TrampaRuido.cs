using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrampaRuido : MonoBehaviour
{
    [Header("Configuración de la Trampa")]
    public AudioClip sonidoTrampa;
    public float cantidadDeRuido = 1f; // Sumará exactamente 1 punto

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

        Debug.Log("Milo piso una trampa de sonido.");
    }
}