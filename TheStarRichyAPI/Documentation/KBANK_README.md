# Kbank Payment API Integration

## Overview
This integration provides QR Payment functionality using Kasikorn Bank (Kbank) API for The Star Richy project.

## Features
- ✅ OAuth2 Token Authentication with auto-refresh
- ✅ Create QR Payment (Thai QR/PromptPay)
- ✅ Payment Inquiry (Check payment status)
- ✅ Cancel Payment
- ✅ Void Payment
- ✅ Settlement Information

## Configuration

### 1. Update `appsettings.json`
Replace the placeholder values with your actual Kbank credentials:

```json
"Kbank": {
  "BaseUrl": "https://openapi-sandbox.kasikornbank.com",
  "ConsumerKey": "YOUR_ACTUAL_CONSUMER_KEY",
  "ConsumerSecret": "YOUR_ACTUAL_CONSUMER_SECRET",
  "PartnerId": "YOUR_ACTUAL_PARTNER_ID",
  "PartnerSecret": "YOUR_ACTUAL_PARTNER_SECRET",
  "MerchantId": "YOUR_ACTUAL_MERCHANT_ID",
  "TerminalId": "YOUR_ACTUAL_TERMINAL_ID"
}
```

**Note:** For production, change `BaseUrl` to: `https://openapi.kasikornbank.com`

### 2. Credentials from Kbank
You need to obtain these credentials from Kbank API Portal:
- Go to: https://apiportal.kasikornbank.com
- Register your application
- Get Consumer Key & Secret
- Setup Partner credentials
- Get Merchant ID and Terminal ID

## API Endpoints

### Base URL
```
/api/KbankPayment
```

### 1. Create QR Payment
**POST** `/api/KbankPayment/qr/create`

**Request Body:**
```json
{
  "amount": 100.50,
  "reference1": "INV001",
  "reference2": "Customer123",
  "reference3": null,
  "reference4": null,
  "metadata": "Additional info"
}
```

**Response:**
```json
{
  "partnerTxnUid": "PTR202510061530123",
  "partnerId": "PTR0000001",
  "statusCode": "00",
  "errorCode": null,
  "errorDesc": null,
  "accountName": "Kasikorn API",
  "qrCode": "00020101021230810016A000000677...",
  "sof": ["PP"]
}
```

**QR Code Usage:**
- Display the `qrCode` string as a QR code image
- Customer scans with mobile banking app
- Payment is processed automatically

### 2. Inquiry Payment Status
**POST** `/api/KbankPayment/qr/inquiry`

**Request Body:**
```json
{
  "origPartnerTxnUid": "PTR202510061530123"
}
```

**Response:**
```json
{
  "partnerTxnUid": "PTR202510061531456",
  "partnerId": "PTR0000001",
  "statusCode": "00",
  "txnStatus": "PAID",
  "txnAmount": "100.50",
  "reference1": "INV001",
  "channel": "kplus"
}
```

**Transaction Status Values:**
- `REQUESTED` - QR created, waiting for payment
- `PAID` - Payment completed successfully
- `CANCELLED` - Payment cancelled
- `EXPIRED` - QR code expired

### 3. Cancel Payment
**POST** `/api/KbankPayment/qr/cancel`

**Request Body:**
```json
{
  "origPartnerTxnUid": "PTR202510061530123"
}
```

### 4. Void Payment
**POST** `/api/KbankPayment/qr/void`

**Request Body:**
```json
{
  "origPartnerTxnUid": "PTR202510061530123"
}
```

### 5. Get Settlement
**POST** `/api/KbankPayment/qr/settlement`

**Response:**
```json
{
  "partnerTxnUid": "PTR202510061532789",
  "partnerId": "PTR0000001",
  "statusCode": "00",
  "accountNo": "xxx-2-04553-x",
  "accountName": "Your Company Name",
  "settlementAmount": 15000,
  "settlementCurrencyCode": "THB"
}
```

## Implementation Examples

### Example 1: Simple Payment Flow

