# 1. Faza: Build aplikacije
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TicketSystem.UI.csproj", "./"]
RUN dotnet restore "TicketSystem.UI.csproj"
COPY . .

# Publikovanje u Release konfiguraciji
RUN dotnet publish "TicketSystem.UI.csproj" -c Release -o /app/publish

# 2. Faza: Runtime (Nginx)
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .

# Automatsko pronalaženje i kreiranje dotnet.js fajla
RUN cd /usr/share/nginx/html/_framework && \
    cp $(ls -1 | grep -E '^dotnet\.[a-zA-Z0-9]+\.js$') dotnet.js

# Brisanje podrazumevanih konfiguracija kako ne bi došlo do konflikta
RUN rm /etc/nginx/conf.d/default.conf

# Kopiranje naše konfiguracije
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# Pokretanje Nginx-a
CMD ["nginx", "-g", "daemon off;"]