ALTER TABLE [dbo].[RssFeed] ADD CONSTRAINT [PK_RssFeed] PRIMARY KEY CLUSTERED  ([Url])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
