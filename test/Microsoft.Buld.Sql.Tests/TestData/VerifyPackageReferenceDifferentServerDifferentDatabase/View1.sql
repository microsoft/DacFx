-- Table1 definition comes from dacpac reference
-- Dacpac is from different server and database which is why extra SQLCMD variables are needed for the 4-part name
CREATE VIEW [dbo].[View1] AS
SELECT c1, c2 FROM [$(RefServer)].[$(RefProj)].[dbo].[Table1]
