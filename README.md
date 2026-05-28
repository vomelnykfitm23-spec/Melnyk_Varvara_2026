# Auction App

Веб-застосунок інтернет-аукціону. Користувачі можуть виставляти лоти, робити ставки та стежити за результатами.

**Стек:**
- **Сервер** — ASP.NET Core 10 Web API (C#), EF Core 9, PostgreSQL, JWT
- **Клієнт** — React 19 + Vite, Zustand, Axios, React Router

---

## Структура

```
auction-app/
├── server/AuctionApp/   ← ASP.NET Core Web API
└── client/              ← React + Vite
```

---

## Запуск

### 1. База даних

Потрібен PostgreSQL. У файлі `server/AuctionApp/appsettings.json` вказати свої дані:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=auction_db;Username=postgres;Password=ВАШ_ПАРОЛЬ"
}
```

### 2. Міграція

```bash
cd server/AuctionApp
dotnet ef migrations add InitialCreate
dotnet ef database update
```

База створюється автоматично. Seed-дані (користувачі, лоти, ставки) вставляються при першому `dotnet run`.

### 3. Сервер

```bash
cd server/AuctionApp
dotnet run
```

API доступний на `https://localhost:7138`

### 4. Клієнт

```bash
cd client
npm install
npm run dev
```

Фронтенд доступний на `http://localhost:5173`

---

## Тестові користувачі

Всі паролі: **`Password1!`**

| Username | Email              | Роль  |
|----------|--------------------|-------|
| admin    | admin@auction.dev  | Admin |
| mykola   | mykola@example.com | User  |
| halyna   | halyna@example.com | User  |
| bohdan   | bohdan@example.com | User  |
| oksana   | oksana@example.com | User  |

---

## API ендпоінти

| Метод  | URL                   | Auth | Опис                                    |
|--------|-----------------------|------|-----------------------------------------|
| POST   | /api/auth/register    | –    | Реєстрація                              |
| POST   | /api/auth/login       | –    | Вхід, повертає JWT                      |
| GET    | /api/tags             | –    | Список усіх тегів                       |
| GET    | /api/lots             | –    | Список лотів (пошук, тег, пагінація)    |
| GET    | /api/lots/{id}        | –    | Деталі лота                             |
| POST   | /api/lots             | JWT  | Створити лот                            |
| DELETE | /api/lots/{id}        | JWT  | Скасувати лот (тільки свій, без ставок) |
| GET    | /api/lots/{id}/bids   | –    | Історія ставок                          |
| POST   | /api/bids             | JWT  | Зробити ставку                          |
| POST   | /api/uploads/image    | JWT  | Завантажити зображення                  |
| GET    | /api/users/me/lots    | JWT  | Мої лоти                                |
| GET    | /api/users/me/bids    | JWT  | Мої ставки                              |

Параметри `GET /api/lots`: `?search=`, `?tagId=`, `?page=`, `?pageSize=`

---

## Seed-дані

При першому старті сервера автоматично створюється:

- **10 лотів**: 6 активних, 3 продані, 1 скасований
- **22 ставки** на активні та продані лоти
- **10 тегів**: Електроніка, Книги, Мистецтво, Одяг та взуття, Колекціонування, Транспорт, Меблі, Ювелірні вироби, Спорт, Інше
