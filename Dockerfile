FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
COPY SpacerTransformationsAPI/SpacerTransformationsAPI/*.csproj ./
COPY SpacerTransformationsAPI/SpacerTransformationsAPI/. ./
RUN dotnet publish -c Production -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env /app/Prose /app/Prose/
ENV LD_LIBRARY_PATH /home/ash/z3/build
ENV LD_DEBUG all
ENTRYPOINT ["dotnet", "SpacerTransformationsAPI.dll"]
