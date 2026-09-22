using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public class CPHInline
{
    public bool Execute()
    {
        try
        {
            string input = args.ContainsKey("rawInput")
                ? args["rawInput"]?.ToString()
                : "";

            if (string.IsNullOrWhiteSpace(input))
            {
                CPH.LogWarn("LoquendoBridge: no se recibió rawInput.");
                return false;
            }
            //Limite de caracteres del mensaje, si cambias el limite debes agregar esta linea a Arguments
            //--max-length ElNumeroDeseado
            if (input.Length > 300)
                input = input.Substring(0, 300);

            // VOZ: cambia "Jorge" por Carlos, Carmen, Diego, Esperanza,
            // Francisca, Leonor, Ludoviko o Soledad.
            string voice = "Jorge";

            // Ajusta esta ruta a la ubicación de tu LoquendoBridge.exe.
            string exePath = @"C:\Users\chano\Downloads\Programs\Loquendo Bridge v1.0\LoquendoBridge.exe";

            if (!File.Exists(exePath))
            {
                CPH.LogError("LoquendoBridge.exe no existe en: " + exePath);
                return false;
            }

            // Antes de cada mensaje se mencionara el usuario y "dice"  
            // si no te gusta esta opcion puedes comentar las lineas
            string user = args.ContainsKey("user") ? args["user"]?.ToString() : "usuario";
            input = "@" + user + " dice: \"" + input + "\"";

            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                // Si no usas VB-CABLE, cambia CABLE Input por default.
                Arguments = "--voice \"" + voice + "\" --device \"default\" --max-length 300 --text64 " + base64,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
            CPH.LogInfo("LoquendoBridge: mensaje enviado a " + voice + ".");
            return true;
        }
        catch (Exception ex)
        {
            CPH.LogError("LoquendoBridge: " + ex.Message);
            return false;
        }
    }
}
