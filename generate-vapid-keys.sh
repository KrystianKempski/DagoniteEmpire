#!/usr/bin/env bash
# Generates a VAPID key pair for Web Push notifications.
#
# The pair identifies this server to the browser push services (FCM, Mozilla, APNs).
# Generate it ONCE per environment and keep it stable: replacing the keys invalidates
# every subscription already stored in WebPushSubscriptions, and users would have to
# re-enable notifications on each device.
#
# Output is printed in "KEY=value" form, ready to paste into a Kubernetes Secret,
# a docker-compose env file, or `dotnet user-secrets set`.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROPS="$ROOT_DIR/Directory.Packages.props"

# Use the same WebPush version the app is built against, so the generated keys are
# guaranteed to be accepted by the runtime that will sign the notifications.
WEBPUSH_VERSION="$(sed -n 's/.*Include="WebPush" Version="\([^"]*\)".*/\1/p' "$PROPS" | head -1)"
if [[ -z "$WEBPUSH_VERSION" ]]; then
  echo "ERROR: could not read the WebPush version from $PROPS" >&2
  exit 1
fi

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

dotnet new console -o "$WORK_DIR/vapidgen" >/dev/null
cd "$WORK_DIR/vapidgen"

# Standalone project: the solution uses central package management, which would
# reject an explicit version here.
cat > Directory.Packages.props <<'PROPS_EOF'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
PROPS_EOF

dotnet add package WebPush --version "$WEBPUSH_VERSION" >/dev/null

cat > Program.cs <<'CS_EOF'
var keys = WebPush.VapidHelper.GenerateVapidKeys();
Console.WriteLine($"WebPush__PublicKey={keys.PublicKey}");
Console.WriteLine($"WebPush__PrivateKey={keys.PrivateKey}");
CS_EOF

echo "# VAPID keys generated with WebPush $WEBPUSH_VERSION on $(date -Iseconds)"
echo "# PublicKey is handed to browsers and is not secret; PrivateKey must stay secret."
dotnet run --no-launch-profile
