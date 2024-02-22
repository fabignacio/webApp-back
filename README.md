# WebApp-Backend

	Ambiente de desarrollo

	Se utilizó Visual Studio 2022 para la creación de este proyecto.
	Deberá de instalar, en caso de que sea necesario los siguientes paquetes Nuget. (Proyecto -> Administrar paquetes Nuget)

	* itext7 (8.0.3)
	* itext7.bouncy-castle-adapter (8.0.3)
	* Microsoft.Entity.FrameworkCore.InMemory (8.0.2)
	* Microsoft.Entity.FrameworkCore.SqlServer (8.0.2)
	* Microsoft.Entity.FrameworkCore.Tools (8.0.2)
	* Microsoft.VisualStudio.Web.CodeGeneration.Desing (8.0.1)
	* Swagger.Net.UI (1.1.0)
	* Swashbuckle.AspNetCore.SwaggerGen (6.5.0)


Este servicio lo que hace es que guarda algún incidente de los colaboradores en la base de datos SQL SERVER.

Una vez lo guarda, genera y envía como respuesta un archivo PDF.

# Conexion a base de datos

Para configurar la conexión a la base de datos y preparar el esquema necesario, siga estos pasos:

Modificar el archivo appsettings.json (ubicado en la razón del proyecto)

	{ 
		"Logging": 
			{  "LogLevel": 
				{ "Default": 
					"Information", 
					"Microsoft.AspNetCore": "Warning" 
				} 
			},
			"ConnectionStrings": {
			"DefaultConnection": "Server=Cambiar aquí con sus datos del servidor; Database=documentosNazar; agregar usuario y contraseña"
					},
		"AllowedHosts": "*"
	}
	

Aclaración, se debe modificar solo el server y añadir su usuario y contraseña respectivo de SQL Server 2019.

Deberá crear una base de datos llamada documentosNazar. Seguido de eso, deberá ejecutar el siguiente script para la creación de tablas y los procedimientos almacenados.

# Creación de base de datos
	
	CREATE DATABASE documentosNazar

# Tablas	

	--Eventos

	CREATE TABLE [dbo].[Eventos](
    [ID] [int] IDENTITY(1,1) NOT NULL,       -- Identificador único del evento
    [Fecha] [date] NOT NULL,                  -- Fecha del evento
    [Descripcion] [nvarchar](500) NOT NULL,   -- Descripción del evento
    [Impacto] [nvarchar](50) NOT NULL,       -- Impacto del evento
    [AccionesInmediatas] [nvarchar](max) NOT NULL,  -- Acciones inmediatas tomadas
    [AtencionEvento] [nvarchar](max) NOT NULL,     -- Atención dada al evento
    [RegistroFotografico] [varbinary](max) NULL,   -- Fotografía del evento
    [PersonalInvolucradoID] [int] NOT NULL,       -- ID del personal involucrado
    [AntecedenteID] [int] NOT NULL,               -- ID del antecedente relacionado
    [RutaImagen] [nvarchar](max) NULL,            -- Ruta de la imagen asociada al evento
    [ImagenBase64] [nvarchar](max) NULL           -- Imagen en formato Base64
	)

	--Personal Involucrado

	CREATE TABLE [dbo].[PersonalInvolucrado](
    [ID] [int] IDENTITY(1,1) NOT NULL,           -- Identificador único del personal involucrado
    [Nombre] [nvarchar](max) NOT NULL,           -- Nombre del personal involucrado
    [Cargo] [nvarchar](max) NOT NULL,            -- Cargo del personal involucrado
    [DanioGenerado] [nvarchar](max) NOT NULL,    -- Daño generado por el personal involucrado
    [Equipos] [nvarchar](max) NOT NULL,         -- Equipos asociados al personal involucrado
    [EventoID] [int] NOT NULL,                   
	PRIMARY KEY CLUSTERED ([ID] ASC),
	FOREIGN KEY([EventoID]) REFERENCES [dbo].[Eventos] ([ID])) -- ID del evento asociado 


	-- Antecedentes
	CREATE TABLE [dbo].[Antecedentes](
    [ID] [int] IDENTITY(1,1) NOT NULL,           -- Identificador único de los antecedentes
    [Operacion] [nvarchar](max) NOT NULL,       -- Operación relacionada con los antecedentes
    [Fecha] [date] NOT NULL,                     -- Fecha de los antecedentes
    [Ubicacion] [nvarchar](max) NOT NULL,        -- Ubicación de los antecedentes
    [HoraInformada] [time](7) NULL,              -- Hora informada de los antecedentes (opcional)
    [EventoID] [int] NOT NULL,                   
	PRIMARY KEY CLUSTERED ([ID] ASC),
	FOREIGN KEY([EventoID]) REFERENCES [dbo].[Eventos] ([ID]) -- ID del evento asociado
	)


