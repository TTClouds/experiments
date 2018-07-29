setlocal

cd /d %~dp0\..

set PROTOC=.\packages\Google.Protobuf.Tools\tools\windows_x64\protoc.exe
set PLUGIN=.\packages\Grpc.Tools\tools\windows_x64\grpc_csharp_plugin.exe

%PROTOC% --proto_path=.\protos --csharp_out=.\CsProto --grpc_out=.\CsProto --csharp_opt=file_extension=.g.cs,base_namespace=CsProto --plugin=protoc-gen-grpc=%PLUGIN% .\protos\HelloWorld.proto
