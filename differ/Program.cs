using BsDiff;
using System.Security.Cryptography;

static string CalculateSha256(string filePath)
{
    using var stream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();

    byte[] hash = sha256.ComputeHash(stream);

    return Convert.ToHexString(hash).ToLowerInvariant();
}

if (!File.Exists("menu_0.tpf.dcx"))
{
  Console.WriteLine("File \"menu_0.tpf.dcx\" does not exist.");
  return 1;
}
byte[] ogBytes = File.ReadAllBytes("menu_0.tpf.dcx");
Console.WriteLine($"menu_0.tpf.dcx -> Sha256: {CalculateSha256("menu_0.tpf.dcx")}");

if (!File.Exists("menu_0_ap.tpf.dcx"))
{
  Console.WriteLine("File \"menu_0_ap.tpf.dcx\" does not exist.");
  return 1;
}
byte[] modedBytes = File.ReadAllBytes("menu_0_ap.tpf.dcx");
Console.WriteLine($"menu_0_ap.tpf.dcx -> Sha256: {CalculateSha256("menu_0_ap.tpf.dcx")}");

Console.WriteLine("Creating patch...");
using var patchStream = File.Create("ap_logo.patch");
BinaryPatch.Create(ogBytes, modedBytes, patchStream);
patchStream.Close();

Console.WriteLine($"ap_logo.patch -> Sha256: {CalculateSha256("ap_logo.patch")}");

Console.WriteLine("Done.");
return 0;