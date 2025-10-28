# Certificates Directory

Place your certificate files here:

## Required Files:

### 1. Client Certificate (YOUR authentication to DentaQuest)
- **File:** `client.pfx`
- **Contains:** Your certificate + private key
- **Password protected:** Yes
- **How to create:**
  ```bash
  openssl pkcs12 -export -out client.pfx -inkey your_private.key -in your_certificate.crt -password pass:yourpassword
  ```

### 2. DentaQuest Test Server Certificate
- **File:** `dentaquest_test.crt`
- **Contains:** DentaQuest's test server public certificate
- **Purpose:** Verify DentaQuest test server identity
- **Subject should contain:** `editest.dentaquest.com`

### 3. DentaQuest Production Server Certificate
- **File:** `dentaquest_prod.crt`
- **Contains:** DentaQuest's production server public certificate
- **Purpose:** Verify DentaQuest production server identity
- **Subject should contain:** `ediprod.dentaquest.com`

## File Structure:
```
certificates/
├── client.pfx              (Your cert + private key)
├── dentaquest_test.crt     (DentaQuest test server cert)
└── dentaquest_prod.crt     (DentaQuest prod server cert)
```

## Security:
- Set restrictive permissions: `chmod 600 client.pfx`
- Keep PFX password secure
- Don't commit certificates to source control

## Testing Certificate Validity:
```bash
# Test client PFX
openssl pkcs12 -info -in client.pfx -noout

# Test server certificates
openssl x509 -in dentaquest_test.crt -text -noout
openssl x509 -in dentaquest_prod.crt -text -noout
```