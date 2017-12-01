FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "Web.dll"]
ARG source=.
WORKDIR /app
COPY $source .
