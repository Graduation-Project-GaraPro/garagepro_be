using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ServicePartCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the old table exists before trying to rename it
            var oldTableExists = migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServicePartCategory') SELECT 1 ELSE SELECT 0");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServicePartCategory')
                BEGIN
                    ALTER TABLE [ServicePartCategory] DROP CONSTRAINT [FK_ServicePartCategory_PartCategories_PartCategoryId];
                    ALTER TABLE [ServicePartCategory] DROP CONSTRAINT [FK_ServicePartCategory_Services_ServiceId];
                    ALTER TABLE [ServicePartCategory] DROP CONSTRAINT [PK_ServicePartCategory];
                    
                    EXEC sp_rename 'ServicePartCategory', 'ServicePartCategories';
                    
                    EXEC sp_rename 'ServicePartCategories.IX_ServicePartCategory_ServiceId', 'IX_ServicePartCategories_ServiceId', 'INDEX';
                    EXEC sp_rename 'ServicePartCategories.IX_ServicePartCategory_PartCategoryId', 'IX_ServicePartCategories_PartCategoryId', 'INDEX';
                    
                    ALTER TABLE [ServicePartCategories] ADD CONSTRAINT [PK_ServicePartCategories] PRIMARY KEY ([ServicePartCategoryId]);
                    ALTER TABLE [ServicePartCategories] ADD CONSTRAINT [FK_ServicePartCategories_PartCategories_PartCategoryId] FOREIGN KEY ([PartCategoryId]) REFERENCES [PartCategories] ([LaborCategoryId]) ON DELETE CASCADE;
                    ALTER TABLE [ServicePartCategories] ADD CONSTRAINT [FK_ServicePartCategories_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE;
                END
                ELSE IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServicePartCategories')
                BEGIN
                    CREATE TABLE [ServicePartCategories] (
                        [ServicePartCategoryId] uniqueidentifier NOT NULL,
                        [ServiceId] uniqueidentifier NOT NULL,
                        [PartCategoryId] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ServicePartCategories] PRIMARY KEY ([ServicePartCategoryId]),
                        CONSTRAINT [FK_ServicePartCategories_PartCategories_PartCategoryId] FOREIGN KEY ([PartCategoryId]) REFERENCES [PartCategories] ([LaborCategoryId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ServicePartCategories_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE
                    );
                    
                    CREATE INDEX [IX_ServicePartCategories_ServiceId] ON [ServicePartCategories] ([ServiceId]);
                    CREATE INDEX [IX_ServicePartCategories_PartCategoryId] ON [ServicePartCategories] ([PartCategoryId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServicePartCategories')
                BEGIN
                    ALTER TABLE [ServicePartCategories] DROP CONSTRAINT [FK_ServicePartCategories_PartCategories_PartCategoryId];
                    ALTER TABLE [ServicePartCategories] DROP CONSTRAINT [FK_ServicePartCategories_Services_ServiceId];
                    ALTER TABLE [ServicePartCategories] DROP CONSTRAINT [PK_ServicePartCategories];
                    
                    EXEC sp_rename 'ServicePartCategories', 'ServicePartCategory';
                    
                    EXEC sp_rename 'ServicePartCategory.IX_ServicePartCategories_ServiceId', 'IX_ServicePartCategory_ServiceId', 'INDEX';
                    EXEC sp_rename 'ServicePartCategory.IX_ServicePartCategories_PartCategoryId', 'IX_ServicePartCategory_PartCategoryId', 'INDEX';
                    
                    ALTER TABLE [ServicePartCategory] ADD CONSTRAINT [PK_ServicePartCategory] PRIMARY KEY ([ServicePartCategoryId]);
                    ALTER TABLE [ServicePartCategory] ADD CONSTRAINT [FK_ServicePartCategory_PartCategories_PartCategoryId] FOREIGN KEY ([PartCategoryId]) REFERENCES [PartCategories] ([LaborCategoryId]) ON DELETE CASCADE;
                    ALTER TABLE [ServicePartCategory] ADD CONSTRAINT [FK_ServicePartCategory_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE;
                END
            ");
        }
    }
}
