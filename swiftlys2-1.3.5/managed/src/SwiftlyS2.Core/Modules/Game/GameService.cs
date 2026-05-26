using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Core.SchemaDefinitions;
using SwiftlyS2.Core.Schemas;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Services;
using SwiftlyS2.Shared.EntitySystem;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.Services;

internal class GameService : IGameService
{
    private readonly IEntitySystemService entitySystemService;
    // m_bMapHasBombZone (hash: 0x6295CF65D3F4FD4D) + 0x02
    private static readonly Lazy<nint> MatchStructOffsetLazy = new(() => Schema.GetOffset(0x6295CF65D3F4FD4D) + 0x02, LazyThreadSafetyMode.None);
    private static nint MatchStructOffset => MatchStructOffsetLazy.Value;

    public GameService( IEntitySystemService entitySystemService )
    {
        this.entitySystemService = entitySystemService;
    }

    public unsafe ref readonly CCSMatch MatchData => ref *GetCCSMatchPtr();

    public unsafe void Reset()
    {
        var match = GetCCSMatchPtr();
        var gameRules = GetGameRules();

        match->ActualRoundsPlayed = 0;
        match->NOvertimePlaying = 0;
        match->CTScoreFirstHalf = 0;
        match->CTScoreSecondHalf = 0;
        match->CTScoreOvertime = 0;
        match->CTScoreTotal = 0;
        match->TerroristScoreFirstHalf = 0;
        match->TerroristScoreSecondHalf = 0;
        match->TerroristScoreOvertime = 0;
        match->TerroristScoreTotal = 0;
        match->Phase = GamePhase.GAMEPHASE_PLAYING_STANDARD;

        gameRules.TotalRoundsPlayed = 0;
        gameRules.OvertimePlaying = 0;
        gameRules.GamePhaseEnum = GamePhase.GAMEPHASE_PLAYING_STANDARD;
        gameRules.TotalRoundsPlayedUpdated();
        gameRules.OvertimePlayingUpdated();
        gameRules.GamePhaseUpdated();

        UpdateTeamScores();
    }

    public unsafe void SetPhase( GamePhase phase )
    {
        var match = GetCCSMatchPtr();
        var gameRules = GetGameRules();

        match->Phase = phase;

        gameRules.GamePhaseEnum = phase;
        gameRules.GamePhaseUpdated();
    }

    public unsafe void AddTerroristWins( int numWins )
    {
        var match = GetCCSMatchPtr();
        var gameRules = GetGameRules();

        match->ActualRoundsPlayed += (short)numWins;

        gameRules.TotalRoundsPlayed = match->ActualRoundsPlayed;
        gameRules.TotalRoundsPlayedUpdated();

        AddTerroristScore(numWins);
    }

    public unsafe void AddCTWins( int numWins )
    {
        var match = GetCCSMatchPtr();
        var gameRules = GetGameRules();

        match->ActualRoundsPlayed += (short)numWins;

        gameRules.TotalRoundsPlayed = match->ActualRoundsPlayed;
        gameRules.TotalRoundsPlayedUpdated();

        AddCTScore(numWins);
    }

    public unsafe void IncrementRound( int numRounds = 1 )
    {
        var match = GetCCSMatchPtr();
        var gameRules = GetGameRules();

        match->ActualRoundsPlayed += (short)numRounds;

        gameRules.TotalRoundsPlayed = match->ActualRoundsPlayed;
        gameRules.TotalRoundsPlayedUpdated();
    }

    public void AddTerroristBonusPoints( int points )
    {
        AddTerroristScore(points);
    }

    public void AddCTBonusPoints( int points )
    {
        AddCTScore(points);
    }

    public unsafe void AddTerroristScore( int score )
    {
        var match = GetCCSMatchPtr();

        match->TerroristScoreTotal += (short)score;

        if (match->NOvertimePlaying > 0)
        {
            match->TerroristScoreOvertime += (short)score;
        }
        else if (match->Phase == GamePhase.GAMEPHASE_PLAYING_FIRST_HALF)
        {
            match->TerroristScoreFirstHalf += (short)score;
        }
        else if (match->Phase == GamePhase.GAMEPHASE_PLAYING_SECOND_HALF)
        {
            match->TerroristScoreSecondHalf += (short)score;
        }

        UpdateTeamScores();
    }

    public unsafe void AddCTScore( int score )
    {
        var match = GetCCSMatchPtr();

        match->CTScoreTotal += (short)score;

        if (match->NOvertimePlaying > 0)
        {
            match->CTScoreOvertime += (short)score;
        }
        else if (match->Phase == GamePhase.GAMEPHASE_PLAYING_FIRST_HALF)
        {
            match->CTScoreFirstHalf += (short)score;
        }
        else if (match->Phase == GamePhase.GAMEPHASE_PLAYING_SECOND_HALF)
        {
            match->CTScoreSecondHalf += (short)score;
        }

        UpdateTeamScores();
    }

