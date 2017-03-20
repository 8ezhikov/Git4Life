CREATE TABLE [dbo].[Batches]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [CrawlerConnectionId] UNIQUEIDENTIFIER NOT NULL, 
    [StartTime] DATETIME NOT NULL, 
    [CrawlingTime] INT NOT NULL, 
    [NumberOfCrawledInternalLinks] INT NOT NULL, 
	[NumberOfCrawledExternalLinks] INT NOT NULL, 
    [SeedIds] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_Batches_ToCrawlerConnection] FOREIGN KEY ([CrawlerConnectionId]) REFERENCES [CrawlerConnections]([Id]) ON DELETE CASCADE, 
  
)
