FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY NuGetLite.Server.sln ./
COPY src/NuGetLite.Server/NuGetLite.Server.csproj src/NuGetLite.Server/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/src/NuGetLite.Server
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NuGetLite.Server.dll"]
