FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /src
COPY ["PrimeApps.Auth/PrimeApps.Auth.csproj", "PrimeApps.Auth/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]

RUN dotnet restore "PrimeApps.Auth/PrimeApps.Auth.csproj"
COPY . .
WORKDIR "/src/PrimeApps.Auth"
RUN dotnet build "PrimeApps.Auth.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Auth.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PrimeApps.Auth.dll"]