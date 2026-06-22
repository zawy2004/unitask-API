-- ============================================================
-- FIX: Vietnamese diacritics lost in Location column
-- Root cause: INSERT used 'Hà Nội' (varchar) instead of N'Hà Nội' (nvarchar)
-- SQL Server converted through code page, losing ẵ ộ ồ → ?
-- Run this ONCE to correct existing data.
-- ============================================================

UPDATE [dbo].[Jobs] SET [Location] = N'Hồ Chí Minh'  WHERE [Location] LIKE N'H%Ch%Minh' AND [Location] <> N'Hồ Chí Minh';
UPDATE [dbo].[Jobs] SET [Location] = N'Hà Nội'       WHERE [Location] LIKE N'H%N%i'     AND [Location] <> N'Hà Nội' AND [Location] NOT LIKE N'%Minh%';
UPDATE [dbo].[Jobs] SET [Location] = N'Đà Nẵng'      WHERE [Location] LIKE N'%à N%ng'   AND [Location] <> N'Đà Nẵng';
UPDATE [dbo].[Jobs] SET [Location] = N'TP.HCM'       WHERE [Location] = 'TP.HCM';
UPDATE [dbo].[Jobs] SET [Location] = N'Lâm Đồng'     WHERE [Location] LIKE N'L%m %ng'   AND [Location] <> N'Lâm Đồng';
UPDATE [dbo].[Jobs] SET [Location] = N'Remote'        WHERE [Location] = 'Remote';

-- Also fix BusinessProfiles.Address if affected
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Cầu Giấy, Hà Nội'     WHERE [Address] LIKE N'C%u Gi%y, H%N%i';
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Thủ Đức, TP.HCM'      WHERE [Address] LIKE N'Th%c, TP.HCM';
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Đống Đa, Hà Nội'      WHERE [Address] LIKE N'%ng %a, H%N%i';
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Quận 1, TP.HCM'       WHERE [Address] LIKE N'Qu%n 1, TP.HCM';
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Hoàn Kiếm, Hà Nội'    WHERE [Address] LIKE N'Ho%n Ki%m, H%N%i';
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Bình Thạnh, TP.HCM'   WHERE [Address] LIKE N'B%nh Th%nh, TP.HCM';
UPDATE [dbo].[BusinessProfiles] SET [Address] = N'Quận 7, TP.HCM'       WHERE [Address] LIKE N'Qu%n 7, TP.HCM';

PRINT 'Unicode location fix complete';
