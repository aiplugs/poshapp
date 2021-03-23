#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
RUN curl -sL https://deb.nodesource.com/setup_14.x |  bash -
RUN apt-get install -y nodejs
WORKDIR /src
COPY ./src/ .
WORKDIR /src/Aiplugs.PoshApp.Electron
RUN npm uninstall nodegit keytar
RUN npm install
RUN npm run build
WORKDIR /src/Aiplugs.PoshApp.Web
RUN dotnet build "Aiplugs.PoshApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aiplugs.PoshApp.Web.csproj" -c Release -r ubuntu.18.04-x64 -o /app/publish
RUN cp -r /app/publish/pses/bin/Common/runtimes/unix/lib/net5.0/Modules/* /app/publish/pses/bin/Common/Modules/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aiplugs.PoshApp.Web.dll"]