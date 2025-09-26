# DentaQuest EDI Testing Instructions

## Prerequisites Checklist

### 1. Certificates Required
- [ ] **Client Certificate:** `bcbsmRealTimeTest` installed in Personal store
- [ ] **Server Certificate:** `editest.dentaquest.com` installed in Trusted People store
- [ ] **IP Whitelisting:** Your IP address registered with DentaQuest

### 2. Contact DentaQuest EDI Team
- **Email:** `editeam@greatdentalplans.com`
- **Request:** Test certificates and IP whitelisting
- **Include:** Your contact information and test environment details

## Testing Steps

### Step 1: Verify Certificate Installation
```powershell
# Check client certificate
Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Subject -like "*bcbsmRealTimeTest*"}

# Check server certificate
Get-ChildItem -Path Cert:\CurrentUser\TrustedPeople | Where-Object {$_.Subject -like "*editest.dentaquest.com*"}
```

### Step 2: Build and Run
1. Open project in Visual Studio (recommended for .NET Framework)
2. Build the solution: `Build > Build Solution`
3. Run: `Debug > Start Without Debugging` (Ctrl+F5)

### Step 3: Execute Test
1. **Select Transaction Type:**
   - `1` for 270 (Eligibility Inquiry)
   - `2` for 276 (Claim Status Inquiry)

2. **Monitor Output:**
   - Certificate validation messages
   - Service connectivity status
   - EDI transaction details
   - Response from DentaQuest

## Expected Results

### Successful Test
```
DentaQuest EDI WCF Test Harness
==============================
Checking client certificate...
✓ Client certificate found: CN=bcbsmRealTimeTest
  Valid from: [date] to [date]
Checking server certificate...
✓ Server certificate found: CN=editest.dentaquest.com
  Valid from: [date] to [date]
WCF client configured successfully!

Select transaction type:
1. 270 (Eligibility Inquiry)
2. 276 (Claim Status Inquiry)
Enter choice (1 or 2): 1
Using 270 (Eligibility Inquiry) transaction

Sending EDI transaction to: https://editest.dentaquest.com/TestEdiWcfRealTime/EdiWcfRealTime.svc
Transaction length: 456 characters

=== RESPONSE RECEIVED ===
ISA*00*          *00*          *ZZ*DENTAQUEST      *ZZ*TESTTRADER      *210507*1431*^*00501*000000001*0*T*:~
GS*HB*DENTAQUEST*TESTTRADER*20210507*1431*1*X*005010X279A1~
ST*271*0001*005010X279A1~
[...EDI response content...]
==========================
```

### Common Issues

#### Certificate Not Found
```
ERROR: Client certificate 'bcbsmRealTimeTest' not found!
Available client certificates:
  - Subject: CN=OtherCert
```
**Solution:** Request and install proper certificates from DentaQuest

#### Connection Refused
```
Error: Web service https://editest.dentaquest.com/TestEdiWcfRealTime/EdiWcfRealTime.svc unavailable: The remote server returned an error: (403) Forbidden
```
**Solution:** Ensure IP address is whitelisted with DentaQuest

#### SSL/TLS Errors
```
Error: The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel
```
**Solution:** Verify server certificate installation and DNS identity

## Transaction Types Supported

### 270 (Eligibility Inquiry)
- **Purpose:** Check patient eligibility and benefits
- **Response:** 271 (Eligibility Response)

### 276 (Claim Status Inquiry)
- **Purpose:** Check status of submitted claims
- **Response:** 277 (Claim Status Response)

## Production Testing

**Production URL:** `https://ediprod.dentaquest.com/EdiWcfRealTime/EdiWcfRealTime.svc`

**Requirements:**
- Production certificates (not self-signed)
- Production IP whitelisting
- Real member/claim data for testing

## Support

For issues:
1. Check certificate installation
2. Verify IP whitelisting
3. Contact DentaQuest EDI team: `editeam@greatdentalplans.com`