FROM microsoft/dotnet:2.1.0-aspnetcore-runtime
WORKDIR /app
COPY . .
RUN ls -la
ENTRYPOINT ["dotnet", "Lykke.Service.PayInvoicePortal.dll"]
