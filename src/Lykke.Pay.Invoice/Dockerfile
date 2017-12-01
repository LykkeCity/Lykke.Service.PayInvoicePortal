FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "Lykke.Pay.Invoice.dll"]
ARG source=.
WORKDIR /app
COPY $source .
