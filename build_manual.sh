#!/bin/zsh
set -euo pipefail

dotnet_bin=/usr/local/share/dotnet/dotnet
sdk_dir=/usr/local/share/dotnet/sdk/10.0.202
csc="$sdk_dir/Roslyn/bincore/csc.dll"

game_dir="${GAME_DIR:-$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64}"
project_dir="$(cd "$(dirname "$0")" && pwd)"
out_subdir="${OUT_SUBDIR:-manual}"
platform_target="${PLATFORM_TARGET:-}"
out_dir="$project_dir/bin/$out_subdir"

if [[ ! -f "$game_dir/sts2.dll" ]]; then
  echo "sts2.dll not found under: $game_dir" >&2
  echo "Set GAME_DIR to your StS2 runtime directory." >&2
  exit 1
fi

mkdir -p "$out_dir"

refs=()
for f in "$game_dir"/*.dll; do
  refs+=("-r:$f")
done

srcs=(
  "$project_dir/src/"*.cs
  "$project_dir/src/Sts2/"*.cs
)

"$dotnet_bin" "$csc" \
  -nologo \
  -langversion:latest \
  -nullable:enable \
  -target:library \
  ${platform_target:+-platform:$platform_target} \
  -out:"$out_dir/rest_site_upgrade_all.dll" \
  "${refs[@]}" \
  "${srcs[@]}"

cp "$project_dir/rest_site_upgrade_all.json" "$out_dir/"

echo "Built:"
echo "  $out_dir/rest_site_upgrade_all.dll"
echo "  $out_dir/rest_site_upgrade_all.json"
