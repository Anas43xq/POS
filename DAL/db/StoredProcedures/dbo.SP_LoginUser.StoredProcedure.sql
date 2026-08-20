USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_LoginUser] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_LoginUser]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Username = LTRIM(RTRIM(@Username));

    IF @Username IS NULL OR @Username = N''
    BEGIN
        RETURN;
    END

    SELECT
        u.[UserId],
        u.[FullName],
        u.[Username],
        u.[PasswordHash],
        u.[RoleId],
        r.[RoleName],
        u.[IsActive]
    FROM [dbo].[Users] AS u
    INNER JOIN [dbo].[Roles] AS r
        ON r.[RoleId] = u.[RoleId]
    WHERE u.[Username] = @Username;
END
GO
