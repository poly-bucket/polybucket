# 2FA Data Cleanup After Concurrency Fix

## Issue
After implementing optimistic concurrency with the `Version` column as a concurrency token, existing 2FA records may have incorrect `Version` values (likely 0) which will cause concurrency exceptions.

## Solution Options

### Option 1: Truncate Tables (Recommended for Dev/Test)

If this is development/test data, the simplest solution is to truncate the tables:

```sql
TRUNCATE TABLE "BackupCodes" CASCADE;
TRUNCATE TABLE "TwoFactorAuths" CASCADE;
```

Or via EF Core Migration:
```csharp
// In a migration's Up method
migrationBuilder.Sql("TRUNCATE TABLE \"BackupCodes\" CASCADE;");
migrationBuilder.Sql("TRUNCATE TABLE \"TwoFactorAuths\" CASCADE;");
```

### Option 2: Update Existing Records (For Production)

If you need to preserve existing data, update the Version values:

```sql
-- Update TwoFactorAuths where Version is 0 or NULL
UPDATE "TwoFactorAuths" 
SET "Version" = 1 
WHERE "Version" = 0 OR "Version" IS NULL;

-- Update BackupCodes where Version is 0 or NULL
UPDATE "BackupCodes" 
SET "Version" = 1 
WHERE "Version" = 0 OR "Version" IS NULL;
```

### Option 3: Create a Data Migration

Create a migration to fix existing data:

```bash
cd backend/PolyBucket.Api
dotnet ef migrations add FixTwoFactorAuthVersionValues
```

Then in the migration:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        UPDATE ""TwoFactorAuths"" 
        SET ""Version"" = 1 
        WHERE ""Version"" = 0;
        
        UPDATE ""BackupCodes"" 
        SET ""Version"" = 1 
        WHERE ""Version"" = 0;
    ");
}
```

## Recommendation

Since this is during setup and the admin's 2FA setup was failing, **Option 1 (Truncate)** is recommended. The admin can simply re-initialize 2FA after the code changes are deployed.

## After Cleanup

After truncating or updating, the admin should:
1. Re-initialize 2FA via the setup process
2. Scan the new QR code
3. Enter the verification code
4. Save the backup codes

