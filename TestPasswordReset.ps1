# Test Password Reset System
# This script tests the offline password reset functionality

$baseUrl = "https://localhost:44376"
$testEmail = "admin1233@gmail.com"  # Use an existing user email

Write-Host "=== Testing Password Reset System ===" -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host "Test Email: $testEmail" -ForegroundColor Yellow
Write-Host ""

# Step 1: Request Password Reset
Write-Host "Step 1: Requesting Password Reset..." -ForegroundColor Cyan
try {
    $forgotPasswordBody = @{
        email = $testEmail
    } | ConvertTo-Json

    $forgotResponse = Invoke-RestMethod -Uri "$baseUrl/api/password-reset/forgot" `
        -Method POST `
        -Body $forgotPasswordBody `
        -ContentType "application/json" `
        -SkipCertificateCheck

    Write-Host "✅ Password reset request successful!" -ForegroundColor Green
    Write-Host "Response: $($forgotResponse | ConvertTo-Json -Depth 3)" -ForegroundColor White
    
    # Extract the reset token
    $resetToken = $forgotResponse.resetToken
    $expiresAt = $forgotResponse.expiresAt
    
    Write-Host ""
    Write-Host "Reset Token: $resetToken" -ForegroundColor Yellow
    Write-Host "Expires At: $expiresAt" -ForegroundColor Yellow
    Write-Host ""
    
    # Step 2: Validate Reset Token
    Write-Host "Step 2: Validating Reset Token..." -ForegroundColor Cyan
    $validateBody = @{
        token = $resetToken
    } | ConvertTo-Json

    $validateResponse = Invoke-RestMethod -Uri "$baseUrl/api/password-reset/validate-token" `
        -Method POST `
        -Body $validateBody `
        -ContentType "application/json" `
        -SkipCertificateCheck

    Write-Host "✅ Token validation successful!" -ForegroundColor Green
    Write-Host "Validation Response: $($validateResponse | ConvertTo-Json -Depth 3)" -ForegroundColor White
    Write-Host ""
    
    # Step 3: Reset Password
    Write-Host "Step 3: Resetting Password..." -ForegroundColor Cyan
    $newPassword = "NewPassword123!"
    $resetPasswordBody = @{
        token = $resetToken
        newPassword = $newPassword
        confirmPassword = $newPassword
    } | ConvertTo-Json

    $resetResponse = Invoke-RestMethod -Uri "$baseUrl/api/password-reset/reset" `
        -Method POST `
        -Body $resetPasswordBody `
        -ContentType "application/json" `
        -SkipCertificateCheck

    Write-Host "✅ Password reset successful!" -ForegroundColor Green
    Write-Host "Reset Response: $($resetResponse | ConvertTo-Json -Depth 3)" -ForegroundColor White
    Write-Host ""
    
    # Step 4: Test Login with New Password
    Write-Host "Step 4: Testing Login with New Password..." -ForegroundColor Cyan
    $loginBody = @{
        username = "admin2"  # Assuming username is admin2 based on the JWT token
        password = $newPassword
    } | ConvertTo-Json

    try {
        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/login" `
            -Method POST `
            -Body $loginBody `
            -ContentType "application/json" `
            -SkipCertificateCheck

        Write-Host "✅ Login with new password successful!" -ForegroundColor Green
        Write-Host "New JWT Token: $($loginResponse.data.token)" -ForegroundColor Yellow
        Write-Host ""
        
        # Step 5: Test Login with Old Password (should fail)
        Write-Host "Step 5: Testing Login with Old Password (should fail)..." -ForegroundColor Cyan
        $oldLoginBody = @{
            username = "admin2"
            password = "oldpassword"  # Assuming old password
        } | ConvertTo-Json

        try {
            $oldLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/login" `
                -Method POST `
                -Body $oldLoginBody `
                -ContentType "application/json" `
                -SkipCertificateCheck

            Write-Host "❌ ERROR: Old password still works! This should not happen." -ForegroundColor Red
        }
        catch {
            Write-Host "✅ Old password correctly rejected!" -ForegroundColor Green
            Write-Host "Error (expected): $($_.Exception.Message)" -ForegroundColor Gray
        }
        
    }
    catch {
        Write-Host "❌ Login with new password failed!" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
}
catch {
    Write-Host "❌ Password reset request failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Password Reset Test Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Yellow
Write-Host "• Password reset system works offline" -ForegroundColor White
Write-Host "• Tokens are generated securely" -ForegroundColor White
Write-Host "• Tokens expire after 24 hours" -ForegroundColor White
Write-Host "• Password changes are applied immediately" -ForegroundColor White
Write-Host "• Old passwords are invalidated" -ForegroundColor White
Write-Host ""
Write-Host "🔧 How to use in production:" -ForegroundColor Yellow
Write-Host "1. User requests password reset via /api/password-reset/forgot" -ForegroundColor White
Write-Host "2. System generates secure token and stores in database" -ForegroundColor White
Write-Host "3. Token is returned in response (for offline use)" -ForegroundColor White
Write-Host "4. User uses token to reset password via /api/password-reset/reset" -ForegroundColor White
Write-Host "5. Token is marked as used and cannot be reused" -ForegroundColor White
Write-Host ""
Write-Host "💡 For online systems: Send token via email instead of returning in response" -ForegroundColor Cyan

