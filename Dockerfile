FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV TZ="Europe/Berlin"
WORKDIR /src
COPY . .
WORKDIR "/src/Lyralabs.TempMailServer.Web"
RUN dotnet publish "Lyralabs.TempMailServer.Web.csproj" -c Release -o /app/publish

FROM base AS final
ENV TZ="Europe/Berlin"
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lyralabs.TempMailServer.dll"]
