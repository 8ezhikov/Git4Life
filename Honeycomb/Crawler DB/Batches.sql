CREATE TABLE [dbo].[Batches]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [CrawlerConnectionId] UNIQUEIDENTIFIER NOT NULL, 
    [StartTime] DATETIME NOT NULL, 
    [CrawlingTime] INT NOT NULL, 
    [NumberOfCrawledLinks] INT NOT NULL, 
    [SeedId] INT NOT NULL, 
    CONSTRAINT [FK_Batches_ToCrawlerConnection] FOREIGN KEY ([CrawlerConnectionId]) REFERENCES [CrawlerConnections]([Id]), 
    CONSTRAINT [FK_Batches_ToSeeds] FOREIGN KEY ([SeedId]) REFERENCES [Seeds]([SeedIndex])
)
