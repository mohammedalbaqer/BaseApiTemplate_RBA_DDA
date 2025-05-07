# Base Api Template RBA

**Version: 1.0.0**

MyIdentityApi is a base template for creating identity management APIs using ASP.NET Core. It includes features such as JWT authentication, role-based access control, and integration with PostgreSQL and SQL Server databases.

## Features

- JWT Authentication
- Role-based Access Control
- Supports PostgreSQL and SQL Server
- Swagger for API documentation
- CORS policy setup for development
- Static File Handling for File Uploads

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL or SQL Server database

## Setup

1. **Clone the repository:**

   ```bash
   git clone https://github.com/mohammedalbaqer/BaseApiTemplate_RBA_DDA.git
   cd BaseApiTemplate_RBA_DDA

   ```

2. **Configure the appsettings.json::**

   Open the appsettings.json file and ensure the following configurations are set:

Database Connection Strings:

```json
"ConnectionStrings": {
"PostgreSqlConnection": "Host=localhost;Port=5432;Database=myIdentityApiDb;Username=postgres;Password=yourpassword;Include Error Detail=true;",

"SqlServerConnection": "Server=localhost\\SQLEXPRESS;Database=myIdentityApiDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
}
```

JWT Settings:

```json
"Jwt": {
"Key": "your secret key",
"Issuer": "YourIssuer",
"Audience": "YourAudience",
"TokenExpiryMinutes": 15,
"RefreshTokenExpiryDays": 7
}
```

3. **Run the migrations:**

   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the API:**

   ```bash
   dotnet run
   ```

## Usage

Access the API documentation at http://localhost:5211/swagger when the application is running.

## Project Structure

- Controllers/ : Contains API controllers for managing accounts, roles, and users.
- Data/ : Contains the ApplicationDbContext for database interactions.
- Dtos/ : Contains Data Transfer Objects for API requests and responses.
- Extensions/ : Contains extension methods for configuring services.
- Models/ : Contains entity models for the application.
- Services/ : Contains service classes for handling business logic.
- wwwroot/ : Contains static files and uploaded media content.

## File Upload Structure

The application supports file uploads with the following structure:

- wwwroot/
  - uploads/
    - profiles/ : For user profile images
    - other directories based on image types

Files are served through the static files middleware and are accessible via URLs like: /uploads/{type}/{filename}

## Direct Data Access Pattern

This project uses the "Direct Data Access" pattern, where controllers directly interact with the DbContext for CRUD operations.

Characteristics:

- Simplicity : Easy to implement for small applications.
- Tight Coupling : Business logic is coupled with data access logic.
- Less Abstraction : Can lead to repetitive code if not managed carefully.

### When to Use:

- For small applications with limited complexity.
- When the application is simple and doesn't require complex data access patterns.

### Considerations:

- Consider refactoring to a repository or service layer as the application grows.
- Keep controllers clean by offloading business logic to service classes.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

This update includes the instructions and guidelines for using the project, ensuring that users understand the rules and how to interact with the codebase. You can further customize it based on your specific needs or additional features.

I've updated the README.md with:
1. Added "Static File Handling for File Uploads" to the Features section
2. Added wwwroot directory to the Project Structure section
3. Added a new "File Upload Structure" section explaining the file upload organization
4. Updated the content to reflect the current state of the project with file upload capabilities

The rest of the original content remains unchanged to maintain consistency while adding the new information about static file handling.

Developed by Mohammed Al Baqer Talib
