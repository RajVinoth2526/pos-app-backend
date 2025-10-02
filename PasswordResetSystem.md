# Password Reset System Documentation

## Overview
This system provides a secure, offline-capable password reset functionality for the POS system. It generates cryptographically secure tokens that are stored in the database and can be used to reset user passwords without requiring internet connectivity.

## Features
- ✅ **Offline Operation**: Works without internet connection
- ✅ **Secure Token Generation**: Uses cryptographically secure random tokens
- ✅ **Token Expiration**: Tokens expire after 24 hours
- ✅ **One-time Use**: Tokens are marked as used and cannot be reused
- ✅ **User Validation**: Only works for active users
- ✅ **Automatic Cleanup**: Expired tokens are automatically cleaned up
- ✅ **Audit Trail**: Tracks IP address and user agent (when available)

## Database Schema

### PasswordResetTokens Table
```sql
CREATE TABLE "PasswordResetTokens" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "Token" TEXT NOT NULL UNIQUE,
    "CreatedAt" TEXT NOT NULL,
    "ExpiresAt" TEXT NOT NULL,
    "IsUsed" INTEGER NOT NULL,
    "IpAddress" TEXT NULL,
    "UserAgent" TEXT NULL,
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);
```

## API Endpoints

### 1. Request Password Reset
**POST** `/api/password-reset/forgot`

**Request Body:**
```json
{
    "email": "user@example.com"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Password reset token generated successfully. Use this token to reset your password.",
    "resetToken": "base64-encoded-secure-token",
    "expiresAt": "2024-01-21T10:30:00Z",
    "instructions": {
        "step1": "Copy the resetToken from this response",
        "step2": "Use the resetToken in the reset password endpoint",
        "step3": "The token expires in 24 hours",
        "note": "In a production system, this token would be sent via email"
    }
}
```

### 2. Validate Reset Token
**POST** `/api/password-reset/validate-token`

**Request Body:**
```json
{
    "token": "base64-encoded-secure-token"
}
```

**Response:**
```json
{
    "success": true,
    "data": {
        "message": "Reset token is valid.",
        "expiresAt": "2024-01-21T10:30:00Z",
        "isValid": true
    }
}
```

### 3. Reset Password
**POST** `/api/password-reset/reset`

**Request Body:**
```json
{
    "token": "base64-encoded-secure-token",
    "newPassword": "NewSecurePassword123!",
    "confirmPassword": "NewSecurePassword123!"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Password has been reset successfully. You can now login with your new password."
}
```

### 4. Cleanup Expired Tokens (Admin)
**GET** `/api/password-reset/cleanup`

**Response:**
```json
{
    "success": true,
    "message": "Expired tokens cleaned up successfully"
}
```

### 5. Get User Reset Tokens (Admin)
**GET** `/api/password-reset/user-tokens/{userId}`

**Response:**
```json
{
    "success": true,
    "data": [
        {
            "id": 1,
            "userId": 1,
            "token": "base64-encoded-token",
            "createdAt": "2024-01-20T10:30:00Z",
            "expiresAt": "2024-01-21T10:30:00Z",
            "isUsed": false,
            "ipAddress": null,
            "userAgent": null
        }
    ]
}
```

### 6. Revoke All User Tokens (Admin)
**POST** `/api/password-reset/revoke-all/{userId}`

**Response:**
```json
{
    "success": true,
    "message": "All reset tokens for user have been revoked"
}
```

## Security Features

### Token Generation
- Uses `RandomNumberGenerator.Create()` for cryptographically secure random tokens
- Tokens are 256-bit (32 bytes) encoded as Base64
- URL-safe encoding (replaces `+`, `/`, `=` with `-`, `_`, and removes padding)

### Token Validation
- Tokens must be unique in the database
- Tokens expire after 24 hours
- Tokens can only be used once
- Only active users can reset passwords
- Tokens are automatically invalidated when used

### Password Security
- Minimum 6 characters required
- Passwords are hashed using SHA256 (same as login system)
- Password confirmation required
- Old passwords are immediately invalidated

## Usage Examples

