FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HrBot/HrBot.csproj", "HrBot/"]
RUN dotnet restore "HrBot/HrBot.csproj"
COPY . .
WORKDIR "/src/HrBot"
RUN dotnet build "HrBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HrBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HrBot.dll"]