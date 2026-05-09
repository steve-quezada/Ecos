using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoCaida : MonoBehaviour
{
    private AudioSource reproductor;
    private bool yaSono = false;

    void Start()
    {
        reproductor = GetComponent<AudioSource>();
    }

    // Esta función de Unity se dispara automáticamente cuando el Rigidbody choca con algo
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si no ha sonado todavía, lo reproducimos
        if (!yaSono)
        {
            reproductor.Play();
            yaSono = true; // Ponemos el seguro para que no vuelva a sonar si la guitarra rueda
        }
    }
}