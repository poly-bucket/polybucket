using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class FixPresignedUrlData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration fixes the existing presigned URL data that was incorrectly stored
            // We need to extract the object keys from the stored presigned URLs
            
            // For MinIO, the object key is typically after the bucket name in the path
            // Example: "http://localhost:9000/polybucket-uploads/models/123/file.stl" 
            // should become "models/123/file.stl"
            
            // First, handle double-encoded URLs (the most problematic case)
            migrationBuilder.Sql(@"
                UPDATE ""Models"" 
                SET ""ThumbnailUrl"" = CASE 
                    WHEN ""ThumbnailUrl"" LIKE '%http%3A%' THEN
                        -- Extract object key from double-encoded URL
                        SUBSTRING(""ThumbnailUrl"" FROM POSITION('/models/' IN ""ThumbnailUrl"") + 8)
                    WHEN ""ThumbnailUrl"" LIKE '%http%3A%' THEN
                        -- Extract object key from double-encoded URL (alternative pattern)
                        SUBSTRING(""ThumbnailUrl"" FROM POSITION('/polybucket-uploads/' IN ""ThumbnailUrl"") + 20)
                    ELSE ""ThumbnailUrl""
                END
                WHERE ""ThumbnailUrl"" LIKE '%http%3A%';
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Models"" 
                SET ""FileUrl"" = CASE 
                    WHEN ""FileUrl"" LIKE '%http%3A%' THEN
                        -- Extract object key from double-encoded URL
                        SUBSTRING(""FileUrl"" FROM POSITION('/models/' IN ""FileUrl"") + 8)
                    WHEN ""FileUrl"" LIKE '%http%3A%' THEN
                        -- Extract object key from double-encoded URL (alternative pattern)
                        SUBSTRING(""FileUrl"" FROM POSITION('/polybucket-uploads/' IN ""FileUrl"") + 20)
                    ELSE ""FileUrl""
                END
                WHERE ""FileUrl"" LIKE '%http%3A%';
            ");
            
            // Update Models table - extract object keys from ThumbnailUrl and FileUrl
            migrationBuilder.Sql(@"
                UPDATE ""Models"" 
                SET ""ThumbnailUrl"" = CASE 
                    WHEN ""ThumbnailUrl"" IS NOT NULL AND ""ThumbnailUrl"" LIKE '%/polybucket-uploads/%' THEN
                        SUBSTRING(""ThumbnailUrl"" FROM POSITION('/polybucket-uploads/' IN ""ThumbnailUrl"") + 20)
                    WHEN ""ThumbnailUrl"" IS NOT NULL AND ""ThumbnailUrl"" LIKE '%/models/%' THEN
                        SUBSTRING(""ThumbnailUrl"" FROM POSITION('/models/' IN ""ThumbnailUrl""))
                    ELSE ""ThumbnailUrl""
                END,
                ""FileUrl"" = CASE 
                    WHEN ""FileUrl"" IS NOT NULL AND ""FileUrl"" LIKE '%/polybucket-uploads/%' THEN
                        SUBSTRING(""FileUrl"" FROM POSITION('/polybucket-uploads/' IN ""FileUrl"") + 20)
                    WHEN ""FileUrl"" IS NOT NULL AND ""FileUrl"" LIKE '%/models/%' THEN
                        SUBSTRING(""FileUrl"" FROM POSITION('/models/' IN ""FileUrl""))
                    ELSE ""FileUrl""
                END
                WHERE ""ThumbnailUrl"" LIKE '%/polybucket-uploads/%' 
                   OR ""FileUrl"" LIKE '%/polybucket-uploads/%'
                   OR ""ThumbnailUrl"" LIKE '%/models/%' 
                   OR ""FileUrl"" LIKE '%/models/%';
            ");
            
            // Update ModelVersions table - extract object keys from ThumbnailUrl and FileUrl
            migrationBuilder.Sql(@"
                UPDATE ""ModelVersions"" 
                SET ""ThumbnailUrl"" = CASE 
                    WHEN ""ThumbnailUrl"" IS NOT NULL AND ""ThumbnailUrl"" LIKE '%/polybucket-uploads/%' THEN
                        SUBSTRING(""ThumbnailUrl"" FROM POSITION('/polybucket-uploads/' IN ""ThumbnailUrl"") + 20)
                    WHEN ""ThumbnailUrl"" IS NOT NULL AND ""ThumbnailUrl"" LIKE '%/models/%' THEN
                        SUBSTRING(""ThumbnailUrl"" FROM POSITION('/models/' IN ""ThumbnailUrl""))
                    ELSE ""ThumbnailUrl""
                END,
                ""FileUrl"" = CASE 
                    WHEN ""FileUrl"" IS NOT NULL AND ""FileUrl"" LIKE '%/polybucket-uploads/%' THEN
                        SUBSTRING(""FileUrl"" FROM POSITION('/polybucket-uploads/' IN ""FileUrl"") + 20)
                    WHEN ""FileUrl"" IS NOT NULL AND ""FileUrl"" LIKE '%/models/%' THEN
                        SUBSTRING(""FileUrl"" FROM POSITION('/models/' IN ""FileUrl""))
                    ELSE ""FileUrl""
                END
                WHERE ""ThumbnailUrl"" LIKE '%/polybucket-uploads/%' 
                   OR ""FileUrl"" LIKE '%/polybucket-uploads/%'
                   OR ""ThumbnailUrl"" LIKE '%/models/%' 
                   OR ""FileUrl"" LIKE '%/models/%';
            ");
            
            // Clean up any remaining URLs that might have query parameters
            migrationBuilder.Sql(@"
                UPDATE ""Models"" 
                SET ""ThumbnailUrl"" = SPLIT_PART(""ThumbnailUrl"", '?', 1),
                    ""FileUrl"" = SPLIT_PART(""FileUrl"", '?', 1)
                WHERE ""ThumbnailUrl"" LIKE '%?%' OR ""FileUrl"" LIKE '%?%';
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""ModelVersions"" 
                SET ""ThumbnailUrl"" = SPLIT_PART(""ThumbnailUrl"", '?', 1),
                    ""FileUrl"" = SPLIT_PART(""FileUrl"", '?', 1)
                WHERE ""ThumbnailUrl"" LIKE '%?%' OR ""FileUrl"" LIKE '%?%';
            ");
            
            // Final cleanup: remove any remaining URLs that still look like presigned URLs
            migrationBuilder.Sql(@"
                UPDATE ""Models"" 
                SET ""ThumbnailUrl"" = NULL
                WHERE ""ThumbnailUrl"" LIKE 'http%' OR ""ThumbnailUrl"" LIKE '%?%' OR ""ThumbnailUrl"" LIKE '%X-Amz%';
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Models"" 
                SET ""FileUrl"" = NULL
                WHERE ""FileUrl"" LIKE 'http%' OR ""FileUrl"" LIKE '%?%' OR ""FileUrl"" LIKE '%X-Amz%';
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""ModelVersions"" 
                SET ""ThumbnailUrl"" = NULL
                WHERE ""ThumbnailUrl"" LIKE 'http%' OR ""ThumbnailUrl"" LIKE '%?%' OR ""ThumbnailUrl"" LIKE '%X-Amz%';
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""ModelVersions"" 
                SET ""FileUrl"" = NULL
                WHERE ""FileUrl"" LIKE 'http%' OR ""FileUrl"" LIKE '%?%' OR ""FileUrl"" LIKE '%X-Amz%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration cannot be safely reversed as it modifies data
            // The data would need to be restored from a backup if needed
        }
    }
}
