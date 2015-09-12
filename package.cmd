%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe pusher-dotnet-client.sln /t:Clean
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe pusher-dotnet-client.sln /t:PusherClient:Rebuild /p:Configuration=Release /fileLogger
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe pusher-dotnet-client.sln /t:PusherClient_Android:Rebuild /p:Configuration=Release /fileLogger
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe pusher-dotnet-client.sln /t:PusherClient_iOS:Rebuild /p:Configuration=Release /fileLogger
"%programfiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" pusher-dotnet-client.sln /t:PusherClient_UWP:Rebuild /p:Configuration=Release /fileLogger

if exist Download\package rm -rf Download\package
if not exist Download\package\lib\net40 mkdir Download\package\lib\net40\
if not exist Download\package\lib\monoandroid22 mkdir Download\package\lib\monoandroid22\
if not exist Download\package\lib\Xamarin.iOS10 mkdir Download\package\lib\Xamarin.iOS10\
if not exist Download\package\lib\uap10.0 mkdir Download\package\lib\uap10.0\

copy README.md Download\Package\

copy PusherClient\bin\Release\PusherClient.dll Download\Package\lib\net40\
copy PusherClient\bin\Release\PusherClient.xml Download\Package\lib\net40\

copy PusherClient.Android\bin\Release\PusherClient.Android.dll Download\Package\lib\monoandroid22\

copy PusherClient.iOS\bin\iPhone\Release\PusherClient.iOS.dll Download\Package\lib\Xamarin.iOS10\

copy PusherClient.UWP\bin\Release\PusherClient.UWP.dll Download\Package\lib\uap10.0\

tools\nuget.exe update -self
tools\nuget.exe pack pusher-dotnet-client.nuspec -BasePath Download\Package -Output Download