#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LuxotticaSorting/LuxotticaSorting.csproj", "LuxotticaSorting/"]
COPY ["LuxotticaSorting.ApplicationServices.Shared/LuxotticaSorting.ApplicationServices.Shared.csproj", "LuxotticaSorting.ApplicationServices.Shared/"]
COPY ["LuxotticaSorting.ApplicationServices/LuxotticaSorting.ApplicationServices.csproj", "LuxotticaSorting.ApplicationServices/"]
COPY ["LuxotticaSorting.Core/LuxotticaSorting.Core.csproj", "LuxotticaSorting.Core/"]
COPY ["LuxotticaSorting.DataAccess/LuxotticaSorting.DataAccess.csproj", "LuxotticaSorting.DataAccess/"]
RUN dotnet restore "LuxotticaSorting/LuxotticaSorting.csproj"
COPY . .
WORKDIR "/src/LuxotticaSorting"
RUN dotnet build "LuxotticaSorting.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LuxotticaSorting.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LuxotticaSorting.dll"]
