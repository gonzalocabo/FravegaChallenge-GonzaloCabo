# Frávega Challenge API

API desarrollada como solución para el challenge técnico de Frávega.

## Descripción

Este proyecto es una **API RESTful** implementada en **.NET 8** usando Minimal API.  
Permite gestionar entidades y operaciones del dominio requerido por el challenge, utilizando como base de datos **MongoDB**.

## Características principales

- **.NET 8 Minimal API:** Moderno, simple y eficiente.
- **Swagger/OpenAPI:** Documentación interactiva para probar los endpoints.
- **Carter:** Routing modular para mantener la API organizada y escalable.
- **Ardalis.Result:** Manejo uniforme de resultados y errores.
- **Mapster:** Mapeo eficiente de entidades y DTOs.
- **MongoDB.Driver / MongoDB.Bson:** Acceso eficiente a MongoDB como base de datos.

## Tecnologías y dependencias

- [.NET 8](https://dotnet.microsoft.com/download)
- [Carter](https://github.com/CarterCommunity/Carter)
- [Ardalis.Result](https://github.com/ardalis/Result)
- [Mapster](https://github.com/MapsterMapper/Mapster)
- [MongoDB.Driver](https://www.mongodb.com/docs/drivers/csharp/)
- [MongoDB.Bson](https://www.mongodb.com/docs/drivers/csharp/)

## Estructura del proyecto
```/
├── src/
│ ├── FravegaChallenge.API # Proyecto principal de la API
│ ├── FravegaChallenge.Application # Lógica de negocio y casos de uso
│ ├── FravegaChallenge.Domain # Entidades y lógica de dominio
│ ├── FravegaChallenge.Infrastructure # Acceso a datos y servicios externos
│ └── docker-compose.yml # Compose para levantar API + MongoDB
├── tests/
│ ├── FravegaChallenge.API.FunctionalTests # Pruebas funcionales/end-to-end de la API
│ ├── FravegaChallenge.Application.UnitTests # Pruebas unitarias de la capa de aplicación
│ ├── FravegaChallenge.Domain.UnitTests # Pruebas unitarias de la capa de dominio
│ └── FravegaChallenge.Infrastructure.UnitTests # Pruebas unitarias de la capa de infraestructura
├── Dockerfile
├── CodeCoverage.runsettings
├── Directory.Build.props
├── Directory.Packages.props
└── README.md
```

## Cómo ejecutar el proyecto

En la carpeta `/src` del repositorio encontrarás un archivo `docker-compose.yml` que levanta tanto la API como una instancia de **MongoDB** lista para usar.

1. **Posicionate en la carpeta `/src`:**
   ```bash
   cd src
   ```
2. **Levantá el entorno con Docker Compose:**
   ```bash
   docker-compose up
   ```
  Este comando inicializará tanto la API como MongoDB.

3. **Creá la base de datos `ChallengeDB` en MongoDB:**
  La API requiere que exista la base de datos `ChallengeDB`.
  Podés crearla accediendo a la instancia de MongoDB que levanta el compose (por ejemplo, usando [MongoDB Compass](https://www.mongodb.com/products/tools/compass) o la CLI de MongoDB) y creando una base vacía llamada exactamente `ChallengeDB`.



## Endpoints y documentación

La API está autodescrita mediante **Swagger**.  
Podés explorar y probar todos los endpoints desde `/swagger` una vez levantada la aplicación.

## Code Coverage en Unit Tests

Para obtener un reporte correcto de code coverage en los tests unitarios, es necesario ejecutar las pruebas utilizando el archivo de configuración  
**`CodeCoverage.runsettings`** ubicado en la raíz del proyecto.

Desde la carpeta `/src`, ejecutá:

```bash
dotnet test FravegaChallenge.sln --settings ../CodeCoverage.runsettings
```
Esto asegurará que se apliquen las exclusiones y configuraciones personalizadas de cobertura según lo definido para el proyecto.

## Decisiones técnicas

A continuación se detallan algunas decisiones técnicas tomadas en el desarrollo del proyecto:

- **Identificadores de órdenes autoincrementales:**  
  Se decidió utilizar una colección específica en MongoDB para llevar el control de los identificadores autoincrementales de las órdenes.  
  Esto se implementa mediante una operación `findOneAndUpdate` con la opción `ReturnDocument.After`, lo que garantiza que, ante múltiples inserciones concurrentes, cada operación obtenga un identificador único y secuencial sin riesgo de colisiones.  
  Este approach asegura atomicidad y evita problemas de concurrencia en la generación de IDs.

- **Manejo centralizado de paquetes NuGet:**  
  Se definió manejar las versiones de los paquetes NuGet de forma centralizada, utilizando un archivo de configuración compartido (`Directory.Packages.props`).  
  Esto permite asegurar que todas las librerías y proyectos del repositorio utilicen la misma versión de cada dependencia, evitando inconsistencias, problemas de compatibilidad y facilitando el mantenimiento.

- **Centralización de configuraciones de proyecto (`Directory.Build.props`):**  
  Se optó por utilizar un archivo `Directory.Build.props` en la raíz de la solución para definir configuraciones comunes a todos los proyectos, tales como:
    - **TargetFramework:** Define la versión de .NET utilizada en todos los proyectos.
    - **TreatWarningsAsErrors:** Fuerza a que todas las advertencias de compilación sean tratadas como errores, asegurando un código más robusto.
    - **ImplicitUsings:** Activa el uso implícito de usings para reducir la repetición de código.
    - **Nullable:** Habilita el soporte de nullabilidad para mejorar la seguridad y calidad del código.
  
  Esta estrategia garantiza coherencia, reduce duplicidad y facilita el mantenimiento de las configuraciones a lo largo de toda la solución.

- **Manejo de errores con Ardalis.Result:**  
  Se utiliza la librería [Ardalis.Result](https://github.com/ardalis/Result) para el manejo uniforme de errores y respuestas en toda la solución.  
  Esto permite evitar el uso de excepciones para controlar el flujo de recursos y respuestas, facilitando la propagación de errores y estados (éxito, fallo, no encontrado, validaciones, etc.) de manera estructurada y consistente.  
  Así se mejora la claridad y mantenibilidad del código, además de simplificar el testing de los distintos flujos.