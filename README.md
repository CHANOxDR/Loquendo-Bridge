# Loquendo Bridge

Puente ligero para Windows que permite utilizar voces clásicas de **Loquendo TTS 7** con **Streamer.bot**, **OBS Studio** y opcionalmente **VB-CABLE**.

> **Importante:** este repositorio no incluye ni redistribuye Loquendo TTS, voces, DLL, instaladores ni `TTSFileGenerator.exe`. El usuario debe proporcionar su propia instalación legal.

## Características

- Generación de TTS mediante `TTSFileGenerator.exe`.
- Voces configuradas: Jorge, Carlos, Carmen, Diego, Esperanza, Francisca, Leonor, Ludoviko y Soledad.
- Entrada segura `--text64` en Base64 para Streamer.bot.
- Cola global para evitar que los mensajes hablen simultáneamente.
- Limpieza automática de WAV temporales.
- Selección de dispositivo de salida con NAudio.
- Compatible con VB-CABLE para tener un canal TTS independiente en OBS.
- `--list-voices`, `--list-devices`, `--max-length`, `--generator`, `--help` y `--version`.
- Proyecto x86 para .NET Framework 4.8.

## Requisitos

- Windows 10/11.
- .NET Framework 4.8.
- Loquendo TTS 7 instalado aparte.
- `TTSFileGenerator.exe` funcional.
- NAudio 2.4.0 (NuGet).
- Streamer.bot (opcional).
- OBS Studio (opcional).
- VB-CABLE (opcional, recomendado).

## Ejemplos

```cmd
LoquendoBridge.exe --voice Jorge --text "Hola mundo"
```

Con VB-CABLE:

```cmd
LoquendoBridge.exe --voice Jorge --device "CABLE Input" --text "Hola mundo"
```

Sin VB-CABLE:

```cmd
LoquendoBridge.exe --voice Jorge --device default --text "Hola mundo"
```

Listar dispositivos:

```cmd
LoquendoBridge.exe --list-devices
```

Listar voces:

```cmd
LoquendoBridge.exe --list-voices
```

## Documentación

Empieza por [`GUIA_RAPIDA.txt`](GUIA_RAPIDA.txt).

También están disponibles:

- [`docs/installation.md`](docs/installation.md)
- [`docs/streamerbot-setup.md`](docs/streamerbot-setup.md)
- [`docs/obs-vbcable.md`](docs/obs-vbcable.md)
- [`docs/troubleshooting.md`](docs/troubleshooting.md)

## Compilación

Abre `LoquendoBridge.sln` en Visual Studio y usa:

- Configuration: `Release`
- Platform: `x86`
- Target framework: `.NET Framework 4.8`

## Licencia

El código original de LoquendoBridge se publica bajo la licencia MIT. Loquendo sigue siendo software de terceros y no forma parte de esta licencia.
