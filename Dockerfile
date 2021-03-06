#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
RUN curl -sL https://deb.nodesource.com/setup_14.x |  bash -
RUN apt-get install -y nodejs unzip
WORKDIR /src
COPY ./src/ .
WORKDIR /lib
COPY ./lib/ .
WORKDIR /src/Aiplugs.PoshApp.Electron
RUN npm uninstall nodegit keytar
RUN npm install
RUN npm run build
WORKDIR /src/Aiplugs.PoshApp.Web
RUN dotnet build "Aiplugs.PoshApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aiplugs.PoshApp.Web.csproj" -c Release -r ubuntu.18.04-x64 -o /app/publish
RUN mkdir /app/publish/Modules/PSScriptAnalyzer & unzip -d /app/publish/Modules/PSScriptAnalyzer /src/PSScriptAnalyzer.zip

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aiplugs.PoshApp.Web.dll"]