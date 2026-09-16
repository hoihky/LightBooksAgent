#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SOURCE_DIR="$ROOT/docs-src"
OUTPUT_DIR="$ROOT/docs"
MDWEB_DIR="${MDWEB_ROOT:-$ROOT/../MDWeb}"
THEME_DIR="$MDWEB_DIR/themes/default"
PORTFOLIO_ASSETS="$MDWEB_DIR/docs/assets"
INDEX_TEMPLATE="$ROOT/scripts/docs-index.html"
FOOTER='<p><a href="index.html">← Documentation overview</a></p>'

if [[ ! -d "$MDWEB_DIR/src/MDWeb.Cli" ]]; then
  echo "MDWeb not found at $MDWEB_DIR" >&2
  echo "Set MDWEB_ROOT to your MDWeb clone path." >&2
  exit 1
fi

if [[ ! -d "$SOURCE_DIR" ]]; then
  echo "Markdown source directory not found: $SOURCE_DIR" >&2
  exit 1
fi

mkdir -p "$OUTPUT_DIR"

echo "Generating HTML from $SOURCE_DIR..."
dotnet run --project "$MDWEB_DIR/src/MDWeb.Cli" -- \
  --source "$SOURCE_DIR" \
  --output "$OUTPUT_DIR" \
  --theme "$THEME_DIR" \
  --title "LightBooksAgent Documentation" \
  --description "LightBooksAgent project documentation" \
  --footer "$FOOTER"

if [[ -d "$PORTFOLIO_ASSETS" ]]; then
  echo "Applying MDWeb portfolio theme assets..."
  mkdir -p "$OUTPUT_DIR/assets"
  cp -R "$PORTFOLIO_ASSETS/." "$OUTPUT_DIR/assets/"
fi

if [[ -f "$INDEX_TEMPLATE" ]]; then
  echo "Installing documentation overview page..."
  cp "$INDEX_TEMPLATE" "$OUTPUT_DIR/index.html"
fi

echo "Linking generated pages back to overview..."
for page in plan.html system-design.html roadmap.html; do
  if [[ -f "$OUTPUT_DIR/$page" ]]; then
    perl -0pi -e 's|<a href="[^"]*" class="brand">|<a href="index.html" class="brand">|g' "$OUTPUT_DIR/$page"
  fi
done

echo "Done. Open docs/index.html or run: npx serve docs"
