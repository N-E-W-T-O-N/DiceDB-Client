#!/bin/bash
set -e

PROTOC=protoc
PROTO_ROOT="dicedb-protos"
PROTO_OUT="Protos"

mkdir -p "$PROTO_OUT"

$PROTOC --proto_path="$PROTO_ROOT" --csharp_out="$PROTO_OUT" --csharp_opt=internal_access,file_extension=.cs "$PROTO_ROOT"/*.proto

echo "C# files generated in $PROTO_OUT"



protoc --proto_path=dicedb-protos --csharp_out=Protos --csharp_opt=file_extension=.cs  dicedb-protos/*.proto
