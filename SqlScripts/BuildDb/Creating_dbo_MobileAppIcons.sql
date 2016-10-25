CREATE TABLE [dbo].[MobileAppIcons]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[setID] [int] NOT NULL CONSTRAINT [DF_MobileAppIcons_setID] DEFAULT ((0)),
[actionID] [int] NOT NULL CONSTRAINT [DF_MobileAppIcons_type] DEFAULT ((0)),
[url] [nvarchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_MobileAppIcons_url] DEFAULT ('')
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
