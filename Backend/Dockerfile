# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et
COPY CassMach.API/CassMach.API.csproj CassMach.API/
COPY CassMach.Application/CassMach.Application.csproj CassMach.Application/
COPY CassMach.Domain/CassMach.Domain.csproj CassMach.Domain/
COPY CassMach.Infrastructure/CassMach.Infrastructure.csproj CassMach.Infrastructure/

RUN dotnet restore CassMach.API/CassMach.API.csproj

# Tüm kaynak kodunu kopyala ve build et
COPY . .
RUN dotnet publish CassMach.API/CassMach.API.csproj -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CassMach.API.dll"]