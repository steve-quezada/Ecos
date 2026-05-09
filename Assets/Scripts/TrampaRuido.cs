using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrampaRuido : MonoBehaviour
{
    [Header("Configuración de la Trampa")]
    public AudioClip sonidoTrampa;
    public float cantidadDeRuido = 1f; // Sumará exactamente 1 punto

    private AudioSource audioSource;

    void Start()
    {
        // Configuramos el reproductor de audio internamente
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = sonidoTrampa;
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos que sea Milo (Player) quien entra en la trampa
        if (collision.CompareTag("Player"))
        {
            // Reproducimos el sonido (ej. los vidrios)
            audioSource.Play();
            
            // Enviamos exactamente 1 punto al medidor oficial
            ManagerRuido.instancia.AgregarRuido(cantidadDeRuido);
            
            // Confirmación en consola del script oficial
            Debug.Log("Milo pisó una trampa de sonido.");
        }
    }
}