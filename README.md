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
### Flowchart вариантов использования

🔵 Синий: Авторизация (Login/Register)

🟡 Желтый: Главная навигация (Index)

🟢 Зеленый: CRUD списки (Entities)

🟣 Фиолетовый: Действия (Create/Edit/Delete)

🔴 Красный: Обработка ошибок

🟦 Голубой: API и Swagger

```mermaid
flowchart TD
    %% === Стили ===
    classDef auth fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    classDef main fill:#fff9c4,stroke:#fbc02d,stroke-width:2px
    classDef crud fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px
    classDef action fill:#f3e5f5,stroke:#7b1fa2,stroke-dasharray: 5 5
    classDef error fill:#ffebee,stroke:#c62828,stroke-width:2px
    classDef api fill:#e0f7fa,stroke:#00838f,stroke-width:2px

    %% === Точка входа и Авторизация ===
    Start([Пользователь]) --> CheckAuth{Авторизован?}
    
    CheckAuth -- Нет --> LoginPage[🔐 Страница входа / Регистрация]:::auth
    LoginPage -->|Успех | Index
    LoginPage -->|"Ошибка валидации"| LoginPage
    LoginPage -->|Ошибка БД | ErrorPage

    CheckAuth -- Да --> Index[🏠 Index / Главное меню]:::main

    %% === Навигация из меню ===
    Index --> ClientsList[👥 Clients: Список]:::crud
    Index --> OrdersList[🛒 Orders: Список]:::crud
    Index --> ProductsList[📦 Products: Список]:::crud
    Index -->|Logout | LoginPage
    Index --> API_Doc[📄 Swagger UI /api]:::api

    %% === CRUD: Clients ===
    subgraph Clients_Flow [Clients CRUD]
        ClientsList -->|Create | ClientCreate[➕ Создание клиента]:::action
        ClientsList -->|Edit | ClientEdit[✏️ Редактирование]:::action
        ClientsList -->|Delete | ClientDelete[🗑️ Подтверждение удаления]:::action
        
        ClientCreate -->|"Save [BindProperty]"| ClientsList
        ClientCreate -->|Cancel | ClientsList
        ClientCreate -->|"Validation Error"| ClientCreate
        
        ClientEdit -->|Save | ClientsList
        ClientEdit -->|Cancel | ClientsList
        
        ClientDelete -->|Confirm | ClientsList
        ClientDelete -->|Cancel | ClientsList
    end

    %% === CRUD: Orders ===
    subgraph Orders_Flow [Orders CRUD]
        OrdersList -->|Create | OrderCreate[➕ Новый заказ]:::action
        OrdersList -->|Edit | OrderEdit[✏️ Изменить заказ]:::action
        OrdersList -->|Delete | OrderDelete[🗑️ Удалить заказ]:::action
        
        OrderCreate -->|Save | OrdersList
        OrderCreate -->|Cancel | OrdersList
        
        OrderEdit -->|Save | OrdersList
        OrderEdit -->|Cancel | OrdersList
        
        OrderDelete -->|Confirm | OrdersList
    end

    %% === CRUD: Products ===
    subgraph Products_Flow [Products CRUD]
        ProductsList -->|Create | ProductCreate[➕ Добавить товар]:::action
        ProductsList -->|Edit | ProductEdit[✏️ Редактировать товар]:::action
        ProductsList -->|Delete | ProductDelete[🗑️ Удалить товар]:::action
        
        ProductCreate -->|Save | ProductsList
        ProductEdit -->|Save | ProductsList
        ProductDelete -->|Confirm | ProductsList
    end

    %% === API Flow ===
    API_Doc --> JwtGen[🔑 POST /api/orders/jwt]:::api
    JwtGen -->|"200 OK + Token"| ApiTest[🧪 Тестирование в OrdersJwt.http]:::api
    ApiTest -->|Authorized | OrdersAPI[🛒 REST API: Orders]:::api

    %% === Обработка ошибок (Global) ===
    Index -.->|Exception | ErrorPage[❌ Error Page]:::error
    ClientsList -.->|Exception | ErrorPage
    OrdersList -.->|Exception | ErrorPage
    ProductsList -.->|Exception | ErrorPage
    LoginPage -.->|Exception | ErrorPage
    
    ErrorPage -->|Home | Index
    ErrorPage -->|Login | LoginPage

    %% === Связи данных (опционально) ===
    OrdersList -.->|Выбор Client | ClientsList
    OrdersList -.->|Выбор Product | ProductsList
```