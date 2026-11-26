FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Portfolio.slnx ./
COPY src/Portfolio.Core/Portfolio.Core.csproj src/Portfolio.Core/
COPY src/Portfolio.Infrastructure/Portfolio.Infrastructure.csproj src/Portfolio.Infrastructure/
COPY src/Portfolio.Web/Portfolio.Web.csproj src/Portfolio.Web/
RUN dotnet restore src/Portfolio.Web/Portfolio.Web.csproj

COPY src/ src/
RUN dotnet publish src/Portfolio.Web/Portfolio.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .

CMD ["sh", "-c", "exec dotnet Portfolio.Web.dll --urls http://0.0.0.0:${PORT:-10000}"]
