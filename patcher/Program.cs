using BsDiff;
using System.Security.Cryptography;

static string CalculateSha256(string filePath)
{
    using var stream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();

    byte[] hash = sha256.ComputeHash(stream);

    return Convert.ToHexString(hash).ToLowerInvariant();
}

static int Error(string err)
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine(err);
  Console.ResetColor();
  return Exit(1);
}

static int Question(string question)
{
  Console.ForegroundColor = ConsoleColor.Yellow;
  Console.Write(question);
  Console.ResetColor();
  return Console.Read();
}

static int Exit(int errorCode)
{
  Console.ForegroundColor = ConsoleColor.Gray;
  Console.WriteLine("Press any key to exit.");
  Console.ResetColor();
  Console.Read();
  return errorCode;
}

const string VanillaSha256 = "d4513776f3927420a675d9145d1c645ff409e6f8fc9c9b11c06b78cb1f2623ab";
const string PatchedSha256 = "42e61fed65ae5be59e4f07f84c92e987182cacde2fc570c2bb7d861699531396";
const string PatchSha256   = "dc31d380acd8938875857df5a0d76dbfa0834695347336dfc36961dd74c06b3c";

if (!File.Exists("menu/menu_0.tpf.dcx"))
  return Error("Could not locate assets. Make sure this script is run from the base Dark Souls Remastered installation directory.");

Console.WriteLine("Checking present assets...");
var existingSha256 = CalculateSha256("menu/menu_0.tpf.dcx");

if (existingSha256 == VanillaSha256)
{
  Console.WriteLine("AP Logo has not been patched into the game yet.");
  if (!File.Exists("ap_logo.patch"))
    return Error("Could not locate patch file. Make sure the patch file is present in the same directory as this script.");

  var resp = Question("Do you want to patch the AP logo into the game? (Y/n): ");
  if (resp == 'N' || resp == 'n')
  {
    Console.WriteLine("Canceled patching process.");
    return Exit(0);
  }
  else
  {
    Console.WriteLine("Checking patch file...");
    var existingPatchSha256 = CalculateSha256("ap_logo.patch");
    if (existingPatchSha256 != PatchSha256)
      return Error("Invalid patch file. Please ensure you downloaded the patch file from the official repo.");
    
    if (!File.Exists("menu/menu_0.tpf.dcx.apbak"))
    {
      Console.WriteLine("Creating backup...");
      File.Copy("menu/menu_0.tpf.dcx", "menu/menu_0.tpf.dcx.apbak");
    }
    else
    {
      Console.WriteLine("Backup already exists.");
    }

    Console.WriteLine("Applying patch...");
    using (var original = File.OpenRead("menu/menu_0.tpf.dcx")){
      using var patched = File.Create("menu/menu_0.tpf.dcx.tmp");
      BinaryPatch.Apply(
          original,
          () => File.OpenRead("ap_logo.patch"),
          patched
      );
    }
    Console.WriteLine("Verifying patch...");
    var tempSha256 = CalculateSha256("menu/menu_0.tpf.dcx.tmp");
    if (tempSha256 != PatchedSha256)
      return Error("Aborting. Patching process failed. Game files have not been modified.");

    File.Copy("menu/menu_0.tpf.dcx.tmp", "menu/menu_0.tpf.dcx", true);
    File.Delete("menu/menu_0.tpf.dcx.tmp");

    Console.WriteLine("Done. AP logo has been patched into the game.");
    return Exit(0);
  }
}
else if (existingSha256 == PatchedSha256)
{
  Console.WriteLine("AP Logo has already been patched into the game.");
  var resp = Question("Do you want to revert the patch? (y/N): ");
  if (resp == 'Y' || resp == 'y')
  {
    if (!File.Exists("menu/menu_0.tpf.dcx.apbak"))
      return Error("Backup not found. Please use the \"Verify integrity of game files\" function on Steam to restore the original assets.");

    Console.WriteLine("Restoring backup...");
    File.Copy("menu/menu_0.tpf.dcx.apbak", "menu/menu_0.tpf.dcx", true);
    Console.WriteLine("Done.");
    return Exit(0);
  }
  else
  {
    Console.WriteLine("Nothing to do.");
    return Exit(0);
  }
}
else
{
  return Error("Existing file has already been modified by an external application. Please use the \"Verify integrity of game files\" function on Steam to restore the original assets.");
}