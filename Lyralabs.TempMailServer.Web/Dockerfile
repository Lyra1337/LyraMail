#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Lyralabs.TempMailServer.Web/Lyralabs.TempMailServer.Web.csproj", "Lyralabs.TempMailServer.Web/"]
RUN dotnet restore "Lyralabs.TempMailServer.Web/Lyralabs.TempMailServer.Web.csproj"
COPY . .
WORKDIR "/src/Lyralabs.TempMailServer.Web"
RUN dotnet build "Lyralabs.TempMailServer.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lyralabs.TempMailServer.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lyralabs.TempMailServer.Web.dll"]