# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore the dependencies (via nuget)
COPY TaskExecutor/*.csproj ./TaskExecutor/
RUN dotnet restore TaskExecutor/TaskExecutor.csproj

# Copy the rest of the application files and build the app
COPY . ./
RUN dotnet publish TaskExecutor/TaskExecutor.csproj -c Release -o out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /app/out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "TaskExecutor.dll"]
