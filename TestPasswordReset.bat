@echo off
echo === Testing Password Reset System ===
echo Base URL: https://localhost:44376
echo Test Email: admin1233@gmail.com
echo.

echo Step 1: Requesting Password Reset...
curl -X POST "https://localhost:44376/api/password-reset/forgot" ^
  -H "Content-Type: application/json" ^
  -d "{\"email\": \"admin1233@gmail.com\"}" ^
  -k

echo.
echo.
echo Step 2: Testing with invalid email...
curl -X POST "https://localhost:44376/api/password-reset/forgot" ^
  -H "Content-Type: application/json" ^
  -d "{\"email\": \"nonexistent@example.com\"}" ^
  -k

echo.
echo.
echo Step 3: Testing validation endpoint...
curl -X POST "https://localhost:44376/api/password-reset/validate-token" ^
  -H "Content-Type: application/json" ^
  -d "{\"token\": \"invalid-token\"}" ^
  -k

echo.
echo.
echo === Password Reset Test Complete ===
echo.
echo To test the full flow:
echo 1. Copy the resetToken from Step 1 response
echo 2. Use it in the reset password endpoint:
echo    curl -X POST "https://localhost:44376/api/password-reset/reset" ^
echo      -H "Content-Type: application/json" ^
echo      -d "{\"token\": \"YOUR_TOKEN_HERE\", \"newPassword\": \"NewPass123!\", \"confirmPassword\": \"NewPass123!\"}" ^
echo      -k
echo.
echo 3. Test login with new password:
echo    curl -X POST "https://localhost:44376/api/users/login" ^
echo      -H "Content-Type: application/json" ^
echo      -d "{\"username\": \"admin2\", \"password\": \"NewPass123!\"}" ^
echo      -k
echo.
pause

