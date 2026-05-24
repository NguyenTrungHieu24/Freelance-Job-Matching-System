# Add migrate: 
dotnet ef migrations add <MigrateName> -p BusinessObjects -s API
# Run migrate:
dotnet ef database update -p BusinessObjects -s API
# Drop data:
dotnet ef database drop -p BusinessObjects -s API