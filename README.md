# 📚 Bazar.com - Microservice Bookstore CLI

A .NET-based CLI application that simulates a mini online bookstore where users can browse, search, and purchase books — all powered by Dockerized microservices.

---

## 🚀 Features

-  Microservices: CatalogService, OrderService, and FrontendService
-  CLI-based user interface for interaction
-  Docker Compose for seamless orchestration
-  File-based data persistence
-  Live-rebuild support with `docker compose watch`

---

## 🧰 Tech Stack

- C# (.NET 8)
- Docker & Docker Compose
- REST APIs (ASP.NET Core)
- JSON file storage

---

## ⚙️ Getting Started

### Clone the repository

```bash
git clone https://github.com/your-username/Bazar-Book-Store.git
cd Bazar.com
```
Then after moveing to the Bazar.com you will use docker-compose
```swift
docker-compose up --build
```
Now press V button to open it from the desktop docker application and after this open the frontend Service then exec option and hit 
```bash
dotnet UserAction.dll build
```
Now you will see the main menue like this 
```bash
📚 Welcome to the Online Bookstore!
1. View All Catalog Items
2. View Catalog Item
3. Purchase Book
4. Search Book Topic
5. Exit
```
## API Reference
* Catalog Service

| Method | Endpoint | Usage|
|-------|----------- |---|
|  GET    | /catalog/info |List all books|
|  Get    | /catalog/info/{id}     |Get book by ID|
|  Post  | /catalog/update/decrement/{id}       |Decrement stock|
|  Get   | /catalog/search/{topic}     |Search by topic|
|  Post  | /order/purchase/{itemNumber}|Purchase an item|

* Order Service

| Method | Endpoint | Usage|
|-------|----------- |---|
|  Post   | /order/purchase/{id} |Purchase book by ID|
## TODO
- Add authentication

- Add database support

- Add unit tests

