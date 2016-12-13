CREATE TABLE [dbo].[Seeds]
(
	SeedIndex int not NULL PRIMARY KEY, 
	SeedDomainName nvarchar(MAX) NOT NULL,
	SeedShortName nvarchar(MAX),
	SeedFullName nvarchar(MAX) NOT NULL,
	IsProcessed bit NOT NULL
)
