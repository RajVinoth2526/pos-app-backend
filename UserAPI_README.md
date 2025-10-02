# User API Documentation

This document describes the User API endpoints for the POS system, including authentication, user management, and security features.

## 🔐 **Authentication Overview**

The User API uses JWT (JSON Web Tokens) for authentication. All protected endpoints require a valid JWT token in the Authorization header.

**Authorization Header Format:**
```
Authorization: Bearer <your-jwt-token>
```

## 📋 **Models**

### User
- `Id`: Unique identifier
- `Username`: Unique username for login
- `Email`: Unique email address
- `PasswordHash`: Hashed password (not exposed in responses)
- `FirstName`: User's first name
- `LastName`: User's last name
- `PhoneNumber`: Contact phone number
- `Role`: User role (Admin, Manager, Cashier, User)
- `IsActive`: Whether the account is active
- `LastLoginDate`: Last successful login timestamp
- `CreatedAt`: Account creation timestamp
- `UpdatedAt`: Last update timestamp
- `LastPasswordChangeDate`: Last password change timestamp
- `FailedLoginAttempts`: Count of failed login attempts
- `AccountLockedUntil`: Account lock expiration (if locked)

### UserSession
- `Id`: Session identifier
- `UserId`: Reference to user
- `Token`: JWT token
- `CreatedAt`: Session creation timestamp
- `ExpiresAt`: Token expiration timestamp
- `IsActive`: Whether session is active
- `IpAddress`: IP address of login
- `UserAgent`: Browser/client information

## 🚀 **API Endpoints**

### **1. User Registration**
**POST** `/api/users/register`

Creates a new user account.

**Request Body:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "role": "Cashier"
}
```

**Password Requirements:**
- Minimum 8 characters
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain number
- Must contain special character

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "role": "Cashier",
    "isActive": true,
    "createdAt": "2024-12-01T10:30:00Z"
  }
}
```

### **2. User Login**
**POST** `/api/users/login`

Authenticates user and returns JWT token.

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-12-02T10:30:00Z",
    "user": {
      "id": 1,
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Cashier",
      "isActive": true
    }
  }
}
```

### **3. User Logout**
**POST** `/api/users/logout`

Logs out the current user session.

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": "Logged out successfully"
}
```

### **4. Logout All Sessions**
**POST** `/api/users/logout-all`

Logs out all active sessions for the current user.

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": "All sessions logged out successfully"
}
```

### **5. Get Current User**
**GET** `/api/users/me`

Retrieves information about the currently authenticated user.

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Cashier",
    "isActive": true,
    "lastLoginDate": "2024-12-01T10:30:00Z"
  }
}
```

### **6. Get User by ID**
**GET** `/api/users/{id}`

Retrieves a specific user by ID (Admin/Manager only).

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Cashier",
    "isActive": true
  }
}
```

### **7. Get All Users**
**GET** `/api/users`

Retrieves all users with optional filtering and pagination (Admin/Manager only).

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `username`: Filter by username
- `email`: Filter by email
- `role`: Filter by role
- `isActive`: Filter by active status
- `startDate`: Filter users created from this date
- `endDate`: Filter users created until this date

**Example:**
```
GET /api/users?page=1&pageSize=20&role=Cashier&isActive=true
```

### **8. Update User**
**PATCH** `/api/users/{id}`

Updates an existing user (Admin/Manager only).

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Request Body:**
```json
{
  "firstName": "Johnny",
  "lastName": "Smith",
  "role": "Manager",
  "isActive": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "Johnny",
    "lastName": "Smith",
    "role": "Manager",
    "isActive": true
  }
}
```

### **9. Delete User**
**DELETE** `/api/users/{id}`

Deletes a user account (Admin only).

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": "User deleted successfully"
}
```

### **10. Change Password**
**POST** `/api/users/change-password`

Changes the current user's password.

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Request Body:**
```json
{
  "currentPassword": "SecurePass123!",
  "newPassword": "NewSecurePass456!",
  "confirmNewPassword": "NewSecurePass456!"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": "Password changed successfully"
}
```

### **11. Reset Password**
**POST** `/api/users/reset-password`

Initiates password reset process.

**Request Body:**
```json
{
  "email": "john@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": "If the email exists, a password reset link has been sent"
}
```

### **12. Validate Token**
**POST** `/api/users/validate-token`

Validates if a JWT token is still valid.

**Request Body:**
```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": "Token is valid"
}
```

### **13. Get Users by Role**
**GET** `/api/users/role/{role}`

Retrieves all users with a specific role (Admin/Manager only).

**Headers:**
```
Authorization: Bearer <jwt-token>
```

**Example:**
```
GET /api/users/role/Cashier
```

### **14. Get Active Users**
**GET** `/api/users/active`

Retrieves all active users (Admin/Manager only).

**Headers:**
```
Authorization: Bearer <jwt-token>
```

## 🔒 **Security Features**

### **1. Account Lockout**
- Account is locked after 5 failed login attempts
- Lock duration: 15 minutes
- Failed attempts are reset on successful login

### **2. Password Security**
- Passwords are hashed using SHA256
- Strong password requirements enforced
- Password change tracking

### **3. Session Management**
- JWT tokens with 24-hour expiration
- Multiple session support
- IP address and user agent tracking
- Session cleanup for expired tokens

### **4. Role-Based Access Control**
- **Admin**: Full access to all endpoints
- **Manager**: User management, cannot delete users
- **Cashier**: Basic operations
- **User**: Limited access

## 🛡️ **Error Handling**

### **Common Error Responses:**

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Registration data is required",
  "data": null
}
```

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "Access denied",
  "data": null
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "User not found",
  "data": null
}
```

## 📱 **Usage Examples**

### **Complete Authentication Flow:**

1. **Register User:**
```bash
curl -X POST "https://localhost:44376/api/users/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "cashier1",
    "email": "cashier1@store.com",
    "password": "CashierPass123!",
    "confirmPassword": "CashierPass123!",
    "firstName": "Jane",
    "lastName": "Cashier",
    "role": "Cashier"
  }'
```

2. **Login:**
```bash
curl -X POST "https://localhost:44376/api/users/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "cashier1",
    "password": "CashierPass123!"
  }'
```

3. **Use Protected Endpoint:**
```bash
curl -X GET "https://localhost:44376/api/users/me" \
  -H "Authorization: Bearer <jwt-token-from-login>"
```

4. **Logout:**
```bash
curl -X POST "https://localhost:44376/api/users/logout" \
  -H "Authorization: Bearer <jwt-token>"
```

## ⚙️ **Configuration**

### **JWT Settings in appsettings.json:**
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKey123!@#",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers"
  }
}
```

### **Required NuGet Packages:**
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`

## 🔍 **Testing Scenarios**

### **1. Authentication Tests**
- Register with valid data
- Register with duplicate username/email
- Register with weak password
- Login with correct credentials
- Login with incorrect credentials
- Login with locked account

### **2. Authorization Tests**
- Access protected endpoints without token
- Access admin endpoints with non-admin role
- Access manager endpoints with user role

### **3. Session Tests**
- Multiple login sessions
- Token expiration
- Logout single session
- Logout all sessions

### **4. User Management Tests**
- Create, read, update, delete users
- Filter users by various criteria
- Pagination functionality
- Role-based access control
