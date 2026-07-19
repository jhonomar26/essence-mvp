# Configuración de Git — essence-mvp

## Remoto
- **origin**: `https://github.com/jhonomar26/essence-mvp` (fetch y push)

## Ramas
- **Rama actual**: `main`
- **Tracking**: `main` sigue a `origin/main`
- **Ramas remotas existentes** (sin copia local):
  - `origin/feature/Snapshots-Dashboard`
  - `origin/feature/login`
  - `origin/feature/ui-improvements`

## Config local (`.git/config`)
```
core.repositoryformatversion=0
core.filemode=true
core.bare=false
core.logallrefupdates=true
submodule.active=.
remote.origin.url=https://github.com/jhonomar26/essence-mvp
remote.origin.fetch=+refs/heads/*:refs/remotes/origin/*
branch.main.remote=origin
branch.main.merge=refs/heads/main
```
> `submodule.active=.` está definido pero no hay `.gitmodules` en el repo, por lo que no aplica a ningún submódulo real.

## Identidad de usuario
⚠️ **No configurada** — no existe `~/.gitconfig` ni `user.name`/`user.email` en la config local del repo. Los commits antiguos muestran como autor "Omar Jhon Hualpa <101675711+jhonomar26@users.noreply.github.com>", pero eso viene del historial, no de la config activa. Para poder commitear desde este entorno hay que configurarlo, por ejemplo:
```bash
git config user.name "Tu Nombre"
git config user.email "tu@email.com"
```

## Hooks
Carpeta `.git/hooks` solo contiene los hooks de ejemplo (`*.sample`); ningún hook está activo.

## `.gitignore`
```
bin/
obj/
/packages/
riderModule.iml
/_ReSharper.Caches/
.vs/
*.csproj.user
```
Ignora artefactos típicos de build/IDE de un proyecto .NET/Rider.

## `.gitattributes`
Vacío — sin reglas de normalización de line endings ni de diff/merge definidas.

## Estado al momento de este documento
- Última confirmación en `main`: `6a51e90` — "Delete SLIDE5_SCRIPT_PRESENTACION.md"
- Cambios pendientes sin commitear:
  - Modificado: `src/EssenceMvp.Mvc/Program.cs`
  - Sin trackear: `.idea/.idea.EssenceMvp/.idea/.name`, `FRONTEND_MIGRATION_PLAN.md`, `frontend/`
