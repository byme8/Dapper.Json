param (
    [string]$version = (Get-Date -Format "999.yyMM.ddHH.mmss")
) 

dotnet clean
dotnet pack -c Release ./src/Dapper.Json/Dapper.Json.csproj --verbosity normal /p:Version=$version -o ./nugets
dotnet pack -c Release ./src/Dapper.Json.System/Dapper.Json.System.csproj --verbosity normal /p:Version=$version -o ./nugets
dotnet pack -c Release ./src/Dapper.Json.Newtonsoft/Dapper.Json.Newtonsoft.csproj --verbosity normal /p:Version=$version -o ./nugets
