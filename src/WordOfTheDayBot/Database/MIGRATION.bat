@echo off
cd ..
set /p migrationDescription= Name/describe this migration (DONT USE SPACES BREH... USE PASCAL CASE): 
dotnet ef migrations add %migrationDescription% --output-dir Database\Migrations
dotnet ef database update
PAUSE