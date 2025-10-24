#!/usr/bin/env bash
set -euo pipefail

# Build, pack, and publish a NuGet package for SolidAvalonia.
#
# Usage examples:
#   scripts/publish-nuget.sh --version 0.1.0 --api-key $NUGET_API_KEY
#   scripts/publish-nuget.sh --no-push            # Just build + pack
#   scripts/publish-nuget.sh --source https://api.nuget.org/v3/index.json
#
# Options:
#   -p, --project      Path to csproj (default: SolidAvalonia.csproj)
#   -c, --configuration Build config (default: Release)
#   -o, --output       Output dir for packages (default: artifacts/nuget)
#   -v, --version      Package version to use (overrides csproj)
#   -k, --api-key      NuGet API key (or set NUGET_API_KEY env var)
#   -s, --source       NuGet source URL (default: https://api.nuget.org/v3/index.json)
#       --no-push      Do not push to NuGet (build/pack only)
#       --dry-run      Print commands only, do not execute
#       --help         Show help

PROJECT="SolidAvalonia.csproj"
CONFIGURATION="Release"
OUTPUT_DIR="artifacts/nuget"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"
API_KEY="${NUGET_API_KEY:-}"
VERSION=""
NO_PUSH=false
DRY_RUN=false

log() { echo "[publish] $*"; }
err() { echo "[publish][error] $*" >&2; }
run() {
  if $DRY_RUN; then
    echo "+ $*"
  else
    eval "$@"
  fi
}

show_help() {
  sed -n '1,80p' "$0" | sed -n '1,60p' | sed 's/^# \{0,1\}//'
}

parse_args() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      -p|--project) PROJECT="$2"; shift 2 ;;
      -c|--configuration) CONFIGURATION="$2"; shift 2 ;;
      -o|--output) OUTPUT_DIR="$2"; shift 2 ;;
      -v|--version) VERSION="$2"; shift 2 ;;
      -k|--api-key) API_KEY="$2"; shift 2 ;;
      -s|--source) NUGET_SOURCE="$2"; shift 2 ;;
      --no-push) NO_PUSH=true; shift ;;
      --dry-run) DRY_RUN=true; shift ;;
      --help|-h) show_help; exit 0 ;;
      *) err "Unknown argument: $1"; show_help; exit 1 ;;
    esac
  done
}

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    err "Required command not found: $1"
    exit 1
  fi
}

extract_xml_value() {
  local file="$1" tag="$2"
  # macOS/BSD sed friendly: extract first occurrence of <Tag>value</Tag>
  sed -n "s:.*<${tag}>\\(.*\\)</${tag}>.*:\\1:p" "$file" | head -n 1 || true
}

ensure_version() {
  if [[ -n "$VERSION" ]]; then
    return
  fi
  if [[ -f "$PROJECT" ]]; then
    local detected
    detected=$(extract_xml_value "$PROJECT" Version || true)
    if [[ -n "$detected" ]]; then
      VERSION="$detected"
      return
    fi
  fi
  err "Package version not found. Provide via --version or add <Version> to $PROJECT"
  exit 1
}

main() {
  parse_args "$@"

  require_cmd dotnet
  mkdir -p "$OUTPUT_DIR"

  if [[ ! -f "$PROJECT" ]]; then
    err "Project file not found: $PROJECT"
    exit 1
  fi

  # Determine PackageId
  PACKAGE_ID=$(extract_xml_value "$PROJECT" PackageId || true)
  if [[ -z "$PACKAGE_ID" ]]; then
    # Fallback to project filename without extension
    PACKAGE_ID="$(basename "$PROJECT" .csproj)"
  fi

  # Determine Authors (optional)
  AUTHORS=$(extract_xml_value "$PROJECT" Authors || true)

  # Try to determine RepositoryUrl from git
  REPOSITORY_URL=""
  if command -v git >/dev/null 2>&1; then
    REPOSITORY_URL=$(git -C "$(dirname "$PROJECT")" config --get remote.origin.url 2>/dev/null || true)
    REPOSITORY_URL=${REPOSITORY_URL%.git}
  fi

  ensure_version

  log "Project      : $PROJECT"
  log "Configuration: $CONFIGURATION"
  log "Output dir   : $OUTPUT_DIR"
  log "PackageId    : $PACKAGE_ID"
  log "Version      : $VERSION"
  log "NuGet source : $NUGET_SOURCE"

  # Restore and Build
  run dotnet restore "$PROJECT"
  run dotnet build "$PROJECT" -c "$CONFIGURATION" -nologo

  # Pack with sensible defaults; allow csproj to override.
  PACK_PROPS=(
    "/p:PackageId=$PACKAGE_ID"
    "/p:Version=$VERSION"
    "/p:ContinuousIntegrationBuild=true"
    "/p:IncludeSymbols=true"
    "/p:SymbolPackageFormat=snupkg"
    "/p:PackageLicenseExpression=MIT"
    "/p:PackageReadmeFile=README.md"
  )
  if [[ -n "$AUTHORS" ]]; then PACK_PROPS+=("/p:Authors=$AUTHORS"); fi
  if [[ -n "$REPOSITORY_URL" ]]; then PACK_PROPS+=("/p:RepositoryUrl=$REPOSITORY_URL"); fi

  run dotnet pack "$PROJECT" -c "$CONFIGURATION" -o "$OUTPUT_DIR" "${PACK_PROPS[@]}"

  # Resolve package file names based on PACKAGE_ID and VERSION
  NUPKG="$OUTPUT_DIR/${PACKAGE_ID}.${VERSION}.nupkg"
  SNUPKG="$OUTPUT_DIR/${PACKAGE_ID}.${VERSION}.snupkg"

  if [[ ! -f "$NUPKG" ]]; then
    err "Package not found: $NUPKG"
    err "Available packages in $OUTPUT_DIR:"; ls -1 "$OUTPUT_DIR" || true
    exit 1
  fi

  if $NO_PUSH; then
    log "Package created: $NUPKG"
    [[ -f "$SNUPKG" ]] && log "Symbols created: $SNUPKG"
    log "--no-push specified; skipping publish."
    exit 0
  fi

  if [[ -z "$API_KEY" ]]; then
    err "NuGet API key not provided. Set NUGET_API_KEY or use --api-key."
    exit 1
  fi

  # Push main package
  run dotnet nuget push "$NUPKG" \
    --api-key "$API_KEY" \
    --source "$NUGET_SOURCE" \
    --skip-duplicate

  # Push symbols package if present
  if [[ -f "$SNUPKG" ]]; then
    run dotnet nuget push "$SNUPKG" \
      --api-key "$API_KEY" \
      --source "$NUGET_SOURCE" \
      --skip-duplicate
  fi

  log "Publish complete for $PACKAGE_ID $VERSION"
}

main "$@"

