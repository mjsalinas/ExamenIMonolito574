# RefugioMascotas — Proyecto base del Examen Práctico I

Web API en .NET (arquitectura Monolito, mismo patrón que `BibliotecaMonolito`) para el
sistema de cuidadores y mascotas del refugio "Patitas Felices". Este es el punto de
partida del examen — **no lo borres ni lo reescribas desde cero**: tu trabajo es
completar lo que falta, siguiendo los tickets del documento del examen.

## Qué ya funciona

- `GET /api/cuidadores` y `GET /api/mascotas` — listado completo, probado.
- `POST /api/cuidadores` y `POST /api/mascotas` — creación básica, pero **incompleta**
  (sin normalización de texto, sin validar duplicados, sin validar formato de `Turno`).
- Modelos `Cuidador` y `Mascota`, con la relación 1 a N ya configurada en
  `RefugioDbContext`.
- Swagger habilitado en modo desarrollo.

## Qué falta (ver el examen para el detalle de cada ticket)

Cada `// TODO` en los controllers indica qué ticket cubre esa funcionalidad. No los
borres al completarlos — puedes dejarlos como comentario tachado o simplemente
reemplazar el TODO por el código final.

## Cómo correr el proyecto

```bash
dotnet restore
dotnet tool install --global dotnet-ef   # si no lo tienes instalado
dotnet ef migrations add Init
dotnet ef database update
dotnet run
```

Luego abre la URL de Swagger que imprime la consola (o `http://localhost:5080/swagger`).

## Solución de problemas

**`dotnet-ef` no reconocido / "no existe el comando o el archivo"**
Instala la herramienta global y asegúrate de que esté en tu PATH:

```bash
dotnet tool install --global dotnet-ef
```

En macOS, si tras instalarlo sigue sin encontrarse, agrega esto a tu `~/.zshrc` y abre
una terminal nueva:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

**`UnauthorizedAccessException` al instalar la herramienta global**
Suele deberse a permisos sobre `~/.dotnet/tools`. Verifica que la carpeta exista y sea
tuya (`ls -la ~/.dotnet`), o reinstala con `dotnet tool install --global dotnet-ef
--tool-path ~/.dotnet/tools` para forzar la ruta.

**Solo aparece la tabla `__EFMigrationsHistory` y no `Cuidadores`/`Mascotas`**
Significa que corriste `dotnet run` antes de crear la migración. Corre `dotnet ef
migrations add Init` y `dotnet ef database update` primero.

**Cambié el modelo y la base de datos no refleja el cambio**
Crea una nueva migración: `dotnet ef migrations add NombreDelCambio` y luego `dotnet ef
database update`. No borres `refugio.db` a menos que no te importe perder los datos de
prueba.
