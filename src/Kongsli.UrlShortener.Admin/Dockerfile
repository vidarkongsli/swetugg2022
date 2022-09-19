FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 5032

ENV ASPNETCORE_URLS=http://+:5033

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["src/Kongsli.UrlShortener.Admin/Kongsli.UrlShortener.Admin.csproj", "src/Kongsli.UrlShortener.Admin/"]
RUN dotnet restore "src/Kongsli.UrlShortener.Admin/Kongsli.UrlShortener.Admin.csproj"
COPY . .
WORKDIR "/src/src/Kongsli.UrlShortener.Admin"
RUN dotnet build "Kongsli.UrlShortener.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kongsli.UrlShortener.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kongsli.UrlShortener.Admin.dll"]
