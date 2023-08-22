FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0.302 as builder
# FROM mcr.microsoft.com/dotnet/sdk:6.0.302 as builder
ARG PROJECT_PATH=src/AccelByte.PluginArch.Revocation.Demo.Server
WORKDIR /build
COPY $PROJECT_PATH/*.csproj ./
RUN dotnet restore
COPY $PROJECT_PATH ./
RUN dotnet publish -c Release -o output

FROM mcr.microsoft.com/dotnet/sdk:6.0.302
WORKDIR /app
COPY --from=builder /build/output/* ./
RUN chmod +x /app/AccelByte.PluginArch.Revocation.Demo.Server
# Plugin arch gRPC server port
EXPOSE 6565
# Prometheus /metrics web server port
EXPOSE 8080
ENTRYPOINT ["/app/AccelByte.PluginArch.Revocation.Demo.Server"]