```csharp
// In your service or controller
private readonly IKbankQrPaymentService _qrPaymentService;

public async Task<IActionResult> CreatePayment(decimal amount, string invoiceNo)
{
    try
    {
        // 1. Create QR Payment
        var request = new QrPaymentRequest
        {
            PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
            TxnAmount = amount,
            Reference1 = invoiceNo
        };

        var qrResult = await _qrPaymentService.CreateQrPaymentAsync(request);
        
        // 2. Store transaction in database
        await SaveTransactionToDb(qrResult.PartnerTxnUid, invoiceNo, amount);
        
        // 3. Return QR code to display
        return Ok(new { 
            transactionId = qrResult.PartnerTxnUid,
            qrCode = qrResult.QrCode 
        });
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

### Example 2: Check Payment Status

```csharp
public async Task<bool> CheckPaymentStatus(string transactionId)
{
    var inquiryRequest = new QrInquiryRequest
    {
        PartnerTxnUid = _qrPaymentService.GeneratePartnerTxnUid(),
        OrigPartnerTxnUid = transactionId
    };

    var result = await _qrPaymentService.InquiryPaymentAsync(inquiryRequest);
    
    return result.TxnStatus == "PAID";
}
```

### Example 3: Periodic Status Check (Background Job)

```csharp
// Check payment status every 30 seconds for unpaid transactions
public async Task CheckPendingPayments()
{
    var pendingTransactions = await GetPendingTransactionsFromDb();
    
    foreach (var txn in pendingTransactions)
    {
        var status = await CheckPaymentStatus(txn.PartnerTxnUid);
        
        if (status)
        {
            // Payment successful
            await UpdateTransactionStatus(txn.Id, "PAID");
            await ProcessOrder(txn.OrderId);
        }
    }
}
```

## Architecture

### Files Created
```
TheStarRichyApi/
├── Models/
│   └── Kbank/
│       ├── KbankSettings.cs           # Configuration model
│       ├── OAuthTokenResponse.cs      # OAuth token response
│       └── QrPaymentModels.cs         # All QR payment models
├── Services/
│   ├── IKbankAuthService.cs           # Auth service interface
│   ├── KbankAuthService.cs            # OAuth authentication
│   ├── IKbankQrPaymentService.cs      # QR service interface
│   └── KbankQrPaymentService.cs       # QR payment operations
└── Controllers/
    └── KbankPaymentController.cs      # Payment API endpoints
```

## Error Handling

### Common Status Codes
- `00` - Success
- `01` - Invalid request
- `02` - Authentication failed
- `03` - Transaction not found
- `04` - Duplicate transaction

### Error Response Example
```json
{
  "statusCode": "01",
  "errorCode": "ERR001",
  "errorDesc": "Invalid transaction amount"
}
```

## Security Notes

1. **Never commit credentials** to source control
2. Use **environment variables** or **Azure Key Vault** for production
3. Enable **HTTPS only** for production
4. Implement **rate limiting** on payment endpoints
5. Log all transactions for **audit trail**
6. Validate amounts and references before creating payments

## Testing

### Sandbox Testing
- Use sandbox URL: `https://openapi-sandbox.kasikornbank.com`
- Test credentials provided by Kbank
- No real money transactions

### Test Flow
1. Create QR payment → Get QR code
2. Use Kbank test mobile app to scan
3. Confirm payment in test app
4. Check status via inquiry endpoint
5. Should see status = "PAID"

## Migration to Production

1. Update `BaseUrl` in `appsettings.json` to production URL
2. Replace all credentials with production values
3. Test thoroughly in production environment
4. Monitor logs for any errors
5. Setup alerting for failed transactions

## Support

For Kbank API issues:
- Documentation: https://docs.claude.com (if available)
- API Portal: https://apiportal.kasikornbank.com
- Contact Kbank API support team

For implementation issues:
- Check logs in Application Insights or your logging system
- Review this README
- Check Kbank API documentation

## Changelog

### 2025-10-06
- ✅ Initial implementation
- ✅ QR Payment support
- ✅ Full CRUD operations (Create, Read, Cancel, Void)
- ✅ Settlement information
- ✅ Auto-refresh OAuth tokens
- ✅ Comprehensive error handling
- ✅ Logging integration

## Future Enhancements

Potential additions:
- [ ] Credit Card payment support
- [ ] Webhook handler for payment notifications
- [ ] Transaction history API
- [ ] Refund support
- [ ] Installment payment
- [ ] Payment link generation
- [ ] Database integration for transaction tracking
- [ ] Admin dashboard for monitoring
