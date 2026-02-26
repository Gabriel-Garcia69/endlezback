$cs = 'Server=localhost;Database=Endlez;User Id=sa;Password=test;TrustServerCertificate=True;'
$conn = New-Object System.Data.SqlClient.SqlConnection($cs)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = 'SELECT TOP 1 CAST(Id AS NVARCHAR(36)) FROM PendingPayment ORDER BY CreatedAt DESC'
$r = $cmd.ExecuteScalar()
$conn.Close()
Write-Output $r