    public unsafe void GoToOvertime( int numOvertimesToAdd = 1 )
    {
        var match = GetCCSMatchPtr();
        var gameRules = GetGameRules();

        match->NOvertimePlaying += (short)numOvertimesToAdd;

        gameRules.OvertimePlaying = match->NOvertimePlaying;
        gameRules.OvertimePlayingUpdated();
    }

    public unsafe void SwapTeamScores()
    {
        var match = GetCCSMatchPtr();

        (match->TerroristScoreFirstHalf, match->CTScoreFirstHalf) = (match->CTScoreFirstHalf, match->TerroristScoreFirstHalf);
        (match->TerroristScoreSecondHalf, match->CTScoreSecondHalf) = (match->CTScoreSecondHalf, match->TerroristScoreSecondHalf);
        (match->TerroristScoreOvertime, match->CTScoreOvertime) = (match->CTScoreOvertime, match->TerroristScoreOvertime);
        (match->TerroristScoreTotal, match->CTScoreTotal) = (match->CTScoreTotal, match->TerroristScoreTotal);

        UpdateTeamScores();
    }

    public unsafe int GetWinningTeam()
    {
        var match = GetCCSMatchPtr();
        var teams = entitySystemService.GetAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");

        foreach (var team in teams)
        {
            if (team.TeamNum == (int)Team.T && team.Surrendered)
            {
                return (int)Team.CT;
            }

            if (team.TeamNum == (int)Team.CT && team.Surrendered)
            {
                return (int)Team.T;
            }
        }

        return match->TerroristScoreTotal > match->CTScoreTotal
            ? (int)Team.T
            : match->TerroristScoreTotal < match->CTScoreTotal ? (int)Team.CT : (int)Team.None;
    }

    private unsafe void UpdateTeamScores()
    {
        var match = GetCCSMatchPtr();
        var teams = entitySystemService.GetAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");

        foreach (var team in teams)
        {
            switch (team.TeamNum)
            {
                case (int)Team.T:
                    UpdateTeamEntity(team, match->TerroristScoreTotal, match->TerroristScoreFirstHalf, match->TerroristScoreSecondHalf, match->TerroristScoreOvertime);
                    break;

                case (int)Team.CT:
                    UpdateTeamEntity(team, match->CTScoreTotal, match->CTScoreFirstHalf, match->CTScoreSecondHalf, match->CTScoreOvertime);
                    break;
                default:
                    break;
            }
        }
    }

    private CCSGameRules GetGameRules()
    {
        var gameRules = entitySystemService.GetGameRules();
        return gameRules?.IsValid ?? false ? gameRules : throw new InvalidOperationException("GameRules not found.");
    }

    private unsafe CCSMatch* GetCCSMatchPtr()
    {
        var gameRules = GetGameRules();
        return (CCSMatch*)(gameRules.Address + MatchStructOffset);
    }

    private static void UpdateTeamEntity( CCSTeam team, int totalScore, int firstHalfScore, int secondHalfScore, int overtimeScore )
    {
        team.Score = totalScore;
        team.ScoreFirstHalf = firstHalfScore;
        team.ScoreSecondHalf = secondHalfScore;
        team.ScoreOvertime = overtimeScore;
        team.ScoreUpdated();
        team.ScoreFirstHalfUpdated();
        team.ScoreSecondHalfUpdated();
        team.ScoreOvertimeUpdated();
    }

    public void TerminateRound( RoundEndReason reason, float delay )
    {
        GetGameRules().TerminateRound(reason, delay);
    }
    
    public void GoToIntermission(bool abortedMatch = false)
    {
        GetGameRules().GoToIntermission(abortedMatch);
    }

    public CHEGrenadeProjectile EmitHEGrenade( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
        => CHEGrenadeProjectileImpl.EmitGrenade(pos, angle, velocity, owner);

    public Task<CHEGrenadeProjectile> EmitHEGrenadeAsync( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
        => CHEGrenadeProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, owner);

    public CFlashbangProjectile EmitFlashbang( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
        => CFlashbangProjectileImpl.EmitGrenade(pos, angle, velocity, owner);

    public Task<CFlashbangProjectile> EmitFlashbangAsync( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
        => CFlashbangProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, owner);

    public CSmokeGrenadeProjectile EmitSmokeGrenade( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CSmokeGrenadeProjectileImpl.EmitGrenade(pos, angle, velocity, team, owner);

    public Task<CSmokeGrenadeProjectile> EmitSmokeGrenadeAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CSmokeGrenadeProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, team, owner);

    public CMolotovProjectile EmitMolotov( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CMolotovProjectileImpl.EmitGrenade(pos, angle, velocity, team, owner);

    public Task<CMolotovProjectile> EmitMolotovAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CMolotovProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, team, owner);

    public CDecoyProjectile EmitDecoy( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
        => CDecoyProjectileImpl.EmitGrenade(pos, angle, velocity, owner);

    public Task<CDecoyProjectile> EmitDecoyAsync( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
        => CDecoyProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, owner);
}