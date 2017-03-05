USE [KKT]
GO

ALTER DATABASE KKT SET TRUSTWORTHY ON
GO

SET NOCOUNT ON

DECLARE @name  NVARCHAR(255)
DECLARE @sql   NVARCHAR(4000)

IF OBJECT_ID('TempDB..#Procedures') IS NOT NULL
    DROP TABLE #Procedures
    
CREATE TABLE #Procedures
(
    NAME NVARCHAR(255) NOT NULL
)

INSERT INTO #Procedures
  (
    NAME
  )
SELECT 'ReadTransportFile' UNION ALL
SELECT 'WriteTransportFile'

DECLARE proceduresCursor CURSOR LOCAL FORWARD_ONLY FAST_FORWARD READ_ONLY 
FOR
    SELECT NAME
    FROM   #Procedures

OPEN proceduresCursor

FETCH NEXT FROM proceduresCursor INTO @name
WHILE @@FETCH_STATUS = 0
BEGIN  
    SET @sql = ''
    SET @sql = @sql + 'IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[exch].[' + @name + ']'') AND type in (N''P'', N''PC''))'
    SET @sql = @sql + 'DROP PROCEDURE [exch].[' + @name + ']'
    
    EXEC (@sql)
    FETCH NEXT FROM proceduresCursor INTO @name
END

CLOSE proceduresCursor
DEALLOCATE proceduresCursor


IF EXISTS (SELECT * FROM sys.assemblies asms WHERE  asms.name = N'ProtoBufTransportSql')
    DROP ASSEMBLY ProtoBufTransportSql
GO

IF EXISTS (SELECT * FROM sys.assemblies asms WHERE  asms.name = N'ProtoBufTransport')
    DROP ASSEMBLY ProtoBufTransport
GO

IF EXISTS (SELECT * FROM sys.assemblies asms WHERE  asms.name = N'ProtoBufData')
    DROP ASSEMBLY ProtoBufData
GO

IF EXISTS (SELECT * FROM sys.assemblies asms WHERE  asms.name = N'ProtoBuf')
    DROP ASSEMBLY ProtoBuf
GO




CREATE ASSEMBLY ProtoBuf from 'C:\git\inet\protobuf-net-transport-sql\Temp\Net40\protobuf-net.dll' WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY ProtoBufData from 'C:\git\inet\protobuf-net-transport-sql\Temp\Net40\protobuf-net-data.dll' WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY ProtoBufTransport from 'C:\git\inet\protobuf-net-transport-sql\Temp\Net40\protobuf-net-transport.dll' WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY ProtoBufTransportSql from 'C:\git\inet\protobuf-net-transport-sql\Transport.Sql.Net40\bin\Debug\protobuf-net-transport-sql.dll' WITH PERMISSION_SET = UNSAFE
GO




CREATE PROCEDURE [exch].[ReadTransportFile]
	@fullFileName [nvarchar](255),
	@tableName [nvarchar](255),
	@userName [nvarchar](255),
	@password [nvarchar](255)
WITH 
 EXECUTE AS CALLER
AS
EXTERNAL NAME [ProtoBufTransportSql].[ProtoBuf.Transport.Sql.StoredProcedures].[ReadTransportFile]

GO

CREATE PROCEDURE [exch].[WriteTransportFile]
	@fullFileName [nvarchar](255),
	@tableName [nvarchar](255)
WITH 
 EXECUTE AS CALLER
AS
EXTERNAL NAME [ProtoBufTransportSql].[ProtoBuf.Transport.Sql.StoredProcedures].[WriteTransportFile]

GO