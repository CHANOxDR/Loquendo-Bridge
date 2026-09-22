# Streamer.bot

## Recompensa

Crea una recompensa de puntos de canal de Twitch y activa la entrada de texto del espectador.

## Action

1. En Streamer.bot abre `Actions`.
2. Crea una Action, por ejemplo `TTS Loquendo Jorge`.
3. Añade el trigger `Twitch -> Channel Reward -> Reward Redemption`.
4. Selecciona tu recompensa.
5. Añade `Core -> C# -> Execute C# Code`.
6. Copia el contenido de `StreamerBot/UniversalTTS.cs`.
7. Ajusta `exePath` a la ubicación real de tu `LoquendoBridge.exe`.
8. Ajusta la variable `voice`.
9. Pulsa `Compile` y después `Save and Compile`.

El script toma `rawInput`, limita el texto, lo codifica en Base64 y lanza LoquendoBridge sin mostrar CMD.

Para no usar VB-CABLE cambia en los argumentos:

`--device "CABLE Input"`

a:

`--device default`
