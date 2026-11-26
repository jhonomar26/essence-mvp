# Entorno de desarrollo con Podman (devcontainer)

Entorno de desarrollo aislado: .NET 10, Node 22, pnpm, Postgres 17 y Claude Code dentro de contenedores. El sistema anfitrión solo necesita `podman` y `podman-compose`.

## Cada día (atajo)

Con la función de fish `essence` (ver abajo), un solo comando levanta los contenedores si hacen falta, entra y abre Claude:

```bash
essence          # levanta si está apagado, entra y corre claude
essence bash     # una terminal dentro del contenedor
essence down     # apagar los contenedores
```

La función vive en `~/.config/fish/functions/essence.fish` (es de tu equipo, no del repo). Para recrearla en otro equipo:

```fish
function essence
    set -l dir ~/Proyectos/essence-mvp/.devcontainer
    set -l ctr essence-mvp-dev_app_1
    # No -f: podman-compose then also loads compose.override.yaml
    if test "$argv[1]" = down; pushd $dir >/dev/null; podman-compose down; popd >/dev/null; return; end
    if test (podman inspect -f '{{.State.Running}}' $ctr 2>/dev/null) != true
        pushd $dir >/dev/null; podman-compose up -d >/dev/null; set -l rc $status; popd >/dev/null
        test $rc -eq 0; or return 1
    end
    set -l tty -i; test -t 0; and set tty -it
    test (count $argv) -eq 0; and set argv claude
    podman exec $tty -w /workspace $ctr $argv
end
```

Guardar con `funcsave essence`. Sin fish, usar los comandos manuales de abajo.

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
| Volumen `claude-home` | Login, historial y ajustes de Claude Code propios del contenedor (`CLAUDE_CONFIG_DIR=/home/dev/.claude`) |
| Volumen `engram-data` | Memoria de Engram propia del contenedor (`ENGRAM_DATA_DIR=/home/dev/.engram`) |
| `compose.override.yaml` (no se sube a git) | Montajes propios de tu equipo: binarios `engram` y `gentle-ai` en solo lectura y la bóveda de notas. Plantilla en `compose.override.example.yaml` |
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

## Claude dentro del contenedor

El contenedor tiene su propia configuración de Claude, no la de tu equipo. Se crea una vez con:

```bash
essence bash
bash /workspace/.devcontainer/setup-claude.sh   # instala el stack Gentle AI y registra los MCP
```

El script ejecuta `gentle-ai install` (mismo preset y persona que en el anfitrión) y copia los MCP (`engram`, `context7`) donde Claude los lee, porque con `CLAUDE_CONFIG_DIR` Claude ignora `~/.claude.json`. Todo queda en el volumen `claude-home` y sobrevive a recrear el contenedor.

Por qué no se monta tu `~/.claude`:
- `.credentials.json` es tu login: compartirlo expondría tu token a un entorno con `sudo` libre.
- `settings.json` define hooks: uno modificado desde el contenedor se ejecutaría en tu equipo.
- Plugins y MCP guardan rutas absolutas de tu home.

Consecuencia: la memoria de Engram y los ajustes del contenedor son independientes de los del anfitrión. Lo que deba perdurar y compartirse se escribe en documentos (repo o bóveda), no solo en la memoria.

## Dónde se escriben los documentos

Los documentos son archivos `.md`, por eso no hay nada que sincronizar ni nada que instalar en el contenedor (ni Obsidian ni Joplin):

- El repo se monta en `/workspace`: lo que escribe Claude en `docs/` se ve en tu equipo al instante.
- La bóveda de notas se monta en la misma ruta que en tu equipo (vía `compose.override.yaml`): Obsidian, en tu equipo, ve los cambios en vivo.

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
