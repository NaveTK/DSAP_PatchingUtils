using BsDiff;
using System.Security.Cryptography;

static string CalculateSha256(string filePath)
{
    using var stream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();

    byte[] hash = sha256.ComputeHash(stream);

    return Convert.ToHexString(hash).ToLowerInvariant();
}

static int Error(string msg)
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine(msg);
  Console.ResetColor();
  return Exit(1);
}

static void Success(string msg)
{
  Console.ForegroundColor = ConsoleColor.Green;
  Console.WriteLine(msg);
  Console.ResetColor();
}

static ConsoleKeyInfo Question(string msg)
{
  Console.ForegroundColor = ConsoleColor.Yellow;
  Console.Write(msg);
  Console.ResetColor();
  var resp = Console.ReadKey();
  Console.WriteLine();
  return resp;
}

static int Exit(int errorCode)
{
  Console.ForegroundColor = ConsoleColor.DarkGray;
  Console.Write("Press any key to exit.");
  Console.ResetColor();
  Console.ReadKey();
  return errorCode;
}

const string VanillaSha256 = "d4513776f3927420a675d9145d1c645ff409e6f8fc9c9b11c06b78cb1f2623ab";
const string PatchedSha256 = "db80026726cc901dc5344560638fec3de021f9f523e8f77f749cc8939fd47b16";
const string PatchSha256   = "ac5445e7f4d38e2d62c031288b1162d95e1040bcc8f0587da54c1e0a000af3eb";

const string vanilla = "menu/menu_0.tpf.dcx";
const string backup = "menu/menu_0.tpf.dcx.apbak";
const string tmpPatch = "menu/menu_0.tpf.dcx.tmp";
const string patch = "ap_logo.patch";

if (!File.Exists(vanilla))
  return Error("Could not locate assets. Make sure this script is run from the base Dark Souls Remastered installation directory.");

Console.WriteLine("Checking present assets...");
var existingSha256 = CalculateSha256(vanilla);

if (existingSha256 == VanillaSha256)
{
  Console.WriteLine("AP Logo has not been patched into the game yet.");
  if (!File.Exists(patch))
    return Error("Could not locate patch file. Make sure the patch file is present in the same directory as this script.");

  var resp = Question("Do you want to patch the AP logo into the game? (Y/n): ");
  if (resp.KeyChar == 'N' || resp.KeyChar == 'n')
  {
    Console.WriteLine("Canceled patching process.");
    return Exit(0);
  }
  else
  {
    Console.WriteLine("Checking patch file...");
    var existingPatchSha256 = CalculateSha256(patch);
    if (existingPatchSha256 != PatchSha256)
      return Error("Invalid patch file. Please ensure you downloaded the patch file from the official repo.");
    
    if (!File.Exists(backup))
    {
      Console.WriteLine("Creating backup...");
      File.Copy(vanilla, backup);
    }
    else
    {
      Console.WriteLine("Backup already exists. Verifying...");

      var backupSha256 = CalculateSha256(backup);

      if (backupSha256 != VanillaSha256)
      {
        Console.WriteLine("Invalid Backup. Recreating...");
        File.Copy(vanilla, backup, true);
      }
    }

    Console.WriteLine("Applying patch...");
    using (var original = File.OpenRead(vanilla)){
      using var patched = File.Create(tmpPatch);
      BinaryPatch.Apply(
          original,
          () => File.OpenRead(patch),
          patched
      );
    }
    Console.WriteLine("Verifying patch...");
    var tmpSha256 = CalculateSha256(tmpPatch);
    if (tmpSha256 != PatchedSha256)
    {
      File.Delete(tmpPatch);
      return Error("Aborting. Patching process failed. Game files have not been modified.");
    }

    File.Copy(tmpPatch, vanilla, true);
    File.Delete(tmpPatch);

    Success("Done. AP logo has been patched into the game.");
    return Exit(0);
  }
}
else if (existingSha256 == PatchedSha256)
{
  Console.WriteLine("AP Logo has already been patched into the game.");
  var resp = Question("Do you want to revert the patch? (y/N): ");
  if (resp.KeyChar == 'Y' || resp.KeyChar == 'y')
  {
    if (!File.Exists(backup))
      return Error("Backup not found. Please use the \"Verify integrity of game files\" function on Steam to restore the original assets.");

    Console.WriteLine("Verifying backup...");
    var backupSha256 = CalculateSha256(backup);

    if (backupSha256 != VanillaSha256)
      return Error("Backup does not match vanilla asset. Please use the \"Verify integrity of game files\" function on Steam instead.");

    Console.WriteLine("Restoring backup...");
    File.Copy(backup, vanilla, true);
    File.Delete(backup);

    Success("Done. The AP logo has been removed.");
    return Exit(0);
  }
  else
  {
    Success("Nothing to do.");
    return Exit(0);
  }
}
else
{
  return Error("Existing file has already been modified by an external application. Please use the \"Verify integrity of game files\" function on Steam to restore the original assets.");
}