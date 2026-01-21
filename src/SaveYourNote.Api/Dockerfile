# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY ["src/SaveYourNote.Api/SaveYourNote.Api.csproj", "src/SaveYourNote.Api/"]
COPY ["src/SaveYourNote.Application/SaveYourNote.Application.csproj", "src/SaveYourNote.Application/"]
COPY ["src/SaveYourNote.Domain/SaveYourNote.Domain.csproj", "src/SaveYourNote.Domain/"]
COPY ["src/SaveYourNote.Infrastructure/SaveYourNote.Infrastructure.csproj", "src/SaveYourNote.Infrastructure/"]
RUN dotnet restore "src/SaveYourNote.Api/SaveYourNote.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR "/app/src/SaveYourNote.Api"
RUN dotnet publish "SaveYourNote.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage: environment image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port and start
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "SaveYourNote.Api.dll"]
