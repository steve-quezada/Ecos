using UnityEngine;

public static class MensajeriaJugador
{
    public const string Prefijo = "[USUARIO]";

    public static void Mostrar(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        Debug.Log(Prefijo + " " + mensaje.Trim());
    }
}