### PowerShell Test Script
```powershell
# Request password reset
$forgotResponse = Invoke-RestMethod -Uri "https://localhost:44376/api/password-reset/forgot" `
    -Method POST `
    -Body '{"email": "user@example.com"}' `
    -ContentType "application/json" `
    -SkipCertificateCheck

# Extract token
$token = $forgotResponse.resetToken

# Reset password
$resetResponse = Invoke-RestMethod -Uri "https://localhost:44376/api/password-reset/reset" `
    -Method POST `
    -Body "{\"token\": \"$token\", \"newPassword\": \"NewPass123!\", \"confirmPassword\": \"NewPass123!\"}" `
    -ContentType "application/json" `
    -SkipCertificateCheck
```

### cURL Examples
```bash
# Request password reset
curl -X POST 'https://localhost:44376/api/password-reset/forgot' \
  -H 'Content-Type: application/json' \
  -d '{"email": "user@example.com"}'

# Reset password
curl -X POST 'https://localhost:44376/api/password-reset/reset' \
  -H 'Content-Type: application/json' \
  -d '{"token": "your-token-here", "newPassword": "NewPass123!", "confirmPassword": "NewPass123!"}'
```

## Error Handling

### Common Error Responses
```json
{
    "success": false,
    "message": "Invalid or expired reset token."
}
```

```json
{
    "success": false,
    "message": "Account is deactivated. Please contact administrator."
}
```

```json
{
    "success": false,
    "message": "Passwords do not match."
}
```

```json
{
    "success": false,
    "message": "Password must be at least 6 characters long"
}
```

## Configuration

### Token Expiration
Default: 24 hours
```csharp
private readonly TimeSpan _tokenExpiry = TimeSpan.FromHours(24);
```

### Password Requirements
- Minimum length: 6 characters
- Must match confirmation password
- Uses same hashing as login system (SHA256)

## Database Maintenance

### Automatic Cleanup
The system includes automatic cleanup of expired tokens:
```csharp
await _passwordResetService.CleanupExpiredTokensAsync();
```

### Manual Cleanup
Admins can manually trigger cleanup:
```bash
curl -X GET 'https://localhost:44376/api/password-reset/cleanup'
```

## Production Considerations

### For Online Systems
1. **Email Integration**: Send reset tokens via email instead of returning in API response
2. **Rate Limiting**: Implement rate limiting to prevent abuse
3. **Audit Logging**: Log all password reset attempts
4. **IP Tracking**: Track IP addresses for security monitoring
5. **User Notification**: Notify users when password is reset

### Security Enhancements
1. **Token Encryption**: Encrypt tokens in database
2. **Additional Validation**: Add CAPTCHA or other anti-bot measures
3. **Account Lockout**: Lock accounts after multiple failed attempts
4. **Password History**: Prevent reuse of recent passwords
5. **Two-Factor Authentication**: Require 2FA for password resets

## Testing

Run the test script to verify functionality:
```powershell
.\TestPasswordReset.ps1
```

The test script will:
1. Request a password reset
2. Validate the token
3. Reset the password
4. Test login with new password
5. Verify old password is invalidated

## Troubleshooting

### Common Issues
1. **Token Not Found**: Check if token is correct and not expired
2. **User Not Found**: Verify email exists in system
3. **Account Inactive**: Ensure user account is active
4. **Token Already Used**: Each token can only be used once

### Debug Information
Enable detailed logging in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "ClientAppPOSWebAPI.Services.PasswordResetService": "Debug"
    }
  }
}
```

## Dependencies

- Entity Framework Core 6.0
- ASP.NET Core 6.0
- System.Security.Cryptography
- Microsoft.Extensions.Logging

## Files Modified/Created

### New Files
- `Models/PasswordReset.cs` - DTOs and models
- `Services/PasswordResetService.cs` - Business logic
- `Controllers/PasswordResetController.cs` - API endpoints
- `Migrations/20250920195000_AddPasswordResetTables.cs` - Database migration
- `TestPasswordReset.ps1` - Test script
- `PasswordResetSystem.md` - This documentation

### Modified Files
- `Data/POSDbContext.cs` - Added PasswordResetTokens DbSet
- `Program.cs` - Registered PasswordResetService
- `Migrations/POSDbContextModelSnapshot.cs` - Updated model snapshot

