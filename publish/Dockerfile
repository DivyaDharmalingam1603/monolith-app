# Use the official Microsoft ASP.NET Core runtime image for Linux, version 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory inside the container
WORKDIR /app

# Copy the published output from your local 'publish' folder into the container
COPY . .

# Expose port 80 to allow traffic to the container
EXPOSE 80

# Set environment variable so the app listens on port 80 inside the container
ENV ASPNETCORE_URLS=http://+:80

# Define the command to run the application
ENTRYPOINT ["dotnet", "EnergyLegacyApp.Web.dll"]
