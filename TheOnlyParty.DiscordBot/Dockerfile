#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TheOnlyParty.DiscordBot/TheOnlyParty.DiscordBot.csproj", "TheOnlyParty.DiscordBot/"]
RUN dotnet restore "TheOnlyParty.DiscordBot/TheOnlyParty.DiscordBot.csproj"
COPY . .
WORKDIR "/src/TheOnlyParty.DiscordBot"
RUN dotnet build "TheOnlyParty.DiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
COPY /app/Data /data
RUN dotnet publish "TheOnlyParty.DiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TheOnlyParty.DiscordBot.dll"]