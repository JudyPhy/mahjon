@echo ���ɿͻ�����Ϣ�ļ�

xcopy /R /Y mahjon.proto protobuf-net\ProtoGen\
cd protobuf-net\ProtoGen
protogen.exe -i:mahjon.proto -o:pbmsg.cs
xcopy /R /Y pbmsg.cs ..\..\client\

@echo ɾ���м��ļ�
del mahjon.proto
del pbmsg.cs
cd ../../

@echo ���ɷ������Ϣ�ļ�
xcopy /R /Y mahjon.proto server\
cd server
protoc.exe  --plugin=protoc-gen-go=protoc-gen-go.exe --go_out . --proto_path .  mahjon.proto

@echo ɾ���м��ļ�
del mahjon.proto
cd ../

pause