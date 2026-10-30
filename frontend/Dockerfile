# Dev-mode Dockerfile: runs the Vite dev server with HMR, keeping the
# course environment editable rather than rebuilding a static bundle after
# every change.

FROM node:20-alpine

WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install

COPY . .

EXPOSE 5173
CMD ["npm", "run", "dev"]
