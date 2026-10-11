using BsDiff;
using System.Security.Cryptography;

static string CalculateSha256(string filePath)
{
    using var stream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();

    byte[] hash = sha256.ComputeHash(stream);

    return Convert.ToHexString(hash).ToLowerInvariant();
}

if (args.Length != 3)
{
  Console.Write("Please use \"differ <vanilla_file> <modified_file> <output_patch>\"");
  return 0;
}

var vanilla = args[0];
var modified = args[1];
var patch = args[2];

if (!File.Exists(vanilla))
{
  Console.WriteLine($"File \"{vanilla}\" does not exist.");
  return 1;
}
byte[] ogBytes = File.ReadAllBytes(vanilla);
Console.WriteLine($"{vanilla} -> Sha256: {CalculateSha256(vanilla)}");

if (!File.Exists(modified))
{
  Console.WriteLine($"File \"{modified}\" does not exist.");
  return 1;
}
byte[] modedBytes = File.ReadAllBytes(modified);
Console.WriteLine($"{modified} -> Sha256: {CalculateSha256(modified)}");

Console.WriteLine("Creating patch...");
using (var patchStream = File.Create(patch)) {
  BinaryPatch.Create(ogBytes, modedBytes, patchStream);
}

Console.WriteLine($"{patch} -> Sha256: {CalculateSha256(patch)}");

Console.WriteLine("Done.");
return 0;