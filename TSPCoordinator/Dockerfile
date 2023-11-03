FROM mcr.microsoft.com/dotnet/aspnet:7.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 5000
# EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5000
# ENV ASPNETCORE_URLS=http://+:5000;https://+:5001

# Install packages
RUN apt update
RUN apt install tzdata

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim AS build
WORKDIR /src
COPY ["TspCoordinator.csproj", "./"]
RUN dotnet restore "TspCoordinator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet tool install -g nbgv
RUN dotnet build "TspCoordinator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TspCoordinator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TspCoordinator.dll", "--server.urls", "http://+:5000"]
