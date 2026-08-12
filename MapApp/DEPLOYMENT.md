# MapApp - Deployment & Setup Guide

## Quick Start - Development

### Local Development (No Docker)

#### 1. Backend Setup
```bash
cd MapApp/Backend/MapApp.API
dotnet restore
dotnet run
```
API available at: http://localhost:5000
Swagger UI at: http://localhost:5000/swagger

#### 2. Frontend Setup
```bash
cd MapApp/Frontend
npm install
npm start
```
App opens at: http://localhost:3000

---

## Docker Deployment

### Prerequisites
- Docker Desktop (https://www.docker.com/products/docker-desktop)
- Docker Compose (included with Docker Desktop)

### Deploy Entire Stack

```bash
cd MapApp
docker-compose up -d
```

Access the application:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger Docs: http://localhost:5000/swagger
- PostgreSQL: localhost:5432

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f mapapp-api
docker-compose logs -f mapapp-frontend
```

### Stop Services
```bash
docker-compose down
```

### Rebuild Services
```bash
docker-compose build --no-cache
docker-compose up -d
```

---

## Production Deployment

### Azure App Service

#### 1. Create Container Registry
```bash
az acr create --resource-group myResourceGroup \
  --name mapappregistry --sku Basic
```

#### 2. Build and Push Images
```bash
# Backend
docker build -t mapappregistry.azurecr.io/mapapp-api:latest ./Backend/MapApp.API
docker push mapappregistry.azurecr.io/mapapp-api:latest

# Frontend
docker build -t mapappregistry.azurecr.io/mapapp-frontend:latest ./Frontend
docker push mapappregistry.azurecr.io/mapapp-frontend:latest
```

#### 3. Deploy to App Service
```bash
# Create resource group
az group create --name mapappGroup --location eastus

# Create App Service Plan
az appservice plan create --name mapappPlan \
  --resource-group mapappGroup --sku B2

# Deploy backend
az container create --resource-group mapappGroup \
  --name mapapp-api \
  --image mapappregistry.azurecr.io/mapapp-api:latest \
  --ports 5000 5001 \
  --registry-login-server mapappregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password>

# Deploy frontend
az container create --resource-group mapappGroup \
  --name mapapp-frontend \
  --image mapappregistry.azurecr.io/mapapp-frontend:latest \
  --ports 3000 \
  --registry-login-server mapappregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password>
```

### AWS Deployment (ECS + CloudFront)

#### 1. Create ECR Repository
```bash
aws ecr create-repository --repository-name mapapp-api
aws ecr create-repository --repository-name mapapp-frontend
```

#### 2. Push Images
```bash
# Authenticate
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Backend
docker build -t <account-id>.dkr.ecr.us-east-1.amazonaws.com/mapapp-api:latest ./Backend/MapApp.API
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/mapapp-api:latest

# Frontend
docker build -t <account-id>.dkr.ecr.us-east-1.amazonaws.com/mapapp-frontend:latest ./Frontend
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/mapapp-frontend:latest
```

#### 3. Deploy with ECS
```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name mapapp-cluster

# Create task definitions (see tasks.json files in deploy folder)
aws ecs register-task-definition --cli-input-json file://api-task-definition.json
aws ecs register-task-definition --cli-input-json file://frontend-task-definition.json

# Create services
aws ecs create-service --cluster mapapp-cluster \
  --service-name mapapp-api \
  --task-definition mapapp-api:1 \
  --desired-count 2 \
  --launch-type FARGATE
```

### Heroku Deployment

#### 1. Install Heroku CLI
```bash
npm install -g heroku
heroku login
```

#### 2. Create Apps
```bash
heroku create mapapp-api
heroku create mapapp-frontend
```

#### 3. Configure Environment
```bash
# Backend
heroku config:set -a mapapp-api DATABASE_URL=<connection-string>
heroku config:set -a mapapp-api ASPNETCORE_ENVIRONMENT=Production

# Frontend
heroku config:set -a mapapp-frontend REACT_APP_API_URL=https://mapapp-api.herokuapp.com/api
```

#### 4. Deploy
```bash
# Backend
git subtree push --prefix Backend/MapApp.API heroku-api main

# Frontend
git subtree push --prefix Frontend heroku-frontend main
```

---

## Database Setup

### Using PostgreSQL

#### 1. Restore NuGet for EF Tools
```bash
dotnet tool install --global dotnet-ef
```

#### 2. Update Connection String
In `Program.cs`, replace:
```csharp
// Change from:
options.UseInMemoryDatabase("MapAppDb")

// To:
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
options.UseNpgsql(connectionString);
```

#### 3. Create Migrations
```bash
cd MapApp/Backend/MapApp.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 4. Connection String in appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Host=localhost;Port=5432;Database=mapappdb;Username=mapapp_user;Password=mapapp_password"
  }
}
```

---

## Performance Monitoring

### Application Insights (Azure)
```bash
# Add package
dotnet add package Microsoft.ApplicationInsights.AspNetCore

# Configure in Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Prometheus & Grafana
```yaml
# Add to docker-compose.yml
prometheus:
  image: prom/prometheus
  ports:
	- "9090:9090"
  volumes:
	- ./prometheus.yml:/etc/prometheus/prometheus.yml

grafana:
  image: grafana/grafana
  ports:
	- "3001:3000"
  depends_on:
	- prometheus
```

---

## Scaling & Load Balancing

### Kubernetes Deployment

#### 1. Create Deployment Files
```yaml
# api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mapapp-api
spec:
  replicas: 3
  selector:
	matchLabels:
	  app: mapapp-api
  template:
	metadata:
	  labels:
		app: mapapp-api
	spec:
	  containers:
	  - name: mapapp-api
		image: mapappregistry.azurecr.io/mapapp-api:latest
		ports:
		- containerPort: 5000
		resources:
		  requests:
			memory: "256Mi"
			cpu: "250m"
		  limits:
			memory: "512Mi"
			cpu: "500m"
```

#### 2. Deploy to Kubernetes
```bash
kubectl apply -f api-deployment.yaml
kubectl apply -f frontend-deployment.yaml
kubectl apply -f service.yaml
```

#### 3. Check Status
```bash
kubectl get pods
kubectl get services
kubectl describe pod <pod-name>
```

---

## Health Checks & Monitoring

### Health Check Endpoint
```bash
curl http://localhost:5000/health
```

### Logging Configuration
Edit `Program.cs` for custom Serilog settings:
```csharp
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console()
	.WriteTo.File("logs/mapapp-.txt", 
		rollingInterval: RollingInterval.Day,
		retainedFileCountLimit: 7)
	.WriteTo.Seq("http://localhost:5341") // Seq logging server
	.CreateLogger();
```

---

## Troubleshooting

### API Port Already in Use
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5000
kill -9 <PID>
```

### Docker Container Not Starting
```bash
# Check logs
docker logs <container-name>

# Rebuild without cache
docker-compose build --no-cache
```

### Frontend Can't Connect to API
1. Check backend is running: `curl http://localhost:5000/health`
2. Verify `REACT_APP_API_URL` environment variable
3. Check CORS configuration in backend

### Database Connection Issues
```bash
# Test connection
psql -h localhost -U mapapp_user -d mapappdb

# Check Docker volume
docker volume inspect mapapp_postgres_data
```

---

## CI/CD Pipeline

### GitHub Actions Example
```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
	branches: [main]

jobs:
  build-and-deploy:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v2

	  - name: Build Docker Images
		run: |
		  docker build -t ${{ secrets.REGISTRY_LOGIN_SERVER }}/mapapp-api:latest ./Backend/MapApp.API
		  docker build -t ${{ secrets.REGISTRY_LOGIN_SERVER }}/mapapp-frontend:latest ./Frontend

	  - name: Push to ACR
		run: |
		  docker login -u ${{ secrets.REGISTRY_USERNAME }} -p ${{ secrets.REGISTRY_PASSWORD }} ${{ secrets.REGISTRY_LOGIN_SERVER }}
		  docker push ${{ secrets.REGISTRY_LOGIN_SERVER }}/mapapp-api:latest
		  docker push ${{ secrets.REGISTRY_LOGIN_SERVER }}/mapapp-frontend:latest

	  - name: Deploy to Azure
		uses: azure/container-instances-deploy-action@v1
		with:
		  resource-group: ${{ secrets.RESOURCE_GROUP }}
		  command: create
```

---

## Backup & Recovery

### Database Backup
```bash
# PostgreSQL
pg_dump -h localhost -U mapapp_user mapappdb > backup.sql

# Restore
psql -h localhost -U mapapp_user mapappdb < backup.sql
```

### Docker Volume Backup
```bash
docker run --rm -v mapapp_postgres_data:/data \
  -v $(pwd):/backup ubuntu tar czf /backup/postgres_backup.tar.gz /data
```

---

## Security Checklist

- [ ] Enable HTTPS in production
- [ ] Add authentication/authorization
- [ ] Implement rate limiting
- [ ] Set environment variables for secrets
- [ ] Enable CORS restrictions
- [ ] Add request validations
- [ ] Regular security audits
- [ ] Monitor for unauthorized access
- [ ] Keep dependencies updated
- [ ] Use secrets management (Azure Key Vault, AWS Secrets Manager)

---

## Support & Documentation

- **Backend API Docs**: http://localhost:5000/swagger
- **GitHub Issues**: Report bugs and feature requests
- **Docker Docs**: https://docs.docker.com
- **ASP.NET Core**: https://docs.microsoft.com/en-us/aspnet/core
- **React Docs**: https://react.dev
