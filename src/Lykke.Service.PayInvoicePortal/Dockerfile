FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY . .
RUN ls -la
ENTRYPOINT ["dotnet", "Lykke.Service.PayInvoicePortal.dll"]
