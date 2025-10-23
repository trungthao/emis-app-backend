# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Services/StudentService/Student.API/Student.API.csproj", "src/Services/StudentService/Student.API/"]
COPY ["src/Services/StudentService/Student.Application/Student.Application.csproj", "src/Services/StudentService/Student.Application/"]
COPY ["src/Services/StudentService/Student.Infrastructure/Student.Infrastructure.csproj", "src/Services/StudentService/Student.Infrastructure/"]
COPY ["src/Services/StudentService/Student.Domain/Student.Domain.csproj", "src/Services/StudentService/Student.Domain/"]

RUN dotnet restore "src/Services/StudentService/Student.API/Student.API.csproj"

# Copy all files and build
COPY . .
WORKDIR "/src/src/Services/StudentService/Student.API"
RUN dotnet build "Student.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Student.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Student.API.dll"]
