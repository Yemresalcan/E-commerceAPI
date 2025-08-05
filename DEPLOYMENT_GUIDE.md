# ECommerce API - Deployment Guide

Bu rehber, ECommerce API'sini yerel geliştirme ortamında ve production'da nasıl çalıştıracağınızı gösterir.

## 🚀 Hızlı Başlangıç

### Gereksinimler

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Projeyi Klonlayın

```bash
git clone <repository-url>
cd ECommerce.Solution
```

### 2. Infrastructure Servislerini Başlatın

```bash
# Tüm infrastructure servislerini başlat
docker-compose up -d postgres redis elasticsearch rabbitmq

# Servislerin durumunu kontrol et
docker-compose ps
```

### 3. Veritabanı Migration'larını Çalıştırın

```bash
# Migration'ları uygula
dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure --startup-project src/Presentation/ECommerce.WebAPI --context ECommerceDbContext
```

### 4. Uygulamayı Çalıştırın

```bash
# Development ortamında çalıştır
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Presentation/ECommerce.WebAPI

# Windows PowerShell için:
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run --project src/Presentation/ECommerce.WebAPI
```

### 5. API'ye Erişim

Uygulama başarıyla başladıktan sonra:

- **API Base URL**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health Checks**: http://localhost:8080/health
- **Health Checks UI**: http://localhost:8080/healthchecks-ui

## 📊 Swagger UI Kullanımı

### Swagger'a Erişim

1. Tarayıcınızda http://localhost:8080/swagger adresine gidin
2. Tüm API endpoint'lerini görebilirsiniz

### API Endpoint'leri

#### Products API
- `GET /api/v1.0/products` - Ürünleri listele
- `GET /api/v1.0/products/search` - Ürün ara
- `POST /api/v1.0/products` - Yeni ürün oluştur
- `PUT /api/v1.0/products/{id}` - Ürün güncelle
- `DELETE /api/v1.0/products/{id}` - Ürün sil

#### Orders API
- `GET /api/v1.0/orders` - Siparişleri listele
- `GET /api/v1.0/orders/{id}` - Sipariş detayı
- `POST /api/v1.0/orders` - Yeni sipariş oluştur
- `PUT /api/v1.0/orders/{id}/status` - Sipariş durumu güncelle

#### Customers API
- `GET /api/v1.0/customers` - Müşterileri listele
- `GET /api/v1.0/customers/{id}` - Müşteri detayı
- `POST /api/v1.0/customers` - Yeni müşteri kaydet
##
# Örnek API Kullanımı

#### 1. Yeni Müşteri Oluşturma

```json
POST /api/v1.0/customers
{
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "email": "ahmet@example.com",
  "phoneNumber": "+905551234567"
}
```

#### 2. Yeni Ürün Oluşturma

```json
POST /api/v1.0/products
{
  "name": "Kablosuz Kulaklık",
  "description": "Yüksek kaliteli kablosuz kulaklık",
  "price": 299.99,
  "currency": "TRY",
  "stockQuantity": 50,
  "categoryId": "123e4567-e89b-12d3-a456-426614174000"
}
```

#### 3. Sipariş Oluşturma

```json
POST /api/v1.0/orders
{
  "customerId": "123e4567-e89b-12d3-a456-426614174001",
  "items": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "quantity": 2,
      "unitPrice": 299.99
    }
  ],
  "shippingAddress": {
    "street": "Atatürk Cad. No:123",
    "city": "İstanbul",
    "state": "İstanbul",
    "postalCode": "34000",
    "country": "Türkiye"
  }
}
```

## 🔧 Sorun Giderme

### RabbitMQ Bağlantı Sorunu

Eğer RabbitMQ bağlantı hatası alıyorsanız:

```bash
# RabbitMQ'yu yeniden başlat
docker-compose restart rabbitmq

# Logları kontrol et
docker-compose logs rabbitmq
```

### Elasticsearch Yellow State

Elasticsearch tek node'da çalıştığı için "yellow" durumda olabilir. Bu normal bir durumdur.

### Health Check Durumu

Health check'leri kontrol etmek için:
- http://localhost:8080/health - Basit health check
- http://localhost:8080/health/detailed - Detaylı health check

## 🐳 Docker ile Tam Çalıştırma

Tüm uygulamayı Docker ile çalıştırmak için:

```bash
# Tüm servisleri başlat (uygulama dahil)
docker-compose up -d

# Logları takip et
docker-compose logs -f ecommerce-api
```

## 📝 Önemli Notlar

1. **Development Environment**: Uygulama development modunda çalışırken detaylı loglar ve Swagger UI aktiftir.

2. **Database**: PostgreSQL veritabanı otomatik olarak oluşturulur ve migration'lar uygulanır.

3. **Caching**: Redis cache servisi aktiftir ve API response'ları cache'lenir.

4. **Search**: Elasticsearch search servisi aktiftir ve ürün aramaları için kullanılır.

5. **Messaging**: RabbitMQ event messaging için kullanılır.

## 🔍 Monitoring

- **Health Checks UI**: http://localhost:8080/healthchecks-ui
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Elasticsearch**: http://localhost:9200

## 🚀 Production Deployment

Production deployment için `docker-compose.prod.yml` dosyasını kullanın:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 📞 Destek

Herhangi bir sorun yaşarsanız:
1. GitHub Issues'da yeni bir issue açın
2. Logları kontrol edin: `docker-compose logs`
3. Health check'leri kontrol edin: http://localhost:8080/health