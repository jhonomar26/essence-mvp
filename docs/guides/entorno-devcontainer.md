# Entorno de desarrollo con Podman (devcontainer)

Entorno de desarrollo aislado: .NET 10, Node 22, pnpm, Postgres 17 y Claude Code dentro de contenedores. El sistema anfitrión solo necesita `podman` y `podman-compose`.

## Arranque rápido

```bash
cd .devcontainer
podman-compose up -d                                   # levanta app + db
podman exec -it -w /workspace essence-mvp-dev_app_1 bash   # entrar al entorno
```

Dentro del contenedor:

```bash
dotnet run --project src/EssenceMvp.Mvc      # API en http://localhost:5070
cd frontend && pnpm install && pnpm dev --host   # Vite en http://localhost:5173
claude                                        # login la primera vez
gh auth login                                 # primera vez, para git push
```

Apagar: `podman-compose down` (conserva datos). Borrar también datos: `podman-compose down -v`.

## Qué hay

| Pieza | Detalle |
|---|---|
| Contenedor `app` | Ubuntu 24.04, .NET SDK 10, Node 22, pnpm, `git`, `gh`, `psql`, Claude Code. Usuario `dev` (uid 1000) con `sudo` sin password **solo dentro del contenedor** |
| Contenedor `db` | Postgres 17. Al crearse ejecuta `db/essence_mvp_schema.sql` (tablas + datos semilla) |
| Volumen del proyecto | La carpeta del repo se monta en `/workspace`; no hay copia, los cambios se ven en ambos lados |
| Volumen `pgdata` | Datos de Postgres. Persisten entre reinicios |
| Volumen `claude-home` | Login y config de Claude Code dentro del contenedor |
| Puertos | `127.0.0.1:5070` (API), `5173` (Vite), `5432` (Postgres). Solo local |

La cadena de conexión se sobrescribe con la variable `ConnectionStrings__DefaultConnection` (host `db`, contraseña de desarrollo `postgres`). El `appsettings` no cambia.

## Archivos

- `.devcontainer/Dockerfile`: imagen del entorno de desarrollo.
- `.devcontainer/compose.yaml`: define los servicios `app` y `db`, volúmenes y puertos.
- `.devcontainer/devcontainer.json`: permite abrirlo desde VS Code ("Reopen in Container").

## Podman y Docker

Podman usa los mismos formatos que Docker (`Dockerfile`, `compose.yaml`, imágenes OCI) y casi los mismos comandos. Diferencias:

- No hay daemon: cada contenedor es un proceso de tu usuario.
- Rootless por defecto: aunque `dev` tenga `sudo` dentro, no puede tocar el sistema anfitrión.
- `userns_mode: keep-id` en el compose hace que tu uid del host sea el uid 1000 del contenedor, así los archivos que se crean dentro quedan con tu dueño. Es específico de Podman: con Docker hay que ajustar permisos.

## Reutilizarlo en otro repo

1. Copiar `.devcontainer/` al repo nuevo.
2. Ajustar en `Dockerfile` el SDK y las herramientas (versión de .NET/Node, etc.).
3. Ajustar en `compose.yaml`: nombre (`name:`), puertos, nombre de la base y el script SQL de arranque.
4. Si la app lee su conexión de `appsettings`, sobrescribirla con variables de entorno en lugar de editar el archivo.
5. Probar con `podman-compose build` y `podman-compose up -d`.

## Equipo nuevo

```bash
sudo pacman -S podman podman-compose      # o el equivalente de la distro
git clone <repo> && cd <repo>/.devcontainer
podman-compose up -d
```

Se repiten por equipo: login de `claude` y `gh auth login` dentro del contenedor. Los datos de `pgdata` no viajan, solo el schema con su semilla; si importan, usar `pg_dump`.

## Seguridad

- No montar `~/.ssh` ni todo `~/.claude` en el contenedor. El contenedor aísla los comandos, no protege las credenciales que se le montan.
- Usar un token de `gh` limitado a este repositorio.
- La contraseña `postgres` es solo de desarrollo.
- En el anfitrión, la instalación de paquetes por parte de Claude (fuera del contenedor) se limita a un script con sudoers que solo acepta nombres de paquete de pacman. Dentro del contenedor no hace falta.

## Problemas encontrados al montarlo

| Síntoma | Causa | Solución |
|---|---|---|
| `looking up compose provider failed` | Falta un proveedor de compose | `pacman -S podman-compose` |
| `useradd: UID 1000 is not unique` | La imagen Ubuntu ya trae el usuario `ubuntu` con uid 1000 | El `Dockerfile` lo renombra a `dev` |
| `RUN corepack enable` falla | Node copiado desde otra imagen no resuelve bien los enlaces de corepack | Se instala `pnpm` con `npm install -g pnpm` |
| `syntax error at or near "//"` al crear la base | Comentario `//` inválido en `db/essence_mvp_schema.sql` | Cambiado a `--` |
| El schema no se recarga | `docker-entrypoint-initdb.d` solo corre con el volumen vacío | `podman-compose down -v` y volver a levantar |
