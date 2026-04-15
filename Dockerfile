# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj first (IMPORTANT for restore caching)
COPY *.sln ./
COPY OttooGennie.API/OttooGennie.API.csproj ./OttooGennie.API/

RUN dotnet restore OttooGennie.API/OttooGennie.API.csproj

# Copy everything else
COPY . .

RUN dotnet publish OttooGennie.API/OttooGennie.API.csproj -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "OttooGennie.API.dll"]
