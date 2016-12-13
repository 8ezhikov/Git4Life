CREATE TABLE [dbo].[ExternalLinks]
(
	LinkId int not NULL PRIMARY KEY IDENTITY, 
	LinkPath nvarchar(MAX) NOT NULL,
	LinkAnchor nvarchar(MAX) NOT NULL,
	PageSeedLink nvarchar(MAX) NOT NULL,
	OriginalPageLink nvarchar(MAX) NOT NULL,
	LinkWeight float NULL,
	OriginalPageLevel int not NULL,
	LinkCount int NULL DEFAULT 0	
)
