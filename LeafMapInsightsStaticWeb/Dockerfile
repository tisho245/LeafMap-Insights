# Build from repo root: docker build -f LeafMapInsightsStaticWeb/Dockerfile .
FROM nginx:alpine
COPY LeafMapInsightsStaticWeb /usr/share/nginx/html
RUN chmod -R 755 /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
