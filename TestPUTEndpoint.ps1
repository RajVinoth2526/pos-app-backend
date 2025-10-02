# PowerShell script to test the PUT endpoint with your JSON structure
# Run this script after starting the application

$baseUrl = "http://localhost:5172/api"

Write-Host "Testing PUT Endpoint with your JSON structure..." -ForegroundColor Green

# Your exact JSON structure
$orderJson = @{
    id = "18"
    orderId = "ORD-20250920-0003"
    orderItems = @(
        @{
            product = @{
                id = "1"
                name = "Carrot juice"
                price = 100
                unitType = ""
                unit = ""
                isAvailable = $true
                isPartialAllowed = $false
                description = ""
                stockQuantity = 0
                sku = ""
                barcode = ""
                category = ""
                unitValue = 0
                taxRate = 0
                manufactureDate = "2025-09-20T19:55:14.156Z"
                expiryDate = "2025-09-20T19:55:14.156Z"
                createdAt = "2025-09-20T19:55:14.156Z"
                updatedAt = "2025-09-20T19:55:14.156Z"
            }
            productId = "1"
            name = "Carrot juice"
            price = 100
            quantity = 1
            total = 100
            tax = 0
            discount = 0
        }
    )
    subtotal = 100
    taxAmount = 0
    discountAmount = 0
    totalAmount = 100
    paymentMethod = "Cash"
    customerName = $null
    notes = ""
    createdAt = "2025-09-20T19:41:18.682Z"
    updatedAt = "2025-09-20T19:41:18.682Z"
    cartDate = "2025-09-20"
    isTakeaway = $false
    isDraft = $false
    orderStatus = "completed"
} | ConvertTo-Json -Depth 10

Write-Host "`nTesting PUT with your JSON structure..." -ForegroundColor Yellow
Write-Host "JSON being sent:" -ForegroundColor Cyan
Write-Host $orderJson -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/orders/18" -Method PUT -Body $orderJson -ContentType "application/json"
    Write-Host "✅ PUT request successful!" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 10 | Write-Host -ForegroundColor White
} catch {
    Write-Host "❌ PUT request failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response body: $responseBody" -ForegroundColor Yellow
    }
}

Write-Host "`n🎉 PUT Endpoint Test Complete!" -ForegroundColor Green

