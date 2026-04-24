#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Luxottica/Luxottica.csproj", "Luxottica/"]
COPY ["Luxottica.ApplicationServices.Shared/Luxottica.ApplicationServices.Shared.csproj", "Luxottica.ApplicationServices.Shared/"]
COPY ["Luxottica.ApplicationServices/Luxottica.ApplicationServices.csproj", "Luxottica.ApplicationServices/"]
COPY ["Luxottica.Core/Luxottica.Core.csproj", "Luxottica.Core/"]
COPY ["Luxottica.DataAccess/Luxottica.DataAccess.csproj", "Luxottica.DataAccess/"]
RUN dotnet restore "Luxottica/Luxottica.csproj"
COPY . .
WORKDIR "/src/Luxottica"
RUN dotnet build "Luxottica.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Luxottica.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Luxottica.dll"]