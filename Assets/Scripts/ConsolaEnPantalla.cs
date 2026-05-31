using UnityEngine;
using TMPro; // Crucial para usar TextMeshPro

public class ConsolaEnPantalla : MonoBehaviour
{
    [Header("UI de la Consola")]
    public TextMeshProUGUI textoConsola;
    
    [Header("Configuración")]
    public int maxLineas = 8; // Para que no tape toda la pantalla
    public bool mostrarLogsInternos = false;

    [Header("Estilo de Mensajes")]
    [SerializeField] private Color colorMensajesJugador = new Color(0.45f, 0.98f, 0.62f, 1f);
    
    private string registroLogs = "";

    // OnEnable se conecta a la consola secreta de Unity cuando el script despierta
    void OnEnable()
    {
        Application.logMessageReceived += CapturarLog;
    }

    // OnDisable se desconecta por seguridad si apagas el objeto
    void OnDisable()
    {
        Application.logMessageReceived -= CapturarLog;
    }

    // Esta función recibe en automático cada Debug.Log de tu juego
    void CapturarLog(string mensaje, string rastro, LogType tipo)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        string mensajeLimpio = mensaje.Trim();
        string prefijoUsuario = MensajeriaJugador.Prefijo + " ";
        bool esMensajeUsuario = mensajeLimpio.StartsWith(prefijoUsuario);

        if (!esMensajeUsuario && !mostrarLogsInternos)
        {
            return;
        }

        if (esMensajeUsuario)
        {
            mensajeLimpio = mensajeLimpio.Substring(prefijoUsuario.Length);
            string colorHex = ColorUtility.ToHtmlStringRGBA(colorMensajesJugador);
            mensajeLimpio = "<color=#" + colorHex + ">" + mensajeLimpio + "</color>";
        }
        else
        {
            if (tipo == LogType.Error || tipo == LogType.Exception)
            {
                mensajeLimpio = "<color=red>" + mensajeLimpio + "</color>";
            }
            else if (tipo == LogType.Warning)
            {
                mensajeLimpio = "<color=yellow>" + mensajeLimpio + "</color>";
            }
        }

        // Agregamos el nuevo mensaje a nuestra lista
        registroLogs += mensajeLimpio + "\n";

        // Magia para borrar las líneas viejas y que no se llene la memoria
        string[] lineas = registroLogs.Split('\n');
        if (lineas.Length > maxLineas)
        {
            // Nos quedamos solo con las líneas más recientes
            registroLogs = string.Join("\n", lineas, lineas.Length - maxLineas, maxLineas);
        }

        // Lo mandamos al Canvas
        if (textoConsola != null)
        {
            textoConsola.text = registroLogs;
        }
    }
}