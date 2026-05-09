using UnityEngine;
using UnityEngine.UI;

public class ManagerRuido : MonoBehaviour
{
    public static ManagerRuido instancia;

    [Header("Configuración Visual")]
    public Slider sliderMedidor;
    
    [Header("Valores")]
    public float ruidoMaximo = 15f; 
    public float umbralEstatica = 5f; 
    
    // AQUÍ ESTÁ EL PUBLIC QUE UNITY SE NIEGA A LEER
    public float ruidoActual = 0f; 
    
    private bool estaticaInvocada = false;

    void Awake()
    {
        if (instancia == null) instancia = this;
    }

    void Start()
    {
        ActualizarSliderVisual();
    }

    public void AgregarRuido(float cantidad)
    {
        ruidoActual += cantidad;
        
        if (ruidoActual > ruidoMaximo) 
        {
            ruidoActual = ruidoMaximo; 
        }
        
        ActualizarSliderVisual();
        Debug.Log("SUBE RUIDO -> Nivel real: " + ruidoActual);

        if (ruidoActual >= umbralEstatica && !estaticaInvocada)
        {
            InvocarEstatica();
        }
    }

    public void ReducirRuido(float cantidad)
    {
        ruidoActual -= cantidad;
        
        if (ruidoActual < 0f) 
        {
            ruidoActual = 0f; 
        }

        if (ruidoActual < umbralEstatica)
        {
            estaticaInvocada = false;
        }
        
        ActualizarSliderVisual();
        Debug.Log("BAJA RUIDO -> Nivel real: " + ruidoActual);
    }

    private void ActualizarSliderVisual()
    {
        if (sliderMedidor != null)
        {
            sliderMedidor.maxValue = ruidoMaximo; 
            sliderMedidor.value = ruidoActual;
        }
    }

    void InvocarEstatica()
    {
        estaticaInvocada = true;
        Debug.Log("¡EL RUIDO LLEGÓ A 5! La Estática ha aparecido.");
    }
}