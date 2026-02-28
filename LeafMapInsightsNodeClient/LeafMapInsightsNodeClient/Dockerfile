# Build from repo root: docker build -f LeafMapInsightsNodeClient/Dockerfile .
FROM node:20-alpine
WORKDIR /app
COPY LeafMapInsightsNodeClient/package.json LeafMapInsightsNodeClient/package-lock.json* ./
RUN npm ci --omit=dev 2>/dev/null || npm install --omit=dev
COPY LeafMapInsightsNodeClient/server.js ./
COPY LeafMapInsightsNodeClient/public ./public/
ENV NODE_ENV=production
EXPOSE 3000
CMD ["node", "server.js"]
