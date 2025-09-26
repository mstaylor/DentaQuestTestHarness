# DentaQuest Test Harness - Ubuntu 22.04 Setup

## Prerequisites for Ubuntu 22.04

### 1. Install .NET SDK (Multiple Methods)

#### Method A: Official Microsoft Repository (Recommended)
```bash
# Update package index
sudo apt update

# Install prerequisites
sudo apt install -y wget apt-transport-https software-properties-common

# Add Microsoft package repository
wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update

# Install .NET SDK
sudo apt install -y dotnet-sdk-6.0

# Verify installation
dotnet --version
```

#### Method B: If Method A fails - Use Snap
```bash
# Remove any broken installation first
sudo apt remove --purge dotnet*
sudo apt autoremove

# Install via Snap
sudo snap install dotnet-sdk --classic --channel=6.0/stable

# Add to PATH (add to ~/.bashrc for permanent)
export DOTNET_ROOT=/snap/dotnet-sdk/current
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# Verify installation
dotnet --version
```

#### Method C: Manual Installation
```bash
# Download and install manually
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --version latest --install-dir $HOME/.dotnet

# Add to PATH
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

# Add to ~/.bashrc for permanent
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc

# Verify installation
dotnet --version
```

#### Troubleshooting the "[/usr/share/dotnet/host/fxr] does not exist" Error
```bash
# Check what's installed
dpkg -l | grep dotnet

# Remove conflicting installations
sudo apt remove --purge dotnet* aspnetcore* netstandard*
sudo apt autoremove

# Remove Microsoft repo if corrupted
sudo rm /etc/apt/sources.list.d/microsoft-prod.list*
sudo apt update

# Clear any cached packages
sudo apt autoclean

# Try installation again with Method A or B
```

### 2. Install OpenSSL (usually already installed)
```bash
# OpenSSL is typically pre-installed, but verify:
openssl version

# If not installed:
sudo apt install -y openssl
```

## Certificate Setup

### 1. Create Certificate Directory
```bash
mkdir -p certificates/
```

### 2. Create Client PFX File
You need to combine YOUR certificate and private key:

```bash
# Convert your client certificate and private key to PFX
openssl pkcs12 -export -out certificates/client.pfx \
    -inkey your_client_private.key \
    -in your_client_certificate.crt \
    -password pass:your_password
```

### 3. Copy DentaQuest Server Certificates
```bash
# Copy DentaQuest test server certificate
cp dentaquest_test_server.crt certificates/dentaquest_test.crt

# Copy DentaQuest prod server certificate
cp dentaquest_prod_server.crt certificates/dentaquest_prod.crt
```

### 4. Update Certificate Paths in Code
Edit `Program.cs` and update these paths:
```csharp
private readonly string clientPfxPath = "certificates/client.pfx";
private readonly string clientPfxPassword = "your_password";
private readonly string testServerCertPath = "certificates/dentaquest_test.crt";
private readonly string prodServerCertPath = "certificates/dentaquest_prod.crt";
```

## Directory Structure
```
DentaQuestTestHarness/
├── Program.cs
├── PfxCertificateLoader.cs
├── TestTransactions.cs
├── DentaQuestTestHarness.csproj
└── certificates/
    ├── client.pfx              (YOUR cert + private key)
    ├── dentaquest_test.crt     (DentaQuest test server cert)
    └── dentaquest_prod.crt     (DentaQuest prod server cert)
```

## Build and Run
```bash
# Build
dotnet build

# Run
dotnet run
```

## Security Notes

### 1. Protect PFX Files
```bash
# Set restrictive permissions
chmod 600 certificates/client.pfx
chmod 644 certificates/*.crt
```

### 2. Environment Variables for Passwords
Instead of hardcoding passwords, use environment variables:

```bash
export CLIENT_PFX_PASSWORD="your_secure_password"
dotnet run
```

Update code to read from environment:
```csharp
private readonly string clientPfxPassword = Environment.GetEnvironmentVariable("CLIENT_PFX_PASSWORD") ?? "";
```

### 3. AWS Secrets Manager (Production)
For production, store certificates in AWS Secrets Manager:

```bash
# Store client certificate
aws secretsmanager create-secret \
    --name "dentaquest/client-certificate" \
    --secret-binary fileb://certificates/client.pfx

# Store password
aws secretsmanager create-secret \
    --name "dentaquest/client-password" \
    --secret-string "your_password"
```

## Troubleshooting

### Certificate Loading Issues
```bash
# Test certificate loading
openssl pkcs12 -info -in certificates/client.pfx -noout
openssl x509 -in certificates/dentaquest_test.crt -text -noout
```

### Network Connectivity
```bash
# Test connectivity to DentaQuest
curl -v https://editest.dentaquest.com/TestEdiWcfRealTime/EdiWcfRealTime.svc
```

### Permission Issues
```bash
# Fix file permissions
sudo chown $USER:$USER certificates/*
chmod 600 certificates/client.pfx
```

## Sample Certificate Creation (for testing only)

If you need to create test certificates:

```bash
# Create client certificate with private key
openssl req -x509 -newkey rsa:2048 -keyout client.key -out client.crt -days 365 -nodes \
    -subj "/CN=bcbsmRealTimeTest/O=YourCompany/C=US"

# Convert to PFX
openssl pkcs12 -export -out certificates/client.pfx -inkey client.key -in client.crt -password pass:test123

# Create test server certificate
openssl req -x509 -newkey rsa:2048 -keyout server.key -out certificates/dentaquest_test.crt -days 365 -nodes \
    -subj "/CN=editest.dentaquest.com/O=DentaQuest/C=US"
```

**Note:** These are self-signed certificates for testing only. DentaQuest will provide the actual certificates.