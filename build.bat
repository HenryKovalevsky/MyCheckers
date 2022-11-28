dotnet publish server\MyCheckers.Api.sln -c Release -r linux-x64 --no-self-contained --output dist 
npm run --prefix client build -- --outDir "../dist/wwwroot" --emptyOutDir