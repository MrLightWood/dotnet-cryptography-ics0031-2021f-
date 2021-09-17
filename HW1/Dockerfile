#docker build -t homework1 -f Dockerfile .
#docker run -it homework1
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HW1.csproj", "HW1/"]
RUN dotnet restore "HW1/HW1.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "HW1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HW1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HW1.dll"]
