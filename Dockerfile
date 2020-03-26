ARG ASPNETCORE_RUNTIME=2.1
ARG ASPNETCORE_SDK=2.1

FROM mcr.microsoft.com/dotnet/core/sdk:${ASPNETCORE_SDK} as build

WORKDIR /app
COPY . /
RUN dotnet publish /ExerciseDaemon/ExerciseDaemon.csproj -o ./out -c Release
RUN ls -R

FROM mcr.microsoft.com/dotnet/core/aspnet:${ASPNETCORE_RUNTIME}
WORKDIR /app
COPY --from=build /ExerciseDaemon/out ./
RUN ls -R

ENTRYPOINT ["dotnet", "ExerciseDaemon.dll"]