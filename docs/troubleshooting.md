# Solución de problemas

## No se encontró TTSFileGenerator.exe

Usa `--generator` con la ruta correcta o define `LOQUENDO_TTS_GENERATOR`.

## No se encontró CABLE Input

Ejecuta:

`LoquendoBridge.exe --list-devices`

Si no tienes VB-CABLE, usa `--device default` o cambia `DefaultOutputDevice` en `Program.cs`.

## Streamer.bot ejecuta la Action pero no habla

- Comprueba la ruta `exePath` dentro de `UniversalTTS.cs`.
- Pulsa `Compile` y luego `Save and Compile`.
- Confirma que la recompensa exige texto.
- Revisa los logs de Streamer.bot.

## Loquendo funciona en TTS Director pero no mediante SAPI

LoquendoBridge no depende de SAPI para sintetizar. Usa directamente `TTSFileGenerator.exe`, que fue precisamente la solución elegida para instalaciones antiguas de Loquendo TTS 7.

## Se enciman varias voces

LoquendoBridge utiliza un mutex global. Cada instancia espera a que termine la anterior antes de reproducir.
