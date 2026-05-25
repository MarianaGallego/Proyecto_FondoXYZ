# FondoXYZ

Proyecto para gestionar sitios, disponibilidad, tarifas, reservas, pagos y asociados.

## Requisitos

- .NET SDK 8.0 o superior.
- SQL Server local.
- SQL Server Management Studio para ejecutar el script de base de datos.

## Configuracion inicial

1. Crear la base de datos ejecutando el script:

   ```text
   Database/FondoXYZ.sql
   ```

   El script crea la base de datos `FondoXYZ`, sus tablas y los datos semilla.

4. Revisar la cadena de conexion en `appsettings.json`:

   ```json
   "ConnectionStrings": {
     "FondoXYZ": ""
   }
   ```

   Pon el nombre de tu instancia de SQL Server.

5. Verificar la configuracion JWT en `appsettings.json`. El proyecto necesita `Jwt:Issuer`, `Jwt:Audience` y `Jwt:Key` para iniciar.



## Ejecucion

Se puede ejecutar desde Visual Studio usando los perfiles `http`, `https` o `IIS Express`; O con:

```powershell
dotnet run
```



## Autenticacion

La mayoria de controladores requieren autenticacion JWT. Para iniciar sesion usa:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "maria.lopez@ejemplo.com",
  "clave": "1234567"
}
```

Usuarios semilla incluidos en el script:

- `maria.lopez@ejemplo.com`
- `carlos.ruiz@ejemplo.com`

Importante: la clave de los usuarios semilla ya esta guardada hasheada en la base de datos. La clave para autenticarse con esos usuarios es `1234567`.

La respuesta del login devuelve un token JWT. Para consumir los endpoints protegidos, envia ese token en el encabezado:

```http
Authorization: Bearer <token>
```

## Consideraciones importantes

- Al ejecutar el proyecto, la ventana que se abre automaticamente en el navegador puede parecer que no funciona. Esto ocurre porque todavia no se ha iniciado sesion y las rutas estan protegidas por autenticacion.
- Para probar la API, primero ejecuta `POST /api/auth/login` y luego usa el token JWT en Postman.
- Al ejecutar el endpoint de eliminar asociado (`DELETE /api/asociados/{asociadoId}`), el registro no se borra fisicamente de la base de datos. El asociado pasa a estado inactivo (`Activo = false`).
- El listado de asociados solo devuelve activos por defecto. Para incluir inactivos se puede usar `GET /api/asociados?incluirInactivos=true`.

