-- Proyecto Fondo XYZ - Script de creación de base de datos

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'FondoXYZ')
BEGIN
    CREATE DATABASE FondoXYZ;
END
GO

USE FondoXYZ;
GO

-------> TABLAS SIN DEPENDENCIAS

CREATE TABLE dbo.Region
(
    RegionId      INT            IDENTITY(1, 1) NOT NULL,
    Nombre  NVARCHAR(100)  NOT NULL,
    Activo  BIT            NOT NULL CONSTRAINT DF_Region_Activo DEFAULT (1),
    CONSTRAINT PK_Region PRIMARY KEY (RegionId)
);
GO

CREATE TABLE dbo.CategoriaTarifa
(
    CategoriaTarifaId          INT            IDENTITY(1, 1) NOT NULL,
    Codigo      NVARCHAR(50)   NOT NULL,
    Nombre      NVARCHAR(100)  NOT NULL,
    Descripcion NVARCHAR(500)  NULL,
    CONSTRAINT PK_CategoriaTarifa PRIMARY KEY (CategoriaTarifaId),
    CONSTRAINT UQ_CategoriaTarifa_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE dbo.Temporada
(
    TemporadaId               INT            IDENTITY(1, 1) NOT NULL,
    Codigo           NVARCHAR(30)   NOT NULL,
    Nombre           NVARCHAR(100)  NOT NULL,
    FechaInicio      DATE           NOT NULL,
    FechaFin         DATE           NOT NULL,
    EsTemporadaAlta  BIT            NOT NULL,
    CONSTRAINT PK_Temporada PRIMARY KEY (TemporadaId),
    CONSTRAINT UQ_Temporada_Codigo UNIQUE (Codigo),
    CONSTRAINT CK_Temporada_Fechas CHECK (FechaFin >= FechaInicio)
);
GO

CREATE TABLE dbo.CalendarioEspecial
(
    CalendarioEspecialId          INT            IDENTITY(1, 1) NOT NULL,
    Fecha       DATE           NOT NULL,
    Tipo        NVARCHAR(20)   NOT NULL,
    Descripcion NVARCHAR(100)  NULL,
    CONSTRAINT PK_CalendarioEspecial PRIMARY KEY (CalendarioEspecialId),
    CONSTRAINT UQ_CalendarioEspecial_Fecha UNIQUE (Fecha),
    CONSTRAINT CK_CalendarioEspecial_Tipo CHECK (Tipo IN (N'Festivo', N'SemanaEscolar'))
);
GO

CREATE TABLE dbo.TipoServicio
(
    TipoServicioId           INT            IDENTITY(1, 1) NOT NULL,
    Codigo       NVARCHAR(30)   NOT NULL,
    Nombre       NVARCHAR(100)  NOT NULL,
    Descripcion  NVARCHAR(500)  NULL,
    EsPorUnidad  BIT            NOT NULL CONSTRAINT DF_TipoServicio_EsPorUnidad DEFAULT (1),
    Activo       BIT            NOT NULL CONSTRAINT DF_TipoServicio_Activo DEFAULT (1),
    CONSTRAINT PK_TipoServicio PRIMARY KEY (TipoServicioId),
    CONSTRAINT UQ_TipoServicio_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE dbo.Asociado
(
    AsociadoId               INT            IDENTITY(1, 1) NOT NULL,
    NumeroAsociado   NVARCHAR(20)   NOT NULL,
    TipoDocumento    NVARCHAR(10)   NOT NULL,
    NumeroDocumento  NVARCHAR(20)   NOT NULL,
    Nombres          NVARCHAR(100)  NOT NULL,
    Apellidos        NVARCHAR(100)  NOT NULL,
    Email            NVARCHAR(150)  NOT NULL,
    Telefono         NVARCHAR(20)   NULL,
    Clave     NVARCHAR(255)  NULL,
    Activo           BIT            NOT NULL CONSTRAINT DF_Asociado_Activo DEFAULT (1),
    FechaRegistro    DATETIME       NOT NULL CONSTRAINT DF_Asociado_FechaRegistro DEFAULT (GETDATE()),
    CONSTRAINT PK_Asociado PRIMARY KEY (AsociadoId),
    CONSTRAINT UQ_Asociado_NumeroAsociado UNIQUE (NumeroAsociado),
    CONSTRAINT UQ_Asociado_NumeroDocumento UNIQUE (NumeroDocumento),
    CONSTRAINT UQ_Asociado_Email UNIQUE (Email)
);
GO

-------> SITIOS

CREATE TABLE dbo.Sitio
(
    SitioId                    INT            IDENTITY(1, 1) NOT NULL,
    RegionId            INT            NOT NULL,
    Codigo                NVARCHAR(50)   NOT NULL,
    Nombre                NVARCHAR(150)  NOT NULL,
    TipoSitio             NVARCHAR(20)   NOT NULL,
    Ciudad                NVARCHAR(100)  NOT NULL,
    Descripcion           NVARCHAR(MAX)  NULL,
    Ubicacion             NVARCHAR(255)  NULL,
    CapacidadMaximaTotal  INT            NOT NULL,
    Activo                BIT            NOT NULL CONSTRAINT DF_Sitio_Activo DEFAULT (1),
    CONSTRAINT PK_Sitio PRIMARY KEY (SitioId),
    CONSTRAINT UQ_Sitio_Codigo UNIQUE (Codigo),
    CONSTRAINT CK_Sitio_TipoSitio CHECK (TipoSitio IN (N'SedeRecreativa', N'Apartamento')),
    CONSTRAINT FK_Sitio_Region FOREIGN KEY (RegionId) REFERENCES dbo.Region (RegionId)
);
GO

CREATE TABLE dbo.BloqueAlojamiento
(
    BloqueAlojamientoId               INT            IDENTITY(1, 1) NOT NULL,
    SitioId          INT            NOT NULL,
    Nombre           NVARCHAR(100)  NOT NULL,
    Descripcion      NVARCHAR(MAX)  NULL,
    CapacidadMaxima  INT            NOT NULL,
    CONSTRAINT PK_BloqueAlojamiento PRIMARY KEY (BloqueAlojamientoId),
    CONSTRAINT FK_BloqueAlojamiento_Sitio FOREIGN KEY (SitioId) REFERENCES dbo.Sitio (SitioId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.ServicioSitio
(
    ServicioSitioId           INT            IDENTITY(1, 1) NOT NULL,
    SitioId      INT            NOT NULL,
    Nombre       NVARCHAR(100)  NOT NULL,
    Descripcion  NVARCHAR(500)  NULL,
    Categoria    NVARCHAR(50)   NULL,
    CONSTRAINT PK_ServicioSitio PRIMARY KEY (ServicioSitioId),
    CONSTRAINT FK_ServicioSitio_Sitio FOREIGN KEY (SitioId) REFERENCES dbo.Sitio (SitioId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.UnidadAlojamiento
(
    UnidadAlojamientoId                         INT            IDENTITY(1, 1) NOT NULL,
    SitioId                    INT            NOT NULL,
    BloqueAlojamientoId        INT            NULL,
    CategoriaTarifaId          INT            NOT NULL,
    Codigo                     NVARCHAR(20)   NOT NULL,
    Nombre                     NVARCHAR(100)  NOT NULL,
    Descripcion                NVARCHAR(MAX)  NULL,
    NumeroHabitacionesInternas INT            NOT NULL CONSTRAINT DF_UnidadAlojamiento_NumeroHabitacionesInternas DEFAULT (1),
    CapacidadMaxima            INT            NOT NULL,
    Activo                     BIT            NOT NULL CONSTRAINT DF_UnidadAlojamiento_Activo DEFAULT (1),
    CONSTRAINT PK_UnidadAlojamiento PRIMARY KEY (UnidadAlojamientoId),
    CONSTRAINT UQ_UnidadAlojamiento_Sitio_Codigo UNIQUE (SitioId, Codigo),
    CONSTRAINT FK_UnidadAlojamiento_Sitio FOREIGN KEY (SitioId) REFERENCES dbo.Sitio (SitioId) ON DELETE CASCADE,
    CONSTRAINT FK_UnidadAlojamiento_Bloque FOREIGN KEY (BloqueAlojamientoId) REFERENCES dbo.BloqueAlojamiento (BloqueAlojamientoId),
    CONSTRAINT FK_UnidadAlojamiento_CategoriaTarifa FOREIGN KEY (CategoriaTarifaId) REFERENCES dbo.CategoriaTarifa (CategoriaTarifaId)
);
GO

CREATE TABLE dbo.Tarifa
(
    TarifaId                      INT            IDENTITY(1, 1) NOT NULL,
    SitioId                 INT            NULL,
    CategoriaTarifaId       INT            NULL,
    TemporadaId             INT            NULL,
    TipoConcepto            NVARCHAR(30)   NOT NULL,
    PersonasMin             INT            NOT NULL CONSTRAINT DF_Tarifa_PersonasMin DEFAULT (1),
    PersonasMax             INT            NOT NULL CONSTRAINT DF_Tarifa_PersonasMax DEFAULT (4),
    Precio                  DECIMAL(12, 2) NOT NULL,
    PrecioPersonaAdicional  DECIMAL(12, 2) NULL,
    DiasSemana              TINYINT        NULL,
    ExcluirFestivos         BIT            NOT NULL CONSTRAINT DF_Tarifa_ExcluirFestivos DEFAULT (0),
    ExcluirSemanaEscolar    BIT            NOT NULL CONSTRAINT DF_Tarifa_ExcluirSemanaEscolar DEFAULT (0),
    ExcluirTemporadaAlta    BIT            NOT NULL CONSTRAINT DF_Tarifa_ExcluirTemporadaAlta DEFAULT (0),
    VigenciaDesde           DATE           NULL,
    VigenciaHasta           DATE           NULL,
    Activo                  BIT            NOT NULL CONSTRAINT DF_Tarifa_Activo DEFAULT (1),
    CONSTRAINT PK_Tarifa PRIMARY KEY (TarifaId),
    CONSTRAINT CK_Tarifa_Personas CHECK (PersonasMax >= PersonasMin),
    CONSTRAINT CK_Tarifa_TipoConcepto CHECK (TipoConcepto IN (
        N'NocheBase', N'NochePorPersonas', N'NocheApartamento',
        N'PersonaAdicional', N'VisitaDiaAcompanante', N'ServicioAdicional'
    )),
    CONSTRAINT FK_Tarifa_Sitio FOREIGN KEY (SitioId) REFERENCES dbo.Sitio (SitioId) ON DELETE SET NULL,
    CONSTRAINT FK_Tarifa_CategoriaTarifa FOREIGN KEY (CategoriaTarifaId) REFERENCES dbo.CategoriaTarifa (CategoriaTarifaId),
    CONSTRAINT FK_Tarifa_Temporada FOREIGN KEY (TemporadaId) REFERENCES dbo.Temporada (TemporadaId)
);
GO

-------> RESERVAS

CREATE TABLE dbo.Reserva
(
    ReservaId                         INT            IDENTITY(1, 1) NOT NULL,
    CodigoReserva              NVARCHAR(20)   NOT NULL,
    AsociadoId                 INT            NOT NULL,
    SitioId                    INT            NOT NULL,
    TipoReserva                NVARCHAR(20)   NOT NULL,
    FechaEntrada               DATE           NOT NULL,
    FechaSalida                DATE           NOT NULL,
    NumeroPersonas             INT            NOT NULL,
    NumeroUnidadesSolicitadas  INT            NOT NULL,
    NumeroNoches               INT            NOT NULL,
    Subtotal                   DECIMAL(12, 2) NOT NULL CONSTRAINT DF_Reserva_Subtotal DEFAULT (0),
    TotalServicios             DECIMAL(12, 2) NOT NULL CONSTRAINT DF_Reserva_TotalServicios DEFAULT (0),
    Total                      DECIMAL(12, 2) NOT NULL CONSTRAINT DF_Reserva_Total DEFAULT (0),
    Estado                     NVARCHAR(20)   NOT NULL CONSTRAINT DF_Reserva_Estado DEFAULT (N'Borrador'),
    Observaciones              NVARCHAR(MAX)  NULL,
    FechaCreacion              DATETIME       NOT NULL CONSTRAINT DF_Reserva_FechaCreacion DEFAULT (GETDATE()),
    FechaConfirmacion          DATETIME       NULL,
    CONSTRAINT PK_Reserva PRIMARY KEY (ReservaId),
    CONSTRAINT UQ_Reserva_CodigoReserva UNIQUE (CodigoReserva),
    CONSTRAINT CK_Reserva_TipoReserva CHECK (TipoReserva IN (N'Alojamiento', N'VisitaDia')),
    CONSTRAINT CK_Reserva_Estado CHECK (Estado IN (
        N'Borrador', N'PendientePago', N'Confirmada', N'Completada', N'Cancelada', N'Expirada'
    )),
    CONSTRAINT CK_Reserva_Fechas CHECK (FechaSalida >= FechaEntrada),
    CONSTRAINT FK_Reserva_Asociado FOREIGN KEY (AsociadoId) REFERENCES dbo.Asociado (AsociadoId),
    CONSTRAINT FK_Reserva_Sitio FOREIGN KEY (SitioId) REFERENCES dbo.Sitio (SitioId)
);
GO

CREATE TABLE dbo.ReservaUnidad
(
    ReservaUnidadId                   INT            IDENTITY(1, 1) NOT NULL,
    ReservaId            INT            NOT NULL,
    UnidadAlojamientoId  INT            NOT NULL,
    FechaInicio          DATE           NOT NULL,
    FechaFin             DATE           NOT NULL,
    PrecioNoche          DECIMAL(12, 2) NOT NULL,
    Subtotal             DECIMAL(12, 2) NOT NULL,
    CONSTRAINT PK_ReservaUnidad PRIMARY KEY (ReservaUnidadId),
    CONSTRAINT CK_ReservaUnidad_Fechas CHECK (FechaFin > FechaInicio),
    CONSTRAINT FK_ReservaUnidad_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva (ReservaId) ON DELETE CASCADE,
    CONSTRAINT FK_ReservaUnidad_Unidad FOREIGN KEY (UnidadAlojamientoId) REFERENCES dbo.UnidadAlojamiento (UnidadAlojamientoId)
);
GO

CREATE TABLE dbo.ReservaAcompanante
(
    ReservaAcompananteId             INT            IDENTITY(1, 1) NOT NULL,
    ReservaId      INT            NOT NULL,
    Nombres          NVARCHAR(100)  NOT NULL,
    Apellidos        NVARCHAR(100)  NOT NULL,
	TipoDocumento    NVARCHAR(10)   NOT NULL,
    NumeroDocumento  NVARCHAR(20)   NOT NULL,
    Orden          INT            NOT NULL,
    TarifaAplicada DECIMAL(12, 2) NOT NULL,
    CONSTRAINT PK_ReservaAcompanante PRIMARY KEY (ReservaAcompananteId),
    CONSTRAINT FK_ReservaAcompanante_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva (ReservaId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.ReservaServicio
(
    ReservaServicioId              INT            IDENTITY(1, 1) NOT NULL,
    ReservaId       INT            NOT NULL,
    TipoServicioId  INT            NOT NULL,
    Cantidad        INT            NOT NULL,
    PrecioUnitario  DECIMAL(12, 2) NOT NULL,
    Subtotal        DECIMAL(12, 2) NOT NULL,
    CONSTRAINT PK_ReservaServicio PRIMARY KEY (ReservaServicioId),
    CONSTRAINT FK_ReservaServicio_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva (ReservaId) ON DELETE CASCADE,
    CONSTRAINT FK_ReservaServicio_TipoServicio FOREIGN KEY (TipoServicioId) REFERENCES dbo.TipoServicio (TipoServicioId)
);
GO

CREATE TABLE dbo.AuditoriaTarifa
(
    AuditoriaTarifaId            INT            IDENTITY(1, 1) NOT NULL,
    ReservaId     INT            NOT NULL,
    Fecha         DATE           NOT NULL,
    Concepto      NVARCHAR(100)  NOT NULL,
    Cantidad      INT            NOT NULL,
    ValorUnitario DECIMAL(12, 2) NOT NULL,
    Subtotal      DECIMAL(12, 2) NOT NULL,
    TarifaId      INT            NULL,
    CONSTRAINT PK_AuditoriaTarifa PRIMARY KEY (AuditoriaTarifaId),
    CONSTRAINT FK_AuditoriaTarifa_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva (ReservaId) ON DELETE CASCADE,
    CONSTRAINT FK_AuditoriaTarifa_Tarifa FOREIGN KEY (TarifaId) REFERENCES dbo.Tarifa (TarifaId) ON DELETE SET NULL
);
GO

CREATE TABLE dbo.BloqueoDisponibilidad
(
    BloqueoDisponibilidadId                   INT            IDENTITY(1, 1) NOT NULL,
    UnidadAlojamientoId  INT            NOT NULL,
    FechaInicio          DATE           NOT NULL,
    FechaFin             DATE           NOT NULL,
    Motivo               NVARCHAR(255)  NOT NULL,
    CreadoPor            NVARCHAR(100)  NOT NULL,
    CONSTRAINT PK_BloqueoDisponibilidad PRIMARY KEY (BloqueoDisponibilidadId),
    CONSTRAINT CK_BloqueoDisponibilidad_Fechas CHECK (FechaFin > FechaInicio),
    CONSTRAINT FK_BloqueoDisponibilidad_Unidad FOREIGN KEY (UnidadAlojamientoId) REFERENCES dbo.UnidadAlojamiento (UnidadAlojamientoId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Pago
(
    PagoId                 INT            IDENTITY(1, 1) NOT NULL,
    ReservaId          INT            NOT NULL,
    Monto              DECIMAL(12, 2) NOT NULL,
    MetodoPago         NVARCHAR(50)   NOT NULL,
    Estado             NVARCHAR(20)   NOT NULL,
    FechaPago          DATETIME       NULL,
    FechaCreacion      DATETIME       NOT NULL CONSTRAINT DF_Pago_FechaCreacion DEFAULT (GETDATE()),
    CONSTRAINT PK_Pago PRIMARY KEY (PagoId),
    CONSTRAINT CK_Pago_Estado CHECK (Estado IN (N'Iniciado', N'Aprobado', N'Rechazado', N'Reembolsado')),
    CONSTRAINT FK_Pago_Reserva FOREIGN KEY (ReservaId) REFERENCES dbo.Reserva (ReservaId)
);
GO

-------> DATOS INICIALES

SET IDENTITY_INSERT dbo.Region ON;
INSERT INTO dbo.Region (RegionId, Nombre, Activo) VALUES
(1, N'Cundinamarca', 1),
(2, N'Antioquia', 1),
(3, N'Valle del Cauca', 1),
(4, N'Magdalena', 1);
SET IDENTITY_INSERT dbo.Region OFF;
GO

SET IDENTITY_INSERT dbo.CategoriaTarifa ON;
INSERT INTO dbo.CategoriaTarifa (CategoriaTarifaId, Codigo, Nombre, Descripcion) VALUES
(1, N'UNA_HABITACION',       N'Una habitación',              N'Tarifa sedes: 1 habitación / noche (1-4 personas)'),
(2, N'DOS_HABITACIONES',     N'Dos habitaciones',            N'Tarifa sedes: 2 habitaciones / noche (1-4 personas)'),
(3, N'ESPECIAL_NUEVA_1H',    N'Especial nueva 1 habitación', N'Tarifa reducida lun-jue alojamientos nuevos (1-4 pers.)'),
(4, N'ESPECIAL_NUEVA_2H',    N'Especial nueva 2 habitaciones', N'Tarifa reducida lun-jue alojamientos nuevos (1-4 pers.)'),
(5, N'APTO_6_PERSONAS',      N'Apartamento 6 personas',      N'Santa Marta aptos 301 y 401'),
(6, N'APTO_8_PERSONAS',      N'Apartamento 8 personas',      N'Santa Marta apto 202'),
(7, N'HAB_MEDELLIN',         N'Habitación Medellín',         N'Suramericana Medellín por 1 o 2 personas');
SET IDENTITY_INSERT dbo.CategoriaTarifa OFF;
GO

SET IDENTITY_INSERT dbo.Temporada ON;
INSERT INTO dbo.Temporada (TemporadaId, Codigo, Nombre, FechaInicio, FechaFin, EsTemporadaAlta) VALUES
(1, N'BAJA_2026',  N'Baja temporada 2026',  '2026-01-16', '2026-12-14', 0),
(2, N'ALTA_2026',  N'Alta temporada 2026',   '2026-12-15', '2027-01-15', 1),
(3, N'ALTA_SEMSANTA_2026', N'Alta temporada Semana Santa 2026', '2026-03-28', '2026-04-05', 1);
SET IDENTITY_INSERT dbo.Temporada OFF;
GO

INSERT INTO dbo.CalendarioEspecial (Fecha, Tipo, Descripcion) VALUES
('2026-01-01', N'Festivo',         N'Año Nuevo'),
('2026-01-12', N'Festivo',         N'Día de los Reyes Magos'),
('2026-03-23', N'Festivo',         N'Día de San José'),
('2026-04-02', N'Festivo',         N'Jueves Santo'),
('2026-04-03', N'Festivo',         N'Viernes Santo'),
('2026-05-01', N'Festivo',         N'Día del Trabajo'),
('2026-05-25', N'Festivo',         N'Ascensión del Señor'),
('2026-06-15', N'Festivo',         N'Corpus Christi'),
('2026-06-22', N'Festivo',         N'Sagrado Corazón'),
('2026-06-29', N'Festivo',         N'San Pedro y San Pablo'),
('2026-07-20', N'Festivo',         N'Día de la Independencia'),
('2026-08-07', N'Festivo',         N'Batalla de Boyacá'),
('2026-08-17', N'Festivo',         N'Asunción de la Virgen'),
('2026-10-12', N'Festivo',         N'Día de la Raza'),
('2026-11-02', N'Festivo',         N'Todos los Santos'),
('2026-11-16', N'Festivo',         N'Independencia de Cartagena'),
('2026-12-08', N'Festivo',         N'Inmaculada Concepción'),
('2026-12-25', N'Festivo',         N'Navidad'),
('2026-06-01', N'SemanaEscolar',   N'Semana escolar junio 2026'),
('2026-06-02', N'SemanaEscolar',   N'Semana escolar junio 2026'),
('2026-06-03', N'SemanaEscolar',   N'Semana escolar junio 2026'),
('2026-06-04', N'SemanaEscolar',   N'Semana escolar junio 2026'),
('2026-06-05', N'SemanaEscolar',   N'Semana escolar junio 2026');
GO

SET IDENTITY_INSERT dbo.TipoServicio ON;
INSERT INTO dbo.TipoServicio (TipoServicioId, Codigo, Nombre, Descripcion, EsPorUnidad, Activo) VALUES
(1, N'LAVANDERIA', N'Servicio de lavandería', N'Servicio de lavandería en apartamentos Santa Marta', 1, 1);
SET IDENTITY_INSERT dbo.TipoServicio OFF;
GO

SET IDENTITY_INSERT dbo.Asociado ON;
INSERT INTO dbo.Asociado (AsociadoId, NumeroAsociado, TipoDocumento, NumeroDocumento, Nombres, Apellidos, Email, Telefono, Clave, Activo) VALUES
(1, N'45821', N'CC', N'1032456789', N'María', N'López García', N'maria.lopez@ejemplo.com', N'3001234567', N'AQAAAAIAAYagAAAAED3bDcnmL6Hd9iPX3YI9MjzlwNj/wxopqJs6tFNZhK6VJmIqSjPO7VDOn/YCYmb/1g==',1), -- Clave = 1234567
(2, N'51203', N'CC', N'9876543210', N'Carlos', N'Ruiz Mejía', N'carlos.ruiz@ejemplo.com', N'3109876543', N'AQAAAAIAAYagAAAAELPJNIXD3myUkc9+vfWRcgRZg9qAIZ1H8QAQ9WZZ+exyXaV5dMU2q9/eF3mD1l0ucA==',1); -- Clave = 1234567
SET IDENTITY_INSERT dbo.Asociado OFF;
GO

-- Sitios
SET IDENTITY_INSERT dbo.Sitio ON;
INSERT INTO dbo.Sitio (SitioId, RegionId, Codigo, Nombre, TipoSitio, Ciudad, Descripcion, CapacidadMaximaTotal, Activo) VALUES
(1, 1, N'VILLETA',        N'Villeta',                          N'SedeRecreativa', N'Villeta',               N'Sede recreativa con ocho habitaciones. Capacidad total hasta 32 personas.', 32, 1),
(2, 1, N'EL_PLACER',      N'El Placer – Fusagasugá',           N'SedeRecreativa', N'Fusagasugá',            N'Sede con alojamientos 1-4 y bloque de cabañas 5-8. Capacidad total hasta 34 personas.', 34, 1),
(3, 2, N'CHINCHINA',      N'Gonzalo Morante – Chinchiná',      N'SedeRecreativa', N'Chinchiná',             N'Sede con alojamientos y bloque de cabañas tipo A y B. Capacidad total hasta 30 personas.', 30, 1),
(4, 3, N'TABLONES',       N'Tablones – Palmira',               N'SedeRecreativa', N'Palmira',               N'Sede con cuatro alojamientos. Capacidad total hasta 24 personas.', 24, 1),
(5, 2, N'MANGURUMA',      N'Manguruma – Santa fe de Antioquia', N'SedeRecreativa', N'Santa fe de Antioquia', N'Sede con alojamientos 1-3 y bloque nuevo de 8 unidades. Capacidad total hasta 46 personas.', 46, 1),
(6, 1, N'FEDERMAN',       N'Federman – Bogotá',                N'SedeRecreativa', N'Bogotá',                N'Sede con zona húmeda, gimnasio y 4 habitaciones para alojamiento de asociados.', 16, 1),
(7, 2, N'MED_SURAMERICANA', N'Suramericana – Medellín',        N'Apartamento',    N'Medellín',              N'Apartamento con cinco habitaciones en edificio Suramericana.', 10, 1),
(8, 4, N'SM_RODADERO',    N'El Rodadero – Santa Marta',        N'Apartamento',    N'Santa Marta',           N'Apartamentos 202, 301 y 401 en El Rodadero.', 20, 1);
SET IDENTITY_INSERT dbo.Sitio OFF;
GO

-- Bloques de alojamiento
SET IDENTITY_INSERT dbo.BloqueAlojamiento ON;
INSERT INTO dbo.BloqueAlojamiento (BloqueAlojamientoId, SitioId, Nombre, Descripcion, CapacidadMaxima) VALUES
(1,  2, N'Alojamientos principales',  N'Alojamientos numerados 1 a 4',                    18),
(2,  2, N'Bloque de cabañas',         N'Cuatro cabañas numeradas del 5 al 8',             16),
(3,  3, N'Alojamientos principales',  N'Alojamientos 1, 2 y 4',                           14),
(4,  3, N'Bloque de cabañas',         N'Cabañas tipo A (3) y tipo B (5, 6)',              16),
(5,  5, N'Alojamientos principales',  N'Alojamientos numerados 1 a 3',                    14),
(6,  5, N'Bloque Nuevo',              N'Ocho alojamientos con habitación, cocina y terraza', 32),
(7,  1, N'Habitaciones de la sede',   N'Ocho habitaciones de la sede V001',               32),
(8,  6, N'Habitaciones de alojamiento', N'Cuatro habitaciones para asociados',           16);
SET IDENTITY_INSERT dbo.BloqueAlojamiento OFF;
GO

-- Servicios de sitio
INSERT INTO dbo.ServicioSitio (SitioId, Nombre, Descripcion, Categoria) VALUES
(6, N'Zona húmeda',           N'Baño turco, sauna, jacuzzi, baños y vestieres', N'Recreativo'),
(6, N'Gimnasio',              N'Gimnasio y sala de masajes',                    N'Deportivo'),
(6, N'Billar y juegos de mesa', N'Sala de billar y juegos de mesa',             N'Recreativo'),
(6, N'Salas de música y video', N'Salas de música, video y lectura',           N'Recreativo'),
(6, N'Cafetería y sala social', N'Cafetería y sala social',                     N'Social'),
(6, N'Aeróbicos y Pilates',   N'Bicicleta estática, aeróbicos, pilates y rumba tropical (L-V 5:30-6:30 p.m.)', N'Deportivo');
GO

-- Unidades de alojamiento
SET IDENTITY_INSERT dbo.UnidadAlojamiento ON;

INSERT INTO dbo.UnidadAlojamiento (UnidadAlojamientoId, SitioId, BloqueAlojamientoId, CategoriaTarifaId, Codigo, Nombre, Descripcion, NumeroHabitacionesInternas, CapacidadMaxima, Activo) VALUES
-- Villeta
(1,  1, 7, 1, N'1', N'Habitación 1', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(2,  1, 7, 1, N'2', N'Habitación 2', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(3,  1, 7, 1, N'3', N'Habitación 3', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(4,  1, 7, 1, N'4', N'Habitación 4', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(5,  1, 7, 1, N'5', N'Habitación 5', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(6,  1, 7, 1, N'6', N'Habitación 6', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(7,  1, 7, 1, N'7', N'Habitación 7', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
(8,  1, 7, 1, N'8', N'Habitación 8', N'Alcoba con cama doble y camarote, baño, nevera, televisor y terraza cubierta.', 1, 4, 1),
-- El Placer - Alojamientos 1-4
(9,  2, 1, 2, N'1', N'Alojamiento 1', N'Dos habitaciones, baño y televisor.', 2, 4, 1),
(10, 2, 1, 2, N'2', N'Alojamiento 2', N'Dos habitaciones, baño y televisor.', 2, 5, 1),
(11, 2, 1, 2, N'3', N'Alojamiento 3', N'Una habitación con cama doble y 2 camas sencillas, baño y televisor.', 1, 4, 1),
(12, 2, 1, 2, N'4', N'Alojamiento 4', N'Dos habitaciones, baño y televisor.', 2, 4, 1),
-- El Placer - Cabañas 5-8
(13, 2, 2, 4, N'5', N'Cabaña 5', N'Sala de estar con sofá cama, baño, habitación, cocineta, nevera y terraza comedor.', 1, 4, 1),
(14, 2, 2, 4, N'6', N'Cabaña 6', N'Sala de estar con sofá cama, baño, habitación, cocineta, nevera y terraza comedor.', 1, 4, 1),
(15, 2, 2, 4, N'7', N'Cabaña 7', N'Sala de estar con sofá cama, baño, habitación, cocineta, nevera y terraza comedor.', 1, 4, 1),
(16, 2, 2, 4, N'8', N'Cabaña 8', N'Sala de estar con sofá cama, baño, habitación, cocineta, nevera y terraza comedor.', 1, 4, 1),
-- Chinchiná
(17, 3, 3, 2, N'1', N'Alojamiento 1', N'Cocineta, baño, televisor y 2 habitaciones.', 2, 5, 1),
(18, 3, 3, 2, N'2', N'Alojamiento 2', N'Cocineta, baño, televisor y 2 habitaciones.', 2, 5, 1),
(19, 3, 3, 1, N'4', N'Alojamiento 4', N'Cocineta, baño, televisor y una habitación.', 1, 3, 1),
(20, 3, 4, 2, N'3', N'Cabaña Tipo A - 3', N'Cocineta, dos baños, sala comedor, televisor y dos habitaciones.', 2, 5, 1),
(21, 3, 4, 1, N'5', N'Cabaña Tipo B - 5', N'Cocineta, baño, sala con sofá, televisor, una habitación.', 1, 3, 1),
(22, 3, 4, 1, N'6', N'Cabaña Tipo B - 6', N'Cocineta, baño, sala con sofá, televisor, una habitación.', 1, 3, 1),
-- Tablones Palmira
(23, 4, NULL, 1, N'1', N'Alojamiento 1', N'Una habitación con cama doble y camarote, televisor, baño, cocineta y comedor.', 1, 4, 1),
(24, 4, NULL, 1, N'2', N'Alojamiento 2', N'Una habitación con cama doble y camarote, televisor, baño, cocineta y comedor.', 1, 4, 1),
(25, 4, NULL, 2, N'3', N'Alojamiento 3', N'Dos habitaciones, sala de estar, televisor, baño y cocineta.', 2, 6, 1),
(26, 4, NULL, 2, N'4', N'Alojamiento 4', N'Dos habitaciones, sala de estar, televisor, baño y cocineta.', 2, 6, 1),
-- Manguruma
(27, 5, 5, 1, N'1', N'Alojamiento 1', N'Una cama doble y un camarote, baño, terraza y televisor.', 1, 3, 1),
(28, 5, 5, 1, N'2', N'Alojamiento 2', N'Cama doble, camarote y sofá-cama, baño, terraza y televisor.', 1, 4, 1),
(29, 5, 5, 1, N'3', N'Alojamiento 3', N'Cama doble, camarote y sofá-cama, baño, terraza y televisor.', 1, 4, 1),
(30, 5, 6, 4, N'B1', N'Bloque Nuevo 1', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(31, 5, 6, 4, N'B2', N'Bloque Nuevo 2', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(32, 5, 6, 4, N'B3', N'Bloque Nuevo 3', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(33, 5, 6, 4, N'B4', N'Bloque Nuevo 4', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(34, 5, 6, 4, N'B5', N'Bloque Nuevo 5', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(35, 5, 6, 4, N'B6', N'Bloque Nuevo 6', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(36, 5, 6, 4, N'B7', N'Bloque Nuevo 7', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
(37, 5, 6, 4, N'B8', N'Bloque Nuevo 8', N'Habitación con dos camas gemelas y camarote, baño, terraza-comedor y cocina.', 1, 4, 1),
-- Federman Bogotá (4 habitaciones alojamiento)
(38, 6, 8, 1, N'1', N'Habitación 1', N'Habitación para alojamiento de asociados.', 1, 4, 1),
(39, 6, 8, 1, N'2', N'Habitación 2', N'Habitación para alojamiento de asociados.', 1, 4, 1),
(40, 6, 8, 1, N'3', N'Habitación 3', N'Habitación para alojamiento de asociados.', 1, 4, 1),
(41, 6, 8, 1, N'4', N'Habitación 4', N'Habitación para alojamiento de asociados.', 1, 4, 1),
-- Medellín Suramericana
(42, 7, NULL, 7, N'1', N'Habitación 1', N'2 camas sencillas y baño privado.', 1, 2, 1),
(43, 7, NULL, 7, N'2', N'Habitación 2', N'2 camas sencillas.', 1, 2, 1),
(44, 7, NULL, 7, N'3', N'Habitación 3', N'2 camas sencillas.', 1, 2, 1),
(45, 7, NULL, 7, N'4', N'Habitación 4', N'2 camas sencillas.', 1, 2, 1),
(46, 7, NULL, 7, N'5', N'Habitación 5', N'1 cama sencilla y baño privado.', 1, 1, 1),
-- Santa Marta
(47, 8, NULL, 6, N'202', N'Apartamento 202', N'Sala comedor, cocina, 2 baños, tres habitaciones y parqueadero. Capacidad máxima 8 personas.', 3, 8, 1),
(48, 8, NULL, 5, N'301', N'Apartamento 301', N'Sala comedor, cocina, 1 baño, dos habitaciones y parqueadero. Capacidad máxima 6 personas.', 2, 6, 1),
(49, 8, NULL, 5, N'401', N'Apartamento 401', N'Sala comedor, cocina, 1 baño, dos habitaciones y parqueadero. Capacidad máxima 6 personas.', 2, 6, 1);
SET IDENTITY_INSERT dbo.UnidadAlojamiento OFF;
GO

-- Tarifas
SET IDENTITY_INSERT dbo.Tarifa ON;
INSERT INTO dbo.Tarifa (TarifaId, SitioId, CategoriaTarifaId, TemporadaId, TipoConcepto, PersonasMin, PersonasMax, Precio, PrecioPersonaAdicional, DiasSemana, ExcluirFestivos, ExcluirSemanaEscolar, ExcluirTemporadaAlta, Activo) VALUES
-- Sedes recreativas: tarifa 1 habitación
(1,  NULL, 1, NULL, N'NocheBase', 1, 4, 70000.00, 16000.00, NULL, 0, 0, 0, 1),
-- Sedes recreativas: tarifa 2 habitaciones
(2,  NULL, 2, NULL, N'NocheBase', 1, 4, 90000.00, 16000.00, NULL, 0, 0, 0, 1),
-- Tarifa especial lun-jue: 1 habitación
(3,  NULL, 3, NULL, N'NocheBase', 1, 4, 27000.00, 11000.00, 15, 1, 1, 1, 1),
-- Tarifa especial lun-jue: 2 habitaciones
(4,  NULL, 4, NULL, N'NocheBase', 1, 4, 37000.00, 11000.00, 15, 1, 1, 1, 1),
-- Visita día - acompañantes
(5,  NULL, NULL, NULL, N'VisitaDiaAcompanante', 5, 10, 5500.00, NULL, NULL, 0, 0, 0, 1),
-- Medellín Suramericana
(6,  7,    7, NULL, N'NochePorPersonas', 1, 1, 63000.00, NULL, NULL, 0, 0, 0, 1),
(7,  7,    7, NULL, N'NochePorPersonas', 2, 2, 75000.00, NULL, NULL, 0, 0, 0, 1),
-- Santa Marta - Baja temporada
(8,  8,    5, 1,    N'NocheApartamento', 1, 6, 89000.00, NULL, NULL, 0, 0, 0, 1),
(9,  8,    6, 1,    N'NocheApartamento', 1, 8, 103000.00, NULL, NULL, 0, 0, 0, 1),
-- Santa Marta - Alta temporada
(10, 8,    5, 2,    N'NocheApartamento', 1, 6, 124000.00, NULL, NULL, 0, 0, 0, 1),
(11, 8,    6, 2,    N'NocheApartamento', 1, 8, 143000.00, NULL, NULL, 0, 0, 0, 1),
(12, 8,    5, 3,    N'NocheApartamento', 1, 6, 124000.00, NULL, NULL, 0, 0, 0, 1),
(13, 8,    6, 3,    N'NocheApartamento', 1, 8, 143000.00, NULL, NULL, 0, 0, 0, 1),
-- Lavandería Santa Marta
(14, 8,    NULL, NULL, N'ServicioAdicional', 1, 1, 18000.00, NULL, NULL, 0, 0, 0, 1);
SET IDENTITY_INSERT dbo.Tarifa OFF;
GO

-- Reserva de ejemplo
SET IDENTITY_INSERT dbo.Reserva ON;
INSERT INTO dbo.Reserva (ReservaId, CodigoReserva, AsociadoId, SitioId, TipoReserva, FechaEntrada, FechaSalida, NumeroPersonas, NumeroUnidadesSolicitadas, NumeroNoches, Subtotal, TotalServicios, Total, Estado, FechaCreacion, FechaConfirmacion) VALUES
(1, N'RES-20260610-0042', 1, 2, N'Alojamiento', '2026-06-10', '2026-06-12', 10, 2, 2, 192000.00, 0.00, 192000.00, N'Confirmada', '2026-06-10 14:00:00', '2026-06-10 14:32:00');
SET IDENTITY_INSERT dbo.Reserva OFF;
GO

INSERT INTO dbo.ReservaUnidad (ReservaId, UnidadAlojamientoId, FechaInicio, FechaFin, PrecioNoche, Subtotal) VALUES
(1, 13, '2026-06-10', '2026-06-12', 48000.00, 96000.00),
(1, 14, '2026-06-10', '2026-06-12', 48000.00, 96000.00);
GO

INSERT INTO dbo.AuditoriaTarifa (ReservaId, Fecha, Concepto, Cantidad, ValorUnitario, Subtotal, TarifaId) VALUES
(1, '2026-06-10', N'Tarifa especial 2H - Cabaña 5', 1, 37000.00, 37000.00, 4),
(1, '2026-06-10', N'Persona adicional - Cabaña 5',  1, 11000.00, 11000.00, 4),
(1, '2026-06-10', N'Tarifa especial 2H - Cabaña 6', 1, 37000.00, 37000.00, 4),
(1, '2026-06-10', N'Persona adicional - Cabaña 6',  1, 11000.00, 11000.00, 4),
(1, '2026-06-11', N'Tarifa especial 2H - Cabaña 5', 1, 37000.00, 37000.00, 4),
(1, '2026-06-11', N'Persona adicional - Cabaña 5',  1, 11000.00, 11000.00, 4),
(1, '2026-06-11', N'Tarifa especial 2H - Cabaña 6', 1, 37000.00, 37000.00, 4),
(1, '2026-06-11', N'Persona adicional - Cabaña 6',  1, 11000.00, 11000.00, 4);
GO

INSERT INTO dbo.Pago (ReservaId, Monto, MetodoPago, Estado, FechaPago, FechaCreacion) VALUES
(1, 192000.00, N'TarjetaCredito', N'Aprobado', '2026-06-10 14:32:00', '2026-06-10 14:30:00');
GO

