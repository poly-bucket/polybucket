# Development stage
FROM node:20-alpine as development
WORKDIR /app
COPY package*.json ./
RUN npm install --legacy-peer-deps
COPY . .
EXPOSE 10101
CMD ["npm", "run", "dev"]
