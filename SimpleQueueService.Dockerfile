FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS builder
WORKDIR /app

# Copy everything
COPY . ./

# Restore as distinct layers
RUN dotnet restore ./SimpleQueueService

# Build and publish a release
RUN dotnet publish ./SimpleQueueService -c Release -o worker

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=builder /app/worker .
ENTRYPOINT ["dotnet", "SimpleQueueService.dll"]