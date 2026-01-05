# Copyright (c) 2025 AccelByte Inc. All Rights Reserved.
# ----------------------------------------
# Stage 1: gRPC Server Builder
# ----------------------------------------
# This is licensed software from AccelByte Inc, for limitations
# and restrictions contact your company contract manager.

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.22 AS builder
ARG TARGETARCH

RUN apk update && apk add --no-cache gcompat

# Set working directory.
WORKDIR /build
COPY src/AccelByte.PluginArch.Revocation.Demo.Server/*.csproj .
RUN ([ "$TARGETARCH" = "amd64" ] && echo "linux-musl-x64" || echo "linux-musl-$TARGETARCH") > /tmp/dotnet-rid
RUN dotnet restore -r $(cat /tmp/dotnet-rid)

# Copy application code.
COPY src/AccelByte.PluginArch.Revocation.Demo.Server .

# Build and publish application.
RUN dotnet publish -c Release -r $(cat /tmp/dotnet-rid) --no-restore -o /output


FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.22

# Set working directory.
WORKDIR /app

# Copy server build from stage 1.
COPY --from=builder /output/ .
# Plugin Arch gRPC Server Port., Prometheus /metrics port
EXPOSE 6565

EXPOSE 8080
ENTRYPOINT ["/app/AccelByte.PluginArch.Revocation.Demo.Server"]