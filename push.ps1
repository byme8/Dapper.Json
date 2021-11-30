if (-not ($args.Count -eq 1)) {
    echo "Please povide nuget api key"
    exit 0
}

$key = $args[0]
pwsh ./pack.ps1
cd ./nugets
dotnet nuget push *.nupkg --skip-duplicate -s https://nuget.org -k $key