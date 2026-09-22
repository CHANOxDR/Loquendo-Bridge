using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace LoquendoBridge
{
    internal class Program
    {
        private const string Version = "1.0.0";
        private const string DefaultGeneratorPath = @"C:\Program Files (x86)\Loquendo\LTTS7\bin\TTSFileGenerator.exe";
        private const string WorkingDirectory = @"C:\LoquendoBridge\Temp";
        private const string MutexName = @"Global\LoquendoBridge_TTS_Queue";

        // Cambia este valor a "default" si NO quieres usar VB-CABLE.
        // También puedes sobrescribirlo en cada ejecución con --device "Nombre del dispositivo".
        private const string DefaultOutputDevice = "CABLE Input";

        private static readonly string[] AllowedVoices =
        {
            "Jorge",
            "Carlos",
            "Carmen",
            "Diego",
            "Esperanza",
            "Francisca",
            "Leonor",
            "Ludoviko",
            "Soledad"
        };

        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                if (HasFlag(args, "--help") || HasFlag(args, "-h") || args.Length == 0)
                {
                    PrintHelp();
                    return 0;
                }

                if (HasFlag(args, "--version"))
                {
                    Console.WriteLine("LoquendoBridge " + Version);
                    return 0;
                }

                if (HasFlag(args, "--list-voices"))
                {
                    foreach (string voiceName in AllowedVoices)
                        Console.WriteLine(voiceName);
                    return 0;
                }

                if (HasFlag(args, "--list-devices"))
                {
                    ListDevices();
                    return 0;
                }

                Directory.CreateDirectory(WorkingDirectory);

                string voice = GetArgument(args, "--voice") ?? "Jorge";
                string text = GetText(args);
                string deviceName = GetArgument(args, "--device") ?? DefaultOutputDevice;
                string generatorPath = ResolveGeneratorPath(args);
                int maxLength = GetIntArgument(args, "--max-length", 300, 1, 2000);

                if (!AllowedVoices.Any(v => v.Equals(voice, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.Error.WriteLine("La voz '" + voice + "' no está incluida en la lista permitida.");
                    Console.Error.WriteLine("Usa --list-voices para ver las voces configuradas.");
                    return 2;
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.Error.WriteLine("No se recibió ningún texto.");
                    return 3;
                }

                text = CleanText(text);
                if (text.Length > maxLength)
                    text = text.Substring(0, maxLength);

                if (!File.Exists(generatorPath))
                {
                    Console.Error.WriteLine("No se encontró TTSFileGenerator.exe en:");
                    Console.Error.WriteLine(generatorPath);
                    Console.Error.WriteLine("Usa --generator \"RUTA\" o la variable LOQUENDO_TTS_GENERATOR.");
                    return 4;
                }

                using (Mutex mutex = new Mutex(false, MutexName))
                {
                    bool acquired = false;
                    try
                    {
                        try
                        {
                            acquired = mutex.WaitOne();
                        }
                        catch (AbandonedMutexException)
                        {
                            acquired = true;
                        }

                        if (!acquired)
                            return 5;

                        Speak(generatorPath, voice, text, deviceName);
                    }
                    finally
                    {
                        if (acquired)
                            mutex.ReleaseMutex();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("LoquendoBridge encontró un error:");
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void Speak(string generatorPath, string voice, string text, string deviceName)
        {
            string session = Guid.NewGuid().ToString("N");
            string sessionFolder = Path.Combine(WorkingDirectory, session);
            Directory.CreateDirectory(sessionFolder);
            string prefix = Path.Combine(sessionFolder, "tts");

            try
            {
                GenerateAudio(generatorPath, voice, text, prefix);

                string wav = Directory.GetFiles(sessionFolder, "*.wav")
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (wav == null)
                    throw new Exception("Loquendo no generó ningún archivo WAV.");

                PlayToDevice(wav, deviceName);
            }
            finally
            {
                TryDeleteDirectory(sessionFolder);
            }
        }

        private static void GenerateAudio(string generatorPath, string voice, string text, string prefix)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = generatorPath,
                Arguments = "-v \"" + voice + "\" -f 44100 -s 1 -o \"" + prefix + "\" -e wav",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(generatorPath)
            };

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();

                process.StandardInput.WriteLine(text);
                process.StandardInput.Close();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(
                        "TTSFileGenerator terminó con código " + process.ExitCode +
                        (string.IsNullOrWhiteSpace(error) ? "" : ". " + error.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(output))
                    Console.WriteLine(output.Trim());
            }
        }

        private static void PlayToDevice(string filePath, string deviceName)
        {
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                MMDevice device;

                if (string.IsNullOrWhiteSpace(deviceName) ||
                    deviceName.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
                else
                {
                    device = enumerator
                        .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                        .FirstOrDefault(d => d.FriendlyName.IndexOf(
                            deviceName,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (device == null)
                {
                    throw new Exception(
                        "No se encontró el dispositivo de audio '" + deviceName +
                        "'. Ejecuta --list-devices para ver los dispositivos disponibles.");
                }

                using (device)
                using (AudioFileReader audioFile = new AudioFileReader(filePath))
                using (WasapiOut output = new WasapiOut(
                    device,
                    AudioClientShareMode.Shared,
                    false,
                    100))
                {
                    output.Init(audioFile);
                    output.Play();

                    while (output.PlaybackState == PlaybackState.Playing)
                        Thread.Sleep(50);
                }
            }
        }

        private static void ListDevices()
        {
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    Console.WriteLine(device.FriendlyName);
                    device.Dispose();
                }
            }
        }

        private static string ResolveGeneratorPath(string[] args)
        {
            string fromArg = GetArgument(args, "--generator");
            if (!string.IsNullOrWhiteSpace(fromArg))
                return fromArg;

            string fromEnv = Environment.GetEnvironmentVariable("LOQUENDO_TTS_GENERATOR");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            return DefaultGeneratorPath;
        }

        private static string GetText(string[] args)
        {
            string base64 = GetArgument(args, "--text64");
            if (!string.IsNullOrWhiteSpace(base64))
            {
                byte[] data = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(data);
            }

            string normalText = GetArgument(args, "--text");
            if (!string.IsNullOrWhiteSpace(normalText))
                return normalText;

            if (Console.IsInputRedirected)
                return Console.In.ReadToEnd();

            return null;
        }

        private static string CleanText(string text)
        {
            text = text.Replace("\r", " ").Replace("\n", " ").Replace("\0", "");
            while (text.Contains("  "))
                text = text.Replace("  ", " ");
            return text.Trim();
        }

        private static int GetIntArgument(string[] args, string name, int fallback, int min, int max)
        {
            string value = GetArgument(args, name);
            int parsed;
            if (value != null && int.TryParse(value, out parsed))
                return Math.Max(min, Math.Min(max, parsed));
            return fallback;
        }

        private static string GetArgument(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    return args[i + 1];
            }
            return null;
        }

        private static bool HasFlag(string[] args, string name)
        {
            return args.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static void TryDeleteDirectory(string path)
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    return;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("LoquendoBridge " + Version);
            Console.WriteLine();
            Console.WriteLine("Uso:");
            Console.WriteLine("  LoquendoBridge.exe --voice Jorge --text \"Hola mundo\"");
            Console.WriteLine("  LoquendoBridge.exe --voice Jorge --device \"CABLE Input\" --text \"Hola\"");
            Console.WriteLine();
            Console.WriteLine("Opciones:");
            Console.WriteLine("  --voice <nombre>       Voz de Loquendo. Predeterminada: Jorge");
            Console.WriteLine("  --text <texto>         Texto normal para pruebas manuales");
            Console.WriteLine("  --text64 <base64>      Texto UTF-8 en Base64. Recomendado para Streamer.bot");
            Console.WriteLine("  --device <nombre>      Salida de audio. Predeterminada: CABLE Input");
            Console.WriteLine("  --device default       Usa la salida multimedia predeterminada de Windows");
            Console.WriteLine("  --generator <ruta>     Ruta a TTSFileGenerator.exe");
            Console.WriteLine("  --max-length <n>       Máximo de caracteres, 1-2000. Predeterminado: 300");
            Console.WriteLine("  --list-voices          Lista voces configuradas");
            Console.WriteLine("  --list-devices         Lista dispositivos de reproducción activos");
            Console.WriteLine("  --version              Muestra la versión");
            Console.WriteLine("  --help                 Muestra esta ayuda");
        }
    }
}
