# 部署运维文档（结合现有项目）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 部署运维文档 |
| 目标项目 | Bilibili视频平台 |
| 部署方式 | Docker + Kubernetes |
| 创建时间 | 2026-05-12 |

---

## 2. 现有项目部署分析

### 2.1 现有Docker配置

基于对现有项目的探索，项目已经包含Docker支持：

```
现有Docker配置文件位置：
aspnet-core/services/{ServiceName}/Dockerfile
aspnet-core/docker-compose.yml
aspnet-core/docker-compose.override.yml
aspnet-core/.dockerignore
```

**结论**: 我们将继承现有Docker配置模式，为Bilibili新增服务创建相同结构。

---

## 3. Docker部署设计

### 3.1 docker-compose.yml配置

继承现有配置，扩展Bilibili服务：

```yaml
# docker-compose.yml（继承现有配置）
version: '3.8'

services:
  # ==================== 基础设施服务 ====================
  
  # PostgreSQL数据库（继承现有）
  postgres:
    image: postgres:15-alpine
    container_name: bilibili-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
      POSTGRES_DB: PlatformDB
    volumes:
      - postgres-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    networks:
      - bilibili-network
    restart: always
    
  # Redis缓存（继承现有）
  redis:
    image: redis:7-alpine
    container_name: bilibili-redis
    command: redis-server --requirepass password
    volumes:
      - redis-data:/data
    ports:
      - "6379:6379"
    networks:
      - bilibili-network
    restart: always
    
  # RabbitMQ消息队列（继承现有）
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: bilibili-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: password
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    ports:
      - "5672:5672"   # AMQP端口
      - "15672:15672" # Management UI
    networks:
      - bilibili-network
    restart: always
    
  # Elasticsearch搜索（继承现有）
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.6.0
    container_name: bilibili-elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    ports:
      - "9200:9200"
      - "9300:9300"
    networks:
      - bilibili-network
    restart: always
    
  # MinIO对象存储（继承现有）
  minio:
    image: minio/minio:latest
    container_name: bilibili-minio
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_USER_PASSWORD: minioadmin
    volumes:
      - minio-data:/data
    ports:
      - "9000:9000"  # API端口
      - "9001:9001"  # Console端口
    networks:
      - bilibili-network
    restart: always
    
  # ==================== 现有ABP服务 ====================
  
  # IdentityServer认证服务（继承现有）
  identity-server:
    build:
      context: ./aspnet-core/services/LCH.MicroService.IdentityServer.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-identity-server
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=IdentityDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
    depends_on:
      - postgres
      - redis
    ports:
      - "5000:80"
    networks:
      - bilibili-network
    restart: always
    
  # AuthServer授权服务（继承现有）
  auth-server:
    build:
      context: ./aspnet-core/services/LCH.MicroService.AuthServer.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-auth-server
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=AuthServerDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
    depends_on:
      - postgres
      - redis
      - identity-server
    ports:
      - "5001:80"
    networks:
      - bilibili-network
    restart: always
    
  # ==================== Bilibili新增服务 ====================
  
  # VideoService视频服务（新增）
  video-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Video.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-video-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=VideoDB;Username=postgres;Password=password
      - ConnectionStrings__Identity=Host=postgres;Database=IdentityDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=admin
      - RabbitMQ__Password=password
      - MinIO__EndPoint=minio:9000
      - MinIO__AccessKey=minioadmin
      - MinIO__SecretKey=minioadmin
      - MinIO__BucketName=bilibili-videos
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - minio
      - auth-server
    ports:
      - "5002:80"
    volumes:
      - video-temp:/tmp/transcode  # 转码临时目录
    networks:
      - bilibili-network
    restart: always
    
  # DanmakuService弹幕服务（新增）
  danmaku-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Danmaku.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-danmaku-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=DanmakuDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - auth-server
    ports:
      - "5003:80"
    networks:
      - bilibili-network
    restart: always
    
  # TranscodeWorker转码Worker（新增）
  transcode-worker:
    build:
      context: ./aspnet-core/services/transcode-worker
      dockerfile: Dockerfile
    container_name: bilibili-transcode-worker
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=admin
      - RabbitMQ__Password=password
      - MinIO__EndPoint=minio:9000
      - MinIO__AccessKey=minioadmin
      - MinIO__SecretKey=minioadmin
      - FFmpeg__Path=/usr/bin/ffmpeg
      - FFmpeg__TempDirectory=/tmp/transcode
      - TranscodeSettings__MaxConcurrency=3
    depends_on:
      - rabbitmq
      - minio
    volumes:
      - video-temp:/tmp/transcode
      - ./ffmpeg:/usr/bin/ffmpeg  # FFmpeg二进制文件
    networks:
      - bilibili-network
    restart: always
    
  # InteractionService互动服务（新增）
  interaction-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Interaction.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-interaction-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=InteractionDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - auth-server
    ports:
      - "5004:80"
    networks:
      - bilibili-network
    restart: always
    
  # UserService用户服务（新增）
  user-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.BilibiliUser.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-user-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=UserDB;Username=postgres;Password=password
      - ConnectionStrings__Identity=Host=postgres;Database=IdentityDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - auth-server
    ports:
      - "5005:80"
    networks:
      - bilibili-network
    restart: always
    
  # SearchService搜索服务（新增）
  search-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Search.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-search-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=SearchDB;Username=postgres;Password=password
      - Elasticsearch__Url=http://elasticsearch:9200
      - RabbitMQ__HostName=rabbitmq
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - elasticsearch
      - rabbitmq
      - auth-server
    ports:
      - "5006:80"
    networks:
      - bilibili-network
    restart: always
    
  # RecommendService推荐服务（新增）
  recommend-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Recommend.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-recommend-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=RecommendDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - auth-server
    ports:
      - "5007:80"
    networks:
      - bilibili-network
    restart: always
    
  # LiveService直播服务（新增）
  live-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Live.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-live-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=LiveDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - MinIO__EndPoint=minio:9000
      - MinIO__AccessKey=minioadmin
      - MinIO__SecretKey=minioadmin
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - minio
      - auth-server
    ports:
      - "5008:80"
    networks:
      - bilibili-network
    restart: always
    
  # CategoryService分区服务（新增）
  category-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.Category.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-category-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=CategoryDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - auth-server
    ports:
      - "5009:80"
    networks:
      - bilibili-network
    restart: always
    
  # AdminService管理后台（新增）
  admin-service:
    build:
      context: ./aspnet-core/services/LCH.MicroService.BilibiliAdmin.HttpApi.Host
      dockerfile: Dockerfile
    container_name: bilibili-admin-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Host=postgres;Database=AdminDB;Username=postgres;Password=password
      - Redis__Configuration=redis:6379,password=password
      - RabbitMQ__HostName=rabbitmq
      - AuthServer__Authority=http://auth-server
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - auth-server
    ports:
      - "5010:80"
    networks:
      - bilibili-network
    restart: always
    
  # API Gateway（继承现有）
  gateway:
    build:
      context: ./aspnet-core/services/LCH.MicroService.WebGateway
      dockerfile: Dockerfile
    container_name: bilibili-gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - AuthServer__Authority=http://auth-server
    depends_on:
      - auth-server
      - video-service
      - danmaku-service
      - interaction-service
      - user-service
      - search-service
      - recommend-service
      - live-service
      - category-service
      - admin-service
    ports:
      - "80:80"
    networks:
      - bilibili-network
    restart: always

# 数据卷定义
volumes:
  postgres-data:
  redis-data:
  rabbitmq-data:
  elasticsearch-data:
  minio-data:
  video-temp:  # 视频转码临时目录

# 网络定义
networks:
  bilibili-network:
    driver: bridge
```

