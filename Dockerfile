FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS builder
WORKDIR /app

# Copy everything
COPY . ./

# Restore as distinct layers
RUN dotnet restore ./ReportsWorker

# Build and publish a release
RUN dotnet publish ./ReportsWorker -c Release -o worker

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app
COPY --from=builder /app/worker .
ENTRYPOINT ["dotnet", "ReportsWorker.dll"]