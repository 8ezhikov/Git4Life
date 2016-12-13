-- =============================================
-- Script Template
-- =============================================
--delete  from [Crawler DB].[dbo].[Crawler Settings]
--delete  from [Crawler DB].[dbo].[SeedList]
--delete  from [Crawler DB].[dbo].[InteranalLinks]
--delete  from [Crawler DB].[dbo].[OuterLinks]
--delete  from [Crawler DB].[dbo].[BadLinks]
delete  from [Crawler DB].[dbo].[Crawler Settings]
--Truncate table [Crawler DB].[dbo].[Seeds]
Truncate table [Crawler DB].[dbo].[InternalLinks]
Truncate table [Crawler DB].[dbo].[ExternalLinks]
Truncate table [Crawler DB].[dbo].[BadLinks]

INSERT INTO [Crawler Settings]( MaxLevel) VALUES (5);

--INSERT INTO Seeds(SeedDomainName, IsProcessed, SeedFullName, SeedIndex) VALUES ('http://mathem.krc.karelia.ru/section.php?plang=r&id=589/',0, N'Карелия', 123);
--INSERT INTO SeedList(SeedDomainName, IsProcessed, SeedFullName, SeedIndex) VALUES ('http://www.gao.spb.ru/russian/index.html',0, N'Карелия', 123);
