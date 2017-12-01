FROM microsoft/aspnetcore:2.0
RUN pwd
RUN ls -la
WORKDIR /app
RUN pwd
RUN ls -la
COPY . .
RUN pwd
RUN ls -la
ENTRYPOINT ["dotnet", "Web.dll"]