---

### 3.2 Dockerfile配置示例

#### 3.2.1 VideoService Dockerfile

```dockerfile
# VideoService Dockerfile（继承ABP标准）
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["aspnet-core/services/LCH.MicroService.Video.HttpApi.Host/LCH.MicroService.Video.HttpApi.Host.csproj", "LCH.MicroService.Video.HttpApi.Host/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.HttpApi/LCH.MicroService.Video.HttpApi.csproj", "LCH.MicroService.Video.HttpApi/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.Application/LCH.MicroService.Video.Application.csproj", "LCH.MicroService.Video.Application/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.Application.Contracts/LCH.MicroService.Video.Application.Contracts.csproj", "LCH.MicroService.Video.Application.Contracts/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.Domain/LCH.MicroService.Video.Domain.csproj", "LCH.MicroService.Video.Domain/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.Domain.Shared/LCH.MicroService.Video.Domain.Shared.csproj", "LCH.MicroService.Video.Domain.Shared/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.EntityFrameworkCore/LCH.MicroService.Video.EntityFrameworkCore.csproj", "LCH.MicroService.Video.EntityFrameworkCore/"]

# 复制ABP共享模块
COPY ["aspnet-core/modules/identity/LCH.MicroService.Identity.Domain.Shared/*.csproj", "LCH.MicroService.Identity.Domain.Shared/"]
COPY ["aspnet-core/modules/oss-management/LCH.MicroService.OssManagement.Domain.Shared/*.csproj", "LCH.MicroService.OssManagement.Domain.Shared/"]

# 还原依赖
RUN dotnet restore "LCH.MicroService.Video.HttpApi.Host/LCH.MicroService.Video.HttpApi.Host.csproj"

# 复制所有源代码
COPY aspnet-core/modules/video/. ./modules/video/
COPY aspnet-core/services/LCH.MicroService.Video.HttpApi.Host/. ./services/LCH.MicroService.Video.HttpApi.Host/

# 构建项目
WORKDIR "/src/services/LCH.MicroService.Video.HttpApi.Host"
RUN dotnet build "LCH.MicroService.Video.HttpApi.Host.csproj" -c Release -o /app/build

# 发布项目
FROM build AS publish
RUN dotnet publish "LCH.MicroService.Video.HttpApi.Host.csproj" -c Release -o /app/publish

# 最终镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 配置环境变量
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# 启动应用
ENTRYPOINT ["dotnet", "LCH.MicroService.Video.HttpApi.Host.dll"]
```

