FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el archivo csproj y restaurar las dependencias
COPY ["parcial.csproj", "./"]
RUN dotnet restore "parcial.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet build "parcial.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "parcial.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Exponer el puerto por defecto
EXPOSE 80

# Definir variables de entorno esperadas en Render
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:80

ENTRYPOINT ["dotnet", "parcial.dll"]
