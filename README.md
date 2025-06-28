protoc --proto_path=dicedb-protos --csharp_out=Protos --csharp_opt=file_extension=.cs  dicedb-protos/*.proto


$ docker run -p 7379:7379 dicedb/dicedb:latest --engine ironhawk --log-level debug --enable-wal