---

#### 3.2.2 TranscodeWorker Dockerfile

```dockerfile
# TranscodeWorker Dockerfile（包含FFmpeg）
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# 安装FFmpeg（CPU转码）
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制Worker项目文件
COPY ["aspnet-core/services/transcode-worker/LCH.MicroService.TranscodeWorker.csproj", "LCH.MicroService.TranscodeWorker/"]

# 复制依赖模块
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.Domain.Shared/*.csproj", "LCH.MicroService.Video.Domain.Shared/"]
COPY ["aspnet-core/modules/video/LCH.MicroService.Video.Application.Contracts/*.csproj", "LCH.MicroService.Video.Application.Contracts/"]

# 还原依赖
RUN dotnet restore "LCH.MicroService.TranscodeWorker/LCH.MicroService.TranscodeWorker.csproj"

# 复制源代码
COPY aspnet-core/services/transcode-worker/. ./services/transcode-worker/

# 构建项目
WORKDIR "/src/services/transcode-worker"
RUN dotnet build "LCH.MicroService.TranscodeWorker.csproj" -c Release -o /app/build

# 发布项目
FROM build AS publish
RUN dotnet publish "LCH.MicroService.TranscodeWorker.csproj" -c Release -o /app/publish

# 最终镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建转码临时目录
RUN mkdir -p /tmp/transcode && chmod 777 /tmp/transcode

# 配置环境变量
ENV FFmpeg__Path=/usr/bin/ffmpeg
ENV FFmpeg__TempDirectory=/tmp/transcode
ENV TranscodeSettings__MaxConcurrency=3

# 启动Worker
ENTRYPOINT ["dotnet", "LCH.MicroService.TranscodeWorker.dll"]
```

---

## 4. Kubernetes部署设计

### 4.1 K8s Deployment配置

#### 4.1.1 VideoService Deployment

