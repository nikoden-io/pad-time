# =============================================================================
# PadTime — Dockerfile Angular (nginx)
# Multi-stage : Stage 1 build Angular, Stage 2 sert avec nginx
# =============================================================================

# -----------------------------------------------------------------------------
# STAGE 1 — Build
# On utilise Node pour installer les dépendances et builder Angular
# -----------------------------------------------------------------------------

FROM node:22-alpine AS build

WORKDIR /app

# Copie package.json en premier — Docker cache cette couche
# Si package.json n'a pas changé, npm ci est sauté au prochain build
COPY package.json package-lock.json ./
RUN npm ci --prefer-offline

# Copie le reste du code source
COPY . .

# ng build = production par défaut (voir angular.json defaultConfiguration)
# Output : dist/webfront/browser/
RUN npm run build

# -----------------------------------------------------------------------------
# STAGE 2 — Serve
# Image nginx légère — on ne garde PAS Node (inutile en prod, alourdit l'image)
# Résultat : image finale ~25MB au lieu de ~400MB avec Node
# -----------------------------------------------------------------------------

FROM nginx:alpine AS final

# Supprime la config nginx par défaut
RUN rm /etc/nginx/conf.d/default.conf

# Notre config nginx custom (voir ci-dessous)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copie les fichiers buildés depuis le stage 1
# dist/webfront/browser/ = output Angular 17+ avec le nouveau builder @angular/build
COPY --from=build /app/dist/webfront/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
