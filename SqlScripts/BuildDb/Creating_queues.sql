CREATE QUEUE [dbo].[UpdateAttendStrQueue] 
WITH STATUS=ON, 
RETENTION=OFF,
POISON_MESSAGE_HANDLING (STATUS=ON), 
ACTIVATION (
STATUS=ON, 
PROCEDURE_NAME=[dbo].[UpdateAttendStrProc], 
MAX_QUEUE_READERS=3, 
EXECUTE AS OWNER
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
