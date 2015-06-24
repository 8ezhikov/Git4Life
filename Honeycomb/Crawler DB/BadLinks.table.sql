CREATE TABLE [dbo].[BadLinks]
(
	LinkId int not NULL PRIMARY KEY IDENTITY, 
	LinkPath nvarchar(MAX) NOT NULL,
	OriginalPageLink nvarchar(MAX) NOT NULL
)
