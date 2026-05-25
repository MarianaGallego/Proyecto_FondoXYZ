USE FondoXYZ;
GO

CREATE PROCEDURE dbo.SP_CalcularTarifaReserva
    @SitioId                      INT,
    @FechaEntrada                 DATE,
    @FechaSalida                  DATE,
    @NumeroPersonas               INT,
    @NumeroUnidades               INT = 1,
    @UnidadAlojamientoId          INT = NULL,
    @NumeroHabitacionesInternas   INT = NULL,
    @TemporadaId                  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 7;  -- Domingo=1, Lunes=2 ... Jueves=5

    DECLARE @TipoSitio              NVARCHAR(20);
    DECLARE @CategoriaTarifaId      INT;
    DECLARE @CapacidadPorUnidad     INT;
    DECLARE @PersonasPorUnidad      INT;
    DECLARE @NumeroNoches           INT;
    DECLARE @Fecha                  DATE;
    DECLARE @UnidadNum              INT;
    DECLARE @TarifaId               INT;
    DECLARE @PrecioBase             DECIMAL(12, 2);
    DECLARE @PrecioAdicional        DECIMAL(12, 2);
    DECLARE @PersonasAdicionales      INT;
    DECLARE @SubtotalLinea          DECIMAL(12, 2);
    DECLARE @TemporadaFecha         INT;
    DECLARE @EsFestivo              BIT;
    DECLARE @EsSemanaEscolar        BIT;
    DECLARE @EsTemporadaAlta        BIT;
    DECLARE @EsLunAJue              BIT;
    DECLARE @UsaTarifaEspecial      BIT;
    DECLARE @NombreCategoria        NVARCHAR(100);
    DECLARE @ConceptoBase           NVARCHAR(100);
    DECLARE @Total                  DECIMAL(12, 2) = 0;

    CREATE TABLE #Detalle
    (
        Fecha           DATE           NOT NULL,
        UnidadNum       INT            NOT NULL,
        Concepto        NVARCHAR(100)  NOT NULL,
        Cantidad        INT            NOT NULL,
        ValorUnitario   DECIMAL(12, 2) NOT NULL,
        Subtotal        DECIMAL(12, 2) NOT NULL,
        TarifaId        INT            NULL
    );

    IF @FechaSalida <= @FechaEntrada
    BEGIN
        RAISERROR(N'La fecha de salida debe ser posterior a la fecha de entrada.', 16, 1);
        RETURN;
    END;

    IF @NumeroPersonas < 1 OR @NumeroUnidades < 1
    BEGIN
        RAISERROR(N'El número de personas y de unidades debe ser mayor o igual a 1.', 16, 1);
        RETURN;
    END;

    SELECT @TipoSitio = TipoSitio
    FROM dbo.Sitio
    WHERE SitioId = @SitioId AND Activo = 1;

    IF @TipoSitio IS NULL
    BEGIN
        RAISERROR(N'El sitio no existe o no está activo.', 16, 1);
        RETURN;
    END;

    SET @NumeroNoches = DATEDIFF(DAY, @FechaEntrada, @FechaSalida);

    IF @UnidadAlojamientoId IS NOT NULL
    BEGIN
        SELECT
            @CategoriaTarifaId    = ua.CategoriaTarifaId,
            @CapacidadPorUnidad   = ua.CapacidadMaxima,
            @NumeroHabitacionesInternas = ua.NumeroHabitacionesInternas
        FROM dbo.UnidadAlojamiento ua
        WHERE ua.UnidadAlojamientoId = @UnidadAlojamientoId
          AND ua.SitioId = @SitioId
          AND ua.Activo = 1;

        IF @CategoriaTarifaId IS NULL
        BEGIN
            RAISERROR(N'La unidad de alojamiento no existe o no pertenece al sitio.', 16, 1);
            RETURN;
        END;
    END
    ELSE
    BEGIN
        IF @NumeroHabitacionesInternas IS NULL
        BEGIN
            RAISERROR(N'Indique la unidad de alojamiento o el número de habitaciones internas (1 o 2).', 16, 1);
            RETURN;
        END;

        SELECT TOP 1
            @CategoriaTarifaId  = ua.CategoriaTarifaId,
            @CapacidadPorUnidad = ua.CapacidadMaxima
        FROM dbo.UnidadAlojamiento ua
        WHERE ua.SitioId = @SitioId
          AND ua.Activo = 1
          AND ua.NumeroHabitacionesInternas = @NumeroHabitacionesInternas
        ORDER BY ua.UnidadAlojamientoId;

        IF @CategoriaTarifaId IS NULL
        BEGIN
            RAISERROR(N'No hay alojamiento configurado con ese número de habitaciones en el sitio.', 16, 1);
            RETURN;
        END;
    END;

    SELECT @NombreCategoria = Nombre
    FROM dbo.CategoriaTarifa
    WHERE CategoriaTarifaId = @CategoriaTarifaId;

    SET @PersonasPorUnidad = (@NumeroPersonas + @NumeroUnidades - 1) / @NumeroUnidades;

    IF @PersonasPorUnidad > @CapacidadPorUnidad
    BEGIN
        RAISERROR(N'El número de personas supera la capacidad por unidad de alojamiento.', 16, 1);
        RETURN;
    END;

    IF @NumeroPersonas > @NumeroUnidades * @CapacidadPorUnidad
    BEGIN
        RAISERROR(N'El número de personas supera la capacidad total de las unidades solicitadas.', 16, 1);
        RETURN;
    END;

    SET @Fecha = @FechaEntrada;

    WHILE @Fecha < @FechaSalida
    BEGIN
        SET @EsFestivo = CASE
            WHEN EXISTS (
                SELECT 1 FROM dbo.CalendarioEspecial
                WHERE Fecha = @Fecha AND Tipo = N'Festivo'
            ) THEN 1 ELSE 0 END;

        SET @EsSemanaEscolar = CASE
            WHEN EXISTS (
                SELECT 1 FROM dbo.CalendarioEspecial
                WHERE Fecha = @Fecha AND Tipo = N'SemanaEscolar'
            ) THEN 1 ELSE 0 END;

        SET @EsTemporadaAlta = CASE
            WHEN EXISTS (
                SELECT 1 FROM dbo.Temporada
                WHERE @Fecha BETWEEN FechaInicio AND FechaFin
                  AND EsTemporadaAlta = 1
            ) THEN 1 ELSE 0 END;

        SET @EsLunAJue = CASE
            WHEN DATEPART(WEEKDAY, @Fecha) BETWEEN 2 AND 5 THEN 1
            ELSE 0 END;

        SET @TemporadaFecha = @TemporadaId;

        IF @TemporadaFecha IS NULL
        BEGIN
            SELECT TOP 1 @TemporadaFecha = TemporadaId
            FROM dbo.Temporada
            WHERE @Fecha BETWEEN FechaInicio AND FechaFin
            ORDER BY EsTemporadaAlta DESC, TemporadaId;
        END;

        SET @UnidadNum = 1;

        WHILE @UnidadNum <= @NumeroUnidades
        BEGIN
            SET @TarifaId = NULL;
            SET @PrecioBase = NULL;
            SET @PrecioAdicional = NULL;
            SET @PersonasAdicionales = 0;

            /* ---- Santa Marta (apartamento completo por noche) ---- */
            IF @SitioId = 8
            BEGIN
                SELECT TOP 1
                    @TarifaId    = t.TarifaId,
                    @PrecioBase  = t.Precio
                FROM dbo.Tarifa t
                WHERE t.Activo = 1
                  AND t.SitioId = 8
                  AND t.CategoriaTarifaId = @CategoriaTarifaId
                  AND t.TipoConcepto = N'NocheApartamento'
                  AND t.TemporadaId = @TemporadaFecha
                  AND @NumeroPersonas BETWEEN t.PersonasMin AND t.PersonasMax;

                IF @TarifaId IS NULL
                BEGIN
                    RAISERROR(N'No se encontró tarifa de apartamento para la fecha o temporada indicada.', 16, 1);
                    RETURN;
                END;

                SET @ConceptoBase = N'Apto-noche ' + @NombreCategoria;

                INSERT INTO #Detalle (Fecha, UnidadNum, Concepto, Cantidad, ValorUnitario, Subtotal, TarifaId)
                VALUES (@Fecha, @UnidadNum, @ConceptoBase, 1, @PrecioBase, @PrecioBase, @TarifaId);
            END

            /* ---- Medellín Suramericana (por habitación / personas en la habitación) ---- */
            ELSE IF @SitioId = 7
            BEGIN
                IF @PersonasPorUnidad > 2
                BEGIN
                    RAISERROR(N'En Medellín cada habitación admite máximo 2 personas.', 16, 1);
                    RETURN;
                END;

                SELECT TOP 1
                    @TarifaId    = t.TarifaId,
                    @PrecioBase  = t.Precio
                FROM dbo.Tarifa t
                WHERE t.Activo = 1
                  AND t.SitioId = 7
                  AND t.CategoriaTarifaId = @CategoriaTarifaId
                  AND t.TipoConcepto = N'NochePorPersonas'
                  AND @PersonasPorUnidad BETWEEN t.PersonasMin AND t.PersonasMax;

                IF @TarifaId IS NULL
                BEGIN
                    RAISERROR(N'No se encontró tarifa para Medellín con ese número de personas por habitación.', 16, 1);
                    RETURN;
                END;

                SET @ConceptoBase = N'Habitación/noche (' + CAST(@PersonasPorUnidad AS NVARCHAR(10)) + N' pers.)';

                INSERT INTO #Detalle (Fecha, UnidadNum, Concepto, Cantidad, ValorUnitario, Subtotal, TarifaId)
                VALUES (@Fecha, @UnidadNum, @ConceptoBase, 1, @PrecioBase, @PrecioBase, @TarifaId);
            END

            /* ---- Sedes recreativas (noche base + persona adicional) ---- */
            ELSE IF @TipoSitio = N'SedeRecreativa'
            BEGIN
                SET @UsaTarifaEspecial = CASE
                    WHEN @EsLunAJue = 1
                     AND @EsFestivo = 0
                     AND @EsSemanaEscolar = 0
                     AND @EsTemporadaAlta = 0
                     AND @CategoriaTarifaId IN (3, 4)
                    THEN 1 ELSE 0 END;

                IF @UsaTarifaEspecial = 1
                BEGIN
                    SELECT TOP 1
                        @TarifaId         = t.TarifaId,
                        @PrecioBase       = t.Precio,
                        @PrecioAdicional  = t.PrecioPersonaAdicional
                    FROM dbo.Tarifa t
                    WHERE t.Activo = 1
                      AND t.SitioId IS NULL
                      AND t.CategoriaTarifaId = @CategoriaTarifaId
                      AND t.TipoConcepto = N'NocheBase'
                      AND t.DiasSemana IS NOT NULL;
                END
                ELSE
                BEGIN
                    SELECT TOP 1
                        @TarifaId         = t.TarifaId,
                        @PrecioBase       = t.Precio,
                        @PrecioAdicional  = t.PrecioPersonaAdicional
                    FROM dbo.Tarifa t
                    WHERE t.Activo = 1
                      AND t.SitioId IS NULL
                      AND t.CategoriaTarifaId IN (
                            CASE WHEN @CategoriaTarifaId IN (3, 4) THEN 2 ELSE @CategoriaTarifaId END,
                            CASE WHEN @CategoriaTarifaId IN (3, 4) THEN 4 ELSE @CategoriaTarifaId END
                        )
                      AND t.TipoConcepto = N'NocheBase'
                      AND t.DiasSemana IS NULL
                      AND (
                            (@CategoriaTarifaId NOT IN (3, 4) AND t.CategoriaTarifaId = @CategoriaTarifaId)
                            OR (@CategoriaTarifaId IN (3, 4) AND t.CategoriaTarifaId = 2)
                          );
                END;

                IF @TarifaId IS NULL
                BEGIN
                    RAISERROR(N'No se encontró tarifa para la sede y categoría indicadas.', 16, 1);
                    RETURN;
                END;

                IF @UsaTarifaEspecial = 1
                    SET @ConceptoBase = N'Tarifa especial - ' + @NombreCategoria;
                ELSE
                    SET @ConceptoBase = N'Tarifa - ' + @NombreCategoria;

                INSERT INTO #Detalle (Fecha, UnidadNum, Concepto, Cantidad, ValorUnitario, Subtotal, TarifaId)
                VALUES (@Fecha, @UnidadNum, @ConceptoBase, 1, @PrecioBase, @PrecioBase, @TarifaId);

                IF @PersonasPorUnidad > 4
                BEGIN
                    SET @PersonasAdicionales = @PersonasPorUnidad - 4;

                    SET @SubtotalLinea = @PersonasAdicionales * @PrecioAdicional;

                    INSERT INTO #Detalle (Fecha, UnidadNum, Concepto, Cantidad, ValorUnitario, Subtotal, TarifaId)
                    VALUES (
                        @Fecha, @UnidadNum,
                        N'Persona adicional - ' + @NombreCategoria,
                        @PersonasAdicionales, @PrecioAdicional, @SubtotalLinea, @TarifaId
                    );
                END;
            END
            ELSE
            BEGIN
                RAISERROR(N'Tipo de sitio no soportado para el cálculo de tarifa.', 16, 1);
                RETURN;
            END;

            SET @UnidadNum = @UnidadNum + 1;
        END;

        SET @Fecha = DATEADD(DAY, 1, @Fecha);
    END;

    SELECT @Total = SUM(Subtotal) FROM #Detalle;

    SELECT
        @SitioId                    AS SitioId,
        @FechaEntrada               AS FechaEntrada,
        @FechaSalida                AS FechaSalida,
        @NumeroNoches               AS NumeroNoches,
        @NumeroPersonas             AS NumeroPersonas,
        @NumeroUnidades             AS NumeroUnidades,
        @PersonasPorUnidad          AS PersonasPorUnidad,
        @CategoriaTarifaId          AS CategoriaTarifaId,
        @NombreCategoria            AS CategoriaTarifaNombre,
        @UnidadAlojamientoId       AS UnidadAlojamientoId,
        @NumeroHabitacionesInternas AS NumeroHabitacionesInternas,
        @Total                      AS Total;

    SELECT
        Fecha,
        UnidadNum,
        Concepto,
        Cantidad,
        ValorUnitario,
        Subtotal,
        TarifaId
    FROM #Detalle
    ORDER BY Fecha, UnidadNum, Concepto;

    DROP TABLE #Detalle;
END;
GO