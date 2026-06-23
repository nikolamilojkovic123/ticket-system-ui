FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TicketSystem.UI/TicketSystem.UI.csproj", "TicketSystem.UI/"]
RUN dotnet restore "TicketSystem.UI/TicketSystem.UI.csproj"
COPY . .
WORKDIR "/src/TicketSystem.UI"
RUN dotnet publish "TicketSystem.UI.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
RUN FINGERPRINTED=$(ls _framework/blazor.webassembly.*.js 2>/dev/null | grep -v '\.br$' | grep -v '\.gz$' | head -1 | xargs basename 2>/dev/null) && \
    if [ -n "$FINGERPRINTED" ] && [ "$FINGERPRINTED" != "blazor.webassembly.js" ]; then \
        sed -i "s|_framework/blazor.webassembly.js|_framework/$FINGERPRINTED|g" index.html; \
    fi
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
