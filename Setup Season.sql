use PPF
delete from pick
delete from game
delete from playoffsuperbowlpick
delete from standardpick
delete from standardplayoffsuperbowlpick
delete from season


-- season
insert into Season (startdate, enddate, iscurrent, numberofweeks) values(GETDATE(), dateadd(week, 17, GETDATE()), 1, 17);

declare 
@home_team int, 
@away_team int, 
@season_id int, 
@counter int, 
@nfcgames int,
@afcgames int,
@counterTeams int


select @season_id = id from season where iscurrent = 1; 

set @counter = 0;

while (@counter < 17)
begin;
	select @nfcgames = count(id)/2 from team where upper(conference) = 'NFC';
	set @counterTeams  = 0;
	while (@counterTeams < @nfcgames)
	begin;
		select @home_team = id from team where id not in 
		(select hometeam_id from game where season_id = @season_id and week = @counter + 1)
		and id not in 
		(select awayteam_id from game where season_id = @season_id and week = @counter + 1)
		and upper(conference) = 'NFC';

		select @away_team = id from team where id not in 
		(select awayteam_id from game where season_id = @season_id and week = @counter + 1)
		and id not in 
		(select hometeam_id from game where season_id = @season_id and week = @counter + 1)
		and upper(conference) = 'NFC' and id != @home_team;

		insert into game values(@season_id, dateadd(day, @counter + 1, GETDATE()), 0, 0, @counter + 1, @home_team, @away_team, 0, 0, GETDATE(), 'jstevenson');
		set @counterTeams = @counterTeams + 1;
	end;
	
	select @afcgames = count(id)/2 from team where upper(conference) = 'AFC';
	set @counterTeams  = 0;
	while (@counterTeams < @afcgames)
	begin;
		select @home_team = id from team where id not in 
		(select hometeam_id from game where season_id = @season_id and week = @counter + 1)
		and id not in 
		(select awayteam_id from game where season_id = @season_id and week = @counter + 1)
		and upper(conference) = 'AFC';

		select @away_team = id from team where id not in 
		(select awayteam_id from game where season_id = @season_id and week = @counter + 1)
		and id not in 
		(select hometeam_id from game where season_id = @season_id and week = @counter + 1)
		and upper(conference) = 'AFC' and id != @home_team;

		insert into game values(@season_id, dateadd(day, @counter + 1, GETDATE()), 0, 0, @counter + 1, @home_team, @away_team, 0,0,GETDATE(),'jstevenson');
		set @counterTeams = @counterTeams + 1;
	end;
	set @counter = @counter + 1;
	
end;
	
