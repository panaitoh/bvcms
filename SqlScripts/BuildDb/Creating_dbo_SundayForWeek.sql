CREATE FUNCTION [dbo].[SundayForWeek](@year INT, @week INT)
RETURNS datetime
AS
BEGIN

DECLARE @dt1 DATETIME = DATEFROMPARTS(@year, 1, 1)
DECLARE @dt DATETIME 
SELECT @dt = DATEADD(dd,-1,DATEADD(wk, DATEDIFF(wk,0,dateadd(dd,7-datepart(day,@dt1),@dt1)), 0))

SELECT @dt = DATEADD(ww, @week - 1, @dt) -- sunday for week number

	-- Return the result of the function
	RETURN @dt

END
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
