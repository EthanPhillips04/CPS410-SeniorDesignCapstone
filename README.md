# CPS410-SeniorDesignCapstone
Group project for scientific visualization 

Defined Requirements:
 - Display Professional Teams
 - Display current players
 - display each player Stats
 - Calculate a player rating
 - Visualize overall player strengths / weaknesses
 - allow for user to select two teams and trade / move players
 - Calculate a team rating / update rating as players are moved
 - Create an estimated impact to the team as players are introduced.



Create Schema for players/teams (Rough Draft):
Sport: 
sport_id
name

League:
league_id
sport_id
name
level

Team:
team_id
league_id
name
city/ location

Player:
team_id
player_id
age
name
position

Player_Stats:
player_id
games_played
season
minutes
stat_1
.....
stat_n

Roster:
team_id
player_id
season
league_id

Player_Ratings:


Simulation_Team:

