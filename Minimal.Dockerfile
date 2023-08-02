# minimal build adopted from https://benfoster.io/blog/optimising-dotnet-docker-images/

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 25

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
ENV TZ="Europe/Berlin"
WORKDIR /src
COPY . .
WORKDIR "/src/Web"
RUN dotnet publish "Lyralabs.TempMailServer.Web.csproj" -c Release --runtime linux-musl-x64 -p:PublishTrimmed=true -o /app/publish

FROM base AS final
ENV TZ="Europe/Berlin"
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./Lyralabs.TempMailServer.Web"]
