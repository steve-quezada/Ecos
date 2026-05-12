using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // NUEVO: Para cambiar de escena

public class ManagerRuido : MonoBehaviour
{
    public static ManagerRuido instancia;

    [Header("Configuración Visual")]
    public Slider sliderMedidor;
    
    [Header("Valores")]
    public float ruidoMaximo = 15f; 
    public float umbralEstatica = 5f; 
    
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
        
        // ¡NUEVO! Lógica de derrota si llegamos al tope
        if (ruidoActual >= ruidoMaximo) 
        {
            ruidoActual = ruidoMaximo; 
            ActualizarSliderVisual();
            PerderPorRuido();
            return; // Detenemos el código aquí para que no siga evaluando más cosas
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

    // NUEVO: Función para terminar el juego por exceso de ruido
    void PerderPorRuido()
    {
        Debug.Log("Milo hizo demasiado ruido. GAME OVER.");
        SceneManager.LoadScene("PantallaPerder");
    }
}