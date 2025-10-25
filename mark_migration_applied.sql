-- Script to manually mark the InitialCreate migration as applied
-- This is needed because the AiChatSessions table already exists

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251024085709_InitialCreate', N'9.0.9');