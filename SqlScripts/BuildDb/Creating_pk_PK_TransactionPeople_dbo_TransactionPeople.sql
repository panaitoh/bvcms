ALTER TABLE [dbo].[TransactionPeople] ADD CONSTRAINT [PK_TransactionPeople] PRIMARY KEY CLUSTERED  ([Id], [PeopleId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
