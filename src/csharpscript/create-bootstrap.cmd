pushd %~dp0

SET SEVZIP="C:\Program Files\7-Zip\7z.exe"

rd /s /q bin
rd /s /q obj

dotnet publish CSharpScript.csproj /p:PublishProfile=WindowsProfile
dotnet publish CSharpScript.csproj /p:PublishProfile=LinuxProfile
dotnet publish CSharpScript.csproj /p:PublishProfile=MacProfile

pushd %~dp0bin\Debug\net5.0\win-x86\publish
SET ZIPFILE=csharpscript.zip
SET ZIPFULLPATH=%~dp0%ZIPFILE%
IF EXIST %ZIPFULLPATH% del /q %ZIPFULLPATH%
%SEVZIP% a -r %ZIPFULLPATH% *
popd

REM Send to build server to known location
curl -H "Accept: application / json" -H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJEYXRlTm93IjoiMjAyMS0xMC0xM1QwNDowNjoyMC42MzYxMTA1WiIsImV4cCI6MTY2NTYzMzk4MH0.uv2JOVdLdW7R4KR57aP3u5rbOTtS3mKGdubtiR9EIc8" "https://builds.privapp.com/poststageartifact?pipelineLabel=bootstrap&filename=%ZIPFILE%" -H "Content-Type:application/octet-stream" --data-binary @%ZIPFULLPATH%


pushd %~dp0bin\Debug\net5.0\linux-x64\publish
SET ZIPFILE=csharpscript-linux.zip
SET ZIPFULLPATH=%~dp0%ZIPFILE%
IF EXIST %ZIPFULLPATH% del /q %ZIPFULLPATH%
%SEVZIP% a -r %ZIPFULLPATH% *

popd

REM Send to build server to known location
curl -H "Accept: application / json" -H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJEYXRlTm93IjoiMjAyMS0xMC0xM1QwNDowNjoyMC42MzYxMTA1WiIsImV4cCI6MTY2NTYzMzk4MH0.uv2JOVdLdW7R4KR57aP3u5rbOTtS3mKGdubtiR9EIc8" "https://builds.privapp.com/poststageartifact?pipelineLabel=bootstrap&filename=%ZIPFILE%" -H "Content-Type:application/octet-stream" --data-binary @%ZIPFULLPATH%


pushd %~dp0bin\Debug\net5.0\osx-x64\publish
SET ZIPFILE=csharpscript-mac.zip
SET ZIPFULLPATH=%~dp0%ZIPFILE%
IF EXIST %ZIPFULLPATH% del /q %ZIPFULLPATH%
%SEVZIP% a -r %ZIPFULLPATH% *

popd

REM Send to build server to known location
curl -H "Accept: application / json" -H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJEYXRlTm93IjoiMjAyMS0xMC0xM1QwNDowNjoyMC42MzYxMTA1WiIsImV4cCI6MTY2NTYzMzk4MH0.uv2JOVdLdW7R4KR57aP3u5rbOTtS3mKGdubtiR9EIc8" "https://builds.privapp.com/poststageartifact?pipelineLabel=bootstrap&filename=%ZIPFILE%" -H "Content-Type:application/octet-stream" --data-binary @%ZIPFULLPATH%

pause