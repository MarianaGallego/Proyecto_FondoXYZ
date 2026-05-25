USE FondoXYZ;
GO

CREATE PROCEDURE dbo.SP_ConsultarHabitacionesDisponibles
    @FechaEntrada    DATE,
    @FechaSalida     DATE,
    @SitioId         INT = NULL,
    @NumeroPersonas  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaSalida <= @FechaEntrada
    BEGIN
        RAISERROR(N'La fecha de salida debe ser posterior a la fecha de entrada.', 16, 1);
        RETURN;
    END;

    IF @NumeroPersonas IS NOT NULL AND @NumeroPersonas < 1
    BEGIN
        RAISERROR(N'El número de personas debe ser mayor o igual a 1.', 16, 1);
        RETURN;
    END;

    SELECT
        ua.UnidadAlojamientoId,
        ua.Codigo,
        ua.Nombre,
        ua.CapacidadMaxima,
        ua.NumeroHabitacionesInternas,
        s.SitioId,
        s.Codigo      AS SitioCodigo,
        s.Nombre      AS SitioNombre,
        s.Ciudad,
        s.TipoSitio,
        ct.Codigo     AS CategoriaTarifaCodigo,
        ct.Nombre     AS CategoriaTarifaNombre
    FROM dbo.UnidadAlojamiento ua
    INNER JOIN dbo.Sitio s
        ON s.SitioId = ua.SitioId
    INNER JOIN dbo.CategoriaTarifa ct
        ON ct.CategoriaTarifaId = ua.CategoriaTarifaId
    WHERE ua.Activo = 1
      AND s.Activo = 1
      AND (@SitioId IS NULL OR s.SitioId = @SitioId)
      AND (@NumeroPersonas IS NULL OR ua.CapacidadMaxima >= @NumeroPersonas)
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.ReservaUnidad ru
          INNER JOIN dbo.Reserva r ON r.ReservaId = ru.ReservaId
          WHERE ru.UnidadAlojamientoId = ua.UnidadAlojamientoId
            AND r.Estado NOT IN (N'Cancelada', N'Expirada')
            AND @FechaEntrada < ru.FechaFin
            AND @FechaSalida > ru.FechaInicio
      )
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.BloqueoDisponibilidad bd
          WHERE bd.UnidadAlojamientoId = ua.UnidadAlojamientoId
            AND @FechaEntrada < bd.FechaFin
            AND @FechaSalida > bd.FechaInicio
      )
    ORDER BY s.Nombre, ua.Codigo;
END;
GO