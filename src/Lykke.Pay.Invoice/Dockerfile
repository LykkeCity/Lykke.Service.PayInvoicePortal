FROM microsoft/aspnetcore:latest
ARG source=.
WORKDIR /app
COPY $source .
ENTRYPOINT ["dotnet", "Web.dll"]
