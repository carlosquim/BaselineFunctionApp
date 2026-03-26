FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BaselineFunctionApp.csproj", "."]
RUN dotnet restore "BaselineFunctionApp.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "BaselineFunctionApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BaselineFunctionApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaselineFunctionApp.dll"]