```yaml
# video-service-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: video-service
  namespace: bilibili
spec:
  replicas: 3  # 3个副本
  selector:
    matchLabels:
      app: video-service
  template:
    metadata:
      labels:
        app: video-service
        version: v1
    spec:
      containers:
      - name: video-service
        image: bilibili-video-service:latest
        ports:
        - containerPort: 80
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__Default
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: video-db-connection
        - name: Redis__Configuration
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: redis-connection
        - name: RabbitMQ__HostName
          value: "rabbitmq-service"
        - name: MinIO__EndPoint
          value: "minio-service:9000"
        - name: MinIO__AccessKey
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: minio-access-key
        - name: MinIO__SecretKey
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: minio-secret-key
        - name: AuthServer__Authority
          value: "http://auth-server-service"
        resources:
          requests:
            cpu: "500m"
            memory: "512Mi"
          limits:
            cpu: "1000m"
            memory: "1Gi"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
      initContainers:
      - name: wait-for-dependencies
        image: busybox
        command: ['sh', '-c', 'until nc -z postgres-service 5432 && nc -z redis-service 6379 && nc -z rabbitmq-service 5672; do echo waiting; sleep 5; done;']

---
apiVersion: v1
kind: Service
metadata:
  name: video-service
  namespace: bilibili
spec:
  selector:
    app: video-service
  ports:
  - port: 80
    targetPort: 80
    name: http
  type: ClusterIP

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: video-service-hpa
  namespace: bilibili
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: video-service
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

---

#### 4.1.2 TranscodeWorker Deployment

```yaml
# transcode-worker-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: transcode-worker
  namespace: bilibili
spec:
  replicas: 3  # 3个Worker副本（并发转码）
  selector:
    matchLabels:
      app: transcode-worker
  template:
    metadata:
      labels:
        app: transcode-worker
    spec:
      containers:
      - name: transcode-worker
        image: bilibili-transcode-worker:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: RabbitMQ__HostName
          value: "rabbitmq-service"
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: rabbitmq-username
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: rabbitmq-password
        - name: MinIO__EndPoint
          value: "minio-service:9000"
        - name: FFmpeg__Path
          value: "/usr/bin/ffmpeg"
        - name: TranscodeSettings__MaxConcurrency
          value: "3"
        resources:
          requests:
            cpu: "2000m"    # 高CPU需求（转码）
            memory: "2Gi"   # 高内存需求
          limits:
            cpu: "4000m"
            memory: "4Gi"
        volumeMounts:
        - name: transcode-temp
          mountPath: /tmp/transcode
      volumes:
      - name: transcode-temp
        emptyDir: {}  # 临时目录（每次Pod创建时清空）
```

---

#### 4.1.3 DanmakuService Deployment（SignalR）

```yaml
# danmaku-service-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: danmaku-service
  namespace: bilibili
spec:
  replicas: 3  # 3个副本（SignalR Scaleout）
  selector:
    matchLabels:
      app: danmaku-service
  template:
    metadata:
      labels:
        app: danmaku-service
    spec:
      containers:
      - name: danmaku-service
        image: bilibili-danmaku-service:latest
        ports:
        - containerPort: 80
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__Default
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: danmaku-db-connection
        - name: Redis__Configuration
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: redis-connection
        - name: SignalR__Redis__Configuration
          valueFrom:
            secretKeyRef:
              name: bilibili-secrets
              key: redis-connection
        - name: SignalR__Redis__ChannelPrefix
          value: "danmaku:"
        resources:
          requests:
            cpu: "500m"
            memory: "512Mi"
          limits:
            cpu: "1000m"
            memory: "1Gi"
---
apiVersion: v1
kind: Service
metadata:
  name: danmaku-service
  namespace: bilibili
spec:
  selector:
    app: danmaku-service
  ports:
  - port: 80
    targetPort: 80
    name: http
  type: ClusterIP
```

---

### 4.2 ConfigMap和Secret配置

```yaml
# bilibili-configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: bilibili-config
  namespace: bilibili
