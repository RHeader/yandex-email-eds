# ==== CONFIGURE =====
# Use a Node 16 base image
FROM node:16-alpine as build-stage
# Set the working directory to /app inside the container
WORKDIR /app
# Copy app files
COPY . .
# ==== BUILD =====
# Install dependencies (npm ci makes sure the exact versions in the lockfile gets installed)
RUN npm ci 
# Build the app
RUN npm run build
# ==== RUN =======
# Set the env to "production"
FROM nginx

COPY --from=build-stage /app/dist /usr/share/nginx/html
RUN cd /usr/share/nginx/html && ls
