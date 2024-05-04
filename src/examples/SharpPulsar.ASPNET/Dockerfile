#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
COPY ["src/examples/SharpPulsar.ASPNET/SharpPulsar.ASPNET.csproj", "src/examples/SharpPulsar.ASPNET/"]
RUN dotnet restore "./src/examples/SharpPulsar.ASPNET/./SharpPulsar.ASPNET.csproj"
COPY . .
WORKDIR "/src/src/examples/SharpPulsar.ASPNET"
RUN dotnet build "./SharpPulsar.ASPNET.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SharpPulsar.ASPNET.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharpPulsar.ASPNET.dll"]