data:
  # 基础配置
  ASPNETCORE_ENVIRONMENT: "Production"
  
  # 数据库连接（不含密码）
  POSTGRES_HOST: "postgres-service"
  POSTGRES_PORT: "5432"
  VIDEO_DB_NAME: "VideoDB"
  DANMAKU_DB_NAME: "DanmakuDB"
  INTERACTION_DB_NAME: "InteractionDB"
  
  # Redis配置
  REDIS_HOST: "redis-service"
  REDIS_PORT: "6379"
  
  # RabbitMQ配置
  RABBITMQ_HOST: "rabbitmq-service"
  RABBITMQ_PORT: "5672"
  
  # MinIO配置
  MINIO_ENDPOINT: "minio-service:9000"
  MINIO_BUCKET_NAME: "bilibili-videos"
  
  # Elasticsearch配置
  ELASTICSEARCH_URL: "http://elasticsearch-service:9200"
  
  # FFmpeg配置
  FFMPEG_PATH: "/usr/bin/ffmpeg"
  FFMPEG_TEMP_DIR: "/tmp/transcode"
  TRANSCODE_MAX_CONCURRENCY: "3"

---
# bilibili-secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: bilibili-secrets
  namespace: bilibili
type: Opaque
data:
  # 数据库密码（Base64编码）
  postgres-password: cGFzc3dvcmQ=
  
  # Redis密码
  redis-password: cGFzc3dvcmQ=
  
  # RabbitMQ密码
  rabbitmq-username: YWRtaW4=
  rabbitmq-password: cGFzc3dvcmQ=
  
  # MinIO密钥
  minio-access-key: bWluaW9hZG1pbg==
  minio-secret-key: bWluaW9hZG1pbg==
  
  # 数据库连接字符串
  video-db-connection: SG9zdD1wb3N0Z3Jlcy1zZXJ2aWNlO0RhdGFiYXNlPVZpZGVvREI7VXNlcm5hbWU9cG9zdGdyZXM7UGFzc3dvcmQ9cGFzc3dvcmQ=
  danmaku-db-connection: SG9zdD1wb3N0Z3Jlcy1zZXJ2aWNlO0RhdGFiYXNlPURhbm1ha3VEQjtVc2VybmFtZT1wb3N0Z3JlcztQYXNzd29yZD1wYXNzd29yZA==
  
  # Redis连接字符串
  redis-connection: cmVkaXMtc2VydmljZTo2Mzc5LHBhc3N3b3JkPXBhc3N3b3Jk
```

---

### 4.3 Ingress配置（API Gateway）

```yaml
# bilibili-ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: bilibili-ingress
  namespace: bilibili
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/ssl-redirect: "false"
    nginx.ingress.kubernetes.io/proxy-body-size: "0"  # 无文件大小限制
    nginx.ingress.kubernetes.io/proxy-read-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-send-timeout: "600"
spec:
  ingressClassName: nginx
  rules:
  - host: bilibili.example.com
    http:
      paths:
      # API Gateway路由（所有API请求）
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: gateway-service
            port:
              number: 80
      
      # WebSocket路由（弹幕）
      - path: /hubs/danmaku
        pathType: Exact
        backend:
          service:
            name: danmaku-service
            port:
              number: 80
      
      # WebSocket路由（直播）
      - path: /hubs/live
        pathType: Exact
        backend:
          service:
            name: live-service
            port:
              number: 80
      
      # MinIO直连（视频文件下载）
      - path: /videos
        pathType: Prefix
        backend:
          service:
            name: minio-service
            port:
              number: 9000
      
      # MinIO直连（封面图片）
      - path: /covers
        pathType: Prefix
        backend:
          service:
            name: minio-service
            port:
              number: 9000
```

---

## 5. 监控与日志

### 5.1 Prometheus监控配置

```yaml
# prometheus-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
  namespace: monitoring
