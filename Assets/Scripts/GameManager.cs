using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Sistema de Ruido")]
    public int nivelDeRuido = 0;
    public int ruidoMaximo = 10;
    public bool miloOculto = false; 

    [Header("Reducción de Ruido")]
    public float tiempoParaRestar = 1f; // 1 segundo
    private float temporizador = 0f;

    [Header("Inventario - Cuarto de Milo")]
    public bool tieneMochila = false;
    public bool tieneLlave = false;

    void Update()
    {
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

        nivelDeRuido += cantidad;
        Debug.Log("¡Hiciste ruido! Nivel actual: " + nivelDeRuido);

        if (nivelDeRuido >= ruidoMaximo)
        {
            Debug.Log("¡Alcanzaste el ruido máximo! La Estática te ha encontrado.");
        }
    }

    // Esta función la llamaremos cuando Milo toque un objeto
    public void RecogerObjeto(string nombreObjeto)
    {
        if (nombreObjeto == "Mochila")
        {
            tieneMochila = true;
            Debug.Log("¡Recogiste la Mochila!");
        }
        else if (nombreObjeto == "Llave")
        {
            tieneLlave = true;
            Debug.Log("¡Recogiste la Llave!");
        }
    }

    // Esta función la usará la puerta para saber si te deja salir
    public bool TieneTodosLosObjetos()
    {
        return tieneMochila && tieneLlave;
    }
}