# Procedimientos Almacenados

	
	-- ============================================= 
	-- Author:		<Fabian Alarcon>
	-- Create date: <20/02/2023>
	-- Description:	<Obtiene todos los registros de eventos>
	-- ============================================= 
	CREATE PROCEDURE [dbo].[sp_ObtenerEventos] 
	AS
		BEGIN
			SELECT 
				e.ID,
				e.Fecha,
				e.AccionesInmediatas,
				e.Descripcion,
				e.Impacto,
				e.RegistroFotografico,
				e.AtencionEvento
			FROM dbo.Eventos e
		END

		-- =============================================
		-- Author:		<Fabián Alarcón>
		-- Create date: <20/02/2024>
		-- Description:	<Insertar Evento en la tabla correspondiente>
		-- =============================================
		CREATE PROCEDURE [dbo].[sp_InsertarEvento](
			--Datos para el Evento
			@Fecha NVARCHAR(10) NULL,
			@Descripcion NVARCHAR(500) NULL,
			@Impacto NVARCHAR(5) NULL,
			@RegistroFotografico VARBINARY(MAX) NULL,
			@AccionesInmediatas NVARCHAR(MAX) NULL,
			@AtencionEvento NVARCHAR(MAX) NULL,
			@ImagenBase64 NVARCHAR(MAX) NULL,
			@RutaImagen NVARCHAR(MAX) NULL,

			--Datos para los Antecedentes
			@Operacion NVARCHAR(MAX) NULL,
			@FechaAntecedente NVARCHAR(10) NULL,
			@Ubicacion NVARCHAR(MAX) NULL,
			@HoraInformada TIME NULL,

			--Datos para el PersonalInvolucrado
			@Nombre NVARCHAR(MAX) NULL,
			@Cargo NVARCHAR(MAX) NULL,
			@DanioGenerado NVARCHAR(MAX) NULL,
			@Equipos NVARCHAR(MAX) NULL
		)
		AS
	BEGIN
		DECLARE @IDEvento INT
		DECLARE @IDAntecedente INT
		DECLARE @IDPersonal INT
	

	--Convertimos a fecha los campos correspondientes
	DECLARE @FechaNueva DATE
	SET @FechaNueva = CONVERT(DATE, @Fecha)

	DECLARE @FechaNuevaAnt DATE
	Set @FechaNuevaAnt = CONVERT(DATE, @FechaAntecedente)

	INSERT INTO dbo.Eventos (Fecha, Descripcion, Impacto, AccionesInmediatas, AtencionEvento, RegistroFotografico, PersonalInvolucradoID, AntecedenteID, ImagenBase64, RutaImagen)
	VALUES (@FechaNueva, @Descripcion, @Impacto, @AccionesInmediatas, @AtencionEvento, @RegistroFotografico, 1, 1, @ImagenBase64, @RutaImagen);

	-- Obtener el ID del último evento insertado
	SET @IDEvento = SCOPE_IDENTITY();

	INSERT INTO dbo.Antecedentes (Operacion, Fecha, Ubicacion, HoraInformada, EventoID)
	VALUES (@Operacion, @FechaNuevaAnt, @Ubicacion, @HoraInformada, @IDEvento);

	-- Obtener el ID del último antecedente insertado
	SET @IDAntecedente = SCOPE_IDENTITY();

	INSERT INTO dbo.PersonalInvolucrado(Nombre, Cargo, DanioGenerado, Equipos, EventoID)
	VALUES (@Nombre, @Cargo, @DanioGenerado, @Equipos, @IDEvento)

	--Obtener el ID del último personalInvolucrado insertado
	SET @IDPersonal = SCOPE_IDENTITY();

	--ACTUALIZAMOS LOS ID CORRESPONDIENTES AL EVENTO
	UPDATE dbo.Eventos
	SET 
		AntecedenteID = @IDAntecedente,
		PersonalInvolucradoID = @IDPersonal
	WHERE ID = @IDEvento

	IF @@ROWCOUNT > 0 
		BEGIN
			 SELECT 
				e.ID,
				e.Fecha,
				e.Descripcion, 
				e.AccionesInmediatas,
				e.RegistroFotografico,
				e.AtencionEvento,
				e.Impacto,
				e.RutaImagen,
				e.ImagenBase64,
				a.Operacion,
				a.Fecha As FechaAntecedente,
				a.Ubicacion,
				a.HoraInformada,
				p.Nombre,
				p.Cargo,
				p.DanioGenerado,
				p.Equipos,
				e.AntecedenteID AS AntecedentesID,
				e.PersonalInvolucradoID
			 FROM dbo.Eventos e 
			 JOIN dbo.Antecedentes a  ON e.AntecedenteID = a.ID
			 JOIN  dbo.PersonalInvolucrado p ON e.PersonalInvolucradoID = p.ID
			 WHERE e.ID = @IDEvento
		END
	END

# Pruebas

Antes de probar usando el front end, le recomiendo que haga pruebas usando alguna herramienta como postman con el archivo pruebas.json que encontrará en el correo donde se hace envio del proyecto.
Esto es solo para comprobar el correcto funcionamiento del servidor.

# Agradecimientos

Muchas gracias por darme la oportunidad de realizar esta prueba técnica. Si tienen alguna duda, favor contactarme:

	Correo: fa.alarconm@duocuc.cl
	Telefono: +56 9 4290 8148