data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
      evaluation_interval: 15s
    
    scrape_configs:
    # VideoService监控
    - job_name: 'video-service'
      kubernetes_sd_configs:
      - role: pod
        namespaces:
          names:
          - bilibili
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_label_app]
        regex: video-service
        action: keep
    
    # DanmakuService监控
    - job_name: 'danmaku-service'
      kubernetes_sd_configs:
      - role: pod
        namespaces:
          names:
          - bilibili
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_label_app]
        regex: danmaku-service
        action: keep
    
    # TranscodeWorker监控
    - job_name: 'transcode-worker'
      kubernetes_sd_configs:
      - role: pod
        namespaces:
          names:
          - bilibili
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_label_app]
        regex: transcode-worker
        action: keep
    
    # PostgreSQL监控
    - job_name: 'postgres'
      static_configs:
      - targets: ['postgres-exporter:9187']
    
    # Redis监控
    - job_name: 'redis'
      static_configs:
      - targets: ['redis-exporter:9121']
    
    # RabbitMQ监控
    - job_name: 'rabbitmq'
      static_configs:
      - targets: ['rabbitmq-service:15692']
    
    # MinIO监控
    - job_name: 'minio'
      static_configs:
      - targets: ['minio-service:9000']
```

---

### 5.2 Grafana Dashboard配置

```json
{
  "dashboard": {
    "title": "Bilibili Video Platform Dashboard",
    "panels": [
      {
        "title": "Video Upload Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(video_uploads_total[5m])",
            "legendFormat": "Uploads/min"
          }
        ]
      },
      {
        "title": "Transcode Queue Size",
        "type": "graph",
        "targets": [
          {
            "expr": "transcode_queue_size",
            "legendFormat": "Pending Tasks"
          }
        ]
      },
      {
        "title": "Danmaku Send Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(danmaku_sent_total[1m])",
            "legendFormat": "Danmakus/sec"
          }
        ]
      },
      {
        "title": "API Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ]
  }
}
```

---

## 6. 部署流程

### 6.1 Docker部署流程

```bash
# 1. 构建所有服务镜像
docker-compose build

# 2. 启动基础设施服务
docker-compose up -d postgres redis rabbitmq elasticsearch minio

# 3. 等待基础设施服务启动（约30秒）
sleep 30

# 4. 初始化数据库（运行ABP迁移）
docker-compose run --rm identity-server dotnet run --migrate-database
docker-compose run --rm video-service dotnet run --migrate-database
docker-compose run --rm danmaku-service dotnet run --migrate-database

# 5. 启动所有服务
docker-compose up -d

# 6. 查看服务状态
docker-compose ps

# 7. 查看日志
docker-compose logs -f video-service
```

---

### 6.2 Kubernetes部署流程

```bash
# 1. 创建命名空间
kubectl create namespace bilibili

# 2. 创建Secret和ConfigMap
kubectl apply -f bilibili-secrets.yaml
kubectl apply -f bilibili-configmap.yaml

# 3. 部署基础设施服务
kubectl apply -f postgres-deployment.yaml
kubectl apply -f redis-deployment.yaml
kubectl apply -f rabbitmq-deployment.yaml
kubectl apply -f elasticsearch-deployment.yaml
kubectl apply -f minio-deployment.yaml

# 4. 等待基础设施服务就绪
kubectl wait --for=condition=ready pod -l app=postgres -n bilibili --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n bilibili --timeout=300s

# 5. 部署认证服务
kubectl apply -f identity-server-deployment.yaml
kubectl apply -f auth-server-deployment.yaml

# 6. 部署Bilibili业务服务
kubectl apply -f video-service-deployment.yaml
kubectl apply -f danmaku-service-deployment.yaml
kubectl apply -f transcode-worker-deployment.yaml
kubectl apply -f interaction-service-deployment.yaml
kubectl apply -f user-service-deployment.yaml
kubectl apply -f search-service-deployment.yaml
kubectl apply -f recommend-service-deployment.yaml
kubectl apply -f live-service-deployment.yaml
kubectl apply -f category-service-deployment.yaml
kubectl apply -f admin-service-deployment.yaml

# 7. 部署API Gateway
kubectl apply -f gateway-deployment.yaml
kubectl apply -f bilibili-ingress.yaml

# 8. 验证部署
kubectl get pods -n bilibili
kubectl get services -n bilibili
kubectl get ingress -n bilibili
```

---

## 7. 备份与恢复策略

### 7.1 数据库备份脚本

```bash
# postgres-backup.sh
#!/bin/bash

# PostgreSQL备份脚本（每日备份）

BACKUP_DIR="/backup/postgres"
DATE=$(date +%Y%m%d_%H%M%S)

# 创建备份目录
mkdir -p $BACKUP_DIR

# 备份所有数据库
docker exec bilibili-postgres pg_dumpall -U postgres > $BACKUP_DIR/all_databases_$DATE.sql

# 备份单个数据库
docker exec bilibili-postgres pg_dump -U postgres VideoDB > $BACKUP_DIR/video_db_$DATE.sql
docker exec bilibili-postgres pg_dump -U postgres DanmakuDB > $BACKUP_DIR/danmaku_db_$DATE.sql
docker exec bilibili-postgres pg_dump -U postgres InteractionDB > $BACKUP_DIR/interaction_db_$DATE.sql
docker exec bilibili-postgres pg_dump -U postgres UserDB > $BACKUP_DIR/user_db_$DATE.sql

# 压缩备份文件
gzip $BACKUP_DIR/*.sql

# 删除30天前的备份
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete

echo "PostgreSQL备份完成: $DATE"
```

---

### 7.2 MinIO备份脚本

```bash
# minio-backup.sh
#!/bin/bash

# MinIO备份脚本（每日备份）

BACKUP_DIR="/backup/minio"
DATE=$(date +%Y%m%d_%H%M%S)

# 创建备份目录
mkdir -p $BACKUP_DIR

# 使用MinIO Client (mc)备份
mc mirror bilibili-videos $BACKUP_DIR/bilibili-videos_$DATE

# 压缩备份文件
tar -czf $BACKUP_DIR/minio_backup_$DATE.tar.gz $BACKUP_DIR/bilibili-videos_$DATE

# 删除临时目录
rm -rf $BACKUP_DIR/bilibili-videos_$DATE

# 删除30天前的备份
find $BACKUP_DIR -name "*.tar.gz" -mtime +30 -delete

echo "MinIO备份完成: $DATE"
```

---

## 8. 运维监控指标

### 8.1 关键监控指标

| 服务 | 监控指标 | 阈值 | 说明 |
|------|---------|------|------|
| **VideoService** | CPU使用率 | <80% | 视频处理 |
| | 内存使用率 | <85% | |
| | HTTP响应时间 | <500ms | API响应 |
| | 视频上传速率 | >10/min | |
| **TranscodeWorker** | CPU使用率 | <95% | 转码高CPU |
| | 内存使用率 | <90% | |
| | 转码队列长度 | <100 | |
| | 转码成功率 | >95% | |
| **DanmakuService** | WebSocket连接数 | <10万 | |
| | 弹幕发送速率 | >1000/sec | |
| | Redis连接数 | <500 | |
| **PostgreSQL** | 连接数 | <500 | |
| | 查询响应时间 | <100ms | |
| | 磁盘使用率 | <80% | |
| **Redis** | 内存使用率 | <80% | |
| | 连接数 | <1000 | |
| | 命令响应时间 | <10ms | |
| **RabbitMQ** | 队列长度 | <1000 | |
| | 消息吞吐量 | >5000/sec | |
| **MinIO** | 存储使用率 | <80% | |
| | 下载带宽 | <100MB/s | |
| | 上传带宽 | <50MB/s | |

---

## 9. 总结

### 9.1 部署架构特点

| 特点 | 说明 |
|------|------|
| **继承现有Docker配置** | 使用现有Dockerfile和docker-compose模式 |
| **微服务独立部署** | 每个服务独立Docker镜像和K8s Deployment |
| **高可用性** | K8s Deployment多副本、HPA自动扩缩容 |
| **负载均衡** | K8s Service + Ingress负载均衡 |
| **监控完整** | Prometheus + Grafana监控体系 |
| **日志集中** | ELK Stack日志分析（继承现有） |
| **备份自动化** | 数据库和MinIO自动备份脚本 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 部署运维文档完成