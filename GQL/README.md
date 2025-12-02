# ASP.NET Web API with Entity Framework Core and SQLite

This project demonstrates a simple ASP.NET Web API application using Entity Framework Core with SQLite database.

## Features

- ASP.NET Core Web API with minimal APIs
- Entity Framework Core 9.0
- SQLite database
- CRUD operations for Products
- OpenAPI/Swagger documentation
- Code-first approach with migrations

## Project Structure

```
GQL/
├── Data/
│   └── AppDbContext.cs          # Entity Framework DbContext
├── Models/
│   └── Product.cs               # Product entity model
├── Migrations/                  # EF Core migrations
├── Program.cs                   # Application entry point and configuration
├── appsettings.json            # Configuration including connection string
└── GQL.csproj                  # Project file
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQLite (included with EF Core SQLite provider)

### Running the Application

1. Navigate to the project directory:
   ```bash
   cd GQL
   ```

2. Restore packages (if needed):
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. The API will be available at `http://localhost:5275`

### API Endpoints

The application provides the following endpoints for managing products:

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get a specific product by ID
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update an existing product
- `DELETE /api/products/{id}` - Delete a product

### Sample Product Model

```json
{
  "id": 1,
  "name": "Sample Product",
  "description": "A sample product description",
  "price": 29.99,
  "stock": 100,
  "createdAt": "2025-12-02T08:33:34.123Z",
  "updatedAt": "2025-12-02T08:33:34.123Z"
}
```

## Database

The application uses SQLite with the following configuration:

- **Database File**: `app.db` (created in the project root)
- **Connection String**: `Data Source=app.db`

### Entity Framework Core Commands

- Create a new migration:
  ```bash
  dotnet ef migrations add <MigrationName>
  ```

- Update the database:
  ```bash
  dotnet ef database update
  ```

- Remove the last migration:
  ```bash
  dotnet ef migrations remove
  ```

## Testing the API

You can test the API using:

1. **Swagger UI**: Navigate to `http://localhost:5275/swagger` (when running in Development mode)
2. **HTTP files**: Use the included `GQL.http` file in VS Code with the REST Client extension
3. **curl** or **Postman**: Use the endpoint URLs listed above

### Example curl commands:

```bash
# Get all products
curl -X GET http://localhost:5275/api/products

# Create a new product
curl -X POST http://localhost:5275/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "description": "A test product",
    "price": 19.99,
    "stock": 50
  }'

# Get a specific product
curl -X GET http://localhost:5275/api/products/1
```

## Dependencies

- Microsoft.EntityFrameworkCore.Sqlite (9.0.0)
- Microsoft.EntityFrameworkCore.Design (9.0.0)

## Notes

- The application uses minimal APIs instead of traditional controllers for simplicity
- The database is created automatically when the application starts for the first time
- The project follows Entity Framework Core code-first approach
- OpenAPI documentation is automatically generated and available in development mode