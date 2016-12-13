CREATE TABLE [dbo].[InternalLinks]
(
	PageId int not NULL PRIMARY KEY IDENTITY, 
	PageIdSeedSpecific int not NULL, 
	PageLink nvarchar(MAX) NOT NULL,
	PageSeedLink nvarchar(MAX) NOT NULL,
	OriginalPageLink nvarchar(MAX) NOT NULL,
	PageLevel int NOT NULL,
	IsProcessed bit DEFAULT 0 NOT NULL,
	IsAvailable bit NOT NULL,
	IsHtml bit NOT NULL,
	LinkCount int NOT NULL DEFAULT 0, 
		
)
