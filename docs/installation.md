# Instalación

## Requisitos

- Windows 10/11
- Visual Studio con desarrollo de escritorio .NET
- .NET Framework 4.8
- Loquendo TTS 7 instalado por separado
- Una instalación funcional de TTSFileGenerator.exe

## Compilar

1. Abre `LoquendoBridge.sln`.
2. Restaura los paquetes NuGet si Visual Studio lo solicita.
3. Selecciona `Release`.
4. Selecciona plataforma `x86`.
5. Compila la solución.

La dependencia NAudio se declara en el proyecto con versión 2.4.0.

## Ruta de Loquendo

Por defecto se busca:

`C:\Program Files (x86)\Loquendo\LTTS7\bin\TTSFileGenerator.exe`

Puedes cambiarla mediante `--generator` o la variable `LOQUENDO_TTS_GENERATOR`.
