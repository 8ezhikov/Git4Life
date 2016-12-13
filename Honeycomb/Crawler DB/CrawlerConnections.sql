CREATE TABLE [dbo].[CrawlerConnections]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ConnectionTime] DATETIME NOT NULL, 
    [CrawlerName] NVARCHAR(MAX) NOT NULL, 
    [CrawlerIP] NVARCHAR(20) NOT NULL
)
