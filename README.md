# SportsAnalytics

If PostGreSQL is not setup yet do this first:
Open PgAdmin / PostGreSQL
Open the dropdowns servers => databases
right click databases and create new database
Name the new database EXACTLY "SportsAnalyticsDB"


ALL STAR API KEY: 59880cc7-33cb-4da3-b970-f14938a8eaf9


Steps To Download the updated project and get the API and SQL database connected
1. Select the green "Code" Button and download the ZIP file
2. Extract the file to your own repository file/ where ever you wanna store the capstone
3. Open SportsAnalytics.slnx
4. in Visual Studio right click SportsAnalytics.Web in the solution explorer tab at the top
5. Click "Set as Startup Project"
6. Select Build => Rebuild Solution
7. Right click SportsAnalytics.Web again and select "Manage User Secrets" => this should open secrets.json
8. Copy this code to secrets.json and replace with your own BALLDONTLIE API key and PostGre SQL password

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SportsAnalyticsDB;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
    },
  
  "BallDontLie": {
    "ApiKey": "BALLDONTLIE_API_KEY"
  }
}

9. Then open Tools → NuGet Package Manager → Package Manager Console, select SportsAnalytics.Web, and run these command lines
     
Get-Migration

(Make sure this runs with no errors before running update-database)

Update-Database

10. close and reopen your PgAdmin / postgreSQL => navigate to your SportsAnalyticsDB database => dropdown Schemas => Tables -- you should see 4 tables have been created inside PostGreSQL
11. Launch the project in visual studio and add /basketball to the end of the url | this shoould look like "https://localhost:7008/basketball"
12. Click Import NBA players and this should get the first 100 players alphabetically in the NBA
13. Everything is done, close the program and relaunch in visual studio and navigate back to the /basketball url => the list should automatically appear without importing again since all data should be stored now in the SQL database

