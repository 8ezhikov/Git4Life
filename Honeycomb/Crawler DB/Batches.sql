CREATE TABLE [dbo].[Batches]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [CrawlerConnectionId] UNIQUEIDENTIFIER NOT NULL, 
    [StartTime] DATETIME NOT NULL, 
    [CrawlingTime] INT NOT NULL, 
    [NumberOfCrawledInternalLinks] INT NOT NULL, 
	[NumberOfCrawledExternalLinks] INT NOT NULL, 
    [SeedId] INT NOT NULL, 
    CONSTRAINT [FK_Batches_ToCrawlerConnection] FOREIGN KEY ([CrawlerConnectionId]) REFERENCES [CrawlerConnections]([Id]) ON DELETE CASCADE, 
    CONSTRAINT [FK_Batches_ToSeeds] FOREIGN KEY ([SeedId]) REFERENCES [Seeds]([SeedIndex] ) ON DELETE CASCADE
)
