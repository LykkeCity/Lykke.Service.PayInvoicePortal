FROM microsoft/dotnet:latest
ENTRYPOINT ["dotnet", "Lykke.Pay.Invoice.dll"]
ARG source=.
WORKDIR /app
COPY $source .
