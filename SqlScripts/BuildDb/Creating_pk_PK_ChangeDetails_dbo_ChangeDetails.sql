ALTER TABLE [dbo].[ChangeDetails] ADD CONSTRAINT [PK_ChangeDetails] PRIMARY KEY CLUSTERED  ([Id], [Field])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
