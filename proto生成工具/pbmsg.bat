@echo 生成客户端消息文件

xcopy /R /Y mahjon.proto protobuf-net\ProtoGen\
cd protobuf-net\ProtoGen
protogen.exe -i:mahjon.proto -o:pbmsg.cs
xcopy /R /Y pbmsg.cs ..\..\client\

@echo 删除中间文件
del mahjon.proto
del pbmsg.cs
cd ../../

@echo 生成服务端消息文件
xcopy /R /Y mahjon.proto server\
cd server
protoc.exe  --plugin=protoc-gen-go=protoc-gen-go.exe --go_out . --proto_path .  mahjon.proto

@echo 删除中间文件
del mahjon.proto
cd ../

pause