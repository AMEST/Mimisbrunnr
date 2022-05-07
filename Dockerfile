FROM alpine/git as version
WORKDIR /src
COPY . /src
RUN echo $(git describe --tags --always 2>/dev/null |  sed 's/-g[a-z0-9]\{7\}//') > /version ;\
    echo "Version: "$(cat /version)

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /build
COPY . ./
RUN apt-get update -yq ;\
	apt-get install curl gnupg -yq ;\
	curl -sL https://deb.nodesource.com/setup_14.x | bash - ;\
	apt-get install -y nodejs

RUN sed -i -e "s/<Version>0-develop<\/Version>/<Version>$(cat version | cut -c2- )<\/Version>/g" src/Mimisbrunnr.Web.Host/Mimisbrunnr.Web.Host.csproj;\
	dotnet build -c Release --nologo;\
    dotnet publish src/Mimisbrunnr.Web.Host/Mimisbrunnr.Web.Host.csproj -c Release --no-build -o /dist/app

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS app
WORKDIR /app
COPY --from=build-env /dist/app/. .
CMD ["dotnet", "Mimisbrunnr.Web.Host.dll"]
