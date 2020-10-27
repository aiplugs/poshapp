#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build
RUN curl -sL https://deb.nodesource.com/setup_14.x |  bash -
RUN apt-get install -y nodejs
WORKDIR /src/Aiplugs.PoshApp.Electron
RUN npm install
RUN npm run build
WORKDIR /src
COPY ["Aiplugs.PoshApp.Web/Aiplugs.PoshApp.Web.csproj", "Aiplugs.PoshApp.Web/"]
COPY ["Aiplugs.PoshApp.Deamon/Aiplugs.PoshApp.Deamon.csproj", "Aiplugs.PoshApp.Deamon/"]
RUN dotnet restore "Aiplugs.PoshApp.Web/Aiplugs.PoshApp.Web.csproj"
COPY . .
WORKDIR "/src/Aiplugs.PoshApp.Web"
RUN dotnet build "Aiplugs.PoshApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aiplugs.PoshApp.Web.csproj" -c Release -r ubuntu.18.04-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aiplugs.PoshApp.Web.dll"]