
cd Dapper.Json
dotnet build -c Release
dotnet pack -c Release -o ../nugets

cd ../Dapper.Json.System
dotnet build -c Release
dotnet pack -c Release -o ../nugets

cd ../Dapper.Json.Newtonsoft
dotnet build -c Release
dotnet pack -c Release -o ../nugets

cd ..