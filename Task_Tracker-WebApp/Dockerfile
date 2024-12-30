# Use .NET runtime image as the base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file to restore dependencies
COPY ["Task_Tracker-WebApp.csproj", "./"]

# Restore dependencies
RUN dotnet restore "Task_Tracker-WebApp.csproj"

# Copy the rest of the application files
COPY . .

# Set the working directory to the project folder and build the app
WORKDIR "/src"
RUN dotnet build "Task_Tracker-WebApp.csproj" -c Release -o /app/build --no-restore

# Publish the app to a folder
FROM build AS publish
RUN dotnet publish "Task_Tracker-WebApp.csproj" -c Release -o /app/publish

# Final stage: runtime image
FROM base AS final
WORKDIR /app

# Copy the published app from the publish stage to the container
COPY --from=publish /app/publish .

# Set the entry point for the app
ENTRYPOINT ["dotnet", "Task_Tracker-WebApp.dll"]
