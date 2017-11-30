FROM microsoft/aspnetcore:latest
ENTRYPOINT ["dotnet", "Web.dll"]
ARG source=.
WORKDIR /app
COPY $source .
