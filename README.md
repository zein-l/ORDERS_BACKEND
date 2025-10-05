# 🧾 OrdersAssessment

A lightweight full-stack Orders Management system demonstrating modern .NET 8 + React + Clean Architecture.

## 📘 Overview
This project enables users to:
- Register / Login with JWT authentication
- Create and view orders tied to their account
- Add multiple items per order with automatic total price calculation
- Log and audit all key actions for traceability
- View and manage orders via a React + Tailwind frontend

## 🏗️ Tech Stack
| Layer | Tech |
|--------|------|
| **Backend** | ASP.NET Core 8 Web API |
| **Data** | EF Core (Code-First, SQLite) |
| **Architecture** | Clean Architecture (Domain / Application / Infrastructure / Api) |
| **Auth** | JWT Bearer |
| **Logging** | Serilog |
| **Frontend** | React 18 + Vite + Tailwind CSS |
| **Tests** | xUnit (.NET) |

## ⚙️ How to Run
### 1️⃣ Backend (API)
```bash
dotnet restore
dotnet build
dotnet run --project Orders.Api/Orders.Api.csproj
```
**API:** http://localhost:5238  
**Swagger UI:** http://localhost:5238/swagger

### 2️⃣ Frontend (UI)
```bash
cd orders-ui
npm install
npm run dev
```
**Frontend:** http://localhost:5173  
✅ `.env` is already configured to point to your local API.

## 🔑 Basic Workflow
1. **Register** → creates a new user account  
2. **Login** → get a JWT token automatically handled by the UI  
3. **Create Order** → adds a new draft order  
4. **Add Items** → updates totals dynamically  
5. **Logout** → clear auth session  

## 🧩 Project Structure
```
OrdersAssessment/
├── Orders.Domain/          # Entities & rules (Order, Item, User)
├── Orders.Application/     # Services, DTOs, Interfaces
├── Orders.Infrastructure/  # EF Core, Repositories, Logging
├── Orders.Api/             # Controllers, Auth, Swagger, Serilog
├── Orders.Tests/           # xUnit unit tests
└── orders-ui/              # React + Vite + Tailwind frontend
```

## 🧪 Testing
```bash
cd OrdersAssessment
dotnet test
```
Includes sample unit tests for:
- Order total recalculation  
- Adding/removing order items  

## 🧠 For Assessment
### ✅ Requirements Covered
| Requirement | Status |
|--------------|---------|
| Register/Login (JWT) | ✅ |
| Create/Retrieve Orders | ✅ |
| Multi-item orders with auto totals | ✅ |
| Error logging (Serilog) | ✅ |
| Audit events for traceability | ✅ |
| Unit tests | ✅ |
| Clean Architecture + DTOs | ✅ |

## 🧭 Quick Demo
1. Run API → open **Swagger**  
2. Register → Login → Authorize  
3. Create Order → Add Items → Observe total auto-update  
4. Frontend shows synced results visually  
