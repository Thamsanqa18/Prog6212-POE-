# Build and publish the MVC frontend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY RaceDay.Mvc.csproj ./
RUN dotnet restore RaceDay.Mvc.csproj
COPY . .
RUN dotnet publish RaceDay.Mvc.csproj -c Release -o /app/publish /p:UseAppHost=false

# Small runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
