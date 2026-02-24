# Build from repo root: docker build -f LeafMapInsightsAuthAPI/LeafMapInsightsAuthAPI/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ASPLeadMapInsightsAPI ASPLeadMapInsightsAPI
COPY LeafMapInsightsAuthAPI LeafMapInsightsAuthAPI
WORKDIR /src/LeafMapInsightsAuthAPI/LeafMapInsightsAuthAPI
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
ENTRYPOINT ["dotnet", "LeafMapInsightsAuthAPI.dll"]
