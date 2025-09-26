using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DentaQuestTestHarness
{
    public static class PfxCertificateLoader
    {
        /// <summary>
        /// Loads client certificate from PFX file
        /// </summary>
        public static X509Certificate2 LoadClientCertificateFromPfx(string pfxFilePath, string password = "")
        {
            if (!File.Exists(pfxFilePath))
            {
                throw new FileNotFoundException($"Client PFX file not found: {pfxFilePath}");
            }

            try
            {
                var certificate = new X509Certificate2(pfxFilePath, password,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                Console.WriteLine($"✓ Client certificate loaded from PFX: {certificate.Subject}");
                Console.WriteLine($"  Thumbprint: {certificate.Thumbprint}");
                Console.WriteLine($"  Valid from: {certificate.NotBefore} to {certificate.NotAfter}");
                Console.WriteLine($"  Has private key: {certificate.HasPrivateKey}");

                if (!certificate.HasPrivateKey)
                {
                    throw new Exception("Client certificate PFX must contain private key");
                }

                return certificate;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load client certificate from PFX: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads server certificate from PFX or CRT file
        /// </summary>
        public static X509Certificate2 LoadServerCertificateFromFile(string filePath, string password = "")
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Server certificate file not found: {filePath}");
            }

            try
            {
                X509Certificate2 certificate;

                if (filePath.ToLower().EndsWith(".pfx") || filePath.ToLower().EndsWith(".p12"))
                {
                    // Load from PFX
                    certificate = new X509Certificate2(filePath, password, X509KeyStorageFlags.MachineKeySet);
                }
                else
                {
                    // Load from CRT/PEM file
                    certificate = new X509Certificate2(filePath);
                }

                Console.WriteLine($"✓ Server certificate loaded from file: {certificate.Subject}");
                Console.WriteLine($"  Thumbprint: {certificate.Thumbprint}");
                Console.WriteLine($"  Valid from: {certificate.NotBefore} to {certificate.NotAfter}");

                return certificate;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load server certificate from file: {ex.Message}");
            }
        }
    }
}