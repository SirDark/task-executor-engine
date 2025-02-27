# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY API/*.csproj ./API/
WORKDIR /app/API
RUN dotnet restore

# Copy the rest of the files and build the application
COPY API/. ./
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_HTTP_PORTS=3500
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 3500

# Set the entry point
ENTRYPOINT ["dotnet", "API.dll"]
