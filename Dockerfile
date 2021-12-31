FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["testfunctionprh.csproj", "./"]
RUN dotnet restore "testfunctionprh.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "testfunctionprh.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "testfunctionprh.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "testfunctionprh.dll"]
