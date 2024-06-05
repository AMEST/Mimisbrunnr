FROM alpine/git as version
WORKDIR /src
COPY . /src
RUN echo $(git describe --tags --always 2>/dev/null |  sed 's/-g[a-z0-9]\{7\}//') > /version ;\
    echo "Version: "$(cat /version)

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /build
COPY . ./
COPY --from=version /version /build/version
RUN apt-get update -yq ;\
	apt-get install curl gnupg ca-certificates -yq ;\
    mkdir -p /etc/apt/keyrings ;\
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg ;\
	echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_18.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list ;\
    apt-get update ;\
	apt-get install -y nodejs

RUN sed -i -e "s/<Version>0-develop<\/Version>/<Version>$(cat version | cut -c2- )<\/Version>/g" src/Mimisbrunnr.Web.Host/Mimisbrunnr.Web.Host.csproj;\
	dotnet build -c Release --nologo;\
    dotnet publish src/Mimisbrunnr.Web.Host/Mimisbrunnr.Web.Host.csproj -c Release --no-build -o /dist/app

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS app
WORKDIR /app
COPY --from=build-env /dist/app/. .
CMD ["dotnet", "Mimisbrunnr.Web.Host.dll"]
