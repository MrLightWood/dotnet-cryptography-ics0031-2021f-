FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HW2/HW2.csproj", "HW2/"]
RUN dotnet restore "HW2/HW2.csproj"
COPY . .
WORKDIR "/src/HW2"
RUN dotnet build "HW2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HW2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HW2.dll"]
