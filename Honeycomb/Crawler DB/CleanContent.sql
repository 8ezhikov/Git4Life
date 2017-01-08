CREATE PROCEDURE [dbo].[CleanContent]
as
	--delete  from [Crawler DB].[dbo].[Crawler Settings]
--delete  from [Crawler DB].[dbo].[SeedList]
--delete  from [Crawler DB].[dbo].[InteranalLinks]
--delete  from [Crawler DB].[dbo].[OuterLinks]
--delete  from [Crawler DB].[dbo].[BadLinks]
--Truncate table [Crawler DB].[dbo].[Seeds]
delete  from InternalLinks
delete  from ExternalLinks
delete  from BadLinks
delete  from Batches
delete  from CrawlerConnections
RETURN 0
