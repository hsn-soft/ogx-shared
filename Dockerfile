FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /packages
USER root

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-stage
WORKDIR /build-source

COPY ["./nuget.config", "./"]
COPY ["./common.props", "./"]
COPY ["./common.version.props", "./"]
COPY ["./Ogx.Shared.sln", "./"]

COPY ["./src/Ogx.Shared.Contracts/Ogx.Shared.Contracts.csproj", "./src/Ogx.Shared.Contracts/"]
COPY ["./src/Ogx.Shared.Helper/Ogx.Shared.Helper.csproj", "./src/Ogx.Shared.Helper/"]
COPY ["./src/Ogx.Shared.Localization/Ogx.Shared.Localization.csproj", "./src/Ogx.Shared.Localization/"]
COPY ["./src/Ogx.Shared.Hosting/Ogx.Shared.Hosting.csproj", "./src/Ogx.Shared.Hosting/"]
COPY ["./src/Ogx.Shared.Hosting.Gateways/Ogx.Shared.Hosting.Gateways.csproj", "./src/Ogx.Shared.Hosting.Gateways/"]
COPY ["./src/Ogx.Shared.Hosting.Microservices/Ogx.Shared.Hosting.Microservices.csproj", "./src/Ogx.Shared.Hosting.Microservices/"]

RUN dotnet restore "./Ogx.Shared.sln" --force --verbosity minimal --configfile nuget.config

COPY ["./src/Ogx.Shared.Contracts/.", "./src/Ogx.Shared.Contracts/"]
COPY ["./src/Ogx.Shared.Helper/.", "./src/Ogx.Shared.Helper/"]
COPY ["./src/Ogx.Shared.Localization/.", "./src/Ogx.Shared.Localization/"]
COPY ["./src/Ogx.Shared.Hosting/.", "./src/Ogx.Shared.Hosting/"]
COPY ["./src/Ogx.Shared.Hosting.Gateways/.", "./src/Ogx.Shared.Hosting.Gateways/"]
COPY ["./src/Ogx.Shared.Hosting.Microservices/.", "./src/Ogx.Shared.Hosting.Microservices/"]

RUN dotnet build "./Ogx.Shared.sln" --no-restore --no-incremental --verbosity minimal --configuration Release

RUN dotnet test "./Ogx.Shared.sln" --no-restore --no-build --verbosity minimal --configuration Release --filter "category!=integration"

RUN --mount=type=secret,id=VERSION_NUMBER \
    export VERSION_NUMBER=$(cat /run/secrets/VERSION_NUMBER) && \
    echo ${VERSION_NUMBER} > ./version_number

RUN --mount=type=secret,id=ACTION_NUMBER \
    export ACTION_NUMBER=$(cat /run/secrets/ACTION_NUMBER) && \
    echo ${ACTION_NUMBER} > ./action_number

RUN dotnet pack "./Ogx.Shared.sln" --no-restore --no-build --configuration Release --output ./packages -p:PackageVersion=$(cat ./version_number).$(cat ./action_number)

FROM base AS final
WORKDIR /packages
COPY --from=build-stage /build-source/packages .

RUN --mount=type=secret,id=NUGET_SOURCE \
    export NUGET_SOURCE=$(cat /run/secrets/NUGET_SOURCE) && \
    echo ${NUGET_SOURCE} > ./nuget_source

RUN --mount=type=secret,id=NUGET_SECRET \
    export NUGET_SECRET=$(cat /run/secrets/NUGET_SECRET) && \
    echo ${NUGET_SECRET} > ./nuget_secret

RUN dotnet nuget push *.nupkg --source $(cat ./nuget_source) --api-key $(cat ./nuget_secret) --skip-duplicate
