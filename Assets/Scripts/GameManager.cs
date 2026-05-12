using System.Collections.Generic;
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

    [Header("Inventario por Habitación")]
    [SerializeField] private List<string> objetosRequeridos = new List<string>();
    [SerializeField] private List<string> objetosRecolectados = new List<string>();

    [Header("Compatibilidad (Legacy)")]
    public bool tieneMochila = false;
    public bool tieneLlave = false;

    void Start()
    {
        RegistrarObjetosRequeridosEnEscena();
    }

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

        Debug.Log("¡Recogiste " + nombreNormalizado + "!");
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
}