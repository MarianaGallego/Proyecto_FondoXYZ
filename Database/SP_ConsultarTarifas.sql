USE FondoXYZ;
GO

CREATE PROCEDURE dbo.SP_ConsultarTarifas
    @SitioId              INT = NULL,
    @UnidadAlojamientoId  INT = NULL,
    @TemporadaId          INT = NULL,
    @NumeroPersonas       INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SitioFiltro         INT = @SitioId;
    DECLARE @CategoriaTarifaId INT = NULL;

    IF @NumeroPersonas IS NOT NULL AND @NumeroPersonas < 1
    BEGIN
        RAISERROR(N'El número de personas debe ser mayor o igual a 1.', 16, 1);
        RETURN;
    END;

    IF @UnidadAlojamientoId IS NOT NULL
    BEGIN
        SELECT
            @CategoriaTarifaId = ua.CategoriaTarifaId,
            @SitioFiltro       = COALESCE(@SitioId, ua.SitioId)
        FROM dbo.UnidadAlojamiento ua
        WHERE ua.UnidadAlojamientoId = @UnidadAlojamientoId
          AND ua.Activo = 1;

        IF @CategoriaTarifaId IS NULL
        BEGIN
            RAISERROR(N'La unidad de alojamiento no existe o no está activa.', 16, 1);
            RETURN;
        END;

        IF @SitioId IS NOT NULL AND @SitioId <> @SitioFiltro
        BEGIN
            RAISERROR(N'La unidad de alojamiento no pertenece al sitio indicado.', 16, 1);
            RETURN;
        END;
    END;

    IF @SitioFiltro IS NULL
    BEGIN
        RAISERROR(N'Debe indicar el sitio o la unidad de alojamiento.', 16, 1);
        RETURN;
    END;

    SELECT
        t.TarifaId,
        t.TipoConcepto,
        t.Precio,
        t.PrecioPersonaAdicional,
        t.PersonasMin,
        t.PersonasMax,
        t.DiasSemana,
        t.ExcluirFestivos,
        t.ExcluirSemanaEscolar,
        t.ExcluirTemporadaAlta,
        t.VigenciaDesde,
        t.VigenciaHasta,
        t.SitioId,
        s.Codigo              AS SitioCodigo,
        s.Nombre              AS SitioNombre,
        t.CategoriaTarifaId,
        ct.Codigo             AS CategoriaTarifaCodigo,
        ct.Nombre             AS CategoriaTarifaNombre,
        t.TemporadaId,
        temp.Codigo           AS TemporadaCodigo,
        temp.Nombre           AS TemporadaNombre,
        temp.EsTemporadaAlta,
        @UnidadAlojamientoId  AS UnidadAlojamientoId
    FROM dbo.Tarifa t
    LEFT JOIN dbo.Sitio s
        ON s.SitioId = t.SitioId
    LEFT JOIN dbo.CategoriaTarifa ct
        ON ct.CategoriaTarifaId = t.CategoriaTarifaId
    LEFT JOIN dbo.Temporada temp
        ON temp.TemporadaId = t.TemporadaId
    WHERE t.Activo = 1
      AND (t.SitioId IS NULL OR t.SitioId = @SitioFiltro)
      AND (
            @CategoriaTarifaId IS NULL
            AND (
                t.CategoriaTarifaId IN (
                    SELECT DISTINCT ua.CategoriaTarifaId
                    FROM dbo.UnidadAlojamiento ua
                    WHERE ua.SitioId = @SitioFiltro
                      AND ua.Activo = 1
                )
                OR (
                    t.CategoriaTarifaId IS NULL
                    AND (t.SitioId IS NULL OR t.SitioId = @SitioFiltro)
                )
            )
            OR @CategoriaTarifaId IS NOT NULL
            AND (
                t.CategoriaTarifaId = @CategoriaTarifaId
                OR (
                    t.CategoriaTarifaId IS NULL
                    AND t.SitioId = @SitioFiltro
                    AND t.TipoConcepto = N'ServicioAdicional'
                )
            )
          )
      AND (
            @TemporadaId IS NULL
            OR t.TemporadaId = @TemporadaId
            OR t.TemporadaId IS NULL
          )
      AND (
            @NumeroPersonas IS NULL
            OR (@NumeroPersonas >= t.PersonasMin AND @NumeroPersonas <= t.PersonasMax)
          )
    ORDER BY t.TipoConcepto, t.TemporadaId, t.PersonasMin;
END;
GO