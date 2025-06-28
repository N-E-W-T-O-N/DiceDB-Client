#!/bin/bash
set -e

# Settings
PROTO_VERSION="31.1"
PROTOC_DIR="./tools/protoc"
PROTOC_BIN="$PROTOC_DIR/bin/protoc"
PROTO_ROOT="dicedb-protos"
PROTO_OUT="Protos"

# Determine platform
OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

# Map OS and ARCH to protoc zip naming
case "$OS" in
  linux)
    case "$ARCH" in
      x86_64) PLATFORM="linux-x86_64" ;;
      i386|i686) PLATFORM="linux-x86_32" ;;
      aarch64) PLATFORM="linux-aarch_64" ;;
      ppcle64) PLATFORM="linux-ppcle_64" ;;
      s390x) PLATFORM="linux-s390_64" ;;
      *) echo "Unsupported architecture: $ARCH"; exit 1 ;;
    esac
    ;;
  darwin)
    case "$ARCH" in
      x86_64) PLATFORM="osx-x86_64" ;;
      arm64) PLATFORM="osx-aarch_64" ;;
      *) echo "Unsupported macOS architecture: $ARCH"; exit 1 ;;
    esac
    ;;
  *)
    echo "Unsupported OS: $OS"
    exit 1
    ;;
esac

ZIP_NAME="protoc-${PROTO_VERSION}-${PLATFORM}.zip"
DOWNLOAD_URL="https://github.com/protocolbuffers/protobuf/releases/download/v${PROTO_VERSION}/${ZIP_NAME}"

# Download protoc if not already present
if [ ! -f "$PROTOC_BIN" ]; then
    echo "protoc not found, downloading from $DOWNLOAD_URL..."
    mkdir -p "$PROTOC_DIR"
    curl -L -o "$ZIP_NAME" "$DOWNLOAD_URL"
    unzip -o "$ZIP_NAME" -d "$PROTOC_DIR"
    rm "$ZIP_NAME"
    chmod +x "$PROTOC_BIN"
    echo "protoc downloaded and extracted to $PROTOC_BIN"
else
    echo "Using existing protoc binary at $PROTOC_BIN"
fi

# Create output directory
mkdir -p "$PROTO_OUT"

# Generate C# code
"$PROTOC_BIN" --proto_path="$PROTO_ROOT"   --csharp_out="$PROTO_OUT"   \
  --csharp_opt=file_extension=.cs  "$PROTO_ROOT"/*.proto  

echo "✅ C# files generated in $PROTO_OUT"
