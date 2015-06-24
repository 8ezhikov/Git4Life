CREATE TABLE [dbo].[InternalLinks]
(
	PageId int not NULL PRIMARY KEY IDENTITY, 
	PageIdSeedSpecific int not NULL, 
	PageLink nvarchar(MAX) NOT NULL,
	PageSeedLink nvarchar(MAX) NOT NULL,
	OriginalPageLink nvarchar(MAX) NOT NULL,
	PageLevel int NOT NULL,
	IsProcessed bit DEFAULT 0,
	IsAvailable bit,
	IsHtml bit,
	LinkCount int NULL DEFAULT 0, 
		
)
