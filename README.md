# Tournament Management Mobile Application - Tournament Data Server
This server is responsible for tournament data management, including tournament & stage creation, match generation, match & stage results calculation and update, and more. The server communicates mainly with the `data` schema in the PostgreSQL database.
In the code, "Rr" stand for "Round Robin", and "Se" stand for "Single Elimination".
## Environment variables
After cloning this project, please add the following variables in the `environmentVariables` section in the launchSettings.json:
- JWT_SECRET_KEY: The secret key string for encoding and decoding JWT tokens.
## Database schema
![Database schema](https://drive.google.com/thumbnail?id=13SmD8vU9qfhLpsRa0FEM-rZPTxkGYXts&sz=w1000)
## List of endpoints
###	TournamentController.cs:
- /tournaments/public (GET)
- /tournaments/all (GET)
	- Header key: Authorization
- /tournaments/<id> (GET)
	- Header key: Authorization
- /tournaments (POST)
	- Header key: Authorization
	- Body keys: name (string), start_date (string), end_date (string), places (string[]), description (string), is_private (bool)
- /tournaments/<id> (PUT)
	- Header key: Authorization
	- Body keys: name (string), start_date (string), end_date (string), places (string[]), description (string), is_private (bool)
- /tournaments/<id> (DELETE)
	- Header key: Authorization
### StageController.cs:
- /stages/all/<tournament_id> (GET)
	- Header key: Authorization
- /stages/<id> (GET)
	- Header key: Authorization
- /stages (POST)
	- Header key: Authorization
	- Body keys: name (string), format_id (int), start_date (string), end_date (string), places (string[]), description (string), tournament_id (int), number_of_teams_per_group (int), number_of_groups (int), stage_order (int), include_third_place_match (bool), number_of_legs_per_round (int[]), best_of_per_round (int[]), third_place_match_number_of_legs (int), third_place_match_best_of (int), win_point (double), draw_point (double), lose_point (double), other_criteria_names (string[]), other_criteria_sort_direction (string[])
- /stages/<id> (PUT)
	- Header key: Authorization
	- Body keys: name (string), start_date (string), end_date (string), places (string[]), description (string), tournament_id (int),
- /stages/order (PUT)
	- Header key: Authorization
	- Body keys: List of: id (int), name (string), tournament_id (int), stage_order (int)
- /stages/<id> (DELETE)
	- Header key: Authorization
### StageFormatController.cs:
- /stage_format (GET)
- /stage_format/<id> (GET)
### MatchSeController.cs:
- /matches/se/all/<stage_id> (GET)
	- Header key: Authorization
- /matches/se/<id> (GET)
	- Header key: Authorization
- /matches/se/<id>/team_name (PUT)
	- Header key: Authorization
	- Body keys: team_1 (string), team_2 (string)
- /matches/se/<id>/match_info (PUT)
	- Header key: Authorization
	- Body keys: start_datetime (string), place (string), note (string)
- /matches/se/<id>/match_score (PUT)
	- Header key: Authorization
	- Body keys: winner (string), team_1_scores (double[]), team_2_scores (double[]), team_1_subscores (double[]), team_2_subscores (double[])
### MatchRrController.cs:
- /matches/rr/all/<stage_id> (GET)
	- Header key: Authorization
- /matches/rr/<id> (GET)
	- Header key: Authorization
- /matches/rr/table_results/<stage_id>/<group_number> (GET)
	- Header key: Authorization
- /matches/rr/<id>/team_name (PUT)
	- Header key: Authorization
	- Body keys: old_team_name (string), new_team_name (string)
- /matches/rr/<id>/match_info (PUT)
	- Header key: Authorization
	- Body keys: start_datetime (string), place (string), note (string)
- /matches/rr/<id>/match_score (PUT)
	- Header key: Authorization
	- Body keys: winner (string), team_1_score (double), team_2_score (double), team_1_subscores (double[]), team_2_subscores (double[]), team_1_other_criteria_values (double[]), team_2_other_criteria_values (double[])