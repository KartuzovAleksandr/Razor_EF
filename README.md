# Razor_EF

Проект Razor Pages на **C# ASP.NET Core 8** с **EF Core 8**

## 🔧 Технологический стек

| Frontend | Backend | Database | Tools |
|----------|---------|----------|-------|
| **Razor Pages** | **ASP.NET Core 8** | **SQLite** | **Bogus** (Faker) |
| **Bootstrap 5** | **EF Core 8** | **MS SQL Server** | **Serilog** |
| **Tag Helpers** | **BCrypt** | **PostgreSQL** | **Swagger** |
| **HTML Helpers** | **JWT Auth** | **Docker** | |

**Переключение БД**: `appsettings.json`

## ✨ Функционал

При первом старте (пустая БД) заполняется случайными данными через Bogus (Faker)

Razor Pages CRUD для всех таблиц с:

Tag Helpers, Html Helpers, [BindProperty]

✅ Валидация, имена полей и ошибок валидации из классов модели

✅ Обработка ошибок валидации

✅ Bootstrap дизайн

## Страницы:

🏠 Index главное меню, выбор таблиц

CRUD для 
👥 Clients
🛒 Orders
📦 Products

❌ Error (Production/Development)

## Безопасность:

🔐 Регистрация/Логин с BCrypt (BlowFish) хэшированием паролей

🍪 Cookie авторизация по ролям для страниц Razor

🔑 JWT токены для REST API по ролям

/api/orders/jwt — генерация токена

## API:

REST для Orders + Swagger UI

JWT авторизация

OrdersJwt.http для тестов

## Логирование: 
Serilog → logs/Razor_EF-xxxxxxx.txt

## Deployment

Dockerfile

docker-compose.yml

### 🗄️ ERD Диаграмма базы данных

```mermaid
erDiagram
    Clients {
        int Id PK
        string Name
        string Email "unique"
    }
    Orders {
        int Id PK
        DateTime Date
        int ClientId FK
        int ProductId FK
        int Quantity
    }
    Products {
        int Id PK
        string Name
        decimal Price
    }
    Users {
        int Id PK
        string UserName "unique"
        string PasswordHash
        string Role
    }
    
    Orders ||--o{ Clients : "places"
    Orders }|--|| Products : "contains"
```

### 🗄️ UseCase Диаграмма вариантов использования (упрощенная)

```mermaid
graph LR
  subgraph Actors
    Visitor[(Visitor)]
    User[(User)]
    Admin[(Admin)]
    Manager[(Manager)]
  end

  subgraph UseCases
    ViewCatalog(("View Catalog"))
    Register(("Register"))
    Login(("Login"))
    CreateOrder(("Create Order"))
    TrackOrder(("Track Order"))
    ManageProducts(("Manage Products"))
    EditProduct(("Edit Product"))
    ManageOrders(("Manage Orders"))
  end

  Visitor --> ViewCatalog
  Visitor --> Register
  Visitor --> Login

  User --> CreateOrder
  User --> TrackOrder

  Admin --> ManageProducts
  Admin --> EditProduct

  Manager --> ManageOrders

  Register -.-> CreateOrder
  Login -.-> CreateOrder
```

Для просмотра и редактирования диаграмм рекомендуется https://mermaid.live/