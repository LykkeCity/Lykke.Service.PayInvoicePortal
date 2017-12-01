FROM microsoft/aspnetcore:2.0
RUN pwd
RUN ls -la
WORKDIR /app
ADD ./app /app
RUN pwd
RUN ls -la
COPY . .
RUN ls -la
ENTRYPOINT ["dotnet", "Web.